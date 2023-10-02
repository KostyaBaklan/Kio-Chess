using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces.Config;
using System.Data;
using System.Text;
using Engine.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

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
            HistoryValue value = new HistoryValue();

            var books = Connection.Books.AsNoTracking()
                .Where(x => x.History == history)
                .Select(x => new { x.NextMove, x.White, x.Draw, x.Black });

            foreach (var book in books)
            {
                value.Add(book.NextMove, book.White, book.Draw, book.Black);
            }

            return value;
        }

        public Task LoadAsync(IBookService bookService)
        {
            _loadTask = Task.Factory.StartNew(() =>
            {
                var items = Connection.Books.AsNoTracking()
                .Where(s => s.History.Length < 2 * _search + 1 && (s.White + s.Black + s.Draw) > _games)
                .Select(s => new { s.History, BookMove = new BookMove { Id = s.NextMove, Value = s.White + s.Black + s.Draw } });

                List<SequenceTotalItem> list = new List<SequenceTotalItem>();
                List<BookMove> open = new List<BookMove>();

                foreach (var item in items)
                {
                    var bytes = item.History;

                    if (bytes.Length == 0)
                    {
                        open.Add(item.BookMove);
                    }
                    else
                    {
                        SequenceTotalItem sequenceTotalItem = new SequenceTotalItem
                        {
                            Seuquence = Encoding.Unicode.GetString(bytes),
                            Move = item.BookMove
                        };

                        list.Add(sequenceTotalItem);
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
            List<DataAccess.Entities.Book> records;
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

        public List<DataAccess.Entities.Book> CreateRecords(int white,int draw, int black)
        {
            List<DataAccess.Entities.Book> records = new List<DataAccess.Entities.Book>(_depth);

            MoveKeyList moveKeyList = stackalloc short[_depth];

            MoveKeyList keyCollection = stackalloc short[_depth];

            _moveHistory.GetSequence(ref moveKeyList);

            records.Add(new DataAccess.Entities.Book
            {
                History = new byte[0],
                NextMove = moveKeyList[0],
                White = white,
                Draw = draw,
                Black = black
            });

            keyCollection.Add(moveKeyList[0]);

            for (byte i = 1; i < moveKeyList.Count; i++)
            {
                keyCollection.Order();

                records.Add(new DataAccess.Entities.Book
                {
                    History = keyCollection.AsByteKey(),
                    NextMove = moveKeyList[i],
                    White = white,
                    Draw = draw,
                    Black = black
                });

                keyCollection.Add(moveKeyList[i]);
            }

            return records;
        }

        public void Upsert(List<DataAccess.Entities.Book> records)
        {
            List<DataAccess.Entities.Book> recordsToAdd = new List<DataAccess.Entities.Book>();
            List<DataAccess.Entities.Book> recordsToUpdate = new List<DataAccess.Entities.Book>();

            foreach (var record in records)
            {
                DataAccess.Entities.Book temp = Connection.Books
                    .FirstOrDefault(b => b.History == record.History && b.NextMove == record.NextMove);

                if (temp == null)
                {
                    Connection.Books.Add(record);
                }
                else
                {
                    temp.White += record.White;
                    temp.Draw += record.Draw;
                    temp.Black += record.Black;
                    Connection.Books.Update(temp);
                }
            }

            Connection.SaveChanges();
        }

        private void UpdateBulk(List<DataAccess.Entities.Book> records)
        {
            Connection.Books.UpdateRange(records);

            Connection.SaveChanges();
        }

        private void InsertBulk(List<DataAccess.Entities.Book> records)
        {
            Connection.Books.AddRange(records);

            Connection.SaveChanges();
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
