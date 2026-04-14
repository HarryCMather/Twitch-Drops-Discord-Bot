using System.Runtime;
using Discord;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

public sealed class DiscordNotificationService : IAsyncDisposable
{
    private readonly DiscordEmbedBuilderService _discordEmbedBuilderService;
    private readonly TwitchDropsBotSqlRepository _twitchDropsBotSqlRepository;
    private readonly DiscordBotClient _discordBotClient;
    private readonly TimeProvider _timeProvider;

    public DiscordNotificationService(DiscordEmbedBuilderService discordEmbedBuilderService, TwitchDropsBotSqlRepository twitchDropsBotSqlRepository, DiscordBotClient discordBotClient, TimeProvider timeProvider)
    {
        _discordEmbedBuilderService = discordEmbedBuilderService;
        _twitchDropsBotSqlRepository = twitchDropsBotSqlRepository;
        _discordBotClient = discordBotClient;
        _timeProvider = timeProvider;
    }

    public async Task SendStartupCompleteNotificationAsync(string discordBotToken, ulong discordBotChannelId, bool isServerGc, GCLargeObjectHeapCompactionMode lohCompactionMode, bool isDevelopment, int processId, string hostname)
    {
        if (!_discordBotClient.IsInitialized)
        {
            await _discordBotClient.InitializeAsync(discordBotToken, discordBotChannelId);
        }

        Embed embed = _discordEmbedBuilderService.BuildEmbedForStartupComplete(isServerGc, lohCompactionMode, isDevelopment, processId, hostname);
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
        await _twitchDropsBotSqlRepository.InsertNewDropsAsync(drops);
        await _twitchDropsBotSqlRepository.InsertTimeBasedDropsAsync(timeBasedDrops);
    }

    private async Task SendTwitchDropRewardNotificationAsync(Drop drop)
    {
        Embed embed = _discordEmbedBuilderService.BuildEmbedForTwitchDropReward(drop);
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
