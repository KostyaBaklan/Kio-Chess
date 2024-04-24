using DataAccess.Entities;
using DataAccess.Helpers;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;

namespace DataAccess.Services
{
    public class BulkDbService : LiteDbServiceBase, IBulkDbService
    {
        public BulkDbService()
        {
            _connection = new SqliteConnection("Data Source=C:\\Dev\\ChessDB\\chess.db");
        }

        public override void Connect() => _connection.Open();

        public void AddRange(IEnumerable<PopularPosition> records)
        {
            using var transaction = _connection.BeginTransaction();
            string sql = @"INSERT INTO PopularPositions(History , NextMove, Value) VALUES($H, $M, $T)";

            using var command = _connection.CreateCommand(sql);
            try
            {
                command.Parameters.AddWithValue("$H", string.Empty);
                command.Parameters.AddWithValue("$M", 0);
                command.Parameters.AddWithValue("$T", 0);

                foreach (PopularPosition record in records)
                {
                    command.Parameters[0].Value = record.History;
                    command.Parameters[1].Value = record.NextMove;
                    command.Parameters[2].Value = record.Value;

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

        public void AddRange(IEnumerable<VeryPopularPosition> records)
        {
            using var transaction = _connection.BeginTransaction();
            string sql = @"INSERT INTO VeryPopularPositions(History , NextMove, Value) VALUES($H, $M, $T)";

            using var command = _connection.CreateCommand(sql);
            try
            {
                command.Parameters.AddWithValue("$H", string.Empty);
                command.Parameters.AddWithValue("$M", 0);
                command.Parameters.AddWithValue("$T", 0);

                foreach (VeryPopularPosition record in records)
                {
                    command.Parameters[0].Value = record.History;
                    command.Parameters[1].Value = record.NextMove;
                    command.Parameters[2].Value = record.Value;

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

        public void Upsert(IEnumerable<PositionTotal> item) => _connection.Upsert(item);
    }
}
