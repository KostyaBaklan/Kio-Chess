using Analysis.DataAccess.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Engine.Models.Hash;
using System.Collections.Frozen;

namespace Analysis.DataAccess.Services;

/// <summary>
/// Manages chess opening database with tree-based navigation.
/// Supports import from TSV and PGN formats, position lookup, and variation exploration.
/// NOTE: This service uses the OLD Analysis.DataAccess.Entities.OpeningEntry with 64-bit hashes.
/// For new code, prefer DataAccess.Services.OpeningService with 128-bit hashes.
/// </summary>
public class OpeningExplorerService : IOpeningExplorerService
{
    private readonly FrozenDictionary<UInt128, OpeningEntry> _sequenceCache;

    public OpeningExplorerService(IOpeningService openingService)
    {
        var all = openingService.GetAllOpenings().ToList();
        var cache = all.GroupBy(o => o.SequenceHash)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.Popularity).First());
        _sequenceCache = cache.ToFrozenDictionary();
    }

    public Task<OpeningEntry> GetOpeningByMoveKeysAsync(short[] moveKeys)
    {
        return Task.Factory.StartNew(() =>
        {
            var hash = MoveHashSequenceHasher.ComputeSequenceHash(moveKeys);
            if (_sequenceCache.TryGetValue(hash, out var opening))
            {
                return opening;
            }
            return null;
        });
    }

    public int GetTotalOpeningsCount()
    {
        return _sequenceCache.Count;
    }

    public Task<List<OpeningEntry>> GetRootOpeningsAsync()
    {
        return Task.Factory.StartNew(() =>
        {
            return _sequenceCache.Values.Where(o => o.ParentId == null)
            .OrderByDescending(o => o.Popularity)
            .ToList();
        });
    }

    public Task<List<OpeningEntry>> GetVariationsAsync(int parentId, int moveCount)
    {
        return Task.Factory.StartNew(() =>
        {
            return _sequenceCache.Values.Where(o => o.ParentId == parentId && o.MoveCount == moveCount)
                    .OrderByDescending(o => o.IsMainLine)
                    .ThenByDescending(o => o.Popularity)
            .ToList();
        });
    }

    public Task<List<OpeningEntry>> SearchByNameAsync(string query, int maxResults = 50)
    {
        return Task.Factory.StartNew(() =>
        {
            var lowerQuery = query.ToLowerInvariant();
            return _sequenceCache.Values.Where(o => o.ECO.Contains(lowerQuery, StringComparison.CurrentCultureIgnoreCase) ||
                        o.Name.Contains(lowerQuery, StringComparison.CurrentCultureIgnoreCase) ||
                        o.FullName.Contains(lowerQuery, StringComparison.CurrentCultureIgnoreCase))
             .Take(maxResults * 2) // Get more results for better scoring
            .ToList();
        });
    }
}
