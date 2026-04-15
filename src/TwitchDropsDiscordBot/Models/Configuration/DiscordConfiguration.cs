namespace TwitchDropsDiscordBot.Models.Configuration;

public sealed record DiscordConfiguration
{
    public const string SectionKey = "Discord";

    /// <summary>
    /// The Discord Bot Token, which is required to send messages to your server's text channel.
    /// This can be generated here:  https://discord.com/developers/applications
    /// </summary>
    public string BotToken { get; init; }

    /// <summary>
    /// The Discord ChannelID of the text channel where you want the notifications to be sent.
    /// </summary>
    public ulong TargetChannelId { get; init; }
}
