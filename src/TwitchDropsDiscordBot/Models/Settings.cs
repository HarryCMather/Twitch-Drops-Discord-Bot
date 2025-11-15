namespace TwitchDropsDiscordBot.Models;

/// <summary>
/// Model representing Settings.json
/// </summary>
public sealed class Settings
{
    /// <summary>
    /// The ClientId, created on Twitch's Developer Portal and is unique per application.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// The ClientSecret, created on Twitch's Developer Portal and is unique per application.
    /// This MUST not be exposed or committed.
    /// </summary>
    public string ClientSecret { get; set; }

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
