using DataAccess.Contexts;
using DataAccess.Entities;

namespace DataAccess.Services.EntityServices;

/// <summary>
/// Entity service for PopularPositionEntity with domain-specific queries
/// </summary>
public class PopularPositionService : EntityServiceBase<PopularPositionEntity, AppDbContext>
{
    public PopularPositionService(AppDbContext context) : base(context)
    {
    }
}
