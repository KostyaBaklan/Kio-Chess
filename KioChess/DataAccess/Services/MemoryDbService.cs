using DataAccess.Entities;
using DataAccess.Helpers;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;

namespace DataAccess.Services;

public class MemoryDbService : IMemoryDbService
{
    private readonly SqliteConnection _connection;

    public MemoryDbService()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
    }
    public void Connect()
    {
        _connection.Open();

        using var command = _connection.CreateCommand();
        string sql = "CREATE TABLE Books (History BLOB NOT NULL,NextMove INTEGER NOT NULL, White INTEGER NOT NULL DEFAULT 0, Draw INTEGER NOT NULL DEFAULT 0, Black INTEGER NOT NULL DEFAULT 0, PRIMARY KEY( History, NextMove));";

        command.CommandText = sql;

        command.ExecuteNonQuery();
    }

    public void Disconnect()
    {
        try
        {
            // _context.Dispose();
            _connection.Close();
        }
        catch (Exception)
        {
        }
    }

    public void Execute(string sql, int timeout = 30)
    {
        using var command = _connection.CreateCommand(sql);
        command.ExecuteNonQuery();
    }

    public IEnumerable<T> Execute<T>(string sql)
    {
        throw new NotImplementedException();
    }

    public void Upsert(List<Book> records)
    {
        using var transaction = _connection.BeginTransaction();
        string sql = @"INSERT INTO Books(History , NextMove, White, Draw, Black) VALUES($H, $M, $W, $D, $B)
                          ON CONFLICT DO UPDATE 
                          SET White = White + excluded.White, Draw = Draw + excluded.Draw, Black = Black + excluded.Black";

        using var command = _connection.CreateCommand(sql);
        try
        {
            command.Parameters.AddWithValue("$H", new byte[0]);
            command.Parameters.AddWithValue("$M", 0);
            command.Parameters.AddWithValue("$W", 0);
            command.Parameters.AddWithValue("$D", 0);
            command.Parameters.AddWithValue("$B", 0);

            for (int i = 0; i < records.Count; i++)
            {
                command.Parameters[0].Value = records[i].History;
                command.Parameters[1].Value = records[i].NextMove;
                command.Parameters[2].Value = records[i].White;
                command.Parameters[3].Value = records[i].Draw;
                command.Parameters[4].Value = records[i].Black;

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Transaction failed   {e}");
            transaction.Rollback();
        }
    }

    public IEnumerable<Book> GetBooks()
    {
        using var command = _connection.CreateCommand("select * from Books");
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return new Book
            {
                History = reader.GetFieldValue<byte[]>(0),
                NextMove = reader.GetInt16(1),
                White = reader.GetInt32(2),
                Draw = reader.GetInt32(3),
                Black = reader.GetInt32(4)
            };
        }
    }

    public long GetTotalItems()
    {
        return ExecuteScalar<long>("select count(*) from Books");
    }

    public long GetTotalGames()
    {
        return ExecuteScalar<long>("select sum(White+Draw+Black) from Books where History = x''");
    }

    private T ExecuteScalar<T>(string sql)
    {
        using var command = _connection.CreateCommand(sql);
        var result = command.ExecuteScalar();

        if (result == null) return default;
        return (T)result;
    }
}
