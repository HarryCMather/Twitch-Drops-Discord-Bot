using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface IDropsRepository
{
    Task<bool> HasDropNotificationBeenSentAsync(Guid dropId, Guid timeBasedDropId);

    Task InsertNewDropsAsync(List<Drop> drops);

    Task InsertTimeBasedDropsAsync(IEnumerable<TimeBasedDrop> timeBasedDrops);
}
