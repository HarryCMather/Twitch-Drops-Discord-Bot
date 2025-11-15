using Microsoft.Extensions.Caching.Memory;
using TwitchDropsDiscordBot.Models.TwitchApi;
using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchGameIdFinderService
{
    private const string CacheKey = "GameId";

    private readonly TwitchApiClient _twitchApiClient;
    private readonly TwitchAuthorizationService _twitchAuthorizationService;
    private readonly TimeProvider _timeProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _gameIdCacheExpiration = TimeSpan.FromDays(1);

    public TwitchGameIdFinderService(TwitchApiClient twitchApiClient,
                                     TwitchAuthorizationService twitchAuthorizationService,
                                     TimeProvider timeProvider,
                                     IMemoryCache memoryCache)
    {
        _twitchApiClient = twitchApiClient;
        _twitchAuthorizationService = twitchAuthorizationService;
        _timeProvider = timeProvider;
        _memoryCache = memoryCache;
    }

    public async ValueTask<Dictionary<string, uint>> GetGameIdMapAsync(string clientId, string clientSecret, IReadOnlyCollection<string> gameNames)
    {
        if (!_memoryCache.TryGetValue(CacheKey, out Dictionary<string, uint> gameIdMap))
        {
            gameIdMap = await UpdateGameIdMapAsync(clientId, clientSecret, gameNames);
        }

        return gameIdMap;
    }

    private async Task<Dictionary<string, uint>> UpdateGameIdMapAsync(string clientId, string clientSecret, IReadOnlyCollection<string> gameNames)
    {
        TokenResponse tokenResponse = await _twitchAuthorizationService.GetTokenResponseAsync(clientId, clientSecret);

        List<GetGameResponse> games = await _twitchApiClient.GetGamesAsync(clientId, tokenResponse, gameNames);
        Dictionary<string, uint> gameIdMap = games.ToDictionary(game => game.Name, game => game.Id);

        DateTimeOffset gamesCacheExpiryTime = _timeProvider.GetUtcNow().Add(_gameIdCacheExpiration);
        _memoryCache.Set(CacheKey, gameIdMap, gamesCacheExpiryTime);
        return gameIdMap;
    }
}
