using DataAccess.Entities;

namespace DataAccess.Interfaces;

/// <summary>
/// Service interface for managing pre-computed hash tables in AppDbContext (kioapp.db)
/// </summary>
public interface IAppDbService : IDbService
{
    /// <summary>
    /// Get all MoveHash values as UInt128 array indexed by move key
    /// Optimized for MoveHashSequenceHasher initialization
    /// </summary>
    UInt128[] GetAllMoveHashValues();

    /// <summary>
    /// Get popular positions filtered by total games and sequence length
    /// </summary>
    List<PopularPositionEntity> GetPopularPositions(int games, int search);

    void ClearPositions();
    void Add(PopularPositionEntity[] chunk);
    object GetPositionsCount();
}
