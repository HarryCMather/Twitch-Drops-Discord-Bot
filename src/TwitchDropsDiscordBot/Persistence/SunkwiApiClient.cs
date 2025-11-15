using System.Net.Http.Json;
using TwitchDropsDiscordBot.Models.SunkwiApi;

namespace TwitchDropsDiscordBot.Persistence;

public sealed class SunkwiApiClient
{
    private readonly HttpClient _httpClient;

    public SunkwiApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<GetDropsResponse>> GetDropsAsync()
    {
        const string requestUrl = "https://twitch-drops-api.sunkwi.com/drops";

        List<GetDropsResponse> getDropsResponse = await _httpClient.GetFromJsonAsync<List<GetDropsResponse>>(requestUrl);
        return getDropsResponse;
    }
}
