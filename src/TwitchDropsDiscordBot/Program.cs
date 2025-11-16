using System.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitchDropsDiscordBot.Models;
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

        builder.Services.AddSingleton<SettingsFileRepository>()
                        .AddSingleton<TimeProvider>(TimeProvider.System)
                        .AddHttpClient<SunkwiApiClient>()
                        .SetHandlerLifetime(TimeSpan.FromMinutes(1));

        builder.Services.AddScoped<SunkwiApiClient>()
                        .AddScoped<DiscordBotClient>()
                        .AddScoped<AlertHistoryFileRepository>()
                        .AddScoped<AlertHistoryService>()
                        .AddScoped<DiscordEmbedBuilderService>()
                        .AddScoped<DiscordNotificationService>()
                        .AddScoped<TwitchDropFinderService>();

        builder.Services.AddHostedService<TwitchDropsCheckerBackgroundService>();

        IHost host = builder.Build();
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

        Settings settings = await serviceProvider.GetRequiredService<SettingsFileRepository>().GetSettingsFromFileAsync();
        await using (DiscordNotificationService discordNotificationService = serviceProvider.GetRequiredService<DiscordNotificationService>())
        {
            await discordNotificationService.SendStartupCompleteNotificationAsync(settings.DiscordBotToken, settings.DiscordChannelId, GCSettings.IsServerGC, GCSettings.LargeObjectHeapCompactionMode, isDevelopment, Environment.ProcessId, Environment.MachineName);
        }
    }
}
