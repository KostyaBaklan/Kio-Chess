using DataAccess.Contexts;
using DataAccess.Helpers;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Services;

public abstract class DbServiceBase : IDbService
{
    protected LiteContext Connection;

    protected DbServiceBase()
    {

    }
    public void Connect()
    {
        Connection = new LiteContext();
        OnConnected();
    }

    protected abstract void OnConnected();

    public void Disconnect() => Connection.Dispose();

    public int Execute(string sql, List<SqliteParameter> parameters = null, int timeout = 30)
    {
        using var connction = new SqliteConnection(Connection.Database.GetConnectionString());
        return connction.Execute(sql, parameters, timeout);
    }

    public IEnumerable<T> Execute<T>(string sql, Func<SqliteDataReader, T> factory, List<SqliteParameter> parameters = null, int timeout = 60)
    {
        using var connction = new SqliteConnection(Connection.Database.GetConnectionString());
        return connction.Execute(sql,factory, parameters, timeout);
    }
}
