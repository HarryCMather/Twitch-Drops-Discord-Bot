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
            Settings settings = await _settingsFileRepository.GetSettingsFromFileAsync();

            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                TwitchDropFinderService twitchDropFinderService = scope.ServiceProvider.GetRequiredService<TwitchDropFinderService>();
                List<GetDropsResponse> newDrops = await twitchDropFinderService.FindNewDropsAsync(settings.GameNames);

            }

            Console.WriteLine($"Waiting for {settings.DelayBetweenChecksInMinutes} minutes before checking for new drops again.");
            await Task.Delay(TimeSpan.FromMinutes(settings.DelayBetweenChecksInMinutes), stoppingToken);
        }
    }
}
