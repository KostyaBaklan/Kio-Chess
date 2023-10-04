using DataAccess.Entities;

namespace DataAccess.Interfaces;

public interface IMemoryDbService : IDbService, IBookUpdateService
{
    long GetTotalItems();
    long GetTotalGames();

    IEnumerable<Book> GetBooks();
}
