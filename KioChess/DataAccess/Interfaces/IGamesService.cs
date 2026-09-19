using DataAccess.Entities;

namespace DataAccess.Interfaces;

/// <summary>
/// Service for querying chess game statistics from GamesDbContext with 128-bit hash support
/// </summary>
public interface IGamesService : IDbService
{
    void Add(GameEntity[] gameEntities);

    /// <summary>
    /// Get total count of game records
    /// </summary>
    long GetGameRecordCount();

    /// <summary>
    /// Load popular positions from games.db for kioapp.db cache
    /// Filters by minimum total games and maximum sequence length
    /// </summary>
    IEnumerable<PopularPositionEntity> LoadPopularPositions(int minGames, int maxLength);

    /// <summary>
    /// Get total number of games across all positions
    /// </summary>
    long GetTotalGames();
}
