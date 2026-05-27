using DataAccess.Entities;
using DataAccess.Models;
using Microsoft.Data.Sqlite;

namespace DataAccess.Helpers
{
    public static class SqlExtensions
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

        public static void Insert(this SqliteConnection connection, IEnumerable<PositionEntity> records)
        {
            using var transaction = connection.BeginTransaction();
            string sql = @"INSERT INTO PositionEntity(Sequence, NextMove, Total) VALUES($S, $M, $T)";

            using var command = connection.CreateCommand(sql);
            try
            {
                command.Parameters.AddWithValue("$S", "");
                command.Parameters.AddWithValue("$M", 0);
                command.Parameters.AddWithValue("$T", 0);

                foreach (var record in records)
                {
                    command.Parameters[0].Value = record.Sequence;
                    command.Parameters[1].Value = record.NextMove;
                    command.Parameters[2].Value = record.Total;

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Transaction failed  {nameof(PositionEntity)} {e}");
                transaction.Rollback();
            }
        }

        public static void Insert(this SqliteConnection connection, IEnumerable<PopularPositionEntity> records)
        {
            using var transaction = connection.BeginTransaction();
            string sql = @"INSERT INTO PopularPositions(HashLow, HashHigh, NextMove, Total, Length) 
                          VALUES($HL, $HH, $M, $T, $L)
                          ON CONFLICT(HashLow, HashHigh, NextMove) DO UPDATE 
                          SET Total = Total + excluded.Total,
                              Length = MIN(Length, excluded.Length)";

            using var command = connection.CreateCommand(sql);
            try
            {
                command.Parameters.AddWithValue("$HL", 0L);
                command.Parameters.AddWithValue("$HH", 0L);
                command.Parameters.AddWithValue("$M", 0);
                command.Parameters.AddWithValue("$T", 0);
                command.Parameters.AddWithValue("$L", 0);

                foreach (var record in records)
                {
                    command.Parameters[0].Value = (long)record.HashLow;
                    command.Parameters[1].Value = (long)record.HashHigh;
                    command.Parameters[2].Value = record.NextMove;
                    command.Parameters[3].Value = record.Total;
                    command.Parameters[4].Value = record.Length;

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Transaction failed  {nameof(PopularPositionEntity)} {e}");
                transaction.Rollback();
            }
        }

        public static void Insert(this SqliteConnection connection, IEnumerable<GameEntity> records)
        {
            using var transaction = connection.BeginTransaction();
            string sql = @"INSERT INTO GameEntities(Low, High, NextMove, White, Draw, Black, Length) 
                          VALUES($L, $H, $M, $W, $D, $B, $LEN)
                          ON CONFLICT(Low, High, NextMove) DO UPDATE 
                          SET White = White + excluded.White,
                              Draw = Draw + excluded.Draw,
                              Black = Black + excluded.Black,
                              Length = MIN(Length, excluded.Length)";

            using var command = connection.CreateCommand(sql);
            try
            {
                command.Parameters.AddWithValue("$L", 0L);
                command.Parameters.AddWithValue("$H", 0L);
                command.Parameters.AddWithValue("$M", 0);
                command.Parameters.AddWithValue("$W", 0);
                command.Parameters.AddWithValue("$D", 0);
                command.Parameters.AddWithValue("$B", 0);
                command.Parameters.AddWithValue("$LEN", 0);

                foreach (var record in records)
                {
                    command.Parameters[0].Value = (long)record.Low;
                    command.Parameters[1].Value = (long)record.High;
                    command.Parameters[2].Value = record.NextMove;
                    command.Parameters[3].Value = record.White;
                    command.Parameters[4].Value = record.Draw;
                    command.Parameters[5].Value = record.Black;
                    command.Parameters[6].Value = record.Length;

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Transaction failed  {nameof(GameEntity)} {e}");
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
