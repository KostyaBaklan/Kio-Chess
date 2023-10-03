using DataAccess.Entities;

namespace Engine.Dal.Interfaces;

public interface IMemoryDbService : IDbService
{
    void Upsert(List<Book> records);

    IQueryable<Book> GetBooks();
}
