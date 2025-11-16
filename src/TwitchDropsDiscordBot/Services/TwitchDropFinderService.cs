using TwitchDropsDiscordBot.Models.SunkwiApi;
using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropFinderService
{
    private readonly SunkwiApiClient _sunkwiApiClient;
    private readonly TimeProvider _timeProvider;

    public TwitchDropFinderService(SunkwiApiClient sunkwiApiClient, TimeProvider timeProvider)
    {
        _sunkwiApiClient = sunkwiApiClient;
        _timeProvider = timeProvider;
    }

    public async Task FindNewDropsAsync(IReadOnlyCollection<string> gameNames)
    {
        try
        {
            Console.WriteLine("Checking for new Twitch drops...");
            IAsyncEnumerable<GetDropsResponse> getDropsResponse = _sunkwiApiClient.GetDropsAsync();
            List<GetDropsResponse> dropsForRequestedGames = await ExtractDropsForRequestedGames(getDropsResponse, gameNames);

            foreach (GetDropsResponse drop in dropsForRequestedGames)
            {
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(drop));
            }
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine($"HttpRequestException: {exception.StatusCode} - {exception.HttpRequestError} - {exception.Message}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Exception: {exception.Message}");
        }
    }

    private async Task<List<GetDropsResponse>> ExtractDropsForRequestedGames(IAsyncEnumerable<GetDropsResponse> drops, IReadOnlyCollection<string> requestedGameNames)
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

                if (drop.Rewards.Count > 0)
                {
                    dropsForRequestedGames.Add(drop);
                }
                else
                {
                    Console.WriteLine($"After removing inactive rewards and rewards without time-based drops, there were no rewards left for game '{drop.GameDisplayName}'. Therefore, this drop won't be used.");
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
}
