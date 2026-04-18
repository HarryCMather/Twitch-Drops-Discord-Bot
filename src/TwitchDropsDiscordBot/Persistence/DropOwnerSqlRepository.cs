using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using TwitchDropsDiscordBot.Contexts;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence.Interfaces;

namespace TwitchDropsDiscordBot.Persistence;

public sealed class DropOwnerSqlRepository : IDropOwnerRepository
{
    private readonly TwitchDropsBotDbContext _dbContext;

    public DropOwnerSqlRepository(TwitchDropsBotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Dictionary<string, short>> GetDropOwnersMapAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.DropOwners.ToDictionaryAsync(dropOwner => dropOwner.Name,
                                                                          dropOwner => dropOwner.Id,
                                                             cancellationToken);
    }

    public async Task<List<DropOwner>> InsertNewDropOwnersAsync(List<string> newOwnerNames, CancellationToken cancellationToken)
    {
        IEnumerable<DropOwner> dropOwnersToInsert = newOwnerNames.Select(ownerName => new DropOwner
        {
            Name = ownerName
        });

        await _dbContext.BulkInsertAsync(dropOwnersToInsert, cancellationToken: cancellationToken);

        IQueryable<DropOwner> query = _dbContext.DropOwners.Where(dropOwner => newOwnerNames.Contains(dropOwner.Name));
        return await query.ToListAsync(cancellationToken);
    }
}
