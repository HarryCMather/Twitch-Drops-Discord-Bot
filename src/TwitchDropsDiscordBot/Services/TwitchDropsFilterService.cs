using TwitchDropsDiscordBot.Models;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence.Interfaces;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropsFilterService
{
    private readonly TimeProvider _timeProvider;
    private readonly IDropsRepository _dropsRepository;

    public TwitchDropsFilterService(TimeProvider timeProvider, TwitchDropsBotSqlRepository twitchDropsBotSqlRepository)
                                    IDropsRepository dropsRepository)
    {
        _timeProvider = timeProvider;
        _dropsRepository = dropsRepository;
    }

    public async Task<DropsFilterResult> FilterDropsAsync(IEnumerable<Drop> foundDrops,
                                                          HashSet<string> existingGameNames,
                                                          List<Game> alertableGames,
                                                          Dictionary<string, short> gamesMap,
                                                          Dictionary<string, short> existingDropOwners)
    {
        DropsFilterResult dropsFilterResult = new()
        {
            ValidDrops = [],
            NewGames = []
        };

        HashSet<string> requestedGameNamesSet = new(alertableGames.Select(game => game.Name));

        // Whilst this isn't necessary for extracting the drops themselves, it is meaningful to track this in the
        // database in-case Twitch/Game authors change the names (like with Siege vs Siege X within the past year):
        HashSet<string> foundGameNames = [];

        DateTimeOffset currentUtcDateTime = _timeProvider.GetUtcNow();

        foreach (Drop drop in foundDrops)
        {
            // Don't need to perform a contains check against foundGameNames, as Add will only Add if the element isn't already present:
            if (!existingGameNames.Contains(drop.GameName))
            {
                foundGameNames.Add(drop.GameName);
            }

            // Like with the parent drops which will be checked next, the time-based drops should also be active (by comparing DateTimes):
            drop.TimeBasedDrops = drop.TimeBasedDrops.Where(timeBasedDrop => IsBetweenDateTimes(currentUtcDateTime, timeBasedDrop.StartsAt, timeBasedDrop.EndsAt))
                                                     .ToList();

            // The game for the current drop should be a game with should_alert true in the db,
            // and the drop must be active (started and not ended by time, as well as ACTIVE status):
            if (IsGameValid(drop.GameName, requestedGameNamesSet) &&
                IsBetweenDateTimes(currentUtcDateTime, drop.StartsAt, drop.EndsAt) &&
                IsActiveStatus(drop.Status) &&
                HasTimeBasedRewards(drop.TimeBasedDrops))
            {
                // Performing this check separately as this call will be more expensive, and is realistically unlikely to return any data most of the time:
                // This should be refactored in-future:
                drop.TimeBasedDrops = await GetUnalertedTimeBasedDropsAsync(drop);
                if (drop.TimeBasedDrops.Count > 0)
                {
                    Console.WriteLine($"Found drop for game '{drop.GameName}'");

                    drop.DropOwnerId = await GetDropOwnerIdFromDropOwnerNameAsync(drop.Owner, existingDropOwners);
                    drop.GameId = gamesMap[drop.GameName];
                    dropsFilterResult.ValidDrops.Add(drop);
                }
            }
        }

        dropsFilterResult.NewGames.AddRange(foundGameNames.Select(gameName => new Game
        {
            Name = gameName,
            ShouldAlert = false
        }));

        return dropsFilterResult;
    }

    private async ValueTask<short> GetDropOwnerIdFromDropOwnerNameAsync(string dropOwnerName, Dictionary<string, short> existingDropOwners)
    {
        // I've opted to use ValueTask here, as the likelihood of a DB insert being required is minimal after the first few runs.
        // so the likelihood is the async flow will never be hit.
        // This is because the Drop Owner is usually the Game Publisher, which won't change.
        // This also helps ensure this doesn't become an N+1 problem.

        if (!existingDropOwners.TryGetValue(dropOwnerName, out short dropOwnerId))
        {
            dropOwnerId = await _twitchDropsBotSqlRepository.InsertDropOwnerAsync(dropOwnerName);
            existingDropOwners.Add(dropOwnerName, dropOwnerId);
        }

        return dropOwnerId;
    }

    private static bool IsGameValid(string gameName, HashSet<string> validGames)
    {
        return validGames.Contains(gameName);
    }

    private static bool IsBetweenDateTimes(DateTimeOffset dateTime, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        return dateTime >= startsAt && dateTime <= endsAt;
    }

    private static bool IsActiveStatus(string status)
    {
        const string activeStatus = "ACTIVE";
        return status == activeStatus;
    }

    private static bool HasTimeBasedRewards(List<TimeBasedDrop> timeBasedDrops)
    {
        return timeBasedDrops.Count > 0;
    }

    private async Task<List<TimeBasedDrop>> GetUnalertedTimeBasedDropsAsync(Drop drop)
    {
        // The main aim here is to avoid processing drops that we've already alerted on.
        // TODO: REFACTOR THIS IN-FUTURE TO AVOID N+1 QUERY LOOPING.  THIS SHOULDN'T BE TOO MUCH OF AN ISSUE HERE, AS MOST DROPS HAVE ALREADY BEEN FILTERED OUT BY THIS STAGE.

        List<TimeBasedDrop> unalertedDrops = [];
        foreach (TimeBasedDrop timeBasedDrop in drop.TimeBasedDrops)
        {
            bool alreadyAlerted = await _dropsRepository.HasDropNotificationBeenSentAsync(drop.Id, timeBasedDrop.Id);
            if (!alreadyAlerted)
            {
                unalertedDrops.Add(timeBasedDrop);
            }
        }

        return unalertedDrops;
    }
}
