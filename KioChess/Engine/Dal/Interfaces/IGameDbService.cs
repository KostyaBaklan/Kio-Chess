using DataAccess.Models;
using DataAccess.Entities;
using DataAccess.Interfaces;

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
    IEnumerable<SequenceTotalItem> GetPopular(int minimumGames);

    IEnumerable<PositionTotal> GetPositions();
    void UpdateTotal(IBulkDbService bulkDbService);
    void AddDebuts(IEnumerable<Debut> debuts);
    void AddPopularPositions(IEnumerable<PopularPosition> popularPositions);
    void AddVeryPopularPositions(IEnumerable<VeryPopularPosition> popularPositions);
    void RemovePopularPositions();
    void RemoveVeryPopularPositions();
    IEnumerable<PopularPosition> GetPopularPositions();
    IEnumerable<VeryPopularPosition> GetVeryPopularPositions();
    PopularPositions GeneratePopularPositions();
    object GetPopularSize();
    object GetVeryPopularSize();
}
