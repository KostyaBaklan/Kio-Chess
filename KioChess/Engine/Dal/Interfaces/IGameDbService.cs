using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace Engine.Dal.Interfaces;

public interface IGameDbService : IDbService
{
    long GetTotalGames();
    long GetTotalPopularGames();
    Task LoadAsync();
    void WaitToData();
    HistoryValue Get(byte[] history);
    void UpdateHistory(GameValue value);
    List<Book> CreateRecords(int white, int draw, int black);
    void Upsert(List<Book> records);

    IEnumerable<PositionTotal> GetPositions();

    IEnumerable<PositionEntity> LoadPositions();

    void UpdateTotal(IBulkDbService bulkDbService);
}