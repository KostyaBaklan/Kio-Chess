using DataAccess.Contexts;
using Engine.Dal.Interfaces;
using Engine.Interfaces.Config;
using Microsoft.EntityFrameworkCore;

namespace Engine.Dal.Services;

public abstract class DbServiceBase : IDbService
{
    protected LiteContext Connection;

    protected DbServiceBase(IConfigurationProvider configuration)
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

    public IQueryable<T> Execute<T>(string sql)
    {
        return Connection.Database.SqlQueryRaw<T>(sql);
    }
}
