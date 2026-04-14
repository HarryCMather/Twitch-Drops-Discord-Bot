using System.Net.Http.Json;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Models.SunkwiApi;

namespace TwitchDropsDiscordBot.Persistence;

public sealed class SunkwiApiClient : ITwitchDropFinderRepository
{
    private readonly HttpClient _httpClient;

    public SunkwiApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Drop>> GetDropsAsync()
    {
        const string requestUrl = "https://twitch-drops-api.sunkwi.com/drops";

        IAsyncEnumerable<GetDropsResponse> getDropsResponse = _httpClient.GetFromJsonAsAsyncEnumerable<GetDropsResponse>(requestUrl);

        List<Drop> drops = [];
        await foreach (GetDropsResponse apiDrop in getDropsResponse)
        {
            IEnumerable<Drop> convertedDrops = apiDrop.Rewards.Select(dropReward => ConvertToDrop(dropReward, apiDrop.GameDisplayName));
            drops.AddRange(convertedDrops);
        }
        return drops;
    }

    private static Drop ConvertToDrop(GetDropsReward inputDrop, string gameName)
    {
        List<TimeBasedDrop> timeBasedDrops = ConvertToTimeBasedDrops(inputDrop.TimeBasedDrops, inputDrop.Id);

        Drop dropForRequestedGame = new()
        {
            Id = inputDrop.Id,
            GameName = gameName,
            // GameId = gameId, // TODO: ADD THESE BACK THROUGH THE TWITCHDROPSFILTERSERVICE
            Owner = inputDrop.Owner.Name,
            // DropOwnerId = dropOwnerId, // TODO: ADD THESE BACK THROUGH THE TWITCHDROPSFILTERSERVICE
            Name = inputDrop.Name,
            Description = inputDrop.Description,
            AccountLinkUrl = inputDrop.AccountLinkUrl,
            DetailsUrl = inputDrop.DetailsUrl,
            StartsAt = inputDrop.StartsAt,
            EndsAt = inputDrop.EndsAt,
            Status = inputDrop.Status,
            TimeBasedDrops = timeBasedDrops
        };

        return dropForRequestedGame;
    }

    private static List<TimeBasedDrop> ConvertToTimeBasedDrops(List<GetDropsTimeBasedDrop> inputDrops, Guid parentDropId)
    {
        List<TimeBasedDrop> timeBasedDrops = inputDrops.Select(timeBasedDrop => new TimeBasedDrop
        {
            Id = timeBasedDrop.Id,
            ParentDropId = parentDropId,
            Name =  timeBasedDrop.Name,
            StartsAt = timeBasedDrop.StartsAt,
            EndsAt = timeBasedDrop.EndsAt,
            RequiredMinutesWatched = timeBasedDrop.RequiredMinutesWatched,
            AlertedOn = null // Set this when the alert is actually performed, as I want to avoid instances where this is erroneously set too early
        }).OrderBy(drop => drop.StartsAt)
          .ThenBy(drop => drop.RequiredMinutesWatched)
          .ToList();

        return timeBasedDrops;
    }
}
