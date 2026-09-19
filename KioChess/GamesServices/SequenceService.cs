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
    private readonly IAppDbService _appDbService;

    public SequenceService()
    {
        _queue = new ConcurrentQueue<List<GameEntity>>();

        Boot.SetUp();

        _gamesService = Boot.GetService<IGamesService>();

        _memoryGameService = Boot.GetService<IMemoryGameService>();

        _appDbService = Boot.GetService<IAppDbService>();
    }

    public void ProcessSequence(byte[] sequences)
    {
        List<GameEntity> records = Serializer.Deserialize<List<GameEntity>>(sequences.AsSpan());
        _queue.Enqueue(records);
    }

    public void Save()
    {
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

            int totalInserted = 0;

            // Read aggregated records from memory DB
            IEnumerable<GameEntity> aggregatedRecords = _memoryGameService.GetGameEntities();
            var config = Boot.GetService<IConfigurationProvider>();

            // Bulk insert to games.db using GamesService
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

            CompactDB(_gamesService);

            // Update popular positions cache in kioapp.db
            _appDbService.ProcessPopularPositions(config, _gamesService);
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
            _appDbService.Disconnect();
        }
    }

    private void CompactDB(IDbService dbService)
    {
        var timer = Stopwatch.StartNew();
        Console.WriteLine($"Compacting {dbService.GetType().Name} DB");
        dbService.Shrink();
        timer.Stop();
        Console.WriteLine($"Compacted {dbService.GetType().Name} DB in {timer.Elapsed}");
    }

    public void Initialize()
    {
        _inProgress = true;

        _appDbService.Connect();
        _memoryGameService.Connect();
        _gamesService.Connect();

        _updateTask = Task.Factory.StartNew(UpdateRecords);
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
