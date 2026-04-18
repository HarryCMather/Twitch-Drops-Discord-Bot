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

    public async Task<HashSet<Guid>> GetUnalertedTimeBasedDropsAsync(List<Guid> timeBasedDropIds, CancellationToken cancellationToken)
    {
        // I've opted to use SQL here instead of EF directly, as it was producing sub-optimal execution plans, or attempting to load the whole dbset into memory.
        // This is something that Postgres is capable of handling, and I'd rather the extra control here.  This table will grow the most out of all the db tables,
        // so I need to avoid loading the contents into RAM for every call here:
        IQueryable<Guid> query = _dbContext.Database.SqlQuery<Guid>($"""
                                                                     SELECT input.id
                                                                     FROM unnest({timeBasedDropIds}::uuid[]) AS input(id)
                                                                     WHERE NOT EXISTS (
                                                                          SELECT 1
                                                                          FROM time_based_drops tbd
                                                                          WHERE tbd.id = input.id
                                                                          LIMIT 1
                                                                     )
                                                                     """);

        return await query.ToHashSetAsync(cancellationToken);
    }

    public async Task InsertNewDropsAsync(List<Drop> drops, CancellationToken cancellationToken)
    {
        // I'm opting to use a bulk insert for new drops, but there's a chance some of the drops may already exist.
        // Here I'm firstly calling to get a list of existing drops and filtering out ones which have already been added.
        // Then performing the BulkInsert call.

        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            IEnumerable<Guid> dropIds = drops.Select(drop => drop.Id);

            HashSet<Guid> existingIds = await _dbContext.Drops.Where(dbDrop => dropIds.Contains(dbDrop.Id))
                                                              .Select(drop => drop.Id)
                                                              .ToHashSetAsync(cancellationToken);

            IEnumerable<Drop> newDrops = drops.Where(drop => !existingIds.Contains(drop.Id));
            await _dbContext.BulkInsertAsync(newDrops, cancellationToken: cancellationToken);
        }
    }

    public async Task InsertTimeBasedDropsAsync(IEnumerable<TimeBasedDrop> timeBasedDrops, CancellationToken cancellationToken)
    {
        // I'm opting not to perform a similar contains check to what I wrote for InsertNewDropsAsync
        // because I've already enforced when validating and parsing the drops within the TwitchDropsFinder service
        // that existing TimeBasedDrops will be filtered out early:
        await _dbContext.BulkInsertAsync(timeBasedDrops, cancellationToken: cancellationToken);
    }
}
