using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

    public async Task<Dictionary<string, short>> GetDropOwnersMapAsync()
    {
        return await _dbContext.DropOwners.ToDictionaryAsync(dropOwner => dropOwner.Name,
                                                                          dropOwner => dropOwner.Id);
    }

    public async Task<short> InsertDropOwnerAsync(string dropOwnerName)
    {
        EntityEntry<DropOwner> insertedEntity = await _dbContext.DropOwners.AddAsync(new DropOwner
        {
            Name = dropOwnerName
        });

        await _dbContext.SaveChangesAsync();
        return insertedEntity.Entity.Id;
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

    public async Task InsertNewDropsAsync(List<Drop> drops)
    {
        IEnumerable<Guid> dropIds = drops.Select(drop => drop.Id);

        HashSet<Guid> existingIds = await _dbContext.Drops.Where(dbDrop => dropIds.Contains(dbDrop.Id))
                                                          .Select(drop => drop.Id)
                                                          .ToHashSetAsync();

        IEnumerable<Drop> newDrops = drops.Where(drop => !existingIds.Contains(drop.Id));
        await _dbContext.BulkInsertAsync(newDrops);
    }

    public async Task InsertTimeBasedDropsAsync(IEnumerable<TimeBasedDrop> timeBasedDrops)
    {
        // I'm opting not to perform a similar contains check to what I wrote for InsertNewDropsAsync
        // because I've already enforced when validating and parsing the drops within the TwitchDropsFinder service
        // that existing TimeBasedDrops will be filtered out early:
        await _dbContext.BulkInsertAsync(timeBasedDrops);
    }

    public async Task<List<Game>> GetGamesAsync()
    {
        IQueryable<Game> query = _dbContext.Games;
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<string>> GetExistingMatchingGamesAsync(List<string> gameNames)
    {
        IQueryable<string> query = _dbContext.Games.Where(dbGame => gameNames.Contains(dbGame.Name))
                                                   .Select(game => game.Name);
        return await query.ToListAsync();
    }

    public async Task InsertGamesAsync(IEnumerable<Game> games)
    {
        await _dbContext.BulkInsertAsync(games, new BulkConfig
        {
            PropertiesToIncludeOnCompare = [ nameof(Game.Name) ],
            SetOutputIdentity = false,
            ConflictOption = ConflictOption.Ignore
        });
    }
}
