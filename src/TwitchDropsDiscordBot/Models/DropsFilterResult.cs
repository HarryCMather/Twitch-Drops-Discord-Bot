using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Models;

public sealed record DropsFilterResult
{
    public List<Drop> ValidDrops { get; set; }

    public List<Game> NewGames { get; set; }
}
