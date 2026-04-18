using Analysis.DataAccess.Contexts;
using Analysis.DataAccess.Entities;
using Analysis.DataAccess.Interfaces;
using Engine.Models.Boards;
using Microsoft.EntityFrameworkCore;

namespace Analysis.DataAccess.Services;

/// <summary>
/// Manages chess opening database with tree-based navigation.
/// Supports import from TSV and PGN formats, position lookup, and variation exploration.
/// </summary>
public class OpeningExplorerService : IOpeningExplorerService
{
    private OpeningExplorerContext _context;
    private readonly Dictionary<string, OpeningEntry> _positionCache = new();

    public async Task ConnectAsync()
    {
        _context = new OpeningExplorerContext();
        await _context.Database.EnsureCreatedAsync();
        
        // Preload popular openings into cache
        if (await IsInitializedAsync())
        {
            await PreloadCacheAsync();
        }
    }

    public void Disconnect()
    {
        _context?.Dispose();
        _context = null;
        _positionCache.Clear();
    }

    private async Task<bool> IsInitializedAsync()
    {
        if (_context == null) return false;
        return await _context.Openings.AnyAsync();
    }

    public bool IsInitialized()
    {
        if (_context == null) return false;
        return _context.Openings.Any();
    }

    private async Task PreloadCacheAsync()
    {
        if (_context == null) return;
        
        // Load popular and short openings into memory (< 1MB)
        var popular = await _context.Openings
            .AsNoTracking()
            .Where(o => o.Popularity > 50 || o.MoveCount <= 10)
            .Take(1000)
            .ToListAsync();

        foreach (var opening in popular)
        {
            _positionCache[opening.PositionKey] = opening;
        }
    }

    public async Task<OpeningEntry> GetOpeningByPositionAsync(string positionKey)
    {
        await EnsureConnectedAsync();
        
        // Check cache first
        if (_positionCache.TryGetValue(positionKey, out var cached))
            return cached;
        
        // Query database
        var opening = await _context!.Openings
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.PositionKey == positionKey);
        
        if (opening != null)
            _positionCache[positionKey] = opening;
        
        return opening;
    }

    public async Task<OpeningEntry> GetOpeningByECOAsync(string eco)
    {
        EnsureConnected();
        
        return await _context!.Openings
            .AsNoTracking()
            .Where(o => o.ECO == eco)
            .OrderBy(o => o.MoveCount)
            .FirstOrDefaultAsync();
    }

    public async Task<List<OpeningEntry>> GetRootOpeningsAsync()
    {
        EnsureConnected();
        
        return await _context!.Openings
            .AsNoTracking()
            .Where(o => o.ParentId == null)
            .OrderByDescending(o => o.Popularity)
            .ToListAsync();
    }

    public async Task<List<OpeningEntry>> GetVariationsAsync(int parentId)
    {
        EnsureConnected();
        
        return await _context!.Openings
            .AsNoTracking()
            .Where(o => o.ParentId == parentId)
            .OrderByDescending(o => o.IsMainLine)
            .ThenByDescending(o => o.Popularity)
            .ToListAsync();
    }

    public async Task<List<OpeningEntry>> SearchByNameAsync(string query, int maxResults = 50)
    {
        EnsureConnected();
        
        if (string.IsNullOrWhiteSpace(query))
            return new List<OpeningEntry>();
        
        var lowerQuery = query.ToLowerInvariant();
        
        // Return ALL matches without ordering - let caller handle prioritization
        return await _context!.Openings
            .AsNoTracking()
            .Where(o => o.ECO.ToLower().Contains(lowerQuery) || 
                       o.Name.ToLower().Contains(lowerQuery) ||
                       o.FullName.ToLower().Contains(lowerQuery))
            .Take(maxResults * 2) // Get more results for better scoring
            .ToListAsync();
    }

    public async Task<List<OpeningEntry>> GetPopularOpeningsAsync(int count = 20)
    {
        EnsureConnected();
        
        return await _context!.Openings
            .AsNoTracking()
            .OrderByDescending(o => o.Popularity)
            .ThenBy(o => o.MoveCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetTotalOpeningsCountAsync()
    {
        EnsureConnected();
        return await _context!.Openings.CountAsync();
    }

    public async Task<int> ImportFromTSVAsync(string filePath, IProgress<int> progress = null)
    {
        EnsureConnected();
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"TSV file not found: {filePath}");

        var lines = await File.ReadAllLinesAsync(filePath);
        var entries = new List<OpeningEntry>();
        int imported = 0;

        // Save the current board reference to restore it after parsing
        var originalBoard = Engine.Models.Moves.MoveBase.Board;
        Position parserPosition = null;
        
        try
        {
            // Create a dedicated parser and position for this import operation only
            // These will be garbage collected after import completes
            var parser = new OpeningParser();
            parserPosition = new Position();

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                parserPosition.Clear();
                var entry = parser.ParseTSVLine(lines[i], i, parserPosition);
                if (entry != null)
                {
                    entries.Add(entry);
                    imported++;

                    if (imported % 100 == 0)
                    {
                        progress?.Report(imported);
                    }
                }
            }

            // Batch insert
            await _context!.Openings.AddRangeAsync(entries);
            await _context.SaveChangesAsync();

            // Build parent-child relationships in the opening tree
            await RebuildTreeStructureAsync(progress);

            progress?.Report(imported);
        }
        finally
        {
            // Ensure position is fully cleared before disposal
            parserPosition?.Clear();
            
            // Restore the original board reference
            Engine.Models.Moves.MoveBase.Board = originalBoard;
        }
        
        return imported;
    }

    public async Task<int> ImportFromPGNAsync(string filePath, IProgress<int> progress = null)
    {
        EnsureConnected();
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"PGN file not found: {filePath}");

        var content = await File.ReadAllTextAsync(filePath);
        var games = content.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        var entries = new List<OpeningEntry>();
        int imported = 0;

        // Save the current board reference to restore it after parsing
        var originalBoard = Engine.Models.Moves.MoveBase.Board;
        Position parserPosition = null;
        
        try
        {
            // Create a dedicated parser and position for this import operation only
            var parser = new OpeningParser();
            parserPosition = new Position();

            foreach (var game in games)
            {
                if (string.IsNullOrWhiteSpace(game)) continue;

                try
                {
                    var opening = ParsePGNGame(game, parser, parserPosition);
                    if (opening != null)
                    {
                        entries.Add(opening);
                        imported++;

                        if (imported % 50 == 0)
                        {
                            progress?.Report(imported);
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            await _context!.Openings.AddRangeAsync(entries);
            await _context.SaveChangesAsync();

            // Build parent-child relationships in the opening tree
            await RebuildTreeStructureAsync(progress);

            progress?.Report(imported);
        }
        finally
        {
            // Ensure position is fully cleared before disposal
            parserPosition?.Clear();
            
            // Restore the original board reference
            Engine.Models.Moves.MoveBase.Board = originalBoard;
        }
        
        return imported;
    }

    public async Task RebuildTreeStructureAsync(IProgress<int> progress = null)
    {
        EnsureConnected();
        
        var allOpenings = await _context!.Openings.OrderBy(o => o.MoveCount).ToListAsync();
        var positionMap = new Dictionary<string, OpeningEntry>();
        int processed = 0;

        foreach (var opening in allOpenings)
        {
            // Find the closest ancestor (longest matching prefix) in the database
            var parent = FindClosestAncestor(opening.MovesUCI, positionMap);
            
            if (parent != null)
            {
                opening.ParentId = parent.Id;
            }
            
            positionMap[opening.PositionKey] = opening;
            processed++;

            if (processed % 100 == 0)
            {
                progress?.Report(processed);
            }
        }

        await _context.SaveChangesAsync();
        progress?.Report(processed);
    }

    // Private helpers

    private void EnsureConnected()
    {
        if (_context == null)
            throw new InvalidOperationException("Database not connected. Call ConnectAsync() first.");
    }

    private async Task EnsureConnectedAsync()
    {
        if (_context == null)
        {
            await ConnectAsync();
        }
    }

    /// <summary>
    /// Find the closest ancestor opening by checking progressively shorter move sequences.
    /// Returns the longest matching prefix that exists in the position map.
    /// </summary>
    private OpeningEntry FindClosestAncestor(string movesUCI, Dictionary<string, OpeningEntry> positionMap)
    {
        var moves = movesUCI.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // Try progressively shorter sequences, starting from (n-1) moves down to 1 move
        for (int length = moves.Length - 1; length > 0; length--)
        {
            var ancestorMoves = moves.Take(length);
            var ancestorKey = string.Join("_", ancestorMoves);
            
            if (positionMap.TryGetValue(ancestorKey, out var ancestor))
            {
                return ancestor;
            }
        }
        
        return null; // No ancestor found (this is a root opening)
    }

    private string GetParentPositionKey(string movesUCI)
    {
        var moves = movesUCI.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (moves.Length <= 1) return string.Empty;
        
        var parentMoves = moves.Take(moves.Length - 1);
        return string.Join("_", parentMoves);
    }

    private OpeningEntry ParsePGNGame(string pgnGame, OpeningParser parser, Position position)
    {
        var lines = pgnGame.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        string opening = null;
        string variation = null;
        string eco = null;
        string moves = string.Empty;

        foreach (var line in lines)
        {
            if (line.StartsWith("[Opening "))
                opening = ExtractTagValue(line);
            else if (line.StartsWith("[Variation "))
                variation = ExtractTagValue(line);
            else if (line.StartsWith("[ECO "))
                eco = ExtractTagValue(line);
            else if (!line.StartsWith("["))
                moves = line.Trim();
        }

        if (string.IsNullOrEmpty(opening) || string.IsNullOrEmpty(moves))
            return null;

        var fullName = string.IsNullOrEmpty(variation) 
            ? opening 
            : $"{opening}: {variation}";

        // Parse using OpeningParser
        var tsvLine = $"{eco ?? "A00"}\t{fullName}\t{moves}";
        position.Clear();
        return parser.ParseTSVLine(tsvLine, 0, position);
    }

    private string ExtractTagValue(string line)
    {
        var start = line.IndexOf('"');
        var end = line.LastIndexOf('"');
        if (start >= 0 && end > start)
            return line.Substring(start + 1, end - start - 1);
        return string.Empty;
    }

    public void Dispose()
    {
        Disconnect();
    }
}
