using Microsoft.Extensions.Caching.Memory;
using TwitchDropsDiscordBot.Models.TwitchApi;
using TwitchDropsDiscordBot.Persistence;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchAuthorizationService
{
    private const string CacheKey = "AuthToken";

    private readonly TwitchApiClient _twitchApiClient;
    private readonly TimeProvider _timeProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _closeToExpiryDuration = TimeSpan.FromHours(2);

    public TwitchAuthorizationService(TwitchApiClient twitchApiClient, TimeProvider timeProvider, IMemoryCache memoryCache)
    {
        _twitchApiClient = twitchApiClient;
        _timeProvider = timeProvider;
        _memoryCache = memoryCache;
    }

    public async ValueTask<TokenResponse> GetTokenResponseAsync(string clientId, string clientSecret)
    {
        if (!_memoryCache.TryGetValue(CacheKey, out TokenResponse tokenResponse))
        {
            tokenResponse = await UpdateAccessTokenAsync(clientId, clientSecret);
        }

        return tokenResponse;
    }

    private async Task<TokenResponse> UpdateAccessTokenAsync(string clientId, string clientSecret)
    {
        TokenResponse tokenResponse = await _twitchApiClient.GetAccessTokenAsync(clientId, clientSecret);

        // Twitch API doesn't allow lowercase "bearer":
        if (string.Equals(tokenResponse.TokenType, "bearer", StringComparison.InvariantCulture))
        {
            tokenResponse.TokenType = "Bearer";
        }

        // Add a grace period to ensure the token always remains valid:
        DateTimeOffset tokenExpiryTime = _timeProvider.GetUtcNow().AddSeconds(tokenResponse.ExpiresIn - _closeToExpiryDuration.TotalSeconds);

        _memoryCache.Set(CacheKey, tokenResponse, tokenExpiryTime);
        return tokenResponse;
    }
}
