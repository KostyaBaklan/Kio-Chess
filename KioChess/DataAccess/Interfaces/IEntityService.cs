using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Interfaces;

public interface IEntityService<TEntity, TContext> where TEntity : class where TContext : DbContext
{
    long GetCount();

    IEnumerable<TEntity> Execute(string sql, Func<SqliteDataReader, TEntity> factory, 
        List<SqliteParameter> parameters = null, int timeout = 60);

    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate);

    IQueryable<TEntity> GetAll();

    void Add(TEntity[] entities);

    void ClearAll();
}
