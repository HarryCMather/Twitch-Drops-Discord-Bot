using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface IGamesRepository
{
    Task<List<Game>> GetGamesAsync();

    Task<IEnumerable<string>> GetExistingMatchingGamesAsync(List<string> gameNames);

    Task InsertGamesAsync(IEnumerable<Game> games);
}
