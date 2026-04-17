using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TwitchDropsDiscordBot.Models;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence.Interfaces;
using TwitchDropsDiscordBot.Services.Interfaces;

namespace TwitchDropsDiscordBot.Services;

public sealed class TwitchDropFinderService : ITwitchDropFinderService
{
    private readonly ITwitchDropsFilterService _twitchDropsFilterService;
    private readonly ITwitchDropFinderRepository _twitchDropsFinderRepository;
    private readonly IGamesRepository _gamesRepository;
    private readonly IDropOwnerRepository _dropOwnerRepository;
    private readonly ILogger<TwitchDropFinderService> _logger;

    public TwitchDropFinderService(ITwitchDropsFilterService twitchDropsFilterService,
                                   ITwitchDropFinderRepository twitchDropsFinderRepository,
                                   IGamesRepository gamesRepository,
                                   IDropOwnerRepository dropOwnerRepository,
                                   ILogger<TwitchDropFinderService> logger)
    {
        _twitchDropsFilterService = twitchDropsFilterService;
        _twitchDropsFinderRepository = twitchDropsFinderRepository;
        _gamesRepository = gamesRepository;
        _dropOwnerRepository = dropOwnerRepository;
        _logger = logger;
    }

    public async Task<List<Drop>> FindNewDropsAsync()
    {
        List<Drop> dropsForRequestedGames = [];

        List<Game> games = await _gamesRepository.GetGamesAsync();
        if (!games.Exists(game => game.ShouldAlert))
        {
            _logger.LogWarning("No alertable games were set in the database. Skipping this iteration");
            return dropsForRequestedGames;
        }

        Dictionary<string, short> existingDropOwners = await _dropOwnerRepository.GetDropOwnersMapAsync();

        try
        {
            _logger.LogInformation("Checking for new Twitch drops...");
            IEnumerable<Drop> drops = await _twitchDropsFinderRepository.GetDropsAsync();
            dropsForRequestedGames = await ExtractDropsForRequestedGames(drops, games, existingDropOwners);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to find new drops.  Faulted with a HTTP Request Exception: {StatusCode} - {Message}", ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find new drops.  Faulted with an unknown error: {Message}", ex.Message);
        }

        return dropsForRequestedGames;
    }

    private async Task<List<Drop>> ExtractDropsForRequestedGames(IEnumerable<Drop> drops, List<Game> games, Dictionary<string, short> dropOwnersMap)
    {
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
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
}
