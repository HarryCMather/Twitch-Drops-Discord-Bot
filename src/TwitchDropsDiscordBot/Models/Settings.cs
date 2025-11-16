namespace TwitchDropsDiscordBot.Models;

/// <summary>
/// Model representing Settings.json
/// </summary>
public sealed class Settings
{
    /// <summary>
    /// The Discord Bot Token, which is required to send messages to your server's text channel.
    /// This can be generated here:  https://discord.com/developers/applications
    /// </summary>
    public string DiscordBotToken { get; set; }

    /// <summary>
    /// The Discord ChannelID of the text channel where you want the notifications to be sent.
    /// </summary>
    public ulong DiscordChannelId { get; set; }

    /// <summary>
    /// A list of game names to look for drops for.
    /// The strings in this list should be/have been copied from the title at the top of the game's category page.
    /// For example, https://www.twitch.tv/directory/category/tom-clancys-rainbow-six-siege has the exact title of "Tom Clancy's Rainbow Six Siege X".
    /// </summary>
    public IReadOnlyCollection<string> GameNames { get; set; }

    /// <summary>
    /// The number of minutes to sleep before checking for more drops.
    /// </summary>
    public uint DelayBetweenChecksInMinutes { get; set; }
}
