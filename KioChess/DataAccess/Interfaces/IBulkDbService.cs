using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IBulkDbService : IDbService, IBookUpdateService
    {
        void AddRange(IEnumerable<PopularPosition> chunk);
        void AddRange(IEnumerable<VeryPopularPosition> chunk);
        //void Upsert(IEnumerable<PositionTotal> item);
    }
}
