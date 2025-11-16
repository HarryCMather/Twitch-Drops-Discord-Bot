using System.Runtime;
using Discord;
using TwitchDropsDiscordBot.Models.SunkwiApi;
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

    public async Task SendStartupCompleteNotificationAsync(bool isServerGc, GCLargeObjectHeapCompactionMode lohCompactionMode, bool isDevelopment, int processId, string hostname)
    {
        Embed embed = _discordEmbedBuilderService.BuildEmbedForStartupComplete(isServerGc, lohCompactionMode, isDevelopment, processId, hostname);
        await _discordBotClient.SendMessageAsync(embed);
    }

    public async Task SendTwitchDropNotificationsAsync(List<GetDropsResponse> drops)
    {
        foreach (GetDropsResponse drop in drops)
        {
            foreach (GetDropsReward reward in drop.Rewards)
            {
                await SendTwitchDropRewardNotificationAsync(reward, drop.GameDisplayName);
            }
        }
    }

    private async Task SendTwitchDropRewardNotificationAsync(GetDropsReward reward, string gameDisplayName)
    {
        Embed embed = _discordEmbedBuilderService.BuildEmbedForTwitchDropReward(reward, gameDisplayName);
        await _discordBotClient.SendMessageAsync(embed);

        IEnumerable<Guid> timeBasedDropIds = reward.TimeBasedDrops.Select(drop => drop.Id);
        await _alertHistoryService.RecordDropNotificationSentAsync(reward.Id, timeBasedDropIds);

        // Avoid spamming Discord:
        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    public async ValueTask DisposeAsync()
    {
        await _discordBotClient.DisposeAsync();
    }
}
