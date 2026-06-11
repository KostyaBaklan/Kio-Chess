using DataAccess.Contexts;
using DataAccess.Entities;

namespace DataAccess.Services.EntityServices;

/// <summary>
/// Entity service for ZobristHashKey
/// </summary>
public class ZobristHashKeyService : EntityServiceBase<ZobristHashKey, AppDbContext>
{
    public ZobristHashKeyService(AppDbContext context) : base(context)
    {
    }

    internal void Update(ZobristHashKey[] keys)
    {
        Context.UpdateRange(keys);
        Context.SaveChanges();
    }
}
