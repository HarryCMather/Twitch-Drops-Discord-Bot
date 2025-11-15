namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropFinderService
{
    private readonly TwitchGameIdFinderService _twitchGameIdFinderService;
    private readonly TwitchStreamsFinderService _twitchStreamsFinderService;

    public TwitchDropFinderService(TwitchGameIdFinderService twitchGameIdFinderService, TwitchStreamsFinderService twitchStreamsFinderService)
    {
        _twitchGameIdFinderService = twitchGameIdFinderService;
        _twitchStreamsFinderService = twitchStreamsFinderService;
    }

    public async Task FindNewDropsAsync(string clientId, string clientSecret, IReadOnlyCollection<string> gameNames)
    {
        const string dropsEnabledTag = "DropsEnabled";

        try
        {
            Dictionary<string, uint> gameIdMap = await _twitchGameIdFinderService.GetGameIdMapAsync(clientId, clientSecret, gameNames);

            foreach (KeyValuePair<string, uint> gameMapKvp in gameIdMap)
            {
                Console.WriteLine($"Trying to find twitch drops for game '{gameMapKvp.Key}'");
                bool foundStreamsWithTag = await _twitchStreamsFinderService.AnyStreamsWithTagAsync(clientId, clientSecret, gameMapKvp.Value, dropsEnabledTag);
                Console.WriteLine($"Game '{gameMapKvp.Key}' has drops enabled: {foundStreamsWithTag}");
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
}
