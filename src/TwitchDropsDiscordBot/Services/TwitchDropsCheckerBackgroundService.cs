using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TwitchDropsDiscordBot.Models.Configuration;
using TwitchDropsDiscordBot.Models.SunkwiApi;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropsCheckerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TwitchDropsCheckerBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan? waitDuration = null;

            try
            {
                await using (AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
                {
                    BotConfiguration botConfiguration = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<BotConfiguration>>().Value;
                    waitDuration = GetWaitDelayDuration(botConfiguration.DelayBetweenChecksInMinutes);

                    GameConfiguration gameConfiguration = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<GameConfiguration>>().Value;
                    TwitchDropFinderService twitchDropFinderService = scope.ServiceProvider.GetRequiredService<TwitchDropFinderService>();
                    List<GetDropsResponse> newDrops = await twitchDropFinderService.FindNewDropsAsync(gameConfiguration.GameNames);

                    if (newDrops.Count > 0)
                    {
                        Console.WriteLine("Sending notifications for new drops...");

                        DiscordConfiguration discordConfiguration = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<DiscordConfiguration>>().Value;
                        await using (DiscordNotificationService discordNotificationService = scope.ServiceProvider.GetRequiredService<DiscordNotificationService>())
                        {
                            await discordNotificationService.SendTwitchDropNotificationsAsync(discordConfiguration.BotToken, discordConfiguration.TargetChannelId, newDrops);
                        }

                        Console.WriteLine("Finished sending notifications for new drops.");
                    }
                    else
                    {
                        Console.WriteLine("No new drops found...");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Exception thrown in BackgroundService: {ex.Message}\n{ex.StackTrace}");
            }

            Console.WriteLine($"Waiting for {waitDuration!.Value.TotalMinutes} minutes before checking for new drops again.");
            await Task.Delay(waitDuration.Value, stoppingToken);
        }
    }

    private static TimeSpan GetWaitDelayDuration(uint delayBetweenChecksInMinutes)
    {
        TimeSpan fallbackWaitDuration = TimeSpan.FromMinutes(30);
        TimeSpan configurationWaitDuration = TimeSpan.FromMinutes(delayBetweenChecksInMinutes);

        if (configurationWaitDuration.TotalMinutes < 1 || configurationWaitDuration.TotalHours > 24)
        {
            Console.WriteLine($"An invalid wait duration was supplied in appsettings. Falling back to {fallbackWaitDuration.TotalMinutes} minutes.");
            return fallbackWaitDuration;
        }

        return configurationWaitDuration;
    }
}
