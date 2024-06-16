using DataAccess.Entities;
using DataAccess.Models;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace DataAccess.Helpers
{
    public static  class SqlExtensions
    {

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

        public static void Upsert(this SqliteConnection connection, IEnumerable<Book> records)
        {
            using var transaction = connection.BeginTransaction();
            string sql = @"INSERT INTO Books(History , NextMove, White, Draw, Black) VALUES($H, $M, $W, $D, $B)
                          ON CONFLICT DO UPDATE 
                          SET White = White + excluded.White, Draw = Draw + excluded.Draw, Black = Black + excluded.Black";

            using var command = connection.CreateCommand(sql);
            try
            {
                command.Parameters.AddWithValue("$H", new byte[0]);
                command.Parameters.AddWithValue("$M", 0);
                command.Parameters.AddWithValue("$W", 0);
                command.Parameters.AddWithValue("$D", 0);
                command.Parameters.AddWithValue("$B", 0);

                foreach (Book record in records)
                {
                    command.Parameters[0].Value = record.History;
                    command.Parameters[1].Value = record.NextMove;
                    command.Parameters[2].Value = record.White;
                    command.Parameters[3].Value = record.Draw;
                    command.Parameters[4].Value = record.Black;

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Transaction failed {nameof(Book)} {e}");
                transaction.Rollback();
            }
        }

        public static void Upsert(this SqliteConnection connection, IEnumerable<PositionTotal> records)
        {
            using var transaction = connection.BeginTransaction();
            string sql = @"INSERT INTO PositionTotals(History , NextMove, Total) VALUES($H, $M, $T)
                           ON CONFLICT DO UPDATE 
                           SET Total = excluded.Total
                              WHERE excluded.Total > PositionTotals.Total";

            using var command = connection.CreateCommand(sql);
            try
            {
                command.Parameters.AddWithValue("$H", new byte[0]);
                command.Parameters.AddWithValue("$M", 0);
                command.Parameters.AddWithValue("$T", 0);

                foreach (PositionTotal record in records)
                {
                    command.Parameters[0].Value = record.History;
                    command.Parameters[1].Value = record.NextMove;
                    command.Parameters[2].Value = record.Total;

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Transaction failed {nameof(PositionTotal)} {e}");
                transaction.Rollback();
            }
        }

        public static void Insert(this SqliteConnection connection, IEnumerable<PositionTotalDifference> records)
        {
            using var transaction = connection.BeginTransaction();
            string sql = @"INSERT INTO PositionTotalDifference(Sequence , NextMove, Total, Difference) VALUES($S, $M, $T, $D)";

            using var command = connection.CreateCommand(sql);
            try
            {
                command.Parameters.AddWithValue("$S", "");
                command.Parameters.AddWithValue("$M", 0);
                command.Parameters.AddWithValue("$T", 0);
                command.Parameters.AddWithValue("$D", 0);

                foreach (PositionTotalDifference record in records)
                {
                    command.Parameters[0].Value = record.Sequence;
                    command.Parameters[1].Value = record.NextMove;
                    command.Parameters[2].Value = record.Total;
                    command.Parameters[3].Value = record.Difference;

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Transaction failed  {nameof(PositionTotalDifference)} {e}");
                transaction.Rollback();
            }
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
