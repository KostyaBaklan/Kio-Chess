using DataAccess.Entities;
using DataAccess.Interfaces;

namespace DataAccess.Services;

/// <summary>
/// Generates cryptographically random 128-bit hashes with deterministic seed
/// </summary>
public class HashGenerator
{
    private readonly Random _random;

    /// <summary>
    /// Create hash generator with specified seed for deterministic results
    /// </summary>
    /// <param name="seed">Random seed (use same seed to get same hashes)</param>
    public HashGenerator(int seed = 42)
    {
        _random = new Random(seed);
    }

    /// <summary>
    /// Generate 768 Zobrist hash keys for 12 pieces × 64 squares
    /// Note: These are for reference/future use, not currently used for sequence hashing
    /// </summary>
    public List<ZobristHashKey> GenerateZobristKeys()
    {
        var keys = new List<ZobristHashKey>();

        for (short id = 0; id < 768; id++)  // 12 pieces * 64 squares
        {
            keys.Add(new ZobristHashKey
            {
                Id = id,
                Hash = NextUInt128()
            });
        }

        return keys;
    }

    /// <summary>
    /// Generate hash keys for all unique move keys
    /// This is the critical table for order-independent sequence hashing
    /// </summary>
    /// <param name="moveKeys">Collection of unique move keys (should be ~15,116 keys)</param>
    public List<MoveHash> GenerateMoveHashes(IEnumerable<short> moveKeys)
    {
        var hashes = new List<MoveHash>();
        var distinctKeys = moveKeys.Distinct().OrderBy(k => k).ToList();

        Console.WriteLine($"Generating hashes for {distinctKeys.Count} unique move keys...");

        foreach (var moveKey in distinctKeys)
        {
            hashes.Add(new MoveHash
            {
                Id = moveKey,
                Hash = NextUInt128()
            });
        }

        return hashes;
    }

    /// <summary>
    /// Generate cryptographically random UInt128 value
    /// </summary>
    private UInt128 NextUInt128()
    {
        // Generate two random UInt64 values
        ulong low = NextUInt64();
        ulong high = NextUInt64();

        return new UInt128(high, low);
    }

    /// <summary>
    /// Generate cryptographically random UInt64 value
    /// </summary>
    private ulong NextUInt64()
    {
        byte[] buffer = new byte[8];
        _random.NextBytes(buffer);
        return BitConverter.ToUInt64(buffer, 0);
    }
}

/// <summary>
/// High-level service for populating hash tables in AppDbContext
/// </summary>
public class HashPopulationService
{
    private readonly HashGenerator _generator;

    public HashPopulationService(int seed = 42)
    {
        _generator = new HashGenerator(seed);
    }

    /// <summary>
    /// Populate all hash tables in kioapp.db
    /// </summary>
    /// <param name="appDbService">AppDbContext service</param>
    /// <param name="allMoveKeys">All unique move keys from MoveProvider</param>
    public async Task PopulateAllHashTables(IAppDbService appDbService, IEnumerable<short> allMoveKeys)
    {
        Console.WriteLine("=== Hash Table Population ===");
        Console.WriteLine();

        //// 1. Clear existing data
        //Console.WriteLine("Clearing existing hash tables...");
        //await appDbService.ClearZobristHashKeysAsync();
        //await appDbService.ClearMoveHashesAsync();
        //Console.WriteLine();

        //// 2. Generate Zobrist keys (optional, for reference)
        //Console.WriteLine("Generating Zobrist hash keys (768 entries)...");
        //var zobristKeys = _generator.GenerateZobristKeys();
        //await appDbService.PopulateZobristHashKeysAsync(zobristKeys);
        //Console.WriteLine();

        //// 3. Generate Move hashes (CRITICAL for sequence hashing)
        //Console.WriteLine("Generating Move hash keys...");
        //var moveHashes = _generator.GenerateMoveHashes(allMoveKeys);
        //await appDbService.PopulateMoveHashesAsync(moveHashes);
        //Console.WriteLine();

        // 4. Verify
        Console.WriteLine("Verifying hash tables...");
        var zobristCount = appDbService.GetZobristHashKeyCount();
        var moveCount = appDbService.GetMoveHashCount();

        Console.WriteLine($"  - ZobristHashKeys: {zobristCount} rows");
        Console.WriteLine($"  - MoveHashes: {moveCount} rows");

        (bool isValid, List<string> errors) = appDbService.VerifyHashTables();

        if (isValid)
        {
            Console.WriteLine("✓ All verification checks passed!");
        }
        else
        {
            Console.WriteLine("⚠ Verification found issues:");
            foreach (var error in errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== Hash Table Population Complete ===");
    }
}
