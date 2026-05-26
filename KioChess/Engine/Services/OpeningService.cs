using DataAccess.Contexts;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Engine.Models.Hash;
using Microsoft.EntityFrameworkCore;

namespace Engine.Services;

/// <summary>
/// Service for querying chess openings from AppDbContext with 128-bit hash support
/// Uses MoveHashSequenceHasher for order-independent position lookup
/// </summary>
public class OpeningService : IOpeningService
{
    private AppDbContext _context;
    private readonly Dictionary<UInt128, OpeningEntry> _cache = new(1000);
    private bool _isInitialized = false;

    public void Connect()
    {
        _context = new AppDbContext();
        _context.Database.EnsureCreated();
        PreloadPopularOpenings();
    }

    public void Disconnect()
    {
        _context?.Dispose();
        _context = null;
        _cache.Clear();
    }

    private void PreloadPopularOpenings()
    {
        if (_context == null) return;

        try
        {
            // Preload popular openings (Popularity > 50 or MoveCount <= 10) into cache
            var popular = _context.OpeningEntries
                .AsNoTracking()
                .Where(o => o.Popularity > 50 || o.MoveCount <= 10)
                .Take(500)
                .ToList();

            foreach (var opening in popular)
            {
                _cache[opening.SequenceHash] = opening;
            }

            _isInitialized = true;
            Console.WriteLine($"✓ OpeningService: Preloaded {popular.Count} popular openings");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ OpeningService preload failed: {ex.Message}");
        }
    }

    public OpeningEntry GetOpeningBySequenceHash(UInt128 sequenceHash)
    {
        if (_context == null) return null;

        // Check cache first
        if (_cache.TryGetValue(sequenceHash, out var cached))
            return cached;

        // Query database
        var opening = _context.OpeningEntries
            .AsNoTracking()
            .FirstOrDefault(o => o.SequenceHashLow == (ulong)sequenceHash 
                              && o.SequenceHashHigh == (ulong)(sequenceHash >> 64));

        if (opening != null)
            _cache[sequenceHash] = opening;

        return opening;
    }

    public async Task<OpeningEntry> GetOpeningBySequenceHashAsync(UInt128 sequenceHash)
    {
        if (_context == null) return null;

        // Check cache first
        if (_cache.TryGetValue(sequenceHash, out var cached))
            return cached;

        // Query database
        var hashLow = (ulong)sequenceHash;
        var hashHigh = (ulong)(sequenceHash >> 64);

        var opening = await _context.OpeningEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.SequenceHashLow == hashLow && o.SequenceHashHigh == hashHigh);

        if (opening != null)
            _cache[sequenceHash] = opening;

        return opening;
    }

    public OpeningEntry GetOpeningByMoveKeys(short[] moveKeys)
    {
        if (moveKeys == null || moveKeys.Length == 0)
            return null;

        if (!MoveHashSequenceHasher.IsInitialized)
        {
            Console.WriteLine("⚠ MoveHashSequenceHasher not initialized in OpeningService");
            return null;
        }

        var hash = MoveHashSequenceHasher.ComputeSequenceHash(moveKeys);
        return GetOpeningBySequenceHash(hash);
    }

    public async Task<OpeningEntry> GetOpeningByMoveKeysAsync(short[] moveKeys)
    {
        if (moveKeys == null || moveKeys.Length == 0)
            return null;

        if (!MoveHashSequenceHasher.IsInitialized)
        {
            Console.WriteLine("⚠ MoveHashSequenceHasher not initialized in OpeningService");
            return null;
        }

        var hash = MoveHashSequenceHasher.ComputeSequenceHash(moveKeys);
        return await GetOpeningBySequenceHashAsync(hash);
    }

    public string GetOpeningName(short[] moveKeys)
    {
        var opening = GetOpeningByMoveKeys(moveKeys);
        return FormatOpeningName(opening);
    }

    public async Task<string> GetOpeningNameAsync(short[] moveKeys)
    {
        var opening = await GetOpeningByMoveKeysAsync(moveKeys);
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

    public bool IsInitialized()
    {
        return _isInitialized && _context != null;
    }

    public long GetOpeningCount()
    {
        if (_context == null) return 0;
        return _context.OpeningEntries.Count();
    }
}
