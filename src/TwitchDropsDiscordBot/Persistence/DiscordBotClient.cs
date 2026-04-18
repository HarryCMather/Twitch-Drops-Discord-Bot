using System.Diagnostics;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TwitchDropsDiscordBot.Persistence.Interfaces;

namespace TwitchDropsDiscordBot.Persistence;

public sealed class DiscordBotClient : IDiscordBotClient
{
    private readonly ILogger<DiscordBotClient> _logger;

    private DiscordSocketClient _discordSocketClient;
    private SocketTextChannel _textChannel;
    private TaskCompletionSource<bool> _waitForReadyTaskCompletionSource;

    public DiscordBotClient(ILogger<DiscordBotClient> logger)
    {
        _logger = logger;
    }

    public bool IsInitialized { get; private set; }

    public async Task InitializeAsync(string discordBotToken, ulong discordChannelId)
    {
        _discordSocketClient = new DiscordSocketClient();
        _waitForReadyTaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        _discordSocketClient.Log += OnLog;
        _discordSocketClient.Ready += OnReady;

        await _discordSocketClient.LoginAsync(TokenType.Bot, discordBotToken);
        await _discordSocketClient.StartAsync();

        await _waitForReadyTaskCompletionSource.Task;

        _textChannel = (await _discordSocketClient.GetChannelAsync(discordChannelId)) as SocketTextChannel;
        IsInitialized = true;
    }

    public async Task SendMessageAsync(Embed embed)
    {
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            await _textChannel.SendMessageAsync(embed: embed);
        }
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
        _logger.LogInformation("DiscordBotClient-OnLog: {Message}", logMessage.ToString());
        return Task.CompletedTask;
    }

    private Task OnReady()
    {
        _waitForReadyTaskCompletionSource.TrySetResult(true);
        return Task.CompletedTask;
    }
}
