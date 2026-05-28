using DataAccess.Contexts;
using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Services;
using DataAccess.Services.EntityServices;
using Engine.Models.Hash;

namespace Engine.Dal.Services;

/// <summary>
/// Service for querying chess openings from AppDbContext with 128-bit hash support
/// Uses MoveHashSequenceHasher for order-independent position lookup
/// </summary>
public class OpeningService : DbServiceBase<AppDbContext>, IOpeningService
{
    private OpeningEntryService _openingEntries;

    protected override AppDbContext CreateContext()
    {
        return new AppDbContext();
    }

    protected override void OnConnected()
    {
        _openingEntries = new OpeningEntryService(Connection);
        Connection.Database.EnsureCreated();
    }

    #region IOpeningService implementation

    public OpeningEntry GetOpeningBySequenceHash(UInt128 sequenceHash)
    {

        // Use entity service to query
        var opening = _openingEntries.GetBySequenceHash(sequenceHash);


        return opening;
    }

    public OpeningEntry GetOpeningByMoveKeys(ReadOnlySpan<short> moveKeys)
    {
        var hash = MoveHashSequenceHasher.ComputeSequenceHash(moveKeys);
        return GetOpeningBySequenceHash(hash);
    }

    public string GetOpeningName(ReadOnlySpan<short> moveKeys)
    {
        var opening = GetOpeningByMoveKeys(moveKeys);
        return FormatOpeningName(opening);
    }

    private static string FormatOpeningName(OpeningEntry opening)
    {
        if (opening == null)
            return null;

        // Format: "Name: Variation" or just "Name" if no variation
        if (string.IsNullOrEmpty(opening.Name))
            return null;

        if (!string.IsNullOrEmpty(opening.Variation))
            return $"{opening.Name}: {opening.Variation}";

        return opening.Name;
    }

    public IEnumerable<OpeningEntry> GetAllOpenings()
    {
        return [.. _openingEntries.GetAll()];
    }

    #endregion
}
