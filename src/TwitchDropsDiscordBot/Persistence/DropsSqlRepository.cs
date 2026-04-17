using System.Diagnostics;
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

        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            return await _dbContext.TimeBasedDrops.AnyAsync(timeBasedDrop => timeBasedDrop.Id == timeBasedDropId &&
                                                                             timeBasedDrop.ParentDropId == dropId &&
                                                                             timeBasedDrop.AlertedOn == null);
        }
    }

    public async Task InsertNewDropsAsync(List<Drop> drops)
    {
        // I'm opting to use a bulk insert for new drops, but there's a chance some of the drops may already exist.
        // Here I'm firstly calling to get a list of existing drops and filtering out ones which have already been added.
        // Then performing the BulkInsert call.

        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            IEnumerable<Guid> dropIds = drops.Select(drop => drop.Id);

            HashSet<Guid> existingIds = await _dbContext.Drops.Where(dbDrop => dropIds.Contains(dbDrop.Id))
                                                              .Select(drop => drop.Id)
                                                              .ToHashSetAsync();

            IEnumerable<Drop> newDrops = drops.Where(drop => !existingIds.Contains(drop.Id));
            await _dbContext.BulkInsertAsync(newDrops);
        }
    }

    public async Task InsertTimeBasedDropsAsync(IEnumerable<TimeBasedDrop> timeBasedDrops)
    {
        // I'm opting not to perform a similar contains check to what I wrote for InsertNewDropsAsync
        // because I've already enforced when validating and parsing the drops within the TwitchDropsFinder service
        // that existing TimeBasedDrops will be filtered out early:

        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            await _dbContext.BulkInsertAsync(timeBasedDrops);
        }
    }
}
