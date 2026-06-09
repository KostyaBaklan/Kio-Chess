using DataAccess.Entities;

namespace DataAccess.Interfaces;

/// <summary>
/// In-memory service for aggregating GameEntity records during ingestion
/// </summary>
public interface IMemoryGameService
{
    void Connect();
    void Disconnect();
    void Upsert(IEnumerable<GameEntity> records);
    IEnumerable<GameEntity> GetGameEntities();
    long GetTotalItems();
    long GetTotalGames();
}
