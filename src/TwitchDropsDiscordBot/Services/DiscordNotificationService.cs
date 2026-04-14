using System.Runtime;
using Discord;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

public sealed class DiscordNotificationService : IAsyncDisposable
{
    private readonly AlertHistoryService _alertHistoryService;
    private readonly DiscordEmbedBuilderService _discordEmbedBuilderService;
    private readonly DiscordBotClient _discordBotClient;

    public DiscordNotificationService(AlertHistoryService alertHistoryService, DiscordEmbedBuilderService discordEmbedBuilderService, DiscordBotClient discordBotClient)
    {
        _alertHistoryService = alertHistoryService;
        _discordEmbedBuilderService = discordEmbedBuilderService;
        _discordBotClient = discordBotClient;
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
    }

    private async Task SendTwitchDropRewardNotificationAsync(Drop drop)
    {
        Embed embed = _discordEmbedBuilderService.BuildEmbedForTwitchDropReward(drop);
        await _discordBotClient.SendMessageAsync(embed);

        IEnumerable<Guid> timeBasedDropIds = drop.TimeBasedDrops.Select(drop => drop.Id);
        await _alertHistoryService.RecordDropNotificationSentAsync(drop.Id, timeBasedDropIds);

        // Avoid spamming Discord and ensure we don't get close to their rate limits:
        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    public async ValueTask DisposeAsync()
    {
        await _discordBotClient.DisposeAsync();
    }
}
