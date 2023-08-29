using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Microsoft.Data.SqlClient;
using System.Collections.Immutable;
using System.Data;
using System.Net;

namespace Engine.Book.Services
{
    public class DataAccessService : IDataAccessService
    {
        private readonly int _depth;
        private readonly int _threshold;
        private readonly SqlConnection _connection;
        private readonly IDataKeyService _dataKeyService;
        private readonly IMoveHistoryService _moveHistory;
        private Task _loadTask;

        public DataAccessService(IConfigurationProvider configurationProvider, IDataKeyService dataKeyService,
            IMoveHistoryService moveHistory)
        {
            _depth = configurationProvider.BookConfiguration.Depth;
            _threshold = configurationProvider.BookConfiguration.SuggestedThreshold;
            var hostname = Dns.GetHostName();
            var connection = configurationProvider.BookConfiguration.Connection[hostname];
            _connection = new SqlConnection(connection);
            _dataKeyService = dataKeyService;
            _moveHistory = moveHistory;
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
            string query = "SELECT count(History) FROM [dbo].[Books] WITH (NOLOCK) WHERE [History] = @History and [NextMove] = @NextMove";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@History", history);
            command.Parameters.AddWithValue("@NextMove", key);

            return (int)command.ExecuteScalar() > 0;
        }

        public HistoryValue Get(string history)
        {
            string query = "SELECT [NextMove] ,[White], [Draw], [Black] FROM [dbo].[Books] WITH (NOLOCK) WHERE [History] = @History";

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
                string query = @"  SELECT [History]
                                          ,[NextMove]
                                          ,[White]
                                          ,[Draw]
                                          ,[Black]
                                      FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
                                      WHERE ABS([White]-[Black]) > @Threshold or ABS([Black]-[White]) > @Threshold";

                SqlCommand command = new SqlCommand(query, _connection);
                
                command.Parameters.AddWithValue("@Threshold", _threshold);

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

            historyValue.Sort();

            return historyValue;
        }

        public void Clear()
        {
            Execute(@"delete from [ChessData].[dbo].[Books]");
        }

        public void Execute(string sql)
        {
            SqlCommand command = new SqlCommand(sql, _connection);

            command.ExecuteNonQuery();
        }

        public void Export(string file)
        {
            string query = "SELECT [History] ,[NextMove] ,[White] ,[Draw] ,[Black] FROM [ChessData].[dbo].[Books] WITH (NOLOCK)";

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
        public void UpdateHistory(GameValue value)
        {
            switch (value)
            {
                case GameValue.WhiteWin:
                    UpdateWhiteWinBulk();
                    break;
                case GameValue.Draw:
                    UpdateDrawBulk();
                    break;
                default:
                    UpdateBlackWinBulk();
                    break;
            }
        }

        private void UpdateWhiteWinBulk()
        {
            DataTable table = SetTable();

            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "dbo.UpsertWhite";
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter parameter = command.Parameters.AddWithValue("@UpdateWhite", table);  // See implementation below
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "dbo.BooksTableType";

                command.ExecuteNonQuery();
            }
        }

        private void UpdateDrawBulk()
        {
            DataTable table = SetTable();

            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "dbo.UpsertDraw";
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter parameter = command.Parameters.AddWithValue("@UpdateDraw", table);  // See implementation below
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "dbo.BooksTableType";

                command.ExecuteNonQuery();
            }
        }

        private void UpdateBlackWinBulk()
        {
            DataTable table = SetTable();

            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "dbo.UpsertBlack";
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter parameter = command.Parameters.AddWithValue("@UpdateBlack", table);  // See implementation below
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "dbo.BooksTableType";

                command.ExecuteNonQuery();
            }
        }

        private DataTable SetTable()
        {
            MoveKeyList moveKeyList = stackalloc short[_depth];

            MoveKeyList keyCollection = stackalloc short[_depth];

            _moveHistory.GetSequence(ref moveKeyList);

            DataTable table = CreateDataTable();

            table.Rows.Add(string.Empty, moveKeyList[0]);

            keyCollection.Add(moveKeyList[0]);

            for (byte i = 1; i < moveKeyList.Count; i++)
            {
                keyCollection.Order();

                table.Rows.Add(keyCollection.AsKey(), moveKeyList[i]);

                keyCollection.Add(moveKeyList[i]);
            }

            return table;
        }

        public void AddHistory(GameValue value)
        {
            MoveKeyList moveKeyList = stackalloc short[_depth];

            MoveKeyList keyCollection = stackalloc short[_depth];

            _moveHistory.GetSequence(ref moveKeyList);

            Upsert(string.Empty, moveKeyList[0], value);

            keyCollection.Add(moveKeyList[0]);

            for (byte i = 1; i < moveKeyList.Count; i++)
            {
                keyCollection.Order();

                Upsert(keyCollection.AsKey(), moveKeyList[i], value);

                keyCollection.Add(moveKeyList[i]);
            }
        }

        private static DataTable CreateDataTable()
        {
            DataTable table = new DataTable();

            table.Columns.Add("History", typeof(string));
            table.Columns.Add("NextMove", typeof(short));

            return table;
        }

        public void Upsert(string history, short key, GameValue value)
        {
            switch (value)
            {
                case GameValue.WhiteWin:
                    UpsertWhiteWin(history, key);
                    break;
                case GameValue.BlackWin:
                    UpsertBlackWin(history, key);
                    break;
                default:
                    UpsertDraw(history, key);
                    break;
            }
        }

        private void Upsert(string history, short key, string query)
        {
            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@History", history);
            command.Parameters.AddWithValue("@NextMove", key);

            command.ExecuteNonQuery();
        }

        private void UpsertWhiteWin(string history, short key)
        {
            string query = @"begin tran
                            if exists (select [NextMove] from [dbo].[Books] with (updlock,serializable) WHERE [History] = @History and [NextMove] = @NextMove)
                            begin
                               UPDATE [dbo].[Books] SET [White] = [White] + 1 WHERE [History] = @History and [NextMove] = @NextMove
                            end
                            else
                            begin
                               INSERT INTO [dbo].[Books] ([History] ,[NextMove] ,[White]) VALUES (@History,@NextMove,1)
                            end
                            commit tran";

            Upsert(history, key, query);
        }

        private void UpsertBlackWin(string history, short key)
        {
            string query = @"begin tran
                            if exists (select [NextMove] from [dbo].[Books] with (updlock,serializable) WHERE [History] = @History and [NextMove] = @NextMove)
                            begin
                               UPDATE [dbo].[Books] SET [Black] = [Black] + 1 WHERE [History] = @History and [NextMove] = @NextMove
                            end
                            else
                            begin
                               INSERT INTO [dbo].[Books] ([History] ,[NextMove] ,[Black]) VALUES (@History,@NextMove,1)
                            end
                            commit tran";

            Upsert(history, key, query);
        }

        private void UpsertDraw(string history, short key)
        {
            string query = @"begin tran
                            if exists (select [NextMove] from [dbo].[Books] with (updlock,serializable) WHERE [History] = @History and [NextMove] = @NextMove)
                            begin
                               UPDATE [dbo].[Books] SET [Draw] = [Draw] + 1 WHERE [History] = @History and [NextMove] = @NextMove
                            end
                            else
                            begin
                               INSERT INTO [dbo].[Books] ([History] ,[NextMove] ,[Draw]) VALUES (@History,@NextMove,1)
                            end
                            commit tran";

            Upsert(history, key, query);
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
