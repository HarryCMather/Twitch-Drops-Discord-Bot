using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            return await _dbContext.DropOwners.ToDictionaryAsync(dropOwner => dropOwner.Name,
                                                                              dropOwner => dropOwner.Id,
                                                                 cancellationToken);
        }
    }

    public async Task<short> InsertDropOwnerAsync(string dropOwnerName, CancellationToken cancellationToken)
    {
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            EntityEntry<DropOwner> insertedEntity = await _dbContext.DropOwners.AddAsync(new DropOwner
            {
                Name = dropOwnerName
            }, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return insertedEntity.Entity.Id;
        }
    }
}
