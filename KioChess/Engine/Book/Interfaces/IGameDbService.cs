using DataAccess.Models;

namespace Engine.Book.Interfaces
{
    public interface IGameDbService : IDbService
    {
        Task LoadAsync(IBookService bookService);
        void WaitToData();
        HistoryValue Get(byte[] history);
        void UpdateHistory(GameValue value); 
        List<DataAccess.Entities.Book> CreateRecords(int white, int draw, int black);
        void Upsert(List<DataAccess.Entities.Book> records);
    }
}
