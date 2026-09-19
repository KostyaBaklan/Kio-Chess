using DataAccess.Contexts;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Services.EntityServices;

/// <summary>
/// Entity service for MoveHash with domain-specific queries
/// </summary>
public class MoveHashService : EntityServiceBase<MoveHash, AppDbContext>
{
    public MoveHashService(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get all move hash values as UInt128 array indexed by move key
    /// Optimized for MoveHashSequenceHasher initialization
    /// </summary>
    public UInt128[] GetAllHashValues()
    {
        return [.. Context.MoveHashes
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(b => b.Hash)];
    }
}
