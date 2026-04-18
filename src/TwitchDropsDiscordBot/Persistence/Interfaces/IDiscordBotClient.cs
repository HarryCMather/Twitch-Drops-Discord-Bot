using Discord;

namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface IDiscordBotClient : IAsyncDisposable
{
    bool IsInitialized { get; }

    Task InitializeAsync(string discordBotToken, ulong discordChannelId);

    Task SendMessageAsync(Embed embed);
}
