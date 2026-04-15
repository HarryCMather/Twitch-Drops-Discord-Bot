using System.Runtime;
using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Services.Interfaces;

public interface INotificationService : IAsyncDisposable
{
    Task SendStartupCompleteNotificationAsync(string discordBotToken, ulong discordBotChannelId, bool isServerGc, GCLargeObjectHeapCompactionMode lohCompactionMode, bool isDevelopment, int processId, string hostname);

    Task SendTwitchDropNotificationsAsync(string discordBotToken, ulong discordBotChannelId, List<Drop> drops);
}
