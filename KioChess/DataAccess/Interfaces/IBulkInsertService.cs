using Microsoft.Data.Sqlite;

namespace DataAccess.Interfaces;

/// <summary>
/// Strategy interface for entity-specific bulk insert operations
/// Each entity type provides its own optimized SQL implementation
/// </summary>
public interface IBulkInsertService<TEntity> where TEntity : class
{
    /// <summary>
    /// Perform bulk insert with optimized SQL for this entity type
    /// Uses transactions and parameterized queries for best performance
    /// </summary>
    void BulkInsert(SqliteConnection connection, IEnumerable<TEntity> entities);
}
