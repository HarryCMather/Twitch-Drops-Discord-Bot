using Discord;
using Discord.WebSocket;

namespace TwitchDropsDiscordBot.Persistence;

public sealed class DiscordBotClient : IAsyncDisposable
{
    private DiscordSocketClient _discordSocketClient;
    private SocketTextChannel _textChannel;
    private TaskCompletionSource<bool> _waitForReadyTaskCompletionSource;

    public async Task InitializeAsync(string discordBotToken, ulong discordChannelId)
    {
        _discordSocketClient = new DiscordSocketClient();
        _waitForReadyTaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        _discordSocketClient.Log += OnLog;
        _discordSocketClient.Ready += OnReady;

        await _discordSocketClient.LoginAsync(TokenType.Bot, discordBotToken);
        await _discordSocketClient.StartAsync();

        await _waitForReadyTaskCompletionSource.Task;

        _textChannel = _discordSocketClient.GetChannel(discordChannelId) as SocketTextChannel;
    }

    public async Task SendMessageAsync(Embed embed)
    {
        await _textChannel.SendMessageAsync(embed: embed);
    }

    public async ValueTask DisposeAsync()
    {
        if (_discordSocketClient is not null)
        {
            _discordSocketClient.Log -= OnLog;
            _discordSocketClient.Ready -= OnReady;

            await _discordSocketClient.StopAsync();
            await _discordSocketClient.DisposeAsync();
        }

        _waitForReadyTaskCompletionSource?.Task.Dispose();
    }

    private Task OnLog(LogMessage logMessage)
    {
        Console.WriteLine(logMessage.ToString());
        return Task.CompletedTask;
    }

    private Task OnReady()
    {
        _waitForReadyTaskCompletionSource.TrySetResult(true);
        return Task.CompletedTask;
    }
}
