namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface IDropOwnerRepository
{
    Task<Dictionary<string, short>> GetDropOwnersMapAsync(CancellationToken cancellationToken);

    Task<short> InsertDropOwnerAsync(string dropOwnerName, CancellationToken cancellationToken);
}
