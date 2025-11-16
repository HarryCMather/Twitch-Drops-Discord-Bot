using System.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                        .AddMemoryCache()
                        .AddHttpClient<SunkwiApiClient>()
                        .SetHandlerLifetime(TimeSpan.FromMinutes(1));

        builder.Services.AddScoped<SunkwiApiClient>()
                        .AddScoped<TwitchDropFinderService>();

        builder.Services.AddHostedService<TwitchDropsCheckerBackgroundService>();

        IHost host = builder.Build();
        LogStartupComplete(builder.Environment.IsDevelopment());
        await host.RunAsync();
    }

    private static void SetGcSettings()
    {
        const ulong gcHardLimitBytes = (ulong)200 * 1024 * 1024; // 200MB
        AppContext.SetData("GCHeapHardLimit", gcHardLimitBytes);
        GC.RefreshMemoryLimit();
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
    }

    private static void LogStartupComplete(bool isDevelopment)
    {
        string startupCompleteMessage = "Twitch Drops Discord Bot Started Successfully...\n" +
                                        $"ServerGC: {GCSettings.IsServerGC}\n" +
                                        $"LOHCompactionMode: {GCSettings.LargeObjectHeapCompactionMode}\n" +
                                        $"IsDevelopment: {isDevelopment}\n" +
                                        $"ProcessId: {Environment.ProcessId}\n" +
                                        $"Hostname: {Environment.MachineName}";

        Console.WriteLine(startupCompleteMessage);
        // TODO: LOG THIS TO THE DISCORD TEXT CHANNEL!
    }
}
