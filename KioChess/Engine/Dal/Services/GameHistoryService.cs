using DataAccess.Contexts;
using DataAccess.Entities;
using DataAccess.Helpers;
using DataAccess.Models;
using DataAccess.Services;
using Engine.Dal.Interfaces;
using Engine.Interfaces.Config;
using Engine.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Engine.Dal.Services;

/// <summary>
/// Service for managing game history records using GamesDbContext with 128-bit hash strategy
/// Implements legacy GameDbService behavior adapted for GameEntity and UInt128 hashing
/// </summary>
public class GameHistoryService : DbServiceBase<GamesDbContext>, IGameHistoryService
{
    private readonly int _maxDepth;
    private readonly GameEntityFactory _gameEntityFactory;
    private readonly MoveHistoryService _moveHistory;

    public GameHistoryService(
        IConfigurationProvider configurationProvider,
        GameEntityFactory gameEntityFactory,
        MoveHistoryService moveHistory)
        : base()
    {
        _maxDepth = configurationProvider.BookConfiguration.SaveDepth;
        _gameEntityFactory = gameEntityFactory;
        _moveHistory = moveHistory;
    }

    protected override GamesDbContext CreateContext()
    {
        return new GamesDbContext();
    }

    protected override void OnConnected()
    {
        Connection.Database.EnsureCreated();
    }

    /// <summary>
    /// Get aggregated move outcomes for a given position hash
    /// Replaces legacy Get(byte[] history) with 128-bit hash lookup
    /// </summary>
    public HistoryValue Get(UInt128 hash)
    {
        HistoryValue value = new HistoryValue();

        var games = Connection.GameEntities.AsNoTracking()
            .Where(x => x.Low == (ulong)hash && x.High == (ulong)(hash >> 64))
            .Select(x => new { x.NextMove, x.White, x.Draw, x.Black });

        foreach (var game in games)
        {
            value.Add(game.NextMove, game.White, game.Draw, game.Black);
        }

        return value;
    }

    /// <summary>
    /// Update history records based on game outcome
    /// Uses current move sequence from MoveHistoryService and computes 128-bit hashes
    /// </summary>
    public void UpdateHistory(GameValue value)
    {
        var moves = _moveHistory.GetSaveSequence();

        List<GameEntity> records = value switch
        {
            GameValue.WhiteWin => _gameEntityFactory.CreateRecords(moves, 1, 0, 0),
            GameValue.BlackWin => _gameEntityFactory.CreateRecords(moves, 0, 0, 1),
            _ => _gameEntityFactory.CreateRecords(moves, 0, 1, 0),
        };

        Upsert(records);
    }

    /// <summary>
    /// Upsert game entity records to database
    /// </summary>
    private void Upsert(List<GameEntity> records)
    {
        var connectionString = Connection.Database.GetConnectionString();
        using var connection = new SqliteConnection(connectionString);
        connection.Insert(records);
    }
}
