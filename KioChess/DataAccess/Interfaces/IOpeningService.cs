using DataAccess.Entities;

namespace DataAccess.Interfaces;

/// <summary>
/// Service for querying chess openings from AppDbContext with 128-bit hash support
/// </summary>
public interface IOpeningService:IDbService
{
    IEnumerable<OpeningEntry> GetAllOpenings();
    /// <summary>
    /// Get opening by 128-bit sequence hash
    /// </summary>
    OpeningEntry GetOpeningBySequenceHash(UInt128 sequenceHash);

    /// <summary>
    /// Get opening by move keys (computes 128-bit hash internally)
    /// </summary>
    OpeningEntry GetOpeningByMoveKeys(ReadOnlySpan<short> moveKeys);

    /// <summary>
    /// Get opening name for display (ECO + Name + Variation)
    /// Returns null if opening not found
    /// </summary>
    string GetOpeningName(ReadOnlySpan<short> moveKeys);
}
