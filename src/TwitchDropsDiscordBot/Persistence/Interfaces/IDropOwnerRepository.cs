using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface IDropOwnerRepository
{
    Task<Dictionary<string, short>> GetDropOwnersMapAsync(CancellationToken cancellationToken);

    Task<List<DropOwner>> InsertNewDropOwnersAsync(List<string> newOwnerNames, CancellationToken cancellationToken);
}
