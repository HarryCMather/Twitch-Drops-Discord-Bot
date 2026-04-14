using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Models.SunkwiApi;
using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropFinderService
{
    private readonly SunkwiApiClient _sunkwiApiClient;
    private readonly TwitchDropsBotSqlRepository _twitchDropsBotSqlRepository;
    private readonly TimeProvider _timeProvider;

    public TwitchDropFinderService(SunkwiApiClient sunkwiApiClient,
                                   TwitchDropsBotSqlRepository twitchDropsBotSqlRepository,
                                   TimeProvider timeProvider)
    {
        _sunkwiApiClient = sunkwiApiClient;
        _twitchDropsBotSqlRepository = twitchDropsBotSqlRepository;
        _timeProvider = timeProvider;
    }

    public async Task<List<GetDropsResponse>> FindNewDropsAsync()
    {
        List<GetDropsResponse> dropsForRequestedGames = [];

        List<Game> games = await _twitchDropsBotSqlRepository.GetGamesAsync();
        if (!games.Exists(game => game.ShouldAlert))
        {
            Console.WriteLine("No alertable games were set in the database. Skipping this iteration.");
            return dropsForRequestedGames;
        }

        Dictionary<string, short> existingDropOwners = await _twitchDropsBotSqlRepository.GetDropOwnersMapAsync();

        try
        {
            Console.WriteLine("Checking for new Twitch drops...");
            IAsyncEnumerable<GetDropsResponse> getDropsResponse = _sunkwiApiClient.GetDropsAsync();
            dropsForRequestedGames = await ExtractDropsForRequestedGames(getDropsResponse, games, existingDropOwners);
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine($"HttpRequestException: {exception.StatusCode} - {exception.HttpRequestError} - {exception.Message}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Exception: {exception.Message}");
        }

        return dropsForRequestedGames;
    }

    private async Task<List<GetDropsResponse>> ExtractDropsForRequestedGames(IAsyncEnumerable<GetDropsResponse> drops, List<Game> games, Dictionary<string, short> dropOwnersMap)
    {
        HashSet<string> existingGameNames = new(games.Select(game => game.Name));

        IEnumerable<Game> alertableGames = games.Where(game => game.ShouldAlert).ToList();
        HashSet<string> requestedGameNamesSet = new(alertableGames.Select(game => game.Name));
        Dictionary<string, short> gamesMap = alertableGames.ToDictionary(game => game.Name, game => game.Id);

        // Whilst this isn't necessary for extracting the drops themselves, it is meaningful to track this in the
        // database in-case Twitch/Game authors change the names (like with Siege vs Siege X within the past year):
        HashSet<string> foundGameNames = [];

        // The SunkwiApi is returning DateTimes in the ISO-8601 format, so assuming UTC should (hopefully) be appropriate here:
        DateTimeOffset currentUtcDateTime = _timeProvider.GetUtcNow();

        List<Drop> dropsForRequestedGames = [];
        await foreach (GetDropsResponse drop in drops)
        {
            // Don't need to perform a contains check against foundGameNames, as Add will only Add if the element isn't already present:
            if (!existingGameNames.Contains(drop.GameDisplayName))
            {
                foundGameNames.Add(drop.GameDisplayName);
            }

            if (requestedGameNamesSet.Contains(drop.GameDisplayName) && IsBetweenDateTimes(currentUtcDateTime, drop.StartsAt, drop.EndsAt))
            {
                Console.WriteLine($"Found drop for game '{drop.GameDisplayName}'");

                RemoveRewardsThatHaveNotStartedOrHaveExpired(drop.Rewards, currentUtcDateTime);
                RemoveTimeBasedDropsThatHaveNotStartedOrHaveExpired(drop.Rewards, currentUtcDateTime);
                RemoveInactiveRewards(drop.Rewards);
                RemoveRewardsWithNoTimeBasedDrops(drop.Rewards);
                await RemoveRewardsThatHaveAlreadyBeenAlertedAsync(drop.Rewards);

                if (drop.Rewards.Count > 0)
                {
                    foreach (GetDropsReward dropReward in drop.Rewards)
                    {
                        short dropOwnerId = await GetDropOwnerIdFromDropOwnerNameAsync(dropReward.Owner.Name, dropOwnersMap);
                        short gameId = gamesMap[drop.GameDisplayName];

                        Drop dropForRequestedGame = ConvertToDrop(dropReward, drop.GameDisplayName, gameId, dropOwnerId);
                        dropsForRequestedGames.Add(dropForRequestedGame);
                    }
                }
                else
                {
                    Console.WriteLine($"After filtering rewards, there were no new rewards left for game '{drop.GameDisplayName}'. Therefore, no notification will be sent for this drop.");
                }
            }
        }

        await _twitchDropsBotSqlRepository.InsertNewGamesAsync(foundGameNames);

        return dropsForRequestedGames;
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

    private static Drop ConvertToDrop(GetDropsReward inputDrop, string gameName, short gameId, short dropOwnerId)
    {
        List<TimeBasedDrop> timeBasedDrops = ConvertToTimeBasedDrop(inputDrop.TimeBasedDrops, inputDrop.Id);

        Drop dropForRequestedGame = new()
        {
            Id = inputDrop.Id,
            GameName = gameName,
            GameId = gameId,
            DropOwner = inputDrop.Owner.Name,
            DropOwnerId = dropOwnerId,
            Name = inputDrop.Name,
            Description = inputDrop.Description,
            AccountLinkUrl = inputDrop.AccountLinkUrl,
            DetailsUrl = inputDrop.DetailsUrl,
            StartsAt = inputDrop.StartsAt,
            EndsAt = inputDrop.EndsAt,
            TimeBasedDrops = timeBasedDrops
        };

        return dropForRequestedGame;
    }

    private static List<TimeBasedDrop> ConvertToTimeBasedDrop(List<GetDropsTimeBasedDrop> inputDrops, Guid parentDropId)
    {
        List<TimeBasedDrop> timeBasedDrops = inputDrops.Select(timeBasedDrop => new TimeBasedDrop
        {
            Id = timeBasedDrop.Id,
            ParentDropId = parentDropId,
            Name =  timeBasedDrop.Name,
            StartsAt = timeBasedDrop.StartsAt,
            EndsAt = timeBasedDrop.EndsAt,
            RequiredMinutesWatched = timeBasedDrop.RequiredMinutesWatched,
            AlertedOn = null // Set this when the alert is actually performed, as I want to avoid instances where this is erroneously set too early
        }).ToList();

        return timeBasedDrops;
    }

    private static bool IsBetweenDateTimes(DateTimeOffset dateTime, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        return dateTime >= startsAt && dateTime <= endsAt;
    }

    private static void RemoveRewardsThatHaveNotStartedOrHaveExpired(List<GetDropsReward> rewards, DateTimeOffset currentUtcDateTime)
    {
        int removedRewards = rewards.RemoveAll(drop => !IsBetweenDateTimes(currentUtcDateTime, drop.StartsAt, drop.EndsAt));
        if (removedRewards > 0)
        {
            Console.WriteLine($"{removedRewards} rewards were removed from their associated drop as they had not started or had expired.");
        }
    }

    private static void RemoveTimeBasedDropsThatHaveNotStartedOrHaveExpired(List<GetDropsReward> rewards, DateTimeOffset currentUtcDateTime)
    {
        int removedTimeBasedDrops = 0;

        foreach (GetDropsReward reward in rewards)
        {
            removedTimeBasedDrops += reward.TimeBasedDrops.RemoveAll(drop => !IsBetweenDateTimes(currentUtcDateTime, drop.StartsAt, drop.EndsAt));
        }

        if (removedTimeBasedDrops > 0)
        {
            Console.WriteLine($"{removedTimeBasedDrops} time-based drops were removed from their associated rewards as they had not started or had expired.");
        }
    }

    private static void RemoveInactiveRewards(List<GetDropsReward> rewards)
    {
        int removedRewards = rewards.RemoveAll(reward => reward.Status != "ACTIVE");
        if (removedRewards > 0)
        {
            Console.WriteLine($"{removedRewards} rewards were removed from the drop as they were not active.");
        }
    }

    private static void RemoveRewardsWithNoTimeBasedDrops(List<GetDropsReward> rewards)
    {
        int removedRewards = rewards.RemoveAll(reward => reward.TimeBasedDrops.Count == 0);
        if (removedRewards > 0)
        {
            Console.WriteLine($"{removedRewards} rewards were removed from the drop as they did not contain any time-based drops.");
        }
    }

    private async Task RemoveRewardsThatHaveAlreadyBeenAlertedAsync(List<GetDropsReward> rewards)
    {
        for (int rewardCount = rewards.Count - 1; rewardCount >= 0; rewardCount--)
        {
            for (int timeBasedDropCount = rewards[rewardCount].TimeBasedDrops.Count - 1; timeBasedDropCount >= 0; timeBasedDropCount--)
            {
                Guid rewardId = rewards[rewardCount].Id;
                Guid timeBasedDropId = rewards[rewardCount].TimeBasedDrops[timeBasedDropCount].Id;

                bool alreadyAlerted = await _twitchDropsBotSqlRepository.HasDropNotificationBeenSentAsync(rewardId, timeBasedDropId);
                if (alreadyAlerted)
                {
                    rewards[rewardCount].TimeBasedDrops.RemoveAt(timeBasedDropCount);
                }
            }

            // No point continuing if there aren't any more time-based drops remaining within the reward:
            if (rewards[rewardCount].TimeBasedDrops.Count == 0)
            {
                rewards.RemoveAt(rewardCount);
            }
        }
    }
}
