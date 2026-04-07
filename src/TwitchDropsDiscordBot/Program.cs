using System.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchDropsDiscordBot.Contexts;
using TwitchDropsDiscordBot.Models.Configuration;
using TwitchDropsDiscordBot.Persistence;
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
                        .Configure<GameConfiguration>(builder.Configuration.GetRequiredSection(GameConfiguration.SectionKey))
                        .Configure<BotConfiguration>(builder.Configuration.GetRequiredSection(BotConfiguration.SectionKey));

        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System)
                        .AddHttpClient<SunkwiApiClient>()
                        .SetHandlerLifetime(TimeSpan.FromMinutes(1));

        builder.Services.AddDbContext<TwitchDropsBotDbContext>(dbContextOptions => dbContextOptions.UseNpgsql(builder.Configuration.GetConnectionString("Postgresql"))
                                                                                                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                                                                                   .UseSnakeCaseNamingConvention());

        builder.Services.AddScoped<SunkwiApiClient>()
                        .AddScoped<DiscordBotClient>()
                        .AddScoped<AlertHistoryFileRepository>()
                        .AddScoped<AlertHistoryService>()
                        .AddScoped<DiscordEmbedBuilderService>()
                        .AddScoped<DiscordNotificationService>()
                        .AddScoped<TwitchDropFinderService>();

        builder.Services.AddHostedService<TwitchDropsCheckerBackgroundService>();

        IHost host = builder.Build();
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

        await using (DiscordNotificationService discordNotificationService = serviceProvider.GetRequiredService<DiscordNotificationService>())
        {
            DiscordConfiguration discordConfiguration = serviceProvider.GetRequiredService<IOptions<DiscordConfiguration>>().Value;
            await discordNotificationService.SendStartupCompleteNotificationAsync(discordConfiguration.BotToken,
                                                                                  discordConfiguration.TargetChannelId,
                                                                                  GCSettings.IsServerGC,
                                                                                  GCSettings.LargeObjectHeapCompactionMode,
                                                                                  isDevelopment,
                                                                                  Environment.ProcessId,
                                                                                  Environment.MachineName);
        }
    }

    private static async Task SeedGameNamesAsync(IServiceProvider serviceProvider)
    {
        List<string> gamesToSeed = [
            "Rainbow Six Siege X",
            "Phasmophobia",
            "Counter-Strike"
        ];

        await using (AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope())
        {
            TwitchDropsBotSqlRepository sqlRepository = serviceScope.ServiceProvider.GetRequiredService<TwitchDropsBotSqlRepository>();
            await sqlRepository.InsertNewGamesAsync(gamesToSeed);
        }
    }
}
