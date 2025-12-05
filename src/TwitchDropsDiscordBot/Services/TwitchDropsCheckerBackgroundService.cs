using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TwitchDropsDiscordBot.Models;
using TwitchDropsDiscordBot.Models.SunkwiApi;
using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropsCheckerBackgroundService : BackgroundService
{
    private readonly SettingsFileRepository _settingsFileRepository;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TwitchDropsCheckerBackgroundService(SettingsFileRepository settingsFileRepository, IServiceScopeFactory serviceScopeFactory)
    {
        _settingsFileRepository = settingsFileRepository;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan? waitDuration = null;
            TimeSpan fallbackWaitDuration = TimeSpan.FromMinutes(30);

            try
            {
                Settings settings = await _settingsFileRepository.GetSettingsFromFileAsync();
                waitDuration = TimeSpan.FromMinutes(settings.DelayBetweenChecksInMinutes);

                await using (AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
                {
                    TwitchDropFinderService twitchDropFinderService = scope.ServiceProvider.GetRequiredService<TwitchDropFinderService>();
                    List<GetDropsResponse> newDrops = await twitchDropFinderService.FindNewDropsAsync(settings.GameNames);

                    if (newDrops.Count > 0)
                    {
                        Console.WriteLine("Sending notifications for new drops...");

                        await using (DiscordNotificationService discordNotificationService = scope.ServiceProvider.GetRequiredService<DiscordNotificationService>())
                        {
                            await discordNotificationService.SendTwitchDropNotificationsAsync(settings.DiscordBotToken, settings.DiscordChannelId, newDrops);
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

            if (waitDuration is null || waitDuration.Value.TotalMinutes < 1)
            {
                Console.WriteLine($"An invalid wait duration was supplied in settings. To avoid infinite loops with high CPU usage, falling back to {fallbackWaitDuration.TotalMinutes} minutes.");
                waitDuration = fallbackWaitDuration;
            }
            Console.WriteLine($"Waiting for {waitDuration.Value.TotalMinutes} minutes before checking for new drops again.");
            await Task.Delay(waitDuration.Value, stoppingToken);
        }
    }
}
