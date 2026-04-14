using System.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchDropsDiscordBot.Contexts;
using TwitchDropsDiscordBot.Models.Configuration;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence;
using TwitchDropsDiscordBot.Persistence.Interfaces;
using TwitchDropsDiscordBot.Services;

namespace TwitchDropsDiscordBot;

internal static class Program
{
    private static async Task Main()
    {
        SetGcSettings();

        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Logging.ClearProviders();

        builder.Services.Configure<DiscordConfiguration>(builder.Configuration.GetRequiredSection(DiscordConfiguration.SectionKey))
                        .Configure<BotConfiguration>(builder.Configuration.GetRequiredSection(BotConfiguration.SectionKey));

        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System)
                        .AddHttpClient<SunkwiApiClient>()
                        .SetHandlerLifetime(TimeSpan.FromMinutes(1));

        builder.Services.AddDbContext<TwitchDropsBotDbContext>(dbContextOptions => dbContextOptions.UseNpgsql(builder.Configuration.GetConnectionString("Postgresql"))
                                                                                                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                                                                                   .UseSnakeCaseNamingConvention());

        builder.Services.AddScoped<ITwitchDropFinderRepository, SunkwiApiClient>()
                        .AddScoped<DiscordBotClient>()
                        .AddScoped<IGamesRepository, GamesSqlRepository>()
                        .AddScoped<IDropOwnerRepository, DropOwnerSqlRepository>()
                        .AddScoped<IDropsRepository, DropsSqlRepository>()
                        .AddScoped<IEmbedBuilderService, DiscordEmbedBuilderService>()
                        .AddScoped<INotificationService, DiscordNotificationService>()
                        .AddScoped<ITwitchDropsFilterService, TwitchDropsFilterService>()
                        .AddScoped<ITwitchDropFinderService, TwitchDropFinderService>();

        builder.Services.AddHostedService<TwitchDropsCheckerBackgroundService>();

        IHost host = builder.Build();
        await ApplyDatabaseMigrationsAsync(host.Services);
        await SeedGameNamesAsync(host.Services);
        await LogStartupCompleteAsync(builder.Environment.IsDevelopment(), host.Services);
        await host.RunAsync();
    }

    private static void SetGcSettings()
    {
        const ulong gcHardLimitBytes = (ulong)200 * 1024 * 1024; // 200MB
        AppContext.SetData("GCHeapHardLimit", gcHardLimitBytes);
        GC.RefreshMemoryLimit();
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
    }

    private static async Task LogStartupCompleteAsync(bool isDevelopment, IServiceProvider serviceProvider)
    {
        string startupCompleteMessage = "Twitch Drops Discord Bot Started Successfully...\n" +
                                        $"ServerGC: {GCSettings.IsServerGC}\n" +
                                        $"LOHCompactionMode: {GCSettings.LargeObjectHeapCompactionMode}\n" +
                                        $"IsDevelopment: {isDevelopment}\n" +
                                        $"ProcessId: {Environment.ProcessId}\n" +
                                        $"Hostname: {Environment.MachineName}";

        Console.WriteLine(startupCompleteMessage);

        await using (INotificationService notificationService = serviceProvider.GetRequiredService<INotificationService>())
        {
            DiscordConfiguration discordConfiguration = serviceProvider.GetRequiredService<IOptions<DiscordConfiguration>>().Value;
            await notificationService.SendStartupCompleteNotificationAsync(discordConfiguration.BotToken,
                                                                           discordConfiguration.TargetChannelId,
                                                                           GCSettings.IsServerGC,
                                                                           GCSettings.LargeObjectHeapCompactionMode,
                                                                           isDevelopment,
                                                                           Environment.ProcessId,
                                                                           Environment.MachineName);
        }
    }

    private static async Task ApplyDatabaseMigrationsAsync(IServiceProvider serviceProvider)
    {
        await using (AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope())
        await using (TwitchDropsBotDbContext dbContext = serviceScope.ServiceProvider.GetRequiredService<TwitchDropsBotDbContext>())
        {
            IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                Console.WriteLine("Applying pending database migrations...");
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("Database migrations were successfully applied.");
            }
            else
            {
                Console.WriteLine("No pending database migrations were found.");
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

            IEnumerable<string> existingGames = await gamesRepository.GetExistingMatchingGamesAsync(gameNamesToSeed);
            List<Game> games = gameNamesToSeed.Except(existingGames)
                                              .Select(gameName => new Game
                                              {
                                                  Name = gameName,
                                                  ShouldAlert = true
                                              })
                                              .ToList();

            if (games.Count > 0)
            {
                await gamesRepository.InsertGamesAsync(games);
            }
        }
    }
}
