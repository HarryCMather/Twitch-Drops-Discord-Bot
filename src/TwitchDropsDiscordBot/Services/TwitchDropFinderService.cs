using TwitchDropsDiscordBot.Models.SunkwiApi;
using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropFinderService
{
    private readonly SunkwiApiClient _sunkwiApiClient;
    private readonly AlertHistoryService _alertHistoryService;
    private readonly TimeProvider _timeProvider;

    public TwitchDropFinderService(SunkwiApiClient sunkwiApiClient, AlertHistoryService alertHistoryService, TimeProvider timeProvider)
    {
        _sunkwiApiClient = sunkwiApiClient;
        _alertHistoryService = alertHistoryService;
        _timeProvider = timeProvider;
    }

    public async Task<List<GetDropsResponse>> FindNewDropsAsync(List<string> gameNames)
    {
        List<GetDropsResponse> dropsForRequestedGames = [];

        try
        {
            Console.WriteLine("Checking for new Twitch drops...");
            IAsyncEnumerable<GetDropsResponse> getDropsResponse = _sunkwiApiClient.GetDropsAsync();
            dropsForRequestedGames = await ExtractDropsForRequestedGames(getDropsResponse, gameNames);
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

    private async Task<List<GetDropsResponse>> ExtractDropsForRequestedGames(IAsyncEnumerable<GetDropsResponse> drops, List<string> requestedGameNames)
    {
        HashSet<string> requestedGameNamesSet = new(requestedGameNames);

        // The SunkwiApi is returning DateTimes in the ISO-8601 format, so assuming UTC should (hopefully) be appropriate here:
        DateTimeOffset currentUtcDateTime = _timeProvider.GetUtcNow();

        List<GetDropsResponse> dropsForRequestedGames = [];
        await foreach (GetDropsResponse drop in drops)
        {
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
                    dropsForRequestedGames.Add(drop);
                }
                else
                {
                    Console.WriteLine($"After filtering rewards, there were no new rewards left for game '{drop.GameDisplayName}'. Therefore, no notification will be sent for this drop.");
                }
            }
        }

        return dropsForRequestedGames;
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

                bool alreadyAlerted = await _alertHistoryService.HasDropNotificationBeenSentAsync(rewardId, timeBasedDropId);
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
