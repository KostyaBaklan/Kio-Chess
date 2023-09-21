using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Sorting.Comparers;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.SqlTypes;
using System.Net;
using System.Text;

namespace Engine.Book.Services
{
    public class DataAccessService : IDataAccessService
    {
        private readonly int _depth;
        private readonly int _threshold;
        private readonly short _games;
        private readonly SqlConnection _connection;
        private readonly IDataKeyService _dataKeyService;
        private readonly IMoveHistoryService _moveHistory;
        private Task _loadTask;

        public DataAccessService(IConfigurationProvider configurationProvider, IDataKeyService dataKeyService,
            IMoveHistoryService moveHistory)
        {
            _depth = configurationProvider.BookConfiguration.SaveDepth;
            _threshold = configurationProvider.BookConfiguration.SuggestedThreshold;
            _games = configurationProvider.BookConfiguration.GamesThreshold;
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

        public HistoryValue Get(byte[] history)
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

        public HashSet<string> GetOpeningNames()
        {
            string query = "SELECT [Name] FROM [dbo].[OpeningList] WHERE [Variation] = ''";

            SqlCommand command = new SqlCommand(query, _connection);

            HashSet<string> result = new HashSet<string>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
            }

            return result;
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
                                      WHERE ABS([White]-[Black]) > @Threshold or ([White]+[Draw]+[Black]) > @Games";

                SqlCommand command = new SqlCommand(query, _connection);
                
                command.Parameters.AddWithValue("@Threshold", _threshold);
                command.Parameters.AddWithValue("@Games", _games);

                List<HistoryItem> list = new List<HistoryItem>();
                List<HistoryItem> open = new List<HistoryItem>();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SqlBytes history = reader.GetSqlBytes(0);

                        var bytes = history.Value;

                        if (bytes.Length == 0)
                        {
                            HistoryItem op = new HistoryItem
                            {
                                History = string.Empty,
                                Key = reader.GetInt16(1),
                                White = reader.GetInt32(2),
                                Draw = reader.GetInt32(3),
                                Black = reader.GetInt32(4)
                            };

                            open.Add(op);
                        }
                        else
                        {
                            HistoryItem item = new HistoryItem
                            {
                                History = Encoding.Unicode.GetString(bytes),
                                Key = reader.GetInt16(1),
                                White = reader.GetInt32(2),
                                Draw = reader.GetInt32(3),
                                Black = reader.GetInt32(4)
                            };

                            list.Add(item);
                        }
                    }
                }

                bookService.SetOpening(open);

                foreach(var item in list.GroupBy(l => l.History))
                {
                    bookService.Add(item.Key, GetBook(item));
                }
            });

            return _loadTask;
        }

        private BookMoves GetBook(IGrouping<string, HistoryItem> l)
        {
            List<KeyValuePair<short, BookValue>> list = l.Select(g=>new KeyValuePair<short, BookValue>(g.Key, new BookValue { White = g.White, Black = g.Black, Draw = g.Draw }))
                .ToList();

            if (l.Key.Length % 2 == 1)
            {
                return GetBlackBook(list);
            }
            else
            {
                return GetWhiteBook(list);
            }
        }

        private BookMoves GetBlackBook(List<KeyValuePair<short, BookValue>> list)
        {
            BookMoves book = new BookMoves();

            var totals = list.Where(l => l.Value.GetTotal() > _games)
                            .OrderByDescending(b => b.Value.GetTotal())
                            .ToList();

            BookMove total;
            if (totals.Any())
            {
                var t = totals[0];
                total = new BookMove { Id = t.Key, Value = t.Value.GetBlack()};
                book.SetTotal(total);
            }
            else
            {
                total = new BookMove { Id = -1, Value = 0 };
            }

            BlackBookValueComparer comparer = new BlackBookValueComparer();

            var maxs = total.Id < 0
                ? list.Where(l => l.Value.GetBlack() > _threshold)
                    .OrderByDescending(l => l.Value, comparer)
                    .Select(l => new BookMove { Id = l.Key, Value = l.Value.GetBlack() })
                    .ToList()
                : list.Where(l => l.Key != total.Id && l.Value.GetBlack() > _threshold)
                    .OrderByDescending(l => l.Value, comparer)
                    .Select(l => new BookMove { Id = l.Key, Value = l.Value.GetBlack() })
                    .ToList();

            if (maxs.Any())
            {
                book.SetMax(maxs[0]);
            }

            var mins = total.Id < 0
                ? list.Where(l => l.Value.GetBlack() < -_threshold)
                    .OrderBy(l => l.Value, comparer)
                    .Select(l => new BookMove { Id = l.Key, Value = l.Value.GetBlack() })
                    .ToList()
                : list.Where(l => l.Key != total.Id && l.Value.GetBlack() < -_threshold)
                    .OrderBy(l => l.Value, comparer)
                    .Select(l => new BookMove { Id = l.Key, Value = l.Value.GetBlack() })
                    .ToList();

            if (mins.Any())
            {
                book.SetMin(mins[0]);
            }

            return book;
        }

        private BookMoves GetWhiteBook(List<KeyValuePair<short, BookValue>> list)
        {
            BookMoves book = new BookMoves(); 
            
            var totals = list.Where(l => l.Value.GetTotal() > _games)
                            .OrderByDescending(b => b.Value.GetTotal())
                            .ToList();

            BookMove total;
            if (totals.Any())
            {
                var t = totals[0];
                total = new BookMove { Id = t.Key, Value = t.Value.GetWhite() };
                book.SetTotal(total);
            }
            else
            {
                total = new BookMove { Id = -1, Value = 0 };
            }

            var comparer = new WhiteBookValueComparer();

            var maxs = total.Id < 0
                ? list.Where(l => l.Value.GetWhite() > _threshold)
                    .OrderByDescending(l => l.Value, comparer)
                    .Select(l => new BookMove { Id = l.Key, Value = l.Value.GetWhite() })
                    .ToList()
                : list.Where(l => l.Key != total.Id && l.Value.GetWhite() > _threshold)
                    .OrderByDescending(l => l.Value, comparer)
                    .Select(l => new BookMove { Id = l.Key, Value = l.Value.GetWhite() })
                    .ToList();

            if (maxs.Any())
            {
                book.SetMax(maxs[0]);
            }

            var mins = total.Id < 0
                ? list.Where(l => l.Value.GetWhite() < -_threshold)
                    .OrderBy(l => l.Value, comparer)
                    .Select(l => new BookMove { Id = l.Key, Value = l.Value.GetWhite() })
                    .ToList()
                : list.Where(l => l.Key != total.Id && l.Value.GetWhite() < -_threshold)
                    .OrderBy(l => l.Value, comparer)
                    .Select(l => new BookMove { Id = l.Key, Value = l.Value.GetWhite() })
                    .ToList();

            if (mins.Any())
            {
                book.SetMin(mins[0]);
            }

            return book;
        }

        public void Clear()
        {
            Execute(@"delete from [ChessData].[dbo].[Books]",300);
        }

        public void Execute(string sql, int timeout = 30)
        {
            SqlCommand command = new SqlCommand(sql, _connection) { CommandTimeout = timeout};

            command.ExecuteNonQuery();
        }

        public void Execute(string sql, string[] names, object[] values)
        {
            SqlCommand command = new SqlCommand(sql, _connection);

            for (int i = 0; i < names.Length; i++)
            {
                command.Parameters.AddWithValue(names[i], values[i]);
            }

            command.ExecuteNonQuery();
        }

        public IEnumerable<T> Execute<T>(string query, Func<SqlDataReader, T> factory)
        {
            SqlCommand command = new SqlCommand(query, _connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return factory(reader);
                }
            }
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

        public string GetOpeningName(string key)
        {
            string query = @"SELECT ov.[Name]
                              FROM [dbo].[OpeningSequences] os INNER JOIN [dbo].[OpeningVariations] ov ON os.[OpeningVariationID] = ov.[ID]
                              WHERE os.[Sequence] = @Sequence";

            SqlCommand command = new SqlCommand(query, _connection);
            command.Parameters.AddWithValue("@Sequence", key);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetString(0);
                }
            }

            return string.Empty;
        }

        public int GetOpeningVariationID(string key)
        {
            string query = @"SELECT [OpeningVariationID] FROM [dbo].[OpeningSequences] WHERE [Sequence] = @Sequence";

            SqlCommand command = new SqlCommand(query, _connection);
            command.Parameters.AddWithValue("@Sequence", key);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }

            return 0;
        }

        public void SaveOpening(string key, int id)
        {
            string query = @"INSERT INTO [dbo].[OpeningSequences] ([Sequence] ,[OpeningVariationID]) VALUES  (@Sequence, @OpeningVariationID)";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@OpeningVariationID", id);
            command.Parameters.AddWithValue("@Sequence", key);

            command.ExecuteNonQuery();
        }

    private bool Exists(string opening, string variation)
        {
            string query = @"SELECT COUNT([ID]) FROM [dbo].[OpeningList] WHERE [Name] = @Name AND [Variation] = @Variation";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@Name", opening);
            command.Parameters.AddWithValue("@Variation", variation);

            return (int)command.ExecuteScalar() > 0;
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

            table.Rows.Add(new byte[0], moveKeyList[0]);

            keyCollection.Add(moveKeyList[0]);

            for (byte i = 1; i < moveKeyList.Count; i++)
            {
                keyCollection.Order();

                table.Rows.Add(keyCollection.AsByteKey(), moveKeyList[i]);

                keyCollection.Add(moveKeyList[i]);
            }

            return table;
        }

        private static DataTable CreateDataTable()
        {
            DataTable table = new DataTable();

            table.Columns.Add("History", typeof(byte[]));
            table.Columns.Add("NextMove", typeof(short));

            return table;
        }

        private void InsertName(IEnumerable<string> names, string query)
        {
            foreach (var name in names)
            {
                try
                {
                    SqlCommand command = new SqlCommand(query, _connection);

                    command.Parameters.AddWithValue("@Name", name);

                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine($"{name} --- {e}");
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }

        public void AddOpening(IEnumerable<string> names)
        {
            var query = $"INSERT INTO [dbo].[Openings] ([Name]) VALUES (@Name)";

            InsertName(names, query);
        }

        public void AddVariations(IEnumerable<string> names)
        {
            var query = $"INSERT INTO [dbo].[Variations] ([Name]) VALUES (@Name)";

            InsertName(names, query);
        }

        public short GetOpeningID(string name)
        {
            string query = @"SELECT [ID] FROM [dbo].[Openings] WHERE [Name] = @Name";

            return GetIdByName(name, query);
        }

        public short GetVariationID(string name)
        {
            string query = @"SELECT [ID] FROM [dbo].[Variations] WHERE [Name] = @Name";

            return GetIdByName(name, query);
        }

        private short GetIdByName(string name, string query)
        {
            SqlCommand command = new SqlCommand(query, _connection);
            command.Parameters.AddWithValue("@Name", name);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetInt16(0);
                }
            }

            return -1;
        }

        public bool AddOpeningVariation(string name, short openingID, short variationID, List<string> moves)
        {
            if (!OpeningVariationExists(name))
            {
                string query = @"INSERT INTO [dbo].[OpeningVariations] ([Name] ,[OpeningID] ,[VariationID] ,[Moves])
                             VALUES (@Name ,@OpeningID ,@VariationID ,@Moves)";

                SqlCommand command = new SqlCommand(query, _connection);

                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@OpeningID", openingID);
                command.Parameters.AddWithValue("@VariationID", variationID);
                command.Parameters.AddWithValue("@Moves", string.Join(' ', moves));

                command.ExecuteNonQuery();

                return true;
            }

            return false;
        }

        private bool OpeningVariationExists(string name)
        {
            string query = "SELECT COUNT([ID]) FROM [dbo].[OpeningVariations] WHERE [Name] = @Name";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@Name", name);

            return (int)command.ExecuteScalar() > 0;
        }

        public List<KeyValuePair<int, string>> GetSequences(string filter)
        {
            string query;

            if (string.IsNullOrWhiteSpace(filter))
            {
                query = "SELECT [ID] ,[Moves] FROM [dbo].[OpeningVariations] Order BY LEN ([Moves])";
            }
            else
            {
                query = $"SELECT [ID] ,[Moves] FROM [dbo].[OpeningVariations] WHERE {filter} Order BY LEN ([Moves])";
            }


            SqlCommand command = new SqlCommand(query, _connection);

            List < KeyValuePair<int, string> > values = new List<KeyValuePair<int, string>>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    values.Add(new KeyValuePair<int, string>(reader.GetInt32(0), reader.GetString(1)));
                }
            }

            return values;
        }

        public bool IsOpeningVariationExists(short openingID, short variationID)
        {
            string query = "SELECT COUNT([ID]) FROM [dbo].[OpeningVariations] WHERE [OpeningID] = @OpeningID AND [VariationID] = @VariationID";

            SqlCommand command = new SqlCommand(query, _connection);

            command.Parameters.AddWithValue("@OpeningID", openingID);
            command.Parameters.AddWithValue("@VariationID", variationID);

            return (int)command.ExecuteScalar() > 0;
        }

        public List<HashSet<string>> GetSequences(int size)
        {
            string query = @"SELECT [Moves] FROM [dbo].[OpeningVariations]";

            SqlCommand command = new SqlCommand(query, _connection);

            List<HashSet<string>> values = new List<HashSet<string>>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var result = reader.GetString(0);

                    var set = result.Split(' ').ToHashSet();
                    if(set.Count == size)
                    {
                        values.Add(set);
                    }
                }
            }

            return values;
        }

        public HashSet<string> GetSequenceSets()
        {
            string query = @"SELECT [Moves] FROM [dbo].[OpeningVariations]";

            SqlCommand command = new SqlCommand(query, _connection);

            HashSet<string> values = new HashSet<string>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    values.Add(reader.GetString(0));
                }
            }

            return values;
        }

        public HashSet<string> GetSequenceKeys()
        {
            string query = @"SELECT [Sequence] FROM [dbo].[OpeningSequences]";

            SqlCommand command = new SqlCommand(query, _connection);

            HashSet<string> values = new HashSet<string>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    values.Add(reader.GetString(0));
                }
            }

            return values;
        }

        public string GetMoves(string name)
        {
            string query = @"SELECT [Moves] FROM [dbo].[OpeningVariations] WHERE [Name] = @Name";

            SqlCommand command = new SqlCommand(query, _connection);
            command.Parameters.AddWithValue("@Name", name);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetString(0);
                }
            }

            return string.Empty;
        }
    }
}
