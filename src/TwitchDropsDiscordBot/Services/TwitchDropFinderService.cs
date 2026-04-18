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

    public async Task<List<Drop>> FindNewDropsAsync(CancellationToken cancellationToken)
    {
        List<Drop> dropsForRequestedGames = [];

        List<Game> games = await _gamesRepository.GetGamesAsync(cancellationToken);
        if (!games.Exists(game => game.ShouldAlert))
        {
            _logger.LogWarning("No alertable games were set in the database. Skipping this iteration");
            return dropsForRequestedGames;
        }

        try
        {
            _logger.LogInformation("Checking for new Twitch drops...");

            // I would consider adding the above GetGamesAsync call to this, but if there aren't any games to alert on then we've wasted a call to
            // the third-party (by the GetDropsAsync) call:
            Task<List<Drop>> getDropsTask = _twitchDropsFinderRepository.GetDropsAsync(cancellationToken);
            Task<Dictionary<string, short>> getDropOwnersMapTask = _dropOwnerRepository.GetDropOwnersMapAsync(cancellationToken);
            await Task.WhenAll(getDropsTask, getDropOwnersMapTask);

            // Whilst this looks like a double await, as these tasks have already ran to completion, this will short-circuit the async state machine:
            List<Drop> drops = await getDropsTask;
            Dictionary<string, short> existingDropOwnersMap = await getDropOwnersMapTask;

            dropsForRequestedGames = await ExtractDropsForRequestedGames(drops, games, existingDropOwnersMap, cancellationToken);
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

    private async Task<List<Drop>> ExtractDropsForRequestedGames(List<Drop> drops, List<Game> games, Dictionary<string, short> dropOwnersMap, CancellationToken cancellationToken)
    {
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            HashSet<string> existingGameNames = new(games.Select(game => game.Name));

            List<Game> alertableGames = games.Where(game => game.ShouldAlert).ToList();
            Dictionary<string, short> gamesMap = alertableGames.ToDictionary(game => game.Name,
                                                                                     game => game.Id);

            DropsFilterResult dropsFilterResult = await _twitchDropsFilterService.FilterDropsAsync(drops, existingGameNames, alertableGames, gamesMap, dropOwnersMap, cancellationToken);
            if (dropsFilterResult.NewGames.Count > 0)
            {
                await _gamesRepository.InsertGamesAsync(dropsFilterResult.NewGames, cancellationToken);
            }

            return dropsFilterResult.ValidDrops;
        }
    }
}
