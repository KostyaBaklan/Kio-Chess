using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public  interface IBulkDbService:IDbService
    {
        void Upsert(Book[] books);
    }
}
