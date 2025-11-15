using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TwitchDropsDiscordBot.Models;
using TwitchDropsDiscordBot.Models.TwitchApi;

namespace TwitchDropsDiscordBot.Persistence;

public sealed class TwitchApiClient
{
    private readonly HttpClient _httpClient;
    private readonly Settings _settings;

    public TwitchApiClient(HttpClient httpClient, Settings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public async Task<TokenResponse> GetAccessTokenAsync()
    {
        string requestUrl = $"https://id.twitch.tv/oauth2/token?client_id={_settings.ClientId}&client_secret={_settings.ClientSecret}&grant_type=client_credentials";

        using (HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, null))
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TokenResponse>();
        }
    }

    public async Task<List<GetGameResponse>> GetGamesAsync(TokenResponse tokenResponse)
    {
        string requestUrl = "https://api.twitch.tv/helix/games";

        // The "name" query parameter on this endpoint is weird...
        // If you wanted to query multiple games with the names of "Game1", "Game2" and "Game3", you need to append this to the requestUrl as:
        // ?name=Game1&name=Game2&name=Game3
        // Therefore, this is replicated below:
        foreach (string gameName in _settings.GameNames)
        {
            if (!requestUrl.Contains("?name="))
            {
                requestUrl += $"?name={gameName}";
            }
            else
            {
                requestUrl += $"&name={gameName}";
            }
        }

        string[] jsonPropertiesToRetrieve = [ "data" ];
        return await GetFromTwitchAsync<List<GetGameResponse>>(requestUrl, tokenResponse, jsonPropertiesToRetrieve);
    }

    public async Task<GetStreamsResponse> GetStreamsAsync(TokenResponse tokenResponse, uint gameId, string afterCursor = null)
    {
        string requestUrl = $"https://api.twitch.tv/helix/streams?game_id={gameId}&first=100&type=live";

        if (afterCursor is not null)
        {
            requestUrl += $"&after={afterCursor}";
        }

        return await GetFromTwitchAsync<GetStreamsResponse>(requestUrl, tokenResponse);
    }

    private async Task<TResponse> GetFromTwitchAsync<TResponse>(string requestUrl, TokenResponse tokenResponse, string[] jsonPropertiesToRetrieve = null)
    {
        using (HttpRequestMessage request = new(HttpMethod.Get, requestUrl))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(tokenResponse.TokenType, tokenResponse.AccessToken);
            request.Headers.Add("Client-Id", _settings.ClientId);

            using (HttpResponseMessage response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string responseJson = await response.Content.ReadAsStringAsync();
                return ParseResponseJson<TResponse>(responseJson, jsonPropertiesToRetrieve);
            }
        }
    }

    private static TResponse ParseResponseJson<TResponse>(string responseJson, string[] jsonPropertiesToRetrieve)
    {
        // The jsonPropertiesToRetrieve aren't necessary in all cases, but I've added this to avoid having multiple models for
        // the same data, which would otherwise be messy.  This instead allows the intermediary models to be bypassed.
        // If it's not necessary, then this defaults to null and won't be processed.
        if (jsonPropertiesToRetrieve is null)
        {
            return JsonSerializer.Deserialize<TResponse>(responseJson);
        }

        using (JsonDocument jsonDoc = JsonDocument.Parse(responseJson))
        {
            JsonElement nodesElement = jsonDoc.RootElement;

            foreach (string property in jsonPropertiesToRetrieve)
            {
                nodesElement = nodesElement.GetProperty(property);
            }

            return JsonSerializer.Deserialize<TResponse>(nodesElement.GetRawText());
        }
    }
}
