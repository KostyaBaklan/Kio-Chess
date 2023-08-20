using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace Engine.Book.Services
{
    public class DataAccessService : IDataAccessService
    {
        private readonly int _depth;
        private readonly SqlConnection _connection;
        private readonly IDataKeyService _dataKeyService;

        public DataAccessService(IConfigurationProvider configurationProvider, IDataKeyService dataKeyService)
        {
            _depth = configurationProvider.BookConfiguration.Depth;
            _connection = new SqlConnection(configurationProvider.BookConfiguration.Connection);
            _dataKeyService = dataKeyService;
        }

        public void Connect()
        {
            _connection.Open();
        }

        public void Disconnect()
        {
            _connection.Close();
        }

        public bool Exists(string history, short key)
        {
            string query = "SELECT count(History) FROM [dbo].[Books] WHERE [History] = @History and [NextMove] = @NextMove";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@History", history);
            command.Parameters.AddWithValue("@NextMove", key);

            return (int)command.ExecuteScalar() > 0;
        }

        public HistoryValue Get(string history)
        {
            string query = "SELECT [NextMove] ,[White], [Draw], [Black] FROM [dbo].[Books] WHERE [History] = @History";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@History", history);

            HistoryValue value = new HistoryValue();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    value.Add(reader.GetInt16(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3));
                }
            }

            return value;
        }

        public Task LoadAsync(IBookService bookService)
        {
            return Task.Factory.StartNew(() =>
            {
                string query = "SELECT DISTINCT [History] FROM [ChessData].[dbo].[Books]";

                SqlCommand command = new SqlCommand(query, _connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var history = reader.GetString(0);

                        var historyValue = Get(history);

                        bookService.Add(history, historyValue);
                    }
                }
            });
        }

        public void Export(string file)
        {
            string query = "SELECT [History] ,[NextMove] ,[White] ,[Draw] ,[Black] FROM [ChessData].[dbo].[Books]";

            SqlCommand command = new SqlCommand(query, _connection);

            using (var writter = new StreamWriter(file))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        writter.WriteLine($"INSERT INTO [dbo].[Books] ([History] ,[NextMove] ,[White] ,[Draw] ,[Black]) VALUES ('{reader.GetString(0)}',{reader.GetInt16(1)},{reader.GetInt32(2)},{reader.GetInt32(3)},{reader.GetInt32(4)})");
                    }
                }
            }
        }

        public void AddHistory(IEnumerable<MoveBase> history, GameValue value)
        {
            _dataKeyService.Reset();

            var items = history.Take(_depth).ToArray();

            for (int i = 0; i < items.Length; i++)
            {
                Add(_dataKeyService.Get(), items[i].Key, value);

                _dataKeyService.Add(items[i].Key);
            }
        }

        private void Add(string history, short key, GameValue value)
        {
            if (Exists(history, key))
            {
                Update(history, key, value);
            }
            else
            {
                Insert(history, key, value);
            }
        }

        private void Insert(string history, short key, GameValue value)
        {
            switch (value)
            {
                case GameValue.WhiteWin:
                    InsertWhiteWin(history, key);
                    break;
                case GameValue.BlackWin:
                    InsertBlackWin(history, key);
                    break;
                default:
                    InsertDraw(history, key);
                    break;
            }
        }

        private void InsertDraw(string history, short key)
        {
            string query = "INSERT INTO [dbo].[Books] ([History] ,[NextMove] ,[Draw]) VALUES (@History,@NextMove,1)";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@History", history);
            command.Parameters.AddWithValue("@NextMove", key);

            command.ExecuteNonQuery();
        }

        private void InsertBlackWin(string history, short key)
        {
            string query = "INSERT INTO [dbo].[Books] ([History] ,[NextMove] ,[Black]) VALUES (@History,@NextMove,1)";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@History", history);
            command.Parameters.AddWithValue("@NextMove", key);

            command.ExecuteNonQuery();
        }

        private void InsertWhiteWin(string history, short key)
        {
            string query = "INSERT INTO [dbo].[Books] ([History] ,[NextMove] ,[White]) VALUES (@History,@NextMove,1)";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@History", history);
            command.Parameters.AddWithValue("@NextMove", key);

            command.ExecuteNonQuery();
        }

        private void Update(string history, short key, GameValue value)
        {
            switch (value)
            {
                case GameValue.WhiteWin:
                    UpdateWhiteWin(history, key);
                    break;
                case GameValue.BlackWin:
                    UpdateBlackWin(history, key);
                    break;
                default:
                    UpdateDraw(history, key);
                    break;
            }
        }

        private void UpdateDraw(string history, short key)
        {
            string query = "UPDATE [dbo].[Books] SET [Draw] = [Draw] + 1 WHERE [History] = @History and [NextMove] = @NextMove";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@History", history);
            command.Parameters.AddWithValue("@NextMove", key);

            command.ExecuteNonQuery();
        }

        private void UpdateBlackWin(string history, short key)
        {
            string query = "UPDATE [dbo].[Books] SET [Black] = [Black] + 1 WHERE [History] = @History and [NextMove] = @NextMove";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@History", history);
            command.Parameters.AddWithValue("@NextMove", key);

            command.ExecuteNonQuery();
        }

        private void UpdateWhiteWin(string history, short key)
        {
            string query = "UPDATE [dbo].[Books] SET [White] = [White] + 1 WHERE [History] = @History and [NextMove] = @NextMove";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@History", history);
            command.Parameters.AddWithValue("@NextMove", key);

            command.ExecuteNonQuery();
        }
    }
}
