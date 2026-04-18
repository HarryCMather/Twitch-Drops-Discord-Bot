using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface ITwitchDropFinderRepository
{
    Task<List<Drop>> GetDropsAsync(CancellationToken cancellationToken);
}
