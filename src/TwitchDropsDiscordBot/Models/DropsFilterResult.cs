using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Models;

public sealed record DropsFilterResult
{
    public List<Drop> ValidDrops { get; init; }

    public List<Game> NewGames { get; init; }
}
