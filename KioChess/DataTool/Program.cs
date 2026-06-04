using DataAccess.Entities;
using DataAccess.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Hash;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using Tools.Common;

internal class Program
{
    private static IAppDbService _appDbService;
    private static IGamesService _gameDbService;
    private static volatile bool _cancelRequested = false;

    private static void Main(string[] args)
    {
        // Setup Ctrl+C handler for graceful shutdown
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine();
            Console.WriteLine("⚠️  Ctrl+C detected - requesting graceful shutdown...");
            Console.WriteLine("    Waiting for current transaction to complete...");
            Console.WriteLine("    (Press Ctrl+C again to force quit - may lock database)");

            _cancelRequested = true;
            e.Cancel = true; // Prevent immediate termination
        };

        Boot.SetUp();
        var timer = Stopwatch.StartNew();

        _appDbService = Boot.GetService<IAppDbService>();
        _gameDbService = Boot.GetService<IGamesService>();

        try
        {
            //inMemory.Connect();
            _appDbService.Connect();
            _gameDbService.Connect();

            // ═══════════════════════════════════════════════════════════════
            // GAME MIGRATION: chess.db → games.db (128-bit hash)
            // ═══════════════════════════════════════════════════════════════
            // Uncomment the method you want to run:

            // 1. Show current migration status
            // ShowGameMigrationStatus();

            // 2. Start/resume migration - maximum speed (dedicated system, day time)
            // MigrateGames();

            // 3. Start/resume migration - overnight (8 hours, I/O throttling)
            // MigrateGames(timeLimitHours: 8, sleepMilliseconds: 100);  // RECOMMENDED

            // 4. Start/resume migration - gentle (shared system, battery mode)
            // MigrateGames(timeLimitHours: 8, sleepMilliseconds: 250);

            // 5. Start/resume migration - test run (30 minutes)
            //MigrateGames(timeLimitHours: 0.5, sleepMilliseconds: 100);

            // 6. Reset progress and start over
            // ResetGameMigrationProgress();

            // ═══════════════════════════════════════════════════════════════
            // OPENING MIGRATION: chessApp.db → kioapp.db (128-bit hash)
            // ═══════════════════════════════════════════════════════════════
            // MigrateOpeningEntriesToAppDb();

            // ═══════════════════════════════════════════════════════════════
            // POPULAR POSITIONS: chess.db → kioapp.db (128-bit hash)
            // ═══════════════════════════════════════════════════════════════
            ProcessPopularPositions(_appDbService, _gameDbService, Boot.GetService<IConfigurationProvider>());

            //DbAnalysis(timer);
        }
        finally
        {
            // inMemory.Disconnect();
            _appDbService?.Disconnect();
            _gameDbService?.Disconnect();

            // Force close all database connections to prevent lock issues
            //ForceCloseAllDatabaseConnections();
        }

        timer.Stop();
        Console.WriteLine();
        Console.WriteLine(timer.Elapsed);
        Console.WriteLine();
        Console.WriteLine($"Finished !!!");
        Console.ReadLine();
    }

    private static void ProcessPopularPositions(IAppDbService appDbService, IGamesService gameDbService, IConfigurationProvider configurationProvider)
    {
        appDbService.ProcessPopularPositions(configurationProvider, gameDbService);
    }

    #region Game Migration (chess.db → games.db) - Option 3: ROWID-Based

    private const string GameMigrationProgressFile = "game_migration_progress.txt";

    /// <summary>
    /// Entry point for game migration from chess.db to games.db
    /// </summary>
    /// <param name="timeLimitHours">Maximum hours to run migration (0 = unlimited)</param>
    /// <param name="sleepMilliseconds">Milliseconds to sleep between batches (0 = no sleep, 100 = recommended for overnight)</param>
    private static void MigrateGames(double timeLimitHours = 0, int sleepMilliseconds = 0)
    {
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          Migrate Games: chess.db → games.db (128-bit hash)         ║");
        Console.WriteLine("║                     ROWID-Based Chunking                           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Initialize MoveHashSequenceHasher for 128-bit hash computation
        if (!MoveHashSequenceHasher.IsInitialized)
        {
            Console.WriteLine("Initializing MoveHashSequenceHasher...");
            var moveHashes = _appDbService.GetAllMoveHashValues();
            MoveHashSequenceHasher.Initialize(moveHashes);
        }

        // OPTIMIZATION: Drop indexes before migration for much faster inserts
        Console.WriteLine("⚡ Optimizing target database for bulk insert...");
        DropGameIndexes();

        // Run continuous migration in batches
        // fetchSize: 200,000 records fetched per query (reduces round trips)
        // insertBatchSize: 50,000 records per transaction (balances speed and safety)
        // sleepMilliseconds: pause between batches to reduce I/O pressure
        RunContinuousGameMigration(
            fetchSize: 200000, 
            insertBatchSize: 50000, 
            maxBatches: int.MaxValue,
            timeLimitHours: timeLimitHours,
            sleepMilliseconds: sleepMilliseconds
        );

        // REBUILD: Recreate indexes after migration completes
        Console.WriteLine();
        Console.WriteLine("⚡ Rebuilding indexes on target database...");
        RebuildGameIndexes();

        Console.WriteLine();
        Console.WriteLine("✅ Game migration complete!");
        Console.WriteLine();
    }

    /// <summary>
    /// Get the last processed ROWID from progress file
    /// </summary>
    private static long GetLastProcessedRowId()
    {
        if (!File.Exists(GameMigrationProgressFile))
            return 0;

        var content = File.ReadAllText(GameMigrationProgressFile).Trim();
        return long.TryParse(content, out var rowId) ? rowId : 0;
    }

    /// <summary>
    /// Save progress (last processed ROWID) to file
    /// </summary>
    private static void SaveGameMigrationProgress(long rowId)
    {
        File.WriteAllText(GameMigrationProgressFile, rowId.ToString());
    }

    /// <summary>
    /// Migrate one batch of games from chess.db to games.db
    /// </summary>
    /// <param name="fetchSize">Number of records to fetch from source (larger for efficiency)</param>
    /// <param name="insertBatchSize">Number of records per insert transaction (smaller for safety)</param>
    private static long MigrateGameBatch(int fetchSize, int insertBatchSize)
    {
        long startRowId = GetLastProcessedRowId();

        var sourceConn = new SqliteConnection("Data Source=C:\\Dev\\ChessDB\\chess.db");
        var targetConn = new SqliteConnection("Data Source=C:\\Dev\\ChessDB\\games.db");

        try
        {
            sourceConn.Open();
            targetConn.Open();

            // Query batch from source Books table using ROWID
            var sql = @"
                SELECT rowid, History, NextMove, White, Draw, Black
                FROM Books
                WHERE rowid > @startRowId
                ORDER BY rowid
                LIMIT @fetchSize
            ";

            using var cmd = sourceConn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = 300;
            cmd.Parameters.AddWithValue("@startRowId", startRowId);
            cmd.Parameters.AddWithValue("@fetchSize", fetchSize);

            var allGameEntities = new List<(long rowId, GameEntity entity)>(fetchSize);
            long maxRowId = startRowId;

            // Fetch all records from source and track their rowids
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    maxRowId = reader.GetInt64(0);
                    var history = reader[1] as byte[];

                    var entity = new GameEntity
                    {
                        Hash = MoveHashSequenceHasher.ComputeSequenceHash(history),
                        NextMove = reader.GetInt16(2),
                        White = reader.GetInt32(3),
                        Draw = reader.GetInt32(4),
                        Black = reader.GetInt32(5),
                        Length = (byte)(history.Length / 2)
                    };

                    allGameEntities.Add((maxRowId, entity));
                }
            }

            // Bulk insert to target in smaller chunks for transaction safety
            if (allGameEntities.Count > 0)
            {
                int insertedCount = 0;

                foreach (var chunk in allGameEntities.Chunk(insertBatchSize))
                {
                    // Insert chunk (extract entities only)
                    var entitiesToInsert = chunk.Select(x => x.entity).ToArray();
                    DataAccess.Helpers.SqlExtensions.Insert(targetConn, entitiesToInsert);
                    insertedCount += chunk.Length;

                    // Save progress with the last rowid in this chunk
                    // This ensures Ctrl+C won't lose more than one chunk's work (50K records max)
                    var lastRowIdInChunk = chunk[^1].rowId;
                    SaveGameMigrationProgress(lastRowIdInChunk);
                }
            }

            return allGameEntities.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during batch migration: {ex.Message}");
            throw;
        }
        finally
        {
            // Ensure connections are always closed, even on error or Ctrl+C
            try
            {
                sourceConn?.Close();
                sourceConn?.Dispose();
            }
            catch { /* Ignore disposal errors */ }

            try
            {
                targetConn?.Close();
                targetConn?.Dispose();
            }
            catch { /* Ignore disposal errors */ }
        }
    }

    /// <summary>
    /// Run continuous migration in batches until complete
    /// </summary>
    /// <param name="fetchSize">Number of records to fetch per batch (larger = fewer round trips)</param>
    /// <param name="insertBatchSize">Number of records per insert transaction (smaller = safer, faster commits)</param>
    /// <param name="maxBatches">Maximum number of fetch batches to process</param>
    /// <param name="timeLimitHours">Maximum hours to run migration (0 = unlimited)</param>
    /// <param name="sleepMilliseconds">Milliseconds to sleep between batches to reduce I/O pressure (0 = no sleep)</param>
    private static void RunContinuousGameMigration(int fetchSize = 200000, int insertBatchSize = 50000, int maxBatches = int.MaxValue, double timeLimitHours = 0, int sleepMilliseconds = 0)
    {
        int batchCount = 0;
        long totalMigrated = 0;
        var startTime = DateTime.Now;
        var lastRowId = GetLastProcessedRowId();
        var timeLimit = timeLimitHours > 0 ? TimeSpan.FromHours(timeLimitHours) : TimeSpan.MaxValue;

        Console.WriteLine($"Starting from ROWID: {lastRowId:N0}");
        Console.WriteLine($"Fetch size: {fetchSize:N0} records per query");
        Console.WriteLine($"Insert batch size: {insertBatchSize:N0} records per transaction");
        Console.WriteLine($"Max batches: {(maxBatches == int.MaxValue ? "unlimited" : maxBatches.ToString("N0"))}");

        if (sleepMilliseconds > 0)
        {
            Console.WriteLine($"Batch delay: {sleepMilliseconds}ms (reduces I/O pressure)");
        }
        else
        {
            Console.WriteLine($"Batch delay: none (maximum speed)");
        }

        if (timeLimitHours > 0)
        {
            Console.WriteLine($"Time limit: {timeLimitHours:F1} hours ({TimeSpan.FromHours(timeLimitHours):hh\\:mm\\:ss})");
            Console.WriteLine($"Will stop at: {startTime.AddHours(timeLimitHours):yyyy-MM-dd HH:mm:ss}");
        }
        else
        {
            Console.WriteLine($"Time limit: unlimited");
        }

        Console.WriteLine($"Press Ctrl+C to stop gracefully after current transaction");
        Console.WriteLine();

        while (batchCount < maxBatches && !_cancelRequested)
        {
            var batchTimer = Stopwatch.StartNew();

            try
            {
                long count = MigrateGameBatch(fetchSize, insertBatchSize);

                if (count == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("✓ No more records to migrate - migration complete!");
                    break;
                }

                batchCount++;
                totalMigrated += count;
                batchTimer.Stop();

                var elapsed = DateTime.Now - startTime;
                var rate = elapsed.TotalSeconds > 0 ? totalMigrated / elapsed.TotalSeconds : 0;
                var currentRowId = GetLastProcessedRowId();

                Console.WriteLine(
                    $"Batch {batchCount,5}: {count,6:N0} records | " +
                    $"Total: {totalMigrated,12:N0} | " +
                    $"Rate: {rate,8:N0}/sec | " +
                    $"ROWID: {currentRowId,12:N0} | " +
                    $"Time: {batchTimer.Elapsed.TotalSeconds,6:F2}s | " +
                    $"Elapsed: {elapsed:hh\\:mm\\:ss}"
                );

                // Check for time limit after each batch
                if (timeLimitHours > 0 && elapsed >= timeLimit)
                {
                    Console.WriteLine();
                    Console.WriteLine("⏰ Time limit reached!");
                    Console.WriteLine($"   Elapsed time: {elapsed:hh\\:mm\\:ss} (limit: {timeLimit:hh\\:mm\\:ss})");
                    Console.WriteLine($"✓ Progress saved at ROWID: {currentRowId:N0}");
                    Console.WriteLine($"✓ Total migrated in this session: {totalMigrated:N0} records");
                    Console.WriteLine($"✓ You can resume by running the migration again");
                    Console.WriteLine();
                    break;
                }

                // Check for cancellation after each batch
                if (_cancelRequested)
                {
                    Console.WriteLine();
                    Console.WriteLine("⚠️  Graceful shutdown requested");
                    Console.WriteLine($"✓ Progress saved at ROWID: {currentRowId:N0}");
                    Console.WriteLine($"✓ Total migrated in this session: {totalMigrated:N0} records");
                    Console.WriteLine($"✓ You can resume by running the migration again");
                    break;
                }

                // Sleep between batches to reduce I/O pressure (if configured)
                // Benefits: Allows OS to flush caches, checkpoint WAL, prevents thermal throttling
                if (sleepMilliseconds > 0)
                {
                    Thread.Sleep(sleepMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"❌ Error in batch {batchCount + 1}: {ex.Message}");
                Console.WriteLine($"   Last successful ROWID: {GetLastProcessedRowId():N0}");
                Console.WriteLine($"   You can resume from this point by running the migration again.");
                Console.WriteLine();
                throw;
            }
        }

        var totalElapsed = DateTime.Now - startTime;
        var finalRate = totalElapsed.TotalSeconds > 0 ? totalMigrated / totalElapsed.TotalSeconds : 0;

        Console.WriteLine();
        Console.WriteLine("════════════════════════════════════════════════════════════════════");
        Console.WriteLine($"  Total migrated:    {totalMigrated,12:N0} records");
        Console.WriteLine($"  Total batches:     {batchCount,12:N0}");
        Console.WriteLine($"  Average rate:      {finalRate,12:N0} records/sec");
        Console.WriteLine($"  Total time:        {totalElapsed:hh\\:mm\\:ss}");
        Console.WriteLine($"  Final ROWID:       {GetLastProcessedRowId(),12:N0}");
        Console.WriteLine("════════════════════════════════════════════════════════════════════");
    }

    #endregion

    #region Game Migration Helper Methods

    /// <summary>
    /// Display migration status and statistics
    /// </summary>
    private static void ShowGameMigrationStatus()
    {
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                   Game Migration Status                            ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var sourceConn = new SqliteConnection("Data Source=C:\\Dev\\ChessDB\\chess.db");
        var targetConn = new SqliteConnection("Data Source=C:\\Dev\\ChessDB\\games.db");

        try
        {
            sourceConn.Open();
            targetConn.Open();

            // Get source statistics
            // Note: COUNT(*) is slow on large tables, use MAX(rowid) as approximation
            long totalSourceRecords = 0;
            long maxSourceRowId = 0;

            // Fast: Get MAX(rowid) - uses index, very fast
            using (var cmd = sourceConn.CreateCommand())
            {
                cmd.CommandText = "SELECT MAX(rowid) FROM Books";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    maxSourceRowId = reader.GetInt64(0);
                }
            }

            // Approximate total records from metadata (very fast, may be slightly inaccurate)
            // This reads SQLite's internal statistics instead of counting
            using (var cmd = sourceConn.CreateCommand())
            {
                // Try to get approximate count from sqlite_stat1 or use MAX(rowid) as estimate
                cmd.CommandText = "SELECT seq FROM sqlite_sequence WHERE name='Books'";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    totalSourceRecords = reader.GetInt64(0);
                }
                else
                {
                    // Fallback: use MAX(rowid) as approximation
                    // (close to actual if no deletions, which is likely for Books table)
                    totalSourceRecords = maxSourceRowId;
                }
            }

            // Get target statistics
            long totalTargetRecords = 0;

            using (var cmd = targetConn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM GameEntities";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    totalTargetRecords = reader.GetInt64(0);
                }
            }

            // Get progress
            long lastProcessedRowId = GetLastProcessedRowId();
            double progressPercent = maxSourceRowId > 0 ? (lastProcessedRowId * 100.0 / maxSourceRowId) : 0;

            Console.WriteLine($"Source (chess.db - Books table):");
            Console.WriteLine($"  Total records:        {totalSourceRecords,15:N0}");
            Console.WriteLine($"  Max ROWID:            {maxSourceRowId,15:N0}");
            Console.WriteLine();
            Console.WriteLine($"Target (games.db - GameEntities table):");
            Console.WriteLine($"  Total records:        {totalTargetRecords,15:N0}");
            Console.WriteLine();
            Console.WriteLine($"Migration Progress:");
            Console.WriteLine($"  Last processed ROWID: {lastProcessedRowId,15:N0}");
            Console.WriteLine($"  Progress:             {progressPercent,15:F2}%");
            Console.WriteLine($"  Remaining (approx):   {Math.Max(0, totalSourceRecords - totalTargetRecords),15:N0} records");
            Console.WriteLine();

            if (totalTargetRecords >= totalSourceRecords)
            {
                Console.WriteLine("✅ Migration appears to be complete!");
            }
            else if (lastProcessedRowId > 0)
            {
                Console.WriteLine("⏸  Migration in progress - resume by calling MigrateGames()");
            }
            else
            {
                Console.WriteLine("⚠  Migration not started - call MigrateGames() to begin");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error getting migration status: {ex.Message}");
        }
        finally
        {
            sourceConn?.Close();
            targetConn?.Close();
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Reset migration progress (start over)
    /// </summary>
    private static void ResetGameMigrationProgress()
    {
        if (File.Exists(GameMigrationProgressFile))
        {
            File.Delete(GameMigrationProgressFile);
            Console.WriteLine("✓ Migration progress reset. Next run will start from the beginning.");
        }
        else
        {
            Console.WriteLine("⚠ No progress file found - migration hasn't been started yet.");
        }
    }

    #endregion

    #region Game Migration Index Optimization

    /// <summary>
    /// Drop indexes on GameEntities table for faster bulk insert
    /// Indexes slow down inserts significantly on large tables
    /// </summary>
    private static void DropGameIndexes()
    {
        using var connection = new SqliteConnection("Data Source=C:\\Dev\\ChessDB\\games.db");
        connection.Open();

        try
        {
            // Drop secondary indexes (keep primary key for conflict detection)
            var dropCommands = new[]
            {
                "DROP INDEX IF EXISTS IX_GameEntities_Length",
                "DROP INDEX IF EXISTS IX_GameEntities_Hash"
            };

            foreach (var sql in dropCommands)
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                Console.WriteLine($"  ✓ {sql}");
            }

            Console.WriteLine("  ✓ Indexes dropped successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠ Error dropping indexes: {ex.Message}");
            Console.WriteLine("  Continuing anyway - indexes may not exist yet");
        }
        finally
        {
            connection.Close();
        }
    }

    /// <summary>
    /// Rebuild indexes on GameEntities table after migration completes
    /// This is much faster than maintaining indexes during insert
    /// </summary>
    private static void RebuildGameIndexes()
    {
        using var connection = new SqliteConnection("Data Source=C:\\Dev\\ChessDB\\games.db");
        connection.Open();

        try
        {
            var timer = Stopwatch.StartNew();

            // Recreate secondary indexes
            var indexCommands = new[]
            {
                @"CREATE INDEX IF NOT EXISTS IX_GameEntities_Length 
                  ON GameEntities(Length)",

                @"CREATE INDEX IF NOT EXISTS IX_GameEntities_Hash 
                  ON GameEntities(Low, High)"
            };

            foreach (var sql in indexCommands)
            {
                Console.WriteLine($"  Creating index...");
                var indexTimer = Stopwatch.StartNew();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 3600; // 1 hour for large tables
                cmd.ExecuteNonQuery();

                indexTimer.Stop();
                Console.WriteLine($"  ✓ Index created in {indexTimer.Elapsed:hh\\:mm\\:ss}");
            }

            // VACUUM to reclaim space and optimize database
            Console.WriteLine("  Optimizing database (VACUUM)...");
            var vacuumTimer = Stopwatch.StartNew();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "VACUUM";
                cmd.CommandTimeout = 3600;
                cmd.ExecuteNonQuery();
            }
            vacuumTimer.Stop();
            Console.WriteLine($"  ✓ VACUUM completed in {vacuumTimer.Elapsed:hh\\:mm\\:ss}");

            // ANALYZE to update query planner statistics
            Console.WriteLine("  Updating statistics (ANALYZE)...");
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "ANALYZE";
                cmd.ExecuteNonQuery();
            }
            Console.WriteLine($"  ✓ ANALYZE completed");

            timer.Stop();
            Console.WriteLine();
            Console.WriteLine($"  ✅ All indexes rebuilt in {timer.Elapsed:hh\\:mm\\:ss}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error rebuilding indexes: {ex.Message}");
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    #endregion

    #region Database Lock Management

    /// <summary>
    /// Force close all SQLite connections and clear connection pool
    /// Use this if database appears locked after crash or Ctrl+C
    /// </summary>
    private static void ForceCloseAllDatabaseConnections()
    {
        Console.WriteLine("🔓 Forcing all database connections to close...");

        try
        {
            // Clear SQLite connection pool for each database
            var databases = new[]
            {
                "Data Source=C:\\Dev\\ChessDB\\chess.db",
                "Data Source=C:\\Dev\\ChessDB\\games.db",
                "Data Source=C:\\Dev\\ChessDB\\kioapp.db"
            };

            foreach (var connString in databases)
            {
                try
                {
                    SqliteConnection.ClearPool(new SqliteConnection(connString));
                    Console.WriteLine($"  ✓ Cleared pool: {connString}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠ Could not clear pool: {connString} - {ex.Message}");
                }
            }

            // Force garbage collection to release any lingering connections
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Console.WriteLine("  ✓ Forced garbage collection");
            Console.WriteLine("✅ Database connections cleared");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error clearing connections: {ex.Message}");
        }

        Console.WriteLine();
    }

    #endregion

    /// <summary>
    /// Copy OpeningEntry data from Analysis.DataAccess (chessApp.db) to AppDbContext (kioapp.db)
    /// Migrates to 128-bit hash and removes SubVariation property
    /// </summary>
    private static void MigrateOpeningEntriesToAppDb()
    {
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         Migrate OpeningEntries to AppDb (128-bit hash)           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var timer = Stopwatch.StartNew();

        try
        {
            // Initialize MoveHashSequenceHasher for 128-bit hash computation
            if (!MoveHashSequenceHasher.IsInitialized)
            {
                Console.WriteLine("Initializing MoveHashSequenceHasher...");
                var moveHashes = _appDbService.GetAllMoveHashValues();
                MoveHashSequenceHasher.Initialize(moveHashes);
            }

            // Connect to source database (chessApp.db)
            Console.WriteLine("Loading OpeningEntries from chessApp.db...");
            var sourceConnectionString = "Data Source=C:\\Dev\\ChessDB\\chessApp.db";

            var targetEntries = new List<OpeningEntry>();

            using (var connection = new SqliteConnection(sourceConnectionString))
            {
                connection.Open();
                var sql = @"
                    SELECT Id, ECO, Name, Variation, FullName, MovesUCI, MovesSAN, 
                           MoveCount, FEN, MoveKeys, ParentId, Popularity, IsMainLine
                    FROM OpeningEntries";

                using var command = connection.CreateCommand();
                command.CommandText = sql;

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var moveKeysBlob = reader[9] as byte[];
                    var moveKeys = ConvertBytesToShortArray(moveKeysBlob);

                    var entry = new OpeningEntry
                    {
                        Id = reader.GetInt32(0),
                        ECO = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Name = reader.GetString(2),
                        Variation = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        // SubVariation is removed - not read from source
                        FullName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        MovesUCI = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        MovesSAN = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        MoveCount = reader.GetInt32(7),
                        FEN = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                        ParentId = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                        Popularity = reader.GetInt32(11),
                        IsMainLine = reader.GetInt32(12) != 0
                    };

                    // Compute 128-bit hash from move keys
                    if (moveKeys != null && moveKeys.Length > 0)
                    {
                        entry.SequenceHash = MoveHashSequenceHasher.ComputeSequenceHash(moveKeys);
                    }
                    else
                    {
                        entry.SequenceHash = UInt128.Zero;
                    }

                    targetEntries.Add(entry);
                }
            }

            Console.WriteLine($"Loaded {targetEntries.Count} OpeningEntries from source");

            // Clear existing OpeningEntries in AppDb
            Console.WriteLine("Clearing existing OpeningEntries in AppDb...");
           _appDbService.Execute("DELETE FROM OpeningEntries");

            // Insert in chunks
            Console.WriteLine("Inserting OpeningEntries into AppDb...");
            var chunks = targetEntries.Chunk(5000);
            int totalInserted = 0;
            int chunkCount = 0;

            using (var connection = new SqliteConnection("Data Source=C:\\Dev\\ChessDB\\kioapp.db"))
            {
                connection.Open();

                // Disable foreign key constraints during migration
                using (var pragmaCommand = connection.CreateCommand())
                {
                    pragmaCommand.CommandText = "PRAGMA foreign_keys = OFF";
                    pragmaCommand.ExecuteNonQuery();
                }

                foreach (var chunk in chunks)
                {
                    using var transaction = connection.BeginTransaction();
                    try
                    {
                        var sql = @"
                            INSERT INTO OpeningEntries 
                            (Id, ECO, Name, Variation, FullName, MovesUCI, MovesSAN, MoveCount, FEN, 
                             SequenceHashLow, SequenceHashHigh, ParentId, Popularity, IsMainLine)
                            VALUES 
                            ($id, $eco, $name, $var, $full, $uci, $san, $cnt, $fen, 
                             $hashLow, $hashHigh, $parent, $pop, $main)";

                        using var command = connection.CreateCommand();
                        command.CommandText = sql;

                        command.Parameters.AddWithValue("$id", 0);
                        command.Parameters.AddWithValue("$eco", "");
                        command.Parameters.AddWithValue("$name", "");
                        command.Parameters.AddWithValue("$var", "");
                        command.Parameters.AddWithValue("$full", "");
                        command.Parameters.AddWithValue("$uci", "");
                        command.Parameters.AddWithValue("$san", "");
                        command.Parameters.AddWithValue("$cnt", 0);
                        command.Parameters.AddWithValue("$fen", "");
                        command.Parameters.AddWithValue("$hashLow", 0L);
                        command.Parameters.AddWithValue("$hashHigh", 0L);
                        command.Parameters.AddWithValue("$parent", DBNull.Value);
                        command.Parameters.AddWithValue("$pop", 0);
                        command.Parameters.AddWithValue("$main", 0);

                        foreach (var entry in chunk)
                        {
                            command.Parameters[0].Value = entry.Id;
                            command.Parameters[1].Value = entry.ECO;
                            command.Parameters[2].Value = entry.Name;
                            command.Parameters[3].Value = entry.Variation;
                            command.Parameters[4].Value = entry.FullName;
                            command.Parameters[5].Value = entry.MovesUCI;
                            command.Parameters[6].Value = entry.MovesSAN;
                            command.Parameters[7].Value = entry.MoveCount;
                            command.Parameters[8].Value = entry.FEN;
                            command.Parameters[9].Value = (long)entry.SequenceHashLow;
                            command.Parameters[10].Value = (long)entry.SequenceHashHigh;
                            command.Parameters[11].Value = entry.ParentId.HasValue ? (object)entry.ParentId.Value : DBNull.Value;
                            command.Parameters[12].Value = entry.Popularity;
                            command.Parameters[13].Value = entry.IsMainLine ? 1 : 0;

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        totalInserted += chunk.Length;
                        chunkCount++;
                        Console.WriteLine($"Chunk {chunkCount}: {totalInserted}/{targetEntries.Count} ({timer.Elapsed})");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"❌ Error inserting chunk {chunkCount}: {ex.Message}");
                        throw;
                    }
                }

                // Re-enable foreign key constraints after migration
                using (var pragmaCommand = connection.CreateCommand())
                {
                    pragmaCommand.CommandText = "PRAGMA foreign_keys = ON";
                    pragmaCommand.ExecuteNonQuery();
                }
            }

            timer.Stop();

            Console.WriteLine();
            Console.WriteLine($"✅ Migration complete!");
            Console.WriteLine($"   Total entries migrated: {totalInserted}");
            Console.WriteLine($"   Time elapsed: {timer.Elapsed}");
            Console.WriteLine($"   SubVariation property removed");
            Console.WriteLine($"   Hash upgraded to 128-bit UInt128");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ Error during migration: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Helper method to convert byte[] BLOB to short[] move keys
    /// </summary>
    private static short[] ConvertBytesToShortArray(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return [];

        var shorts = new short[bytes.Length / 2];
        Buffer.BlockCopy(bytes, 0, shorts, 0, bytes.Length);
        return shorts;
    }
}