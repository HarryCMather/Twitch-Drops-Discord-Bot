using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using TwitchDropsDiscordBot.Contexts;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence.Interfaces;

namespace TwitchDropsDiscordBot.Persistence;

public class DropsSqlRepository : IDropsRepository
{
    private readonly TwitchDropsBotDbContext _dbContext;

    public DropsSqlRepository(TwitchDropsBotDbContext dbContext)
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
}
