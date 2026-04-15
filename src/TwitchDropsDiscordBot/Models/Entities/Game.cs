namespace TwitchDropsDiscordBot.Models.Entities;

public class Game
{
    public short Id { get; init; }

    public string Name { get; init; }

    public bool ShouldAlert { get; init; }
}
