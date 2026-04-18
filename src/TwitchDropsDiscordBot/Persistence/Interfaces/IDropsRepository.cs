using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface IDropsRepository
{
    Task<HashSet<Guid>> GetUnalertedTimeBasedDropsAsync(List<Guid> timeBasedDropIds, CancellationToken cancellationToken);

    Task InsertNewDropsAsync(List<Drop> drops, CancellationToken cancellationToken);

    Task InsertTimeBasedDropsAsync(IEnumerable<TimeBasedDrop> timeBasedDrops, CancellationToken cancellationToken);
}
