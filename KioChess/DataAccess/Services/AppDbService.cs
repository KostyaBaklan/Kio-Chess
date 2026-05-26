using DataAccess.Contexts;
using DataAccess.Entities;
using DataAccess.Helpers;
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

    public UInt128[] GetAllMoveHashValues()
    {
        return [.. Connection.MoveHashes
            .AsNoTracking()
            .OrderBy(x=>x.Id)
            .Select(b=>b.Hash)];
    }

    public async Task PopulatePopularPositionsAsync(IEnumerable<PopularPositionEntity> positions)
    {
        var positionsList = positions.ToList();

        // Add in batches to avoid memory issues
        const int batchSize = 10000;
        int totalAdded = 0;

        foreach (var batch in positionsList.Chunk(batchSize))
        {
            await Connection.PopularPositions.AddRangeAsync(batch);
            await Connection.SaveChangesAsync();
            totalAdded += batch.Length;
            Console.WriteLine($"  Added {totalAdded}/{positionsList.Count} popular positions...");
        }

        Console.WriteLine($"✓ Populated {positionsList.Count} Popular positions");
    }

    public long GetPopularPositionCount()
    {
        return Connection.PopularPositions.Count();
    }

    public async Task ClearPopularPositionsAsync()
    {
        await Connection.Database.ExecuteSqlRawAsync("DELETE FROM PopularPositions");
        Console.WriteLine("✓ Cleared PopularPositions table");
    }

    public IEnumerable<PopularPositionEntity> GetAllPopularPositions()
    {
        return [.. Connection.PopularPositions.AsNoTracking()];
    }

    public List<PopularPositionEntity> GetPopularPositions(int games, int search)
    {
        var query = Connection.PopularPositions.AsNoTracking()
                .Where(ptd => ptd.Total > games && ptd.Length < search);

        List<PopularPositionEntity> positions = new(2400000);
        positions.AddRange(query);
        return positions;
    }

    public void ClearPositions()
    {
        Connection.PopularPositions.ExecuteDelete();
        Connection.SaveChanges();
    }

    public void Shrink() => Execute("VACUUM;");

    public void Add(PopularPositionEntity[] records)
    {
        using (var connection = new SqliteConnection(Connection.Database.GetConnectionString()))
        {
            connection.Open();
            connection.Insert(records);
        }
    }

    public object GetPositionsCount()
    {
        return Connection.PopularPositions.AsNoTracking().Count();
    }
}
