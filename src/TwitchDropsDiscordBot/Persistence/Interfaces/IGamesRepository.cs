using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface IGamesRepository
{
    Task<List<Game>> GetGamesAsync(CancellationToken cancellationToken);

    Task<IEnumerable<string>> GetExistingMatchingGamesAsync(List<string> gameNames, CancellationToken cancellationToken);

    Task InsertGamesAsync(IEnumerable<Game> games, CancellationToken cancellationToken);
}
