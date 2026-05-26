using DataAccess.Contexts;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Services;

/// <summary>
/// Service for managing pre-computed hash tables in AppDbContext (kioapp.db)
/// </summary>
public class AppDbService : IAppDbService
{
    protected AppDbContext Connection;

    public void Connect()
    {
        Connection = new AppDbContext();
        OnConnected();
    }

    protected void OnConnected()
    {
        // Ensure directory exists before creating database
        var connectionString = Connection.Database.GetConnectionString();
        var dataSourceMatch = System.Text.RegularExpressions.Regex.Match(
            connectionString, 
            @"Data Source=([^;]+)", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (dataSourceMatch.Success)
        {
            var dbPath = dataSourceMatch.Groups[1].Value;
            var directory = Path.GetDirectoryName(dbPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Console.WriteLine($"?? Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }
        }

        // Ensure database is created
        Connection.Database.EnsureCreated();
        Console.WriteLine($"? Database ready at: {connectionString}");
    }

    public void Disconnect() => Connection?.Dispose();

    // Implement IDbService methods
    public int Execute(string sql, List<SqliteParameter> parameters = null, int timeout = 30)
    {
        using var connection = new SqliteConnection(Connection.Database.GetConnectionString());
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = timeout;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
        }

        return command.ExecuteNonQuery();
    }

    public IEnumerable<T> Execute<T>(string sql, Func<SqliteDataReader, T> factory, List<SqliteParameter> parameters = null, int timeout = 60)
    {
        using var connection = new SqliteConnection(Connection.Database.GetConnectionString());
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = timeout;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
        }

        using var reader = command.ExecuteReader();
        var results = new List<T>();

        while (reader.Read())
        {
            results.Add(factory(reader));
        }

        return results;
    }

    public async Task PopulateZobristHashKeysAsync(IEnumerable<ZobristHashKey> keys)
    {
        var keysList = keys.ToList();

        await Connection.ZobristHashKeys.AddRangeAsync(keysList);
        await Connection.SaveChangesAsync();

        Console.WriteLine($"? Populated {keysList.Count} Zobrist hash keys");
    }

    public async Task PopulateMoveHashesAsync(IEnumerable<MoveHash> hashes)
    {
        var hashesList = hashes.ToList();

        // Add in batches to avoid memory issues
        const int batchSize = 5000;
        int totalAdded = 0;

        foreach (var batch in hashesList.Chunk(batchSize))
        {
            await Connection.MoveHashes.AddRangeAsync(batch);
            await Connection.SaveChangesAsync();
            totalAdded += batch.Length;
            Console.WriteLine($"  Added {totalAdded}/{hashesList.Count} move hashes...");
        }

        Console.WriteLine($"? Populated {hashesList.Count} Move hashes");
    }

    public long GetZobristHashKeyCount()
    {
        return Connection.ZobristHashKeys.Count();
    }

    public long GetMoveHashCount()
    {
        return Connection.MoveHashes.Count();
    }

    public async Task ClearZobristHashKeysAsync()
    {
        await Connection.Database.ExecuteSqlRawAsync("DELETE FROM ZobristHashKeys");
        Console.WriteLine("? Cleared ZobristHashKeys table");
    }

    public async Task ClearMoveHashesAsync()
    {
        await Connection.Database.ExecuteSqlRawAsync("DELETE FROM MoveHashes");
        Console.WriteLine("? Cleared MoveHashes table");
    }

    public IEnumerable<MoveHash> GetAllMoveHashes()
    {
        return Connection.MoveHashes.AsNoTracking().OrderBy(m => m.Id).ToList();
    }

    public (bool isValid, List<string> errors) VerifyHashTables()
    {
        var errors = new List<string>();

        // Check ZobristHashKeys
        var zobristCount = Connection.ZobristHashKeys.Count();
        if (zobristCount != 768)
        {
            errors.Add($"Expected 768 ZobristHashKeys, found {zobristCount}");
        }

        var zobristDuplicates = Connection.ZobristHashKeys
            .GroupBy(z => z.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (zobristDuplicates.Any())
        {
            errors.Add($"Found {zobristDuplicates.Count} duplicate IDs in ZobristHashKeys: {string.Join(", ", zobristDuplicates)}");
        }

        // Check MoveHashes
        var moveHashCount = Connection.MoveHashes.Count();
        if (moveHashCount == 0)
        {
            errors.Add("MoveHashes table is empty");
        }

        var moveHashDuplicates = Connection.MoveHashes
            .GroupBy(m => m.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (moveHashDuplicates.Any())
        {
            errors.Add($"Found {moveHashDuplicates.Count} duplicate IDs in MoveHashes: {string.Join(", ", moveHashDuplicates.Take(10))}...");
        }

        // Check for hash collisions (same Low+High with different IDs)
        // Bring data to client-side first, then check for collisions
        var allMoveHashes = Connection.MoveHashes
            .Select(m => new { m.Id, m.Low, m.High })
            .ToList();

        var hashCollisions = allMoveHashes
            .GroupBy(m => new { m.Low, m.High })
            .Where(g => g.Count() > 1)
            .ToList();

        if (hashCollisions.Any())
        {
            errors.Add($"WARNING: Found {hashCollisions.Count} hash collisions in MoveHashes!");
            foreach (var collision in hashCollisions.Take(5))
            {
                var ids = string.Join(", ", collision.Select(x => x.Id));
                errors.Add($"  Collision: Low={collision.Key.Low:X16}, High={collision.Key.High:X16} ? IDs: {ids}");
            }
        }

        return (errors.Count == 0, errors);
    }
}
