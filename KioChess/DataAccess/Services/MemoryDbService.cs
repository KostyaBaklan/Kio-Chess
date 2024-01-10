using DataAccess.Entities;
using DataAccess.Helpers;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;

namespace DataAccess.Services;

public class MemoryDbService : LiteDbServiceBase, IMemoryDbService
{
    public MemoryDbService()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
    }
    public override void Connect()
    {
        _connection.Open();

        string sql = "CREATE TABLE Books (History BLOB NOT NULL,NextMove INTEGER NOT NULL, White INTEGER NOT NULL DEFAULT 0, Draw INTEGER NOT NULL DEFAULT 0, Black INTEGER NOT NULL DEFAULT 0, PRIMARY KEY( History, NextMove));";

        Execute(sql);
    }

    public IEnumerable<Book> GetBooks() => Execute("select * from Books", reader => new Book
    {
        History = reader.GetFieldValue<byte[]>(0),
        NextMove = reader.GetInt16(1),
        White = reader.GetInt32(2),
        Draw = reader.GetInt32(3),
        Black = reader.GetInt32(4)
    });

    public long GetTotalItems() => ExecuteScalar<long>("select count(*) from Books");

    public long GetTotalGames() => ExecuteScalar<long>("select sum(White+Draw+Black) from Books where History = x''");

    private T ExecuteScalar<T>(string sql)
    {
        using var command = _connection.CreateCommand(sql);
        var result = command.ExecuteScalar();

        if (result == null || result.Equals(DBNull.Value)) return default;
        return (T)result;
    }
}
