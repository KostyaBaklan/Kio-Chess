using DataAccess.Entities;
using DataAccess.Interfaces;
using Engine.Interfaces.Config;
using ProtoBuf;
using System.Collections.Concurrent;
using System.Diagnostics;
using Tools.Common;

namespace GamesServices;

/// <summary>
/// Service for streaming game sequences with in-memory aggregation before writing to games.db
/// Uses in-memory SQLite for deduplication during 10-hour PGN ingestion runs
/// </summary>
public class SequenceService : ISequenceService
{
    private bool _inProgress;
    private ConcurrentQueue<List<GameEntity>> _queue;
    private Task _updateTask;

    private readonly IGamesService _gamesService;
    private readonly IMemoryGameService _memoryGameService;

    public SequenceService()
    {
        _queue = new ConcurrentQueue<List<GameEntity>>();
        Boot.SetUp();

        _gamesService = Boot.GetService<IGamesService>();
        _gamesService.Connect();

        _memoryGameService = Boot.GetService<IMemoryGameService>();
        _memoryGameService.Connect();
    }

    public void ProcessSequence(byte[] sequences)
    {
        List<GameEntity> records = Serializer.Deserialize<List<GameEntity>>(sequences.AsSpan());
        _queue.Enqueue(records);
    }

    public void Save()
    {
        var config = Boot.GetService<IConfigurationProvider>();

        _inProgress = false;
        _updateTask.Wait();

        Console.WriteLine($"Queue = {_queue.Count}");
        var timer = Stopwatch.StartNew();

        try
        {
            // Process any remaining queued records into memory DB
            while (_queue.Count > 0 && _queue.TryDequeue(out List<GameEntity> records))
            {
                _memoryGameService.Upsert(records);
            }

            // Get aggregated count from in-memory DB
            long aggregatedCount = _memoryGameService.GetTotalItems();
            long totalGames = _memoryGameService.GetTotalGames();

            Console.WriteLine($"Total GameEntity records after aggregation: {aggregatedCount:N0}   Games: {totalGames:N0}   {timer.Elapsed}");

            if (aggregatedCount == 0)
            {
                Console.WriteLine("No records to process");
                return;
            }

            // Read aggregated records from memory DB
            IEnumerable<GameEntity> aggregatedRecords = _memoryGameService.GetGameEntities();

            // Bulk insert to games.db using GamesService
            int totalInserted = 0;
            int chunkSize = config.BookConfiguration.Chunk;
            var chunks = aggregatedRecords.Chunk(chunkSize);

            int count = 0;
            foreach (var chunk in chunks)
            {
                var chunkTimer = Stopwatch.StartNew();
                _gamesService.Add(chunk);
                chunkTimer.Stop();

                totalInserted += chunk.Length;
                count++;
                Console.WriteLine($"Chunk {count}: {chunk.Length} records   {chunkTimer.Elapsed}   Total: {timer.Elapsed}");
            }

            Console.WriteLine($"Total upserted to games.db: {totalInserted:N0} GameEntity records   {timer.Elapsed}");
            Console.WriteLine();

            // Get final total games from new database
            var finalTotalGames = _gamesService.GetTotalGames();
            Console.WriteLine($"Total games in games.db: {finalTotalGames:N0}");

            // Update popular positions cache in kioapp.db
            ProcessPopularPositions(chunkSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during save: {ex.ToFormattedString()}");
            throw;
        }
        finally
        {
            _memoryGameService.Disconnect();
            _gamesService.Disconnect();
        }
    }

    public void Initialize()
    {
        _inProgress = true;
        _updateTask = Task.Factory.StartNew(UpdateRecords);
    }

    /// <summary>
    /// Process popular positions from games.db and update kioapp.db cache
    /// Should be called after Save() completes
    /// </summary>
    private void ProcessPopularPositions(int chunkSize)
    {
        var config = Boot.GetService<IConfigurationProvider>();
        var appDbService = Boot.GetService<IAppDbService>();

        Console.WriteLine();
        Console.WriteLine("════════════════════════════════════════════════════════════════════");
        Console.WriteLine("  Updating Popular Positions Cache: games.db → kioapp.db");
        Console.WriteLine("════════════════════════════════════════════════════════════════════");

        var timer = Stopwatch.StartNew();

        try
        {
            Console.WriteLine("Clearing existing popular positions...");
            appDbService.ClearPositions();
            appDbService.Shrink();

            // Load popular positions from games.db
            // minGames = GamesThreshold - 1
            // maxLength = 2 * SearchDepth + 1 (convert to byte length: depth * 2 moves per ply)
            int minGames = config.BookConfiguration.GamesThreshold - 1;
            int maxLength = config.BookConfiguration.SearchDepth + 1;

            Console.WriteLine($"Loading popular positions (min games: {minGames}, max length: {maxLength})...");
            IEnumerable<PopularPositionEntity> positions = _gamesService.LoadPopularPositions(minGames, maxLength);

            var chunks = positions.Chunk(chunkSize);

            int totalSize = 0;
            int chunkCount = 0;

            foreach (var chunk in chunks)
            {
                totalSize += chunk.Length;
                chunkCount++;
                Console.WriteLine($"Chunk {chunkCount}: {chunk.Length:N0} positions | Total: {totalSize:N0} | {timer.Elapsed}");

                appDbService.Add(chunk);
            }

            Console.WriteLine();
            Console.WriteLine($"✓ Total popular positions cached: {appDbService.GetPositionsCount():N0}");
            Console.WriteLine($"✓ Time elapsed: {timer.Elapsed}");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating popular positions: {ex.ToFormattedString()}");
            throw;
        }

        timer.Stop();
    }

    private void UpdateRecords()
    {
        while (_inProgress)
        {
            if (_queue.TryDequeue(out List<GameEntity> records))
            {
                // Stream records to in-memory DB for aggregation
                // This keeps the queue small during the 10-hour ingestion
                _memoryGameService.Upsert(records);
            }
            else
            {
                Thread.Sleep(10);  // Wait for more records
            }
        }
    }
}
