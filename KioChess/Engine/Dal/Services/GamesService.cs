using DataAccess.Contexts;
using DataAccess.Entities;
using DataAccess.Helpers;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Engine.Dal.Services;

/// <summary>
/// Service for querying chess game statistics from GamesDbContext with 128-bit hash support
/// </summary>
public class GamesService : IGamesService
{
    private GamesDbContext _context;

    public void Connect()
    {
        _context = new GamesDbContext();
        _context.Database.EnsureCreated();
    }

    public void Disconnect()
    {
        _context?.Dispose();
        _context = null;
    }

    public GameEntity GetGameByHashAndMove(UInt128 hash, short nextMove)
    {
        var hashLow = (ulong)hash;
        var hashHigh = (ulong)(hash >> 64);

        return _context.GameEntities
            .AsNoTracking()
            .FirstOrDefault(g => g.Low == hashLow 
                              && g.High == hashHigh 
                              && g.NextMove == nextMove);
    }

    public async Task<GameEntity> GetGameByHashAndMoveAsync(UInt128 hash, short nextMove)
    {
        var hashLow = (ulong)hash;
        var hashHigh = (ulong)(hash >> 64);

        return await _context.GameEntities
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Low == hashLow 
                                   && g.High == hashHigh 
                                   && g.NextMove == nextMove);
    }

    public List<GameEntity> GetGamesByHash(UInt128 hash)
    {
        var hashLow = (ulong)hash;
        var hashHigh = (ulong)(hash >> 64);

        return [.. _context.GameEntities
            .AsNoTracking()
            .Where(g => g.Low == hashLow && g.High == hashHigh)];
    }

    public async Task<List<GameEntity>> GetGamesByHashAsync(UInt128 hash)
    {
        var hashLow = (ulong)hash;
        var hashHigh = (ulong)(hash >> 64);

        return await _context.GameEntities
            .AsNoTracking()
            .Where(g => g.Low == hashLow && g.High == hashHigh)
            .ToListAsync();
    }

    public List<GameEntity> GetGamesByLength(byte maxLength)
    {
        return [.. _context.GameEntities
            .AsNoTracking()
            .Where(g => g.Length <= maxLength)];
    }

    public long GetGameRecordCount()
    {
        return _context.GameEntities.Count();
    }

    public long GetTotalGames()
    {
        return _context.GameEntities
            .Where(g => g.Length == 0)
            .Sum(g => (long)g.White + g.Draw + g.Black);
    }

    public void Add(GameEntity[] records)
    {
        using (var connection = new SqliteConnection(_context.Database.GetConnectionString()))
        {
            connection.Open();
            connection.Insert(records);
        }
    }

    public IEnumerable<PopularPositionEntity> LoadPopularPositions(int minGames, int maxLength)
    {
        return _context.GameEntities
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
