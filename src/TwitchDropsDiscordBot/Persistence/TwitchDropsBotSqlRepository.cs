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

    public async Task<bool> HasDropNotificationBeenSentAsync(Guid dropId, Guid timeBasedDropId)
    {
        // Whilst this isn't going to be an issue considering the small scale of this application,
        // It would be more performant to get a list of all drops to check and do this in 1 database call,
        // compared to this code, which is performing an Any check for every valid drop/timeBasedDropId.
        // This shouldn't be a problem here, as only a few drops per week/month will realistically ever hit this stage.
        return await _dbContext.TimeBasedDrops.AnyAsync(timeBasedDrop => timeBasedDrop.Id == timeBasedDropId &&
                                                                         timeBasedDrop.ParentDropId == dropId &&
                                                                         timeBasedDrop.AlertedOn == null);
    }

    public async Task<List<string>> GetAlertableGameNamesAsync()
    {
        IQueryable<string> query = _dbContext.Games.Where(game => game.ShouldAlert)
                                                   .Select(game => game.Name);
        return await query.ToListAsync();
    }

    public async Task InsertGamesAsync(IEnumerable<Game> games)
    {
        await _dbContext.BulkInsertAsync(games, new BulkConfig
        {
            PropertiesToIncludeOnCompare = [ "name" ],
            SetOutputIdentity = false
        });
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
