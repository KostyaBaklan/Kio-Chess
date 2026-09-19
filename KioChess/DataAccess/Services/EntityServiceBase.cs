using DataAccess.Helpers;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Services;

public abstract class EntityServiceBase<TEntity, TContext> : IEntityService<TEntity, TContext>
    where TEntity : class
    where TContext : DbContext
{
    protected TContext Context;

    protected EntityServiceBase(TContext context)
    {
        Context = context;
    }

    public long GetCount()
    {
        return Context.Set<TEntity>().Count();
    }

    public void Add(TEntity[] entities)
    {
        if (entities == null || entities.Length == 0)
            return;

        // Use SqlExtensions generic Insert which delegates to IBulkInsertService<TEntity>
        using var connection = new SqliteConnection(Context.Database.GetConnectionString());
        connection.Insert(entities);
    }

    public void ClearAll()
    {
        Context.Set<TEntity>().ExecuteDelete();
        Context.SaveChanges();
    }

    public IEnumerable<TEntity> Execute(string sql, Func<SqliteDataReader, TEntity> factory, List<SqliteParameter> parameters = null, int timeout = 60)
    {
        using var connction = new SqliteConnection(Context.Database.GetConnectionString());
        return connction.Execute(sql, factory, parameters, timeout);
    }

    public IQueryable<TEntity> GetAll()
    {
        return Context.Set<TEntity>().AsNoTracking();
    }

    public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate)
    {
        return Context.Set<TEntity>().AsNoTracking().Where(predicate);
    }
}
