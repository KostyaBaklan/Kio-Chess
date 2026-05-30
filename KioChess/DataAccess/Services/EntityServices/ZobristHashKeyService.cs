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
}
