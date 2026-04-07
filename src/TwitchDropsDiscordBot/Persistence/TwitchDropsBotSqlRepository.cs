using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using TwitchDropsDiscordBot.Contexts;
using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Persistence;

/// <summary>
/// Implementation of Twitch Drops Bot SQL Repository.
/// Performs database-related actions for recording or retrieving alert history.
/// </summary>
public sealed class TwitchDropsBotSqlRepository
{
    private readonly TwitchDropsBotDbContext _dbContext;

    public TwitchDropsBotSqlRepository(TwitchDropsBotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Game>> GetAlertableGamesAsync()
    {
        IQueryable<Game> query = _dbContext.Games.Where(game => game.ShouldAlert)
                                                 .Select(game => new Game
                                                 {
                                                     Id = game.Id,
                                                     Name =  game.Name
                                                 });
        return await query.ToListAsync();
    }

    public async Task InsertNewGamesAsync(IEnumerable<string> distinctFoundGameNames)
    {
        List<string> existingGameNames = await _dbContext.Games.Select(game => game.Name)
                                                               .ToListAsync();

        IEnumerable<Game> newGames = distinctFoundGameNames.Except(existingGameNames)
                                                           .Select(gameName => new Game
                                                           {
                                                               Name = gameName,
                                                               ShouldAlert = false
                                                           });

        await _dbContext.BulkInsertAsync(newGames, new BulkConfig
        {
            PropertiesToIncludeOnCompare = [ "name" ],
            SetOutputIdentity = false
        });
    }
}
