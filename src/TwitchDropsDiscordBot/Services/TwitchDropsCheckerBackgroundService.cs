using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchDropsDiscordBot.Extensions;
using TwitchDropsDiscordBot.Models.Configuration;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Services.Interfaces;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropsCheckerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TwitchDropsCheckerBackgroundService> _logger;

    public const string TraceName = $"TwitchDropsChecker.{nameof(CheckForDropsAsync)}";
    private static readonly ActivitySource ActivitySource = new(TraceName);

    public TwitchDropsCheckerBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<TwitchDropsCheckerBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Ensure all migrations have had sufficient time to complete before starting the job loop:
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan waitDuration = await CheckForDropsAsync();
            _logger.LogInformation("Waiting for {WaitDurationMinutes} minutes before checking for new drops again", waitDuration.TotalMinutes);
            await Task.Delay(waitDuration, stoppingToken);
        }
    }

    private async Task<TimeSpan> CheckForDropsAsync()
    {
        TimeSpan? waitDuration = null;

        Activity activity = ActivitySource.StartTrace(TraceName);

        try
        {
            await using (AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
            using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
            {
                // The types of configuration I am using here could change in appsettings.json between requests.
                // I was previously handling this by manually re-loading Settings through a Settings Repository before.
                // This has since been switched to IOptionsSnapshot<TOptions> as I didn't previously realise this provides this functionality out of the box.
                // This still works, as the "scope" for my config refers to each iteration within the background job loop.

                BotConfiguration botConfiguration = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<BotConfiguration>>().Value;
                waitDuration = GetWaitDelayDuration(botConfiguration.DelayBetweenChecksInMinutes);

                ITwitchDropFinderService twitchDropFinderService = scope.ServiceProvider.GetRequiredService<ITwitchDropFinderService>();
                List<Drop> newDrops = await twitchDropFinderService.FindNewDropsAsync();

                if (newDrops.Count > 0)
                {
                    _logger.LogInformation("Sending notifications for new drops");

                    DiscordConfiguration discordConfiguration = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<DiscordConfiguration>>().Value;
                    await using (INotificationService notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>())
                    {
                        await notificationService.SendTwitchDropNotificationsAsync(discordConfiguration.BotToken, discordConfiguration.TargetChannelId, newDrops);
                    }

                    _logger.LogInformation("Finished sending notifications for new drops");
                }
                else
                {
                    _logger.LogInformation("No new drops found");
                }
            }
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            _logger.LogError(ex, "Error: Exception thrown in BackgroundService: {ErrorMessage}", ex.Message);
        }
        finally
        {
            activity?.Dispose();
        }

        return waitDuration!.Value;
    }

    private TimeSpan GetWaitDelayDuration(uint delayBetweenChecksInMinutes)
    {
        TimeSpan fallbackWaitDuration = TimeSpan.FromMinutes(30);
        TimeSpan configurationWaitDuration = TimeSpan.FromMinutes(delayBetweenChecksInMinutes);

        if (configurationWaitDuration.TotalMinutes < 1 || configurationWaitDuration.TotalHours > 24)
        {
            _logger.LogError("An invalid wait duration was supplied in appsettings. Falling back to {FallbackWaitDurationMinutes} minutes", fallbackWaitDuration.TotalMinutes);
            return fallbackWaitDuration;
        }

        return configurationWaitDuration;
    }
}
