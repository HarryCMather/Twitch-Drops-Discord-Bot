using TwitchDropsDiscordBot.Models.SunkwiApi;
using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropFinderService
{
    private readonly SunkwiApiClient _sunkwiApiClient;

    public TwitchDropFinderService(SunkwiApiClient sunkwiApiClient)
    {
        _sunkwiApiClient = sunkwiApiClient;
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

    private static async Task<List<GetDropsResponse>> ExtractDropsForRequestedGames(IAsyncEnumerable<GetDropsResponse> drops, IReadOnlyCollection<string> requestedGameNames)
    {
        HashSet<string> requestedGameNamesSet = new(requestedGameNames);

        List<GetDropsResponse> dropsForRequestedGames = [];
        await foreach (GetDropsResponse drop in drops)
        {
            if (requestedGameNamesSet.Contains(drop.GameDisplayName))
            {
                Console.WriteLine($"Found drop for game '{drop.GameDisplayName}'");

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
