using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces.Config;
using System.Data;
using System.Text;
using Engine.Interfaces;
using Microsoft.Data.Sqlite;
using Engine.Book.Helpers;

namespace Engine.Book.Services
{
    public class GameDbService : DbServiceBase, IGameDbService
    {
        private readonly int _depth;
        private readonly int _search;
        private readonly short _games;

        private Task _loadTask;

        private readonly IMoveHistoryService _moveHistory;

        public GameDbService(IConfigurationProvider configurationProvider, IMoveHistoryService moveHistory) : base(configurationProvider)
        {
            _depth = configurationProvider.BookConfiguration.SaveDepth;
            _search = configurationProvider.BookConfiguration.SearchDepth;
            _games = configurationProvider.BookConfiguration.GamesThreshold;
            _moveHistory = moveHistory;
        }

        public HistoryValue Get(byte[] history)
        {
            string query = "SELECT NextMove ,White, Draw, Black FROM Books WHERE History = @History";

            using (var command = Connection.CreateCommand(query))
            {
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
        }

        public Task LoadAsync(IBookService bookService)
        {
            _loadTask = Task.Factory.StartNew(() =>
            {
                string query = @"SELECT History, NextMove, (White + Draw + Black) AS TotalGames
                       FROM Books
                       WHERE length(History) < @MaxSequence and TotalGames > @Games";

                List<SequenceTotalItem> list = new List<SequenceTotalItem>();
                List<BookMove> open = new List<BookMove>();

                using (var command = Connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@Games", _games);
                    command.Parameters.AddWithValue("@MaxSequence", 2 * _search + 1);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var bytes = reader.GetFieldValue<byte[]>(0);

                            if (bytes.Length == 0)
                            {
                                BookMove op = new BookMove
                                {
                                    Id = reader.GetInt16(1),
                                    Value = reader.GetInt32(2)
                                };

                                open.Add(op);
                            }
                            else
                            {
                                SequenceTotalItem item = new SequenceTotalItem
                                {
                                    Seuquence = Encoding.Unicode.GetString(bytes),
                                    Move = new BookMove { Id = reader.GetInt16(1), Value = reader.GetInt32(2) }
                                };

                                list.Add(item);
                            }
                        }
                    }
                }

                bookService.SetOpening(open);

                foreach (var item in list.GroupBy(l => l.Seuquence, v => v.Move))
                {
                    IPopularMoves bookMoves = GetMaxMoves(item);
                    bookService.Add(item.Key, bookMoves);
                }
            });

            return _loadTask;
        }

        public void UpdateHistory(GameValue value)
        {
            List<HistoryRecord> records;
            switch (value)
            {
                case GameValue.WhiteWin:
                    records = CreateRecords(1, 0, 0);
                    break;
                case GameValue.BlackWin:
                    records = CreateRecords(0, 0, 1);
                    break;
                default:
                    records = CreateRecords(0, 1, 0);
                    break;
            }

            Upsert(records);
        }

        private List<HistoryRecord> CreateRecords(int white,int draw, int black)
        {
            List<HistoryRecord> records = new List<HistoryRecord>(_depth);

            MoveKeyList moveKeyList = stackalloc short[_depth];

            MoveKeyList keyCollection = stackalloc short[_depth];

            _moveHistory.GetSequence(ref moveKeyList);

            records.Add(new HistoryRecord
            {
                Sequence = new byte[0],
                Move = moveKeyList[0],
                White = white,
                Draw = draw,
                Black = black
            });

            keyCollection.Add(moveKeyList[0]);

            for (byte i = 1; i < moveKeyList.Count; i++)
            {
                keyCollection.Order();

                records.Add(new HistoryRecord
                {
                    Sequence = keyCollection.AsByteKey(),
                    Move = moveKeyList[i],
                    White = white,
                    Draw = draw,
                    Black = black
                });

                keyCollection.Add(moveKeyList[i]);
            }

            return records;
        }

        private void Upsert(List<HistoryRecord> records)
        {
            List<HistoryRecord> recordsToAdd = new List<HistoryRecord>();
            List<HistoryRecord> recordsToUpdate = new List<HistoryRecord>();

            foreach (var record in records)
            {
                HistoryRecord temp = Get(record.Sequence, record.Move);
                if (temp == null)
                {
                    recordsToAdd.Add(record);
                }
                else
                {
                    record.White += temp.White;
                    record.Draw += temp.Draw;
                    record.Black += temp.Black;
                    recordsToUpdate.Add(record);
                }
            }

            InsertBulk(recordsToAdd);

            UpdateBulk(recordsToUpdate);
        }

        private void UpdateBulk(List<HistoryRecord> records)
        {
            using (var transaction = Connection.BeginTransaction())
            {
                var insert = @"UPDATE Books SET White = $W, Draw = $D, Black = $B WHERE History = $H AND NextMove = $M";

                using (var command = Connection.CreateCommand(insert))
                {
                    command.Parameters.Add(new SqliteParameter("$H", new byte[0]));
                    command.Parameters.Add(new SqliteParameter("$M", 0));
                    command.Parameters.Add(new SqliteParameter("$W", 0));
                    command.Parameters.Add(new SqliteParameter("$D", 0));
                    command.Parameters.Add(new SqliteParameter("$B", 0));

                    foreach (var item in records)
                    {
                        command.Parameters[0].Value = item.Sequence;
                        command.Parameters[1].Value = item.Move;
                        command.Parameters[2].Value = item.White;
                        command.Parameters[3].Value = item.Draw;
                        command.Parameters[4].Value = item.Black;

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        private void InsertBulk(List<HistoryRecord> records)
        {
            using (var transaction = Connection.BeginTransaction())
            {
                var insert = @"INSERT INTO Books VALUES ($H,$M,$W,$D,$B)";

                using (var command = Connection.CreateCommand(insert))
                {
                    command.Parameters.Add(new SqliteParameter("$H", new byte[0]));
                    command.Parameters.Add(new SqliteParameter("$M", 0));
                    command.Parameters.Add(new SqliteParameter("$W", 0));
                    command.Parameters.Add(new SqliteParameter("$D", 0));
                    command.Parameters.Add(new SqliteParameter("$B", 0));

                    foreach (var item in records)
                    {
                        command.Parameters[0].Value = item.Sequence;
                        command.Parameters[1].Value = item.Move;
                        command.Parameters[2].Value = item.White;
                        command.Parameters[3].Value = item.Draw;
                        command.Parameters[4].Value = item.Black;

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        private HistoryRecord Get(byte[] sequence, short move)
        {
            var sql = @"SELECT History, NextMove, White, Draw, Black
                       FROM Books
                       WHERE History = $H AND NextMove = $M";

            using (var command = Connection.CreateCommand(sql))
            {
                command.Parameters.Add(new SqliteParameter("$H", sequence));
                command.Parameters.Add(new SqliteParameter("$M", move));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        HistoryRecord record = new HistoryRecord
                        {
                            Sequence = reader.GetFieldValue<byte[]>(0),
                            Move = reader.GetInt16(1),
                            White = reader.GetInt32(2),
                            Draw = reader.GetInt32(3),
                            Black = reader.GetInt32(4)
                        };

                        return record;
                    }

                    return null;
                }
            }
        }

        public void WaitToData()
        {
            if (!_loadTask.IsCompleted)
            {
                _loadTask.Wait();
            }
        }

        private IPopularMoves GetMaxMoves(IGrouping<string, BookMove> item)
        {
            var moves = item.OrderByDescending(i => i.Value).Take(3).ToArray();

            return moves.Length switch
            {
                3 => new PopularMoves3(moves),
                2 => new PopularMoves2(moves),
                1 => new PopularMoves1(moves),
                _ => new PopularMoves0(),
            };
        }
    }
}
