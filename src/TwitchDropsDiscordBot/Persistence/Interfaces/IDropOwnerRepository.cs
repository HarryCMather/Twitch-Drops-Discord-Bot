namespace TwitchDropsDiscordBot.Persistence.Interfaces;

public interface IDropOwnerRepository
{
    Task<Dictionary<string, short>> GetDropOwnersMapAsync();

    Task<short> InsertDropOwnerAsync(string dropOwnerName);
}
