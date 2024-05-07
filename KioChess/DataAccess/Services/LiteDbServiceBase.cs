using DataAccess.Entities;
using DataAccess.Helpers;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;

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
    public int Execute(string sql, List<SqliteParameter> parameters = null, int timeout = 30)
    {
        using var connction = new SqliteConnection(_connection.ConnectionString);
        return connction.Execute(sql, parameters, timeout);
    }

    public IEnumerable<T> Execute<T>(string sql, Func<SqliteDataReader, T> factory, List<SqliteParameter> parameters = null, int timeout = 60)
    {
        using var connction = new SqliteConnection(_connection.ConnectionString);
        return connction.Execute(sql, factory, parameters, timeout);
    }

    public void Upsert(IEnumerable<Book> records) => _connection.Upsert(records);
}
