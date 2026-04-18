using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface IDropsRepository
{
    Task<bool> HasDropNotificationBeenSentAsync(Guid dropId, Guid timeBasedDropId, CancellationToken cancellationToken);

    Task InsertNewDropsAsync(List<Drop> drops, CancellationToken cancellationToken);

    Task InsertTimeBasedDropsAsync(IEnumerable<TimeBasedDrop> timeBasedDrops, CancellationToken cancellationToken);
}
