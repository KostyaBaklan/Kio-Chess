using DataAccess.Entities;

namespace DataAccess.Interfaces;

public interface IMemoryDbService : IDbService, IBookUpdateService
{
    long GetTotalItems();
    long GetTotalGames();

    IEnumerable<Book> GetBooks();
}

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
