using DataAccess.Contexts;
using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Services.EntityServices;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Services;

/// <summary>
/// Service for managing pre-computed hash tables in AppDbContext (kioapp.db)
/// Provides access to entity-specific services for ZobristHashKey, MoveHash, PopularPositionEntity, and OpeningEntry
/// </summary>
public class AppDbService : DbServiceBase<AppDbContext>, IAppDbService
{
    private MoveHashService _moveHashes;
    private PopularPositionService _popularPositions;

    protected override AppDbContext CreateContext()
    {
        return new AppDbContext();
    }

    protected override void OnConnected()
    {
        _moveHashes = new MoveHashService(Connection);
        _popularPositions = new PopularPositionService(Connection);

    // Ensure directory exists before creating database
    var connectionString = Connection.Database.GetConnectionString();
        var dataSourceMatch = System.Text.RegularExpressions.Regex.Match(
            connectionString,
            @"Data Source=([^;]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (dataSourceMatch.Success)
        {
            var dbPath = dataSourceMatch.Groups[1].Value;
            var directory = Path.GetDirectoryName(dbPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        // Ensure database is created
        Connection.Database.EnsureCreated();
    }

    #region IAppDbService implementation - delegate to entity services

    /// <summary>
    /// Get all MoveHash values as UInt128 array indexed by move key
    /// Optimized for MoveHashSequenceHasher initialization
    /// </summary>
    public UInt128[] GetAllMoveHashValues() => _moveHashes.GetAllHashValues();

    /// <summary>
    /// Get popular positions filtered by total games and sequence length
    /// </summary>
    public List<PopularPositionEntity> GetPopularPositions(int games, int length)
    {
        var query = _popularPositions.Query(p => p.Length < length && p.Total > games);

        List<PopularPositionEntity> positions = new(2400000);
        positions.AddRange(query);
        return positions;
    }

    /// <summary>
    /// Clear all popular positions
    /// </summary>
    public void ClearPositions() => _popularPositions.ClearAll();

    /// <summary>
    /// Add popular position records
    /// </summary>
    public void Add(PopularPositionEntity[] records) => _popularPositions.Add(records);

    /// <summary>
    /// Get count of popular positions
    /// </summary>
    public object GetPositionsCount() => _popularPositions.GetCount();

    #endregion
}
