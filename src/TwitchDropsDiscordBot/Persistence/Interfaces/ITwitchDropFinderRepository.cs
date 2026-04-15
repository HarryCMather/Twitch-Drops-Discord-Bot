using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface ITwitchDropFinderRepository
{
    Task<IEnumerable<Drop>> GetDropsAsync();
}
