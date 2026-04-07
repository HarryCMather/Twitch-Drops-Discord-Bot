namespace TwitchDropsDiscordBot.Models.Configuration;

public sealed record BotConfiguration
{
    public const string SectionKey = "Bot";

    /// <summary>
    /// The number of minutes to sleep before checking for more drops.
    /// </summary>
    public uint DelayBetweenChecksInMinutes { get; set; }
}
