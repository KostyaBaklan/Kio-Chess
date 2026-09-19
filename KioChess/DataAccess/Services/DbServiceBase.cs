using DataAccess.Helpers;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Services;

public abstract class DbServiceBase<TContext> : IDbService where TContext : DbContext
{
    protected TContext Connection; 

    protected abstract void OnConnected();

    protected abstract TContext CreateContext();

    public void Connect()
    {
        Connection = CreateContext();
        OnConnected();
    }

    public void Disconnect()
    {
        Connection.Dispose();
    }

    public int Execute(string sql, List<SqliteParameter> parameters = null, int timeout = 30)
    {
        using var connction = new SqliteConnection(Connection.Database.GetConnectionString());
        return connction.Execute(sql, parameters, timeout);
    }

    public IEnumerable<T> Execute<T>(string sql, Func<SqliteDataReader, T> factory, List<SqliteParameter> parameters = null, int timeout = 60)
    {
        using var connction = new SqliteConnection(Connection.Database.GetConnectionString());
        return connction.Execute(sql, factory, parameters, timeout);
    }

    public void Shrink()
    {
        Execute("VACUUM");
    }
}
