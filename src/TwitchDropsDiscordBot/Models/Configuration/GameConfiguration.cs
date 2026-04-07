namespace TwitchDropsDiscordBot.Models.Configuration;

public sealed record GameConfiguration
{
    public const string SectionKey = "Game";

    /// <summary>
    /// A list of game names to look for drops for.
    /// The strings in this list should be/have been copied from the title at the top of the game's category page.
    /// For example, https://www.twitch.tv/directory/category/tom-clancys-rainbow-six-siege has the exact title of "Tom Clancy's Rainbow Six Siege X".
    /// </summary>
    public List<string> GameNames { get; set; }
}
