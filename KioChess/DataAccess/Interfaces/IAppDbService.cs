using DataAccess.Entities;

namespace DataAccess.Interfaces;

/// <summary>
/// Service interface for managing pre-computed hash tables in AppDbContext (kioapp.db)
/// </summary>
public interface IAppDbService : IDbService
{
    /// <summary>
    /// Populate ZobristHashKey table with 768 pre-computed 128-bit hashes (12 pieces × 64 squares)
    /// </summary>
    Task PopulateZobristHashKeysAsync(IEnumerable<ZobristHashKey> keys);

    /// <summary>
    /// Populate MoveHash table with pre-computed 128-bit hashes for all unique move keys
    /// </summary>
    Task PopulateMoveHashesAsync(IEnumerable<MoveHash> hashes);

    /// <summary>
    /// Get count of ZobristHashKey records
    /// </summary>
    long GetZobristHashKeyCount();

    /// <summary>
    /// Get count of MoveHash records
    /// </summary>
    long GetMoveHashCount();

    /// <summary>
    /// Clear all ZobristHashKey records
    /// </summary>
    Task ClearZobristHashKeysAsync();

    /// <summary>
    /// Clear all MoveHash records
    /// </summary>
    Task ClearMoveHashesAsync();

    /// <summary>
    /// Get all MoveHash records
    /// </summary>
    IEnumerable<MoveHash> GetAllMoveHashes();

    /// <summary>
    /// Verify integrity of hash tables (check for duplicates, missing IDs, etc.)
    /// </summary>
    (bool isValid, List<string> errors) VerifyHashTables();
}
