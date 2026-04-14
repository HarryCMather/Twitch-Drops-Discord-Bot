using TwitchDropsDiscordBot.Models;
using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Services.Interfaces;

public interface ITwitchDropsFilterService
{
    Task<DropsFilterResult> FilterDropsAsync(IEnumerable<Drop> foundDrops,
                                             HashSet<string> existingGameNames,
                                             List<Game> alertableGames,
                                             Dictionary<string, short> gamesMap,
                                             Dictionary<string, short> existingDropOwners);
}
