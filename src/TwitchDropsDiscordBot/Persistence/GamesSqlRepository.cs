using System.Diagnostics;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using TwitchDropsDiscordBot.Contexts;
using TwitchDropsDiscordBot.Models.Entities;
using TwitchDropsDiscordBot.Persistence.Interfaces;

namespace TwitchDropsDiscordBot.Persistence;

public sealed class GamesSqlRepository : IGamesRepository
{
    private readonly TwitchDropsBotDbContext _dbContext;

    public GamesSqlRepository(TwitchDropsBotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Game>> GetGamesAsync()
    {
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            IQueryable<Game> query = _dbContext.Games;
            return await query.ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetExistingMatchingGamesAsync(List<string> gameNames)
    {
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            IQueryable<string> query = _dbContext.Games.Where(dbGame => gameNames.Contains(dbGame.Name))
                                                       .Select(game => game.Name);
            return await query.ToListAsync();
        }
    }

    public async Task InsertGamesAsync(IEnumerable<Game> games)
    {
        using (Activity.Current?.Source?.StartActivity(ActivityKind.Server))
        {
            await _dbContext.BulkInsertAsync(games);
        }
    }
}
