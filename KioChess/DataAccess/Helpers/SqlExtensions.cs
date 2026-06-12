using Microsoft.Data.Sqlite;

namespace DataAccess.Helpers
{
    public static class SqlExtensions
    {
        /// <summary>
        /// Generic bulk insert extension that delegates to entity-specific bulk insert services
        /// </summary>
        public static void Insert<TEntity>(this SqliteConnection connection, IEnumerable<TEntity> entities)
            where TEntity : class
        {
            var service = BulkInsertServiceRegistry.Get<TEntity>();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            service.BulkInsert(connection, entities);
        }

        public static void SetCommand(this SqliteCommand command, string sql, List<SqliteParameter> parameters, int timeout)
        {
            command.CommandText = sql;
            command.CommandTimeout = timeout;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    if (parameter != null)
                    {
                        command.Parameters.Add(parameter);
                    }
                }
            }
        }
        public static SqliteCommand CreateCommand(this SqliteConnection connection, string sql)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            return command;
        }

        public static int Execute(this SqliteConnection connection, string sql, List<SqliteParameter> parameters = null, int timeout = 30)
        {
            connection.Open();

            using var command = connection.CreateCommand();
            command.SetCommand(sql, parameters, timeout);

            return command.ExecuteNonQuery();
        }

        public static IEnumerable<T> Execute<T>(this SqliteConnection connection, string sql, Func<SqliteDataReader, T> factoy, List<SqliteParameter> parameters = null, int timeout = 60)
        {
            connection.Open();

            using var command = connection.CreateCommand();
            command.SetCommand(sql, parameters, timeout);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                yield return factoy(reader);
            }
        }
    }
}
