using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IBulkDbService : IDbService, IBookUpdateService
    {
        void Upsert(IEnumerable<PositionTotal> item);
    }
}
