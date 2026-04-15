using System.Runtime;
using Discord;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence;
using TwitchDropsDiscordBot.Persistence.Interfaces;
using TwitchDropsDiscordBot.Services.Interfaces;

namespace TwitchDropsDiscordBot.Services;

public sealed class DiscordNotificationService : INotificationService
{
    private readonly IEmbedBuilderService _embedBuilderService;
    private readonly IDropsRepository _dropsRepository;
    private readonly DiscordBotClient _discordBotClient;
    private readonly TimeProvider _timeProvider;

    public DiscordNotificationService(IEmbedBuilderService embedBuilderService, IDropsRepository dropsRepository, DiscordBotClient discordBotClient, TimeProvider timeProvider)
    {
        _embedBuilderService = embedBuilderService;
        _dropsRepository = dropsRepository;
        _discordBotClient = discordBotClient;
        _timeProvider = timeProvider;
    }

    public async Task SendStartupCompleteNotificationAsync(string discordBotToken, ulong discordBotChannelId, bool isServerGc, GCLargeObjectHeapCompactionMode lohCompactionMode, bool isDevelopment, int processId, string hostname)
    {
        if (!_discordBotClient.IsInitialized)
        {
            await _discordBotClient.InitializeAsync(discordBotToken, discordBotChannelId);
        }

        Embed embed = _embedBuilderService.BuildEmbedForStartupComplete(isServerGc, lohCompactionMode, isDevelopment, processId, hostname);
        await _discordBotClient.SendMessageAsync(embed);
    }

    public async Task SendTwitchDropNotificationsAsync(string discordBotToken, ulong discordBotChannelId, List<Drop> drops)
    {
        if (!_discordBotClient.IsInitialized)
        {
            await _discordBotClient.InitializeAsync(discordBotToken, discordBotChannelId);
        }

        foreach (Drop drop in drops)
        {
            await SendTwitchDropRewardNotificationAsync(drop);
        }

        IEnumerable<TimeBasedDrop> timeBasedDrops = drops.SelectMany(drop => drop.TimeBasedDrops);
        await _dropsRepository.InsertNewDropsAsync(drops);
        await _dropsRepository.InsertTimeBasedDropsAsync(timeBasedDrops);
    }

    private async Task SendTwitchDropRewardNotificationAsync(Drop drop)
    {
        Embed embed = _embedBuilderService.BuildEmbedForTwitchDropReward(drop);
        await _discordBotClient.SendMessageAsync(embed);

        DateTimeOffset utcTimeStamp = _timeProvider.GetUtcNow();
        foreach (TimeBasedDrop timeBasedDrop in drop.TimeBasedDrops)
        {
            timeBasedDrop.AlertedOn = utcTimeStamp;
        }

        // Avoid spamming Discord and ensure we don't get close to their rate limits:
        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    public async ValueTask DisposeAsync()
    {
        await _discordBotClient.DisposeAsync();
    }
}
