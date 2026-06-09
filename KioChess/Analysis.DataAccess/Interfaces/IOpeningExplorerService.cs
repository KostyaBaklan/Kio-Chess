using DataAccess.Entities;

namespace Analysis.DataAccess.Interfaces;

/// <summary>
/// Service for managing and querying chess opening data.
/// Provides tree-based navigation through opening variations.
/// </summary>
public interface IOpeningExplorerService
{
    int GetTotalOpeningsCount();
    Task<OpeningEntry> GetOpeningByMoveKeysAsync(short[] moveKeys);

    /// <summary>Get all root openings (first moves)</summary>
    Task<List<OpeningEntry>> GetRootOpeningsAsync();

    /// <summary>Get variations for a parent opening at specific move count (depth + 1)</summary>
    /// <param name="parentId">Parent opening ID</param>
    /// <param name="moveCount">Exact move count to filter by (typically currentDepth + 1)</param>
    Task<List<OpeningEntry>> GetVariationsAsync(int parentId, int moveCount);

    /// <summary>Search openings by name (fuzzy match)</summary>
    Task<List<OpeningEntry>> SearchByNameAsync(string query, int maxResults = 50);
}
