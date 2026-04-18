using System.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using TwitchDropsDiscordBot.Contexts;
using TwitchDropsDiscordBot.Extensions;
using TwitchDropsDiscordBot.Models.Configuration;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence;
using TwitchDropsDiscordBot.Persistence.Interfaces;
using TwitchDropsDiscordBot.Services;
using TwitchDropsDiscordBot.Services.Interfaces;

namespace TwitchDropsDiscordBot;

internal class Program
{
    private static async Task Main()
    {
        SetGcSettings();

        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Logging.ClearProviders();
        LogManager.Setup().LoadConfigurationFromFile("NLog.config");
        builder.Logging.AddNLogWeb(new NLogAspNetCoreOptions
        {
            RemoveLoggerFactoryFilter = false
        });

        builder.Services.Configure<ServiceProviderOptions>(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        builder.Services.Configure<DiscordConfiguration>(builder.Configuration.GetRequiredSection(DiscordConfiguration.SectionKey))
                        .Configure<BotConfiguration>(builder.Configuration.GetRequiredSection(BotConfiguration.SectionKey));

        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System)
                        .AddHttpClient<SunkwiApiClient>()
                        .SetHandlerLifetime(TimeSpan.FromMinutes(1));

        builder.Services.AddDbContext<TwitchDropsBotDbContext>(dbContextOptions => dbContextOptions.UseNpgsql(builder.Configuration.GetConnectionString("Postgresql"))
                                                                                                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                                                                                   .UseSnakeCaseNamingConvention());

        bool isDevelopment = builder.Environment.IsDevelopment();
        string hostname = Environment.MachineName;

        builder.Services.AddCustomOpenTelemetry(builder.Configuration, hostname)
                        .AddScoped<ITwitchDropFinderRepository, SunkwiApiClient>()
                        .AddScoped<IDiscordBotClient, DiscordBotClient>()
                        .AddScoped<IGamesRepository, GamesSqlRepository>()
                        .AddScoped<IDropOwnerRepository, DropOwnerSqlRepository>()
                        .AddScoped<IDropsRepository, DropsSqlRepository>()
                        .AddScoped<IEmbedBuilderService, DiscordEmbedBuilderService>()
                        .AddScoped<INotificationService, DiscordNotificationService>()
                        .AddScoped<ITwitchDropsFilterService, TwitchDropsFilterService>()
                        .AddScoped<ITwitchDropFinderService, TwitchDropFinderService>();

        builder.Services.AddHostedService<TwitchDropsCheckerBackgroundService>();

        IHost host = builder.Build();
        await PerformPreStartupTasks(host.Services, isDevelopment, hostname);
        await host.RunAsync();

        LogManager.Shutdown();
    }

    private static void SetGcSettings()
    {
        const ulong gcHardLimitBytes = (ulong)200 * 1024 * 1024; // 200MB
        AppContext.SetData("GCHeapHardLimit", gcHardLimitBytes);
        GC.RefreshMemoryLimit();
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
    }

    private static async Task PerformPreStartupTasks(IServiceProvider serviceProvider, bool isDevelopment, string hostname)
    {
        ILogger<Program> startupLogger = serviceProvider.GetRequiredService<ILogger<Program>>();
        await ApplyDatabaseMigrationsAsync(serviceProvider, startupLogger);
        await SeedGameNamesAsync(serviceProvider);
        await LogStartupCompleteAsync(serviceProvider, startupLogger, isDevelopment, hostname);
    }

    private static async Task ApplyDatabaseMigrationsAsync(IServiceProvider serviceProvider, ILogger<Program> logger)
    {
        await using (AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope())
        await using (TwitchDropsBotDbContext dbContext = serviceScope.ServiceProvider.GetRequiredService<TwitchDropsBotDbContext>())
        {
            IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogWarning("Applying pending database migrations...");
                await dbContext.Database.MigrateAsync();
                logger.LogWarning("Database migrations were successfully applied.");
            }
            else
            {
                logger.LogInformation("No pending database migrations were found.");
            }
        }
    }

    private static async Task SeedGameNamesAsync(IServiceProvider serviceProvider)
    {
        List<string> gameNamesToSeed = [
            "Rainbow Six Siege X",
            "Rainbow Six Siege",
            "Phasmophobia",
            "Counter-Strike"
        ];

        await using (AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope())
        {
            IGamesRepository gamesRepository = serviceScope.ServiceProvider.GetRequiredService<IGamesRepository>();

            IEnumerable<string> existingGames = await gamesRepository.GetExistingMatchingGamesAsync(gameNamesToSeed, CancellationToken.None);
            List<Game> games = gameNamesToSeed.Except(existingGames)
                                              .Select(gameName => new Game
                                              {
                                                  Name = gameName,
                                                  ShouldAlert = true
                                              })
                                              .ToList();

            if (games.Count > 0)
            {
                await gamesRepository.InsertGamesAsync(games, CancellationToken.None);
            }
        }
    }

    private static async Task LogStartupCompleteAsync(IServiceProvider serviceProvider, ILogger<Program> logger, bool isDevelopment, string hostname)
    {
        string startupCompleteMessage = "Twitch Drops Discord Bot Started Successfully...\n" +
                                        $"ServerGC: {GCSettings.IsServerGC}\n" +
                                        $"LOHCompactionMode: {GCSettings.LargeObjectHeapCompactionMode}\n" +
                                        $"IsDevelopment: {isDevelopment}\n" +
                                        $"ProcessId: {Environment.ProcessId}\n" +
                                        $"Hostname: {hostname}";
        logger.LogInformation(startupCompleteMessage);

        await using (INotificationService notificationService = serviceProvider.GetRequiredService<INotificationService>())
        {
            DiscordConfiguration discordConfiguration = serviceProvider.GetRequiredService<IOptions<DiscordConfiguration>>().Value;
            await notificationService.SendStartupCompleteNotificationAsync(discordConfiguration.BotToken,
                                                                           discordConfiguration.TargetChannelId,
                                                                           GCSettings.IsServerGC,
                                                                           GCSettings.LargeObjectHeapCompactionMode,
                                                                           isDevelopment,
                                                                           Environment.ProcessId,
                                                                           hostname);
        }
    }
}
