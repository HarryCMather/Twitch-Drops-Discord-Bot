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

    public IAsyncEnumerable<GetDropsResponse> GetDropsAsync()
    {
        const string requestUrl = "https://twitch-drops-api.sunkwi.com/drops";

        IAsyncEnumerable<GetDropsResponse> getDropsResponse = _httpClient.GetFromJsonAsAsyncEnumerable<GetDropsResponse>(requestUrl);
        return getDropsResponse;
    }
}
