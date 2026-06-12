using DataAccess.Contexts;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Services.EntityServices;

/// <summary>
/// Entity service for OpeningEntry
/// </summary>
public class OpeningEntryService : EntityServiceBase<OpeningEntry, AppDbContext>
{
    public OpeningEntryService(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get opening by 128-bit sequence hash
    /// </summary>
    public OpeningEntry GetBySequenceHash(UInt128 sequenceHash)
    {
        var hashLow = (ulong)sequenceHash;
        var hashHigh = (ulong)(sequenceHash >> 64);

        return Context.OpeningEntries
            .AsNoTracking()
            .FirstOrDefault(o => o.SequenceHashLow == hashLow && o.SequenceHashHigh == hashHigh);
    }
}
