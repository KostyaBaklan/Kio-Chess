using DataAccess.Contexts;
using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Services;
using DataAccess.Services.EntityServices;

namespace Engine.Dal.Services;

/// <summary>
/// Service for querying chess game statistics from GamesDbContext with 128-bit hash support
/// </summary>
public class GamesService : DbServiceBase<GamesDbContext>, IGamesService
{
    private GameEntityService _gameEntities;

    protected override GamesDbContext CreateContext()
    {
        var ctx =  new GamesDbContext();
        _gameEntities = new GameEntityService(ctx);
        return ctx;
    }

    protected override void OnConnected()
    {
        Connection.Database.EnsureCreated();
    }

    #region IGamesService implementation - delegate to GameEntities service

    /// <summary>
    /// Add game entity records
    /// </summary>
    public void Add(GameEntity[] records) => _gameEntities.Add(records);

    /// <summary>
    /// Get total count of game records
    /// </summary>
    public long GetGameRecordCount() => _gameEntities.GetCount();

    /// <summary>
    /// Load popular positions from games.db for kioapp.db cache
    /// Filters by minimum total games and maximum sequence length
    /// </summary>
    public IEnumerable<PopularPositionEntity> LoadPopularPositions(int minGames, int maxLength)
        => _gameEntities.LoadPopularPositions(minGames, maxLength);

    /// <summary>
    /// Get total number of games across all positions
    /// </summary>
    public long GetTotalGames() => _gameEntities.GetTotalGames();

    #endregion
}
