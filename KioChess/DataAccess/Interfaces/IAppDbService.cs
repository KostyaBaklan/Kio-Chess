using DataAccess.Entities;

namespace DataAccess.Interfaces;

/// <summary>
/// Service interface for managing pre-computed hash tables in AppDbContext (kioapp.db)
/// </summary>
public interface IAppDbService : IDbService
{
    /// <summary>
    /// Populate ZobristHashKey table with 768 pre-computed 128-bit hashes (12 pieces × 64 squares)
    /// </summary>
    Task PopulateZobristHashKeysAsync(IEnumerable<ZobristHashKey> keys);

    /// <summary>
    /// Populate MoveHash table with pre-computed 128-bit hashes for all unique move keys
    /// </summary>
    Task PopulateMoveHashesAsync(IEnumerable<MoveHash> hashes);

    /// <summary>
    /// Populate PopularPositions table with position data using 128-bit hashes
    /// </summary>
    Task PopulatePopularPositionsAsync(IEnumerable<PopularPositionEntity> positions);

    /// <summary>
    /// Get count of ZobristHashKey records
    /// </summary>
    long GetZobristHashKeyCount();

    /// <summary>
    /// Get count of MoveHash records
    /// </summary>
    long GetMoveHashCount();

    /// <summary>
    /// Get count of PopularPosition records
    /// </summary>
    long GetPopularPositionCount();

    /// <summary>
    /// Clear all ZobristHashKey records
    /// </summary>
    Task ClearZobristHashKeysAsync();

    /// <summary>
    /// Clear all MoveHash records
    /// </summary>
    Task ClearMoveHashesAsync();

    /// <summary>
    /// Clear all PopularPosition records
    /// </summary>
    Task ClearPopularPositionsAsync();

    /// <summary>
    /// Get all MoveHash records
    /// </summary>
    IEnumerable<MoveHash> GetAllMoveHashes();

    /// <summary>
    /// Get all MoveHash values as UInt128 array indexed by move key
    /// Optimized for MoveHashSequenceHasher initialization
    /// </summary>
    UInt128[] GetAllMoveHashValues();

    /// <summary>
    /// Get all PopularPosition records
    /// </summary>
    IEnumerable<PopularPositionEntity> GetAllPopularPositions();

    /// <summary>
    /// Get popular positions filtered by total games and sequence length
    /// </summary>
    List<PopularPositionEntity> GetPopularPositions(int games, int search);

    void ClearPositions();
    void Shrink();
    void Add(PopularPositionEntity[] chunk);
    object GetPositionsCount();
}
