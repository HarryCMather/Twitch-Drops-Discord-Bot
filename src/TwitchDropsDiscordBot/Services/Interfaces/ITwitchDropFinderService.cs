using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Services.Interfaces;

public interface ITwitchDropFinderService
{
    Task<List<Drop>> FindNewDropsAsync(CancellationToken cancellationToken);
}
