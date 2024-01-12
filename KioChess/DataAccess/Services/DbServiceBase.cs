using DataAccess.Contexts;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

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

    public void Execute(string sql, int timeout = 30) => Connection.Database.ExecuteSqlRaw(sql);

    public IEnumerable<T> Execute<T>(string sql, Func<DbDataReader, T> factoy = null) => Connection.Database.SqlQueryRaw<T>(sql);
}
