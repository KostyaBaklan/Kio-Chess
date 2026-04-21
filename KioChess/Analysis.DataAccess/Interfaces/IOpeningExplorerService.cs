using Analysis.DataAccess.Entities;

namespace Analysis.DataAccess.Interfaces;

/// <summary>
/// Service for managing and querying chess opening data.
/// Provides tree-based navigation through opening variations.
/// </summary>
public interface IOpeningExplorerService : IDisposable
{
    /// <summary>Connect to the opening database</summary>
    Task ConnectAsync();

    /// <summary>Disconnect from the database</summary>
    void Disconnect();

    /// <summary>Check if database is initialized with data</summary>
    bool IsInitialized();

    /// <summary>Get opening by order-independent sequence hash (PREFERRED METHOD)
    /// Works regardless of move sequence order - no sorting needed
    /// Time: O(1) lookup via hash index</summary>
    Task<OpeningEntry> GetOpeningBySequenceHashAsync(ulong sequenceHash);

    /// <summary>Get openings by move key array (NEW - convenience method)
    /// Automatically converts move keys to hash internally
    /// </summary>
    Task<List<OpeningEntry>> GetOpeningsByMoveKeysAsync(short[] moveKeys);

    /// <summary>Get openings by move key list (NEW - convenience overload)
    /// Automatically converts move keys to hash internally
    /// </summary>
    Task<List<OpeningEntry>> GetOpeningsByMoveKeysAsync(List<short> moveKeys);

    /// <summary>Get opening by ECO code</summary>
    Task<OpeningEntry> GetOpeningByECOAsync(string eco);

    /// <summary>Get all root openings (first moves)</summary>
    Task<List<OpeningEntry>> GetRootOpeningsAsync();

    /// <summary>Get all variations for a parent opening</summary>
    Task<List<OpeningEntry>> GetVariationsAsync(int parentId);

    /// <summary>Get variations for a parent opening at specific move count (depth + 1)</summary>
    /// <param name="parentId">Parent opening ID</param>
    /// <param name="moveCount">Exact move count to filter by (typically currentDepth + 1)</param>
    Task<List<OpeningEntry>> GetVariationsAsync(int parentId, int moveCount);

    /// <summary>Search openings by name (fuzzy match)</summary>
    Task<List<OpeningEntry>> SearchByNameAsync(string query, int maxResults = 50);

    /// <summary>Get most popular openings</summary>
    Task<List<OpeningEntry>> GetPopularOpeningsAsync(int count = 20);

    /// <summary>Get opening statistics count</summary>
    Task<int> GetTotalOpeningsCountAsync();

    /// <summary>Import openings from TSV file (Lichess format)</summary>
    Task<int> ImportFromTSVAsync(string filePath, IProgress<int> progress = null);

    /// <summary>Import openings from PGN file</summary>
    Task<int> ImportFromPGNAsync(string filePath, IProgress<int> progress = null);

    /// <summary>Build tree structure and calculate position keys</summary>
    Task RebuildTreeStructureAsync(IProgress<int> progress = null);
}
