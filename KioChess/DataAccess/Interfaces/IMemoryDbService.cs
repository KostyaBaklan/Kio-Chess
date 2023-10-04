using DataAccess.Entities;

namespace DataAccess.Interfaces;

public interface IMemoryDbService : IDbService
{
    long GetTotalItems();
    long GetTotalGames();

    void Upsert(List<Book> records);

    IEnumerable<Book> GetBooks();
}
