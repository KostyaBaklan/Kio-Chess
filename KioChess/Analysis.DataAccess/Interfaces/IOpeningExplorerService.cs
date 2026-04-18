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
    
    /// <summary>Get opening by position key (move sequence)</summary>
    Task<OpeningEntry> GetOpeningByPositionAsync(string positionKey);
    
    /// <summary>Get opening by ECO code</summary>
    Task<OpeningEntry> GetOpeningByECOAsync(string eco);
    
    /// <summary>Get all root openings (first moves)</summary>
    Task<List<OpeningEntry>> GetRootOpeningsAsync();
    
    /// <summary>Get all variations for a parent opening</summary>
    Task<List<OpeningEntry>> GetVariationsAsync(int parentId);
    
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
