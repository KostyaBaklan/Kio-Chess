using DataAccess.Contexts;
using DataAccess.Interfaces;
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
    }

    public void Disconnect()
    {
        Connection.Dispose();
    }

    public void Execute(string sql, int timeout = 30)
    {
        Connection.Database.ExecuteSqlRaw(sql);
    }

    public IEnumerable<T> Execute<T>(string sql)
    {
        return Connection.Database.SqlQueryRaw<T>(sql);
    }
}
