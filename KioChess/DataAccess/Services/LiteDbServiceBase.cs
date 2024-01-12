using DataAccess.Entities;
using DataAccess.Helpers;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace DataAccess.Services;

public abstract class LiteDbServiceBase : IDbService, IBookUpdateService
{
    protected SqliteConnection _connection;
    public abstract void Connect();

    public void Disconnect()
    {
        try
        {
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

    public IEnumerable<T> Execute<T>(string sql, Func<DbDataReader, T> factoy = null)
    {
        if (factoy == null) yield break;

        using var command = _connection.CreateCommand(sql);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            yield return factoy(reader);
        }
    }

    public void Upsert(IEnumerable<Book> records) => _connection.Upsert(records);
}
