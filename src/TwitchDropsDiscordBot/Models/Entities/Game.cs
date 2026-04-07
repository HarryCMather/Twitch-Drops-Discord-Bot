namespace TwitchDropsDiscordBot.Models.Entities;

public class Game
{
    public short Id { get; set; }

    public string Name { get; set; }

    public bool ShouldAlert { get; set; }
}
