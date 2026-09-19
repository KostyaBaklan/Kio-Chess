using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;

namespace DataAccess.Services.BulkInsert;

public abstract class BulkInsertServiceBase<TEntity> : IBulkInsertService<TEntity> where TEntity : class
{
    public void BulkInsert(SqliteConnection connection, IEnumerable<TEntity> entities)
    {
        if (entities == null)
            return;

        var records = entities.ToList();
        if (records.Count == 0)
            return;

        using var transaction = connection.BeginTransaction();
        try
        {
            string sql = GetSql();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            CreateParameters(command);

            foreach (var record in records)
            {
                SetValues(record, command);

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Bulk insert failed for {typeof(TEntity).Name}: {e.Message}");
            transaction.Rollback();
            throw;
        }
    }

    protected abstract void SetValues(TEntity record, SqliteCommand command);
    protected abstract void CreateParameters(SqliteCommand command);
    protected abstract string GetSql();
}
