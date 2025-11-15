using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TwitchDropsDiscordBot.Persistence;
using TwitchDropsDiscordBot.Services;

namespace TwitchDropsDiscordBot;

internal static class Program
{
    private static async Task Main()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Services.AddSingleton<SettingsFileRepository>()
                        .AddSingleton<TimeProvider>(TimeProvider.System)
                        .AddMemoryCache()
                        .AddHttpClient<SunkwiApiClient>()
                        .SetHandlerLifetime(TimeSpan.FromMinutes(1));

        builder.Services.AddScoped<SunkwiApiClient>()
                        .AddScoped<TwitchAuthorizationService>()
                        .AddScoped<TwitchGameIdFinderService>()
                        .AddScoped<TwitchStreamsFinderService>()
                        .AddScoped<TwitchDropFinderService>();

        builder.Services.AddHostedService<TwitchDropsCheckerBackgroundService>();

        IHost host = builder.Build();

        await host.RunAsync();
    }
}
