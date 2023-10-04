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
    }

    public void Disconnect()
    {
        Connection.Dispose();
    }

    public void Execute(string sql, int timeout = 30)
    {
        Connection.Database.ExecuteSqlRaw(sql);
    }

    public IEnumerable<T> Execute<T>(string sql, Func<DbDataReader, T> factoy = null)
    {
        return Connection.Database.SqlQueryRaw<T>(sql);
    }
}
