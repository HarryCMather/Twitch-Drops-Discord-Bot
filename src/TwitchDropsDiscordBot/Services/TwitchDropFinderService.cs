using TwitchDropsDiscordBot.Models;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence.Interfaces;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropFinderService
{
    private readonly TwitchDropsFilterService _twitchDropsFilterService;
    private readonly ITwitchDropFinderRepository _twitchDropsFinderRepository;
    private readonly IGamesRepository _gamesRepository;
    private readonly IDropOwnerRepository _dropOwnerRepository;

    public TwitchDropFinderService(TwitchDropsFilterService twitchDropsFilterService,
                                   ITwitchDropFinderRepository twitchDropsFinderRepository,
                                   IGamesRepository gamesRepository,
                                   IDropOwnerRepository dropOwnerRepository)
    {
        _twitchDropsFilterService = twitchDropsFilterService;
        _twitchDropsFinderRepository = twitchDropsFinderRepository;
        _gamesRepository = gamesRepository;
        _dropOwnerRepository = dropOwnerRepository;
    }

    public async Task<List<Drop>> FindNewDropsAsync()
    {
        List<Drop> dropsForRequestedGames = [];

        List<Game> games = await _gamesRepository.GetGamesAsync();
        if (!games.Exists(game => game.ShouldAlert))
        {
            Console.WriteLine("No alertable games were set in the database. Skipping this iteration.");
            return dropsForRequestedGames;
        }

        Dictionary<string, short> existingDropOwners = await _dropOwnerRepository.GetDropOwnersMapAsync();

        try
        {
            Console.WriteLine("Checking for new Twitch drops...");
            IEnumerable<Drop> drops = await _twitchDropsFinderRepository.GetDropsAsync();
            dropsForRequestedGames = await ExtractDropsForRequestedGames(drops, games, existingDropOwners);
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine($"HttpRequestException: {exception.StatusCode} - {exception.HttpRequestError} - {exception.Message}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Exception: {exception.Message}");
        }

        return dropsForRequestedGames;
    }

    private async Task<List<Drop>> ExtractDropsForRequestedGames(IEnumerable<Drop> drops, List<Game> games, Dictionary<string, short> dropOwnersMap)
    {
        HashSet<string> existingGameNames = new(games.Select(game => game.Name));

        List<Game> alertableGames = games.Where(game => game.ShouldAlert).ToList();
        Dictionary<string, short> gamesMap = alertableGames.ToDictionary(game => game.Name, game => game.Id);

        DropsFilterResult dropsFilterResult = await _twitchDropsFilterService.FilterDropsAsync(drops, existingGameNames, alertableGames, gamesMap, dropOwnersMap);
        if (dropsFilterResult.NewGames.Count > 0)
        {
            await _gamesRepository.InsertGamesAsync(dropsFilterResult.NewGames);
        }

        return dropsFilterResult.ValidDrops;
    }
}
