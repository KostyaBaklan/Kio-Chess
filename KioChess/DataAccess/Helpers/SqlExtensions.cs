using Microsoft.Data.Sqlite;

namespace DataAccess.Helpers
{
    public static  class SqlExtensions
    {
        public static SqliteCommand CreateCommand(this SqliteConnection connection, string sql)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            return command;
        }
    }
}
