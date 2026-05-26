using DataAccess.Interfaces;
using DataAccess.Services;
using Engine.Dal.Interfaces;
using Engine.Services;
using System.Diagnostics;
using System.Text;

internal class Program
{
    private static IOpeningDbService _openingDbService;
    private static IGameDbService _gameDbService;
    private static ILocalDbService _localDbService;
    private static IAppDbService _appDbService;

    private static void Main(string[] args)
    {
        Boot.SetUp();
        var timer = Stopwatch.StartNew();

        _openingDbService = Boot.GetService<IOpeningDbService>();
        _gameDbService = Boot.GetService<IGameDbService>();
        _localDbService = Boot.GetService<ILocalDbService>();
        _appDbService = Boot.GetService<IAppDbService>();

        try
        {
            //inMemory.Connect();
            _openingDbService.Connect();
            _gameDbService.Connect();
            _localDbService.Connect();
            _appDbService.Connect();

            // PHASE 0: Generate pre-computed hash data
            // GeneratePreComputedHashData().Wait();

            //ProcessPositionTotalDifferences();

            //DbAnalysis(timer);
        }
        finally
        {
            // inMemory.Disconnect();
            _openingDbService.Disconnect();
            _gameDbService.Disconnect();
            _localDbService?.Disconnect();
            _appDbService?.Disconnect();
        }

        timer.Stop();
        Console.WriteLine();
        Console.WriteLine(timer.Elapsed);
        Console.WriteLine();
        Console.WriteLine($"Finished !!!");
        Console.ReadLine();
    }

    /// <summary>
    /// PHASE 0: Generate pre-computed hash data for MoveHash and ZobristHashKey tables
    /// This must be done BEFORE any migration work
    /// </summary>
    private static async Task GeneratePreComputedHashData()
    {
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         PHASE 0: Pre-computed Hash Data Generation               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var timer = Stopwatch.StartNew();

        try
        {
            // Get MoveProvider to extract all unique move keys
            var moveProvider = Boot.GetService<MoveProvider>();
            var allMoveKeys = Enumerable.Range(0, moveProvider.MovesCount).
                Select(i=>(short)i).ToList();

            Console.WriteLine($"📊 Statistics:");
            Console.WriteLine($"   Total moves in MoveProvider: {moveProvider.MovesCount}");
            Console.WriteLine($"   Unique move keys: {allMoveKeys.Count}");
            Console.WriteLine();

            // Create hash population service with fixed seed (42) for deterministic results
            var hashPopulationService = new HashPopulationService(seed: 42);

            // Populate all hash tables
            await hashPopulationService.PopulateAllHashTables(_appDbService, allMoveKeys);

            timer.Stop();

            Console.WriteLine();
            Console.WriteLine($"⏱ Time elapsed: {timer.Elapsed}");
            Console.WriteLine();
            Console.WriteLine("✅ Pre-computed hash data generation complete!");
            Console.WriteLine();

            // Display summary
            DisplayHashTableSummary();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ Error generating hash data: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Display summary of hash tables
    /// </summary>
    private static void DisplayHashTableSummary()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════════");
        Console.WriteLine("HASH TABLE SUMMARY");
        Console.WriteLine("═══════════════════════════════════════════════════════════════════");

        var zobristCount = _appDbService.GetZobristHashKeyCount();
        var moveHashCount = _appDbService.GetMoveHashCount();

        Console.WriteLine($"ZobristHashKeys: {zobristCount,6} rows (12 pieces × 64 squares = 768)");
        Console.WriteLine($"MoveHashes:      {moveHashCount,6} rows (unique move keys)");
        Console.WriteLine();

        // Sample some MoveHash entries
        Console.WriteLine("Sample MoveHash entries:");
        var samples = _appDbService.GetAllMoveHashes().Take(5);
        foreach (var sample in samples)
        {
            Console.WriteLine($"  Key {sample.Id,5}: Hash = {sample.Hash} (Low={sample.Low:X16}, High={sample.High:X16})");
        }

        Console.WriteLine("═══════════════════════════════════════════════════════════════════");
        Console.WriteLine();
    }

    private static void DbAnalysis(Stopwatch timer)
    {
        Dictionary<int, int> lengthCount = Enumerable.Range(0, 50).ToDictionary(i => i, i => 0);




        //var differentPositions = new HashSet<string>();
        int count = 0;
        string sql = $@"SELECT distinct History
                        from Books";

        var sequences = _gameDbService.Execute(sql, r =>
        {
            return Encoding.Unicode.GetString(r[0] as byte[]);
        }, timeout: 300);

        foreach (var chunk in sequences.Chunk(25000))
        {
            foreach (var sequence in chunk)
            {
                //differentPositions.Add(sequence);

                lengthCount[sequence.Length]++;
            }

            count += chunk.Length;

            Console.WriteLine($"{count} {timer.Elapsed}");
        }

        Console.WriteLine();
        foreach (var kvp in lengthCount)
        {
            Console.WriteLine($"{kvp.Key} - {kvp.Value}");
        }
        Console.WriteLine();
    }

    private static void ProcessPositionTotalDifferences()
    {
        Console.WriteLine("Clear Positions");
        _localDbService.ClearPositions();

        _localDbService.Shrink();

        var positions = _gameDbService.LoadPositions();

        var chunks = positions.Chunk(25000);

        int size = 0;
        int count = 0;

        foreach (var chunk in chunks)
        {
            size += chunk.Length;
            count++;
            Console.WriteLine($"{count} - {size}");

            _localDbService.Add(chunk);
        }

        Console.WriteLine($"Total Positions = {_localDbService.GetPositionsCount()}");
    }
}