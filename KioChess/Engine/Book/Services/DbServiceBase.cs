using Engine.Book.Interfaces;
using Engine.Interfaces.Config;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace Engine.Book.Services
{
    public abstract class DbServiceBase : IDbService
    {
        protected SqliteConnection Connection;

        protected DbServiceBase(IConfigurationProvider configuration)
        {
            Connection = new SqliteConnection(@"Data Source=C:\Dev\ChessDB\chess.db");
        }
        public void Connect()
        {
            Connection.Open();
        }

        public void Disconnect()
        {
            Connection.Close();
        }

        public void Execute(string sql, int timeout = 30)
        {
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = timeout;

            command.ExecuteNonQuery();
        }

        public IEnumerable<T> Execute<T>(string sql, Func<DbDataReader, T> factory)
        {
            using var command = Connection.CreateCommand();
            command.CommandText = sql;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                yield return factory(reader);
            }
        }
    }
}
