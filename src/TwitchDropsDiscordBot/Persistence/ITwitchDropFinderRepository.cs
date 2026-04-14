using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Persistence;

public interface ITwitchDropFinderRepository
{
    Task<IEnumerable<Drop>> GetDropsAsync();
}
