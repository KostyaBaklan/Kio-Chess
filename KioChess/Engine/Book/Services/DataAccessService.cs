using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using Microsoft.Data.SqlClient;

namespace Engine.Book.Services
{
    public class DataAccessService : IDataAccessService
    {
        private readonly int _depth;
        private readonly SqlConnection _connection;
        private readonly IDataKeyService _dataKeyService;
        private Task _loadTask;

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

        public void WaitToData()
        {
            if (!_loadTask.IsCompleted)
            {
                _loadTask.Wait();
            }
        }

        public Task LoadAsync(IBookService bookService)
        {
            _loadTask = Task.Factory.StartNew(() =>
            {
                string query = "SELECT [History] ,[NextMove] ,[White] ,[Draw] ,[Black] FROM [ChessData].[dbo].[Books]";

                SqlCommand command = new SqlCommand(query, _connection);

                List<HistoryItem> list = new List<HistoryItem>();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HistoryItem item = new HistoryItem
                        {
                            History = reader.GetString(0),
                            Key = reader.GetInt16(1),
                            White = reader.GetInt32(2),
                            Draw = reader.GetInt32(3),
                            Black = reader.GetInt32(4)
                        };

                        list.Add(item);
                    }
                }

                foreach(var item in list.GroupBy(l => l.History))
                {
                    bookService.Add(item.Key, GetValue(item));
                }
            });

            return _loadTask;
        }

        private HistoryValue GetValue(IGrouping<string, HistoryItem> l)
        {
            HistoryValue historyValue = new HistoryValue();

            foreach(var item in l.Select(x => x))
            {
                historyValue.Add(item.Key, item.White, item.Draw, item.Black);
            }

            return historyValue;
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
