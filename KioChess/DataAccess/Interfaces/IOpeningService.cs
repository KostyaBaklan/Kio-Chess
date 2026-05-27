using DataAccess.Entities;

namespace DataAccess.Interfaces;

/// <summary>
/// Service for querying chess openings from AppDbContext with 128-bit hash support
/// </summary>
public interface IOpeningService
{
    /// <summary>
    /// Connect to the opening database and preload popular openings
    /// </summary>
    void Connect();

    /// <summary>
    /// Disconnect and release database resources
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Get opening by 128-bit sequence hash
    /// </summary>
    OpeningEntry GetOpeningBySequenceHash(UInt128 sequenceHash);

    /// <summary>
    /// Get opening by 128-bit sequence hash (async)
    /// </summary>
    Task<OpeningEntry> GetOpeningBySequenceHashAsync(UInt128 sequenceHash);

    /// <summary>
    /// Get opening by move keys (computes 128-bit hash internally)
    /// </summary>
    OpeningEntry GetOpeningByMoveKeys(short[] moveKeys);

    /// <summary>
    /// Get opening by move keys (computes 128-bit hash internally, async)
    /// </summary>
    Task<OpeningEntry> GetOpeningByMoveKeysAsync(short[] moveKeys);

    /// <summary>
    /// Get opening name for display (ECO + Name + Variation)
    /// Returns null if opening not found
    /// </summary>
    string GetOpeningName(short[] moveKeys);

    /// <summary>
    /// Get opening name for display (ECO + Name + Variation, async)
    /// Returns null if opening not found
    /// </summary>
    Task<string> GetOpeningNameAsync(short[] moveKeys);

    /// <summary>
    /// Check if any openings exist in the database
    /// </summary>
    bool IsInitialized();

    /// <summary>
    /// Get count of openings in database
    /// </summary>
    long GetOpeningCount();
}
