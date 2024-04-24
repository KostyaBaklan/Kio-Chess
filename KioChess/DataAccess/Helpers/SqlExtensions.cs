using DataAccess.Entities;
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
                Console.WriteLine($"Transaction failed   {e}");
                transaction.Rollback();
            }
        }

        //public static void Upsert(this SqliteConnection connection, IEnumerable<PositionTotal> records)
        //{
        //    using var transaction = connection.BeginTransaction();
        //    string sql = @"INSERT INTO PositionTotals(History , NextMove, Total) VALUES($H, $M, $T)
        //                   ON CONFLICT DO UPDATE 
        //                   SET Total = excluded.Total
        //                      WHERE excluded.Total > PositionTotals.Total";

        //    using var command = connection.CreateCommand(sql);
        //    try
        //    {
        //        command.Parameters.AddWithValue("$H", new byte[0]);
        //        command.Parameters.AddWithValue("$M", 0);
        //        command.Parameters.AddWithValue("$T", 0);

        //        foreach (PositionTotal record in records)
        //        {
        //            command.Parameters[0].Value = record.History;
        //            command.Parameters[1].Value = record.NextMove;
        //            command.Parameters[2].Value = record.Total;

        //            command.ExecuteNonQuery();
        //        }

        //        transaction.Commit();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"Transaction failed   {e}");
        //        transaction.Rollback();
        //    }
        //}
    }
}
