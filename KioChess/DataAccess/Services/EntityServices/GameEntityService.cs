using DataAccess.Contexts;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Services.EntityServices;

/// <summary>
/// Entity service for GameEntity with domain-specific queries
/// </summary>
public class GameEntityService : EntityServiceBase<GameEntity, GamesDbContext>
{
    public GameEntityService(GamesDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get game statistics for a specific position and next move
    /// </summary>
    public GameEntity GetByHashAndMove(UInt128 hash, short nextMove)
    {
        var hashLow = (ulong)hash;
        var hashHigh = (ulong)(hash >> 64);

        return Context.GameEntities
            .AsNoTracking()
            .FirstOrDefault(g => g.Low == hashLow 
                              && g.High == hashHigh 
                              && g.NextMove == nextMove);
    }

    /// <summary>
    /// Get all game statistics for a specific position (all possible next moves)
    /// </summary>
    public List<GameEntity> GetByHash(UInt128 hash)
    {
        var hashLow = (ulong)hash;
        var hashHigh = (ulong)(hash >> 64);

        return [.. Context.GameEntities
            .AsNoTracking()
            .Where(g => g.Low == hashLow && g.High == hashHigh)];
    }

    /// <summary>
    /// Get game statistics filtered by position length
    /// </summary>
    public List<GameEntity> GetByLength(byte maxLength)
    {
        return [.. Context.GameEntities
            .AsNoTracking()
            .Where(g => g.Length <= maxLength)];
    }

    /// <summary>
    /// Get total number of games across all positions
    /// Sum of White+Draw+Black for all length=0 records
    /// </summary>
    public long GetTotalGames()
    {
        return Context.GameEntities
            .Where(g => g.Length == 0)
            .Sum(g => (g.White + g.Draw + g.Black));
    }

    /// <summary>
    /// Load popular positions for kioapp.db cache
    /// Filters by minimum total games and maximum sequence length
    /// </summary>
    public IEnumerable<PopularPositionEntity> LoadPopularPositions(int minGames, int maxLength)
    {
        return Context.GameEntities
            .AsNoTracking()
            .Where(g => g.Length < maxLength && (g.White + g.Draw + g.Black) > minGames)
            .Select(g => new PopularPositionEntity
            {
                HashLow = g.Low,
                HashHigh = g.High,
                NextMove = g.NextMove,
                Total = g.White + g.Draw + g.Black,
                Length = g.Length
            });
    }
}
