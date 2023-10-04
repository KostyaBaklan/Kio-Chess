using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IBookUpdateService
    {
        void Upsert(IEnumerable<Book> records);
    }
}
