using DataAccess.Models;
using DataAccess.Entities;

namespace Engine.Dal.Interfaces;

public interface IGameDbService : IDbService
{
    Task LoadAsync(IBookService bookService);
    void WaitToData();
    HistoryValue Get(byte[] history);
    void UpdateHistory(GameValue value); 
    List<Book> CreateRecords(int white, int draw, int black);
    void Upsert(List<Book> records);
}
