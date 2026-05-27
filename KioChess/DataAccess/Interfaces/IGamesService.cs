using DataAccess.Entities;

namespace DataAccess.Interfaces;

/// <summary>
/// Service for querying chess game statistics from GamesDbContext with 128-bit hash support
/// </summary>
public interface IGamesService
{
    /// <summary>
    /// Connect to the games database
    /// </summary>
    void Connect();

    /// <summary>
    /// Disconnect and release database resources
    /// </summary>
    void Disconnect();

    void Add(GameEntity[] gameEntities);

    /// <summary>
    /// Get game statistics for a specific position and next move
    /// </summary>
    GameEntity GetGameByHashAndMove(UInt128 hash, short nextMove);

    /// <summary>
    /// Get game statistics for a specific position and next move (async)
    /// </summary>
    Task<GameEntity> GetGameByHashAndMoveAsync(UInt128 hash, short nextMove);

    /// <summary>
    /// Get all game statistics for a specific position (all possible next moves)
    /// </summary>
    List<GameEntity> GetGamesByHash(UInt128 hash);

    /// <summary>
    /// Get all game statistics for a specific position (all possible next moves, async)
    /// </summary>
    Task<List<GameEntity>> GetGamesByHashAsync(UInt128 hash);

    /// <summary>
    /// Get game statistics filtered by position length
    /// </summary>
    List<GameEntity> GetGamesByLength(byte maxLength);

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
