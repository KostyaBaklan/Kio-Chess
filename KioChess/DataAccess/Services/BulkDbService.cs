using DataAccess.Entities;
using DataAccess.Helpers;
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;

namespace DataAccess.Services
{
    public class BulkDbService : IBulkDbService
    {
        private readonly SqliteConnection _connection;

        public BulkDbService()
        {
            _connection= new SqliteConnection("Data Source=C:\\Dev\\ChessDB\\chess.db");
        }
        public void Connect()
        {
            _connection.Open();
        }

        public void Disconnect()
        {
            _connection.Close();
        }

        public void Execute(string sql, int timeout = 30)
        {
            using var command = _connection.CreateCommand(sql);
            command.ExecuteNonQuery();
        }

        public IEnumerable<T> Execute<T>(string sql)
        {
            throw new NotImplementedException();
        }

        public void Upsert(Book[] records)
        {
            using var transaction = _connection.BeginTransaction();
            string sql = @"INSERT INTO Books(History , NextMove, White, Draw, Black) VALUES($H, $M, $W, $D, $B)
                          ON CONFLICT DO UPDATE 
                          SET White = White + excluded.White, Draw = Draw + excluded.Draw, Black = Black + excluded.Black";

            using var command = _connection.CreateCommand(sql);
            try
            {
                command.Parameters.AddWithValue("$H", new byte[0]);
                command.Parameters.AddWithValue("$M", 0);
                command.Parameters.AddWithValue("$W", 0);
                command.Parameters.AddWithValue("$D", 0);
                command.Parameters.AddWithValue("$B", 0);

                for (int i = 0; i < records.Length; i++)
                {
                    command.Parameters[0].Value = records[i].History;
                    command.Parameters[1].Value = records[i].NextMove;
                    command.Parameters[2].Value = records[i].White;
                    command.Parameters[3].Value = records[i].Draw;
                    command.Parameters[4].Value = records[i].Black;

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
    }
}
