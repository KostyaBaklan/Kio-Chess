using DataAccess.Entities;
using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using Engine.Models.Hash;
using Microsoft.Data.Sqlite;
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

            // PHASE 2: Migrate OpeningEntries to AppDb with 128-bit hash
             MigrateOpeningEntriesToAppDb();

            //ProcessPopularPositions();

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

    private static void ProcessPopularPositions()
    {
        var timer = Stopwatch.StartNew();
        Console.WriteLine("Initialize MoveHash");
        var hash = _appDbService.GetAllMoveHashValues();

        MoveHashSequenceHasher.Initialize(hash);

        Console.WriteLine("Clear Positions");
        _appDbService.ClearPositions();

        _appDbService.Shrink();

        IEnumerable<PopularPositionEntity> positions = _gameDbService.LoadPopularPositions();

        var chunks = positions.Chunk(20000);

        int size = 0;
        int count = 0;

        foreach (var chunk in chunks)
        {
            size += chunk.Length;
            count++;
            Console.WriteLine($"{count} - {size} - {timer.Elapsed}");

            _appDbService.Add(chunk);
        }

        Console.WriteLine($"Total Positions = {_appDbService.GetPositionsCount()} - {timer.Elapsed}");

        timer.Stop();
    }

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

            using (var connection = new SqliteConnection("Data Source=D:\\Dev\\ChessDB\\kioapp.db"))
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