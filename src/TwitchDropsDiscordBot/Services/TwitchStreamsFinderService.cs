using TwitchDropsDiscordBot.Models.TwitchApi;
using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchStreamsFinderService
{
    private readonly TwitchApiClient _twitchApiClient;
    private readonly TwitchAuthorizationService _twitchAuthorizationService;

    public TwitchStreamsFinderService(TwitchApiClient twitchApiClient, TwitchAuthorizationService twitchAuthorizationService)
    {
        _twitchApiClient = twitchApiClient;
        _twitchAuthorizationService = twitchAuthorizationService;
    }

    public async Task<bool> AnyStreamsWithTagAsync(string clientId, string clientSecret, uint gameId, string tag)
    {
        TokenResponse tokenResponse = await _twitchAuthorizationService.GetTokenResponseAsync(clientId, clientSecret);

        uint totalPages = 0;
        uint totalStreams = 0;
        bool containsStreamsWithTag = false;
        string cursor = null;

        do
        {
            GetStreamsResponse streamsResponse = await _twitchApiClient.GetStreamsAsync(clientId, tokenResponse, gameId, cursor);
            cursor = streamsResponse.Pagination.Cursor;

            // Two conditions I want to exit this loop are:
            //  - We've confirmed that there are streams containing the requested tag.  Therefore, there's no point continuing to search.
            //  - Cursor is genuinely null or empty (indicating no more streams to check were returned).

            totalPages++;
            totalStreams += (uint)streamsResponse.Data.Count;

            if (string.IsNullOrEmpty(cursor))
            {
                // No more pages to query:
                break;
            }
            else if (streamsResponse.Data.Any(stream => stream.Tags.Contains(tag)))
            {
                containsStreamsWithTag = true;
            }
        } while (!containsStreamsWithTag);

        Console.WriteLine($"Game Id: {gameId} contains streams with tag: {containsStreamsWithTag}; Total pages: {totalPages}; Total streams: {totalStreams}");

        return containsStreamsWithTag;
    }
}
