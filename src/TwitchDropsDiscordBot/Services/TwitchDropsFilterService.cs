using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TwitchDropsDiscordBot.Models;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence.Interfaces;
using TwitchDropsDiscordBot.Services.Interfaces;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropsFilterService : ITwitchDropsFilterService
{
    private readonly TimeProvider _timeProvider;
    private readonly IDropOwnerRepository _dropOwnerRepository;
    private readonly IDropsRepository _dropsRepository;
    private readonly ILogger<TwitchDropsFilterService> _logger;

    public TwitchDropsFilterService(TimeProvider timeProvider,
                                    IDropOwnerRepository dropOwnerRepository,
                                    IDropsRepository dropsRepository,
                                    ILogger<TwitchDropsFilterService> logger)
    {
        _timeProvider = timeProvider;
        _dropOwnerRepository = dropOwnerRepository;
        _dropsRepository = dropsRepository;
        _logger = logger;
    }

    public async Task<DropsFilterResult> FilterDropsAsync(IEnumerable<Drop> foundDrops,
                                                          HashSet<string> existingGameNames,
                                                          List<Game> alertableGames,
                                                          Dictionary<string, short> gamesMap,
                                                          Dictionary<string, short> existingDropOwners,
                                                          CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(foundDrops);
        ArgumentNullException.ThrowIfNull(existingGameNames);
        ArgumentNullException.ThrowIfNull(gamesMap);
        ArgumentNullException.ThrowIfNull(existingDropOwners);

        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            HashSet<string> requestedGameNamesSet = new(alertableGames.Select(game => game.Name));

            // Whilst this isn't necessary for extracting the drops themselves, it is meaningful to track this in the
            // database in-case Twitch/Game authors change the names (like with Siege vs Siege X within the past year):
            HashSet<string> foundGameNames = [];

            DateTimeOffset currentUtcDateTime = _timeProvider.GetUtcNow();

            DropsFilterResult dropsFilterResult = new()
            {
                ValidDrops = [],
                NewGames = []
            };

            List<Drop> dropsPassingInitialFiltering = [];
            List<Guid> timeBasedDropIds = [];
            PopulateDropsFromInitialInMemoryFiltering(foundDrops, dropsPassingInitialFiltering, timeBasedDropIds, existingGameNames, requestedGameNamesSet, foundGameNames, currentUtcDateTime);
            await PopulateDropsWithUnalertedTimeBasedDropsAsync(dropsPassingInitialFiltering, dropsFilterResult.ValidDrops, timeBasedDropIds, cancellationToken);
            await PopulateDropsWithForeignKeyIdsAsync(dropsFilterResult.ValidDrops, existingDropOwners, gamesMap, cancellationToken);

            dropsFilterResult.NewGames.AddRange(foundGameNames.Select(gameName => new Game
            {
                Name = gameName,
                ShouldAlert = false
            }));

            return dropsFilterResult;
        }
    }

    private static void PopulateDropsFromInitialInMemoryFiltering(IEnumerable<Drop> foundDrops,
                                                                  List<Drop> newDrops,
                                                                  List<Guid> timeBasedDropIds,
                                                                  HashSet<string> existingGameNames,
                                                                  HashSet<string> requestedGameNamesSet,
                                                                  HashSet<string> foundGameNames,
                                                                  DateTimeOffset currentUtcDateTime)
    {
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
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
                    newDrops.Add(drop);
                    timeBasedDropIds.AddRange(drop.TimeBasedDrops.Select(timeBasedDrop => timeBasedDrop.Id));
                }
            }
        }
    }

    private async Task PopulateDropsWithUnalertedTimeBasedDropsAsync(List<Drop> dropsPassingInitialFiltering,
                                                                     List<Drop> outputDrops,
                                                                     List<Guid> timeBasedDropIds,
                                                                     CancellationToken cancellationToken)
    {
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            HashSet<Guid> unalertedTimeBasedDropIds = await _dropsRepository.GetUnalertedTimeBasedDropsAsync(timeBasedDropIds, cancellationToken);

            foreach (Drop drop in dropsPassingInitialFiltering)
            {
                // Only populate time-based drops that don't already exist in the database (from the previous db call).
                // If none remain, skip this iteration as there's nothing further to process.
                drop.TimeBasedDrops = drop.TimeBasedDrops.Where(timeBasedDrop => unalertedTimeBasedDropIds.Contains(timeBasedDrop.Id))
                                                         .ToList();

                if (!HasTimeBasedRewards(drop.TimeBasedDrops))
                {
                    continue;
                }

                _logger.LogInformation("Found drop for game '{GameName}'", drop.GameName);
                outputDrops.Add(drop);
            }
        }
    }

    private async Task PopulateDropsWithForeignKeyIdsAsync(List<Drop> drops,
                                                           Dictionary<string, short> dropOwnersMap,
                                                           Dictionary<string, short> gamesMap,
                                                           CancellationToken cancellationToken)
    {
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            await InsertNewDropOwnersIfNeeded(drops, dropOwnersMap, cancellationToken);

            foreach (Drop drop in drops)
            {
                drop.DropOwnerId = dropOwnersMap[drop.Owner];
                drop.GameId = gamesMap[drop.GameName];
            }
        }
    }

    private async Task InsertNewDropOwnersIfNeeded(List<Drop> drops, Dictionary<string, short> dropOwners, CancellationToken cancellationToken)
    {
        List<string> newDropOwnerNames = drops.Select(drop => drop.Owner)
                                              .Distinct()
                                              .Except(dropOwners.Keys)
                                              .ToList();

        if (newDropOwnerNames.Count > 0)
        {
            List<DropOwner> newDropOwners = await _dropOwnerRepository.InsertNewDropOwnersAsync(newDropOwnerNames, cancellationToken);

            foreach (DropOwner newDropOwner in newDropOwners)
            {
                dropOwners.Add(newDropOwner.Name, newDropOwner.Id);
            }
        }
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
}
