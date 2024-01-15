using Engine.DataStructures;
using Engine.Interfaces.Config;
using System.Data;
using System.Text;
using Engine.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Engine.Dal.Interfaces;
using Engine.Dal.Models;
using DataAccess.Entities;
using DataAccess.Services;
using DataAccess.Interfaces;
using CommonServiceLocator;
using Engine.Models.Moves;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;
using Engine.Services;

namespace Engine.Dal.Services;

public class GameDbService : DbServiceBase, IGameDbService
{
    private readonly int _depth;
    private readonly int _search;
    private readonly int _popular;
    private readonly int _minimumPopular;
    private readonly int _minimumPopularThreshold;
    private readonly int _maximumPopularThreshold;
    private readonly int _chunk;
    private readonly short _games;

    private Task _loadTask;
    private readonly IMoveHistoryService _moveHistory;

    public GameDbService(IConfigurationProvider configurationProvider, IMoveHistoryService moveHistory) : base()
    {
        _depth = configurationProvider.BookConfiguration.SaveDepth;
        _search = configurationProvider.BookConfiguration.SearchDepth;
        _games = configurationProvider.BookConfiguration.GamesThreshold;
        _popular = configurationProvider.BookConfiguration.PopularThreshold;
        _minimumPopular = configurationProvider.BookConfiguration.MinimumPopular;
        _minimumPopularThreshold = configurationProvider.BookConfiguration.MinimumPopularThreshold;
        _maximumPopularThreshold = configurationProvider.BookConfiguration.MaximumPopularThreshold;
        _chunk = configurationProvider.BookConfiguration.Chunk;
        _moveHistory = moveHistory;
    }
    protected override void OnConnected()
    {
        var games = GetTotalGames();
    }

    public long GetTotalGames() => Connection.Books.Where(b => b.History == new byte[0])
            .Sum(x => x.White + x.Draw + x.Black);
    public long GetTotalPopularGames() => Connection.Positions.Where(b => b.History == new byte[0])
            .Sum(x => x.Total);

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

    public IEnumerable<PositionTotal> GetPositions() => Connection.Books.AsNoTracking()
                .Where(s => (s.White + s.Black + s.Draw) > _games)
                .Select(s => new PositionTotal { History = s.History, NextMove = s.NextMove, Total = s.White + s.Black + s.Draw });

    public IEnumerable<PositionTotal> GetPositions(ICollection<Book> books) => from b in books
                                                                               let book = Connection.Books.FirstOrDefault(bk => bk.History == b.History && bk.NextMove == b.NextMove)
                                                                               where book != null && (book.White + book.Black + book.Draw) > _games
                                                                               select new PositionTotal { History = book.History, NextMove = book.NextMove, Total = book.White + book.Black + book.Draw };

    public IEnumerable<SequenceTotalItem> GetPopular(int totalGames)
    {
        int length = 2 * _search + 1;
        return Connection.Positions.AsNoTracking()
                .Where(s => s.Total > totalGames && s.History.Length < length)
                .Select(s => new SequenceTotalItem
                {
                    Seuquence = Encoding.Unicode.GetString(s.History),
                    Move = new BookMove
                    {
                        Id = s.NextMove,
                        Value = s.Total
                    }
                });
    }

    public Task LoadAsync()
    {
        _loadTask = Task.Factory.StartNew(() =>
        {
            List<SequenceTotalItem> items = new List<SequenceTotalItem>(2000000);

            var total = GetPopular(_games);

            items.AddRange(total);

            Action ProcessMap = () =>
            {
                Dictionary<string, List<BookMove>> popular = new Dictionary<string, List<BookMove>>(2000000);

                for (int i = 0; i < items.Count; i++)
                {
                    AddPopular(popular, items[i]);
                }

                Dictionary<string, PopularMoves> map = new Dictionary<string, PopularMoves>(popular.Count * 10);

                foreach (var item in popular)
                {
                    map[item.Key] = GetMaxMoves(item.Value);
                }

                _moveHistory.CreateSequenceCache(map);
            };

            Action ProcessPopular = () =>
            {
                Dictionary<string, List<BookMove>> veryPopular = new Dictionary<string, List<BookMove>>(10000);

                foreach (var item in items.Where(item => item.Move.Value >= _minimumPopular))
                {
                    AddPopular(veryPopular, item);
                }

                var moveProvider = ServiceLocator.Current.GetInstance<MoveProvider>();
                Dictionary<string, MoveBase[]> popularMap = new Dictionary<string, MoveBase[]>(5 * veryPopular.Count);

                foreach (var item in veryPopular)
                {
                    if (item.Value.Count < _minimumPopularThreshold)
                        continue;

                    item.Value.Sort();

                    if (item.Key != string.Empty)
                    {
                        popularMap[item.Key] = item.Value
                        .Take(_maximumPopularThreshold)
                        .Select(x => moveProvider.Get(x.Id))
                        .ToArray();
                    }
                    else
                    {
                        var data = item.Value.Take(_maximumPopularThreshold).ToArray();
                        data.Shuffle();
                        popularMap[item.Key] = data.Select(x => moveProvider.Get(x.Id)).ToArray();
                    }
                }

                _moveHistory.CreatePopularCache(popularMap);
            };

            Parallel.Invoke(ProcessPopular,ProcessMap);
        });

        return _loadTask;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddPopular(Dictionary<string, List<BookMove>> map, SequenceTotalItem item)
    {
        if (map.TryGetValue(item.Seuquence, out List<BookMove> list))
        {
            list.Add(item.Move);
        }
        else
        {
            map.Add(item.Seuquence, new List<BookMove> { item.Move });
        }
    }

    public void UpdateTotal(IBulkDbService bulkDbService)
    {
        var positions = GetPositions();

        var chunks = positions.Chunk(_chunk).ToArray();

        for (int i = 0; i < chunks.Length; i++)
        {
            bulkDbService.Upsert(chunks[i]);
        }
    }

    public void UpdateHistory(GameValue value)
    {
        List<Book> records = value switch
        {
            GameValue.WhiteWin => CreateRecords(1, 0, 0),
            GameValue.BlackWin => CreateRecords(0, 0, 1),
            _ => CreateRecords(0, 1, 0),
        };

        var bulk = ServiceLocator.Current.GetInstance<IBulkDbService>();
        bulk.Connect();

        try
        {
            bulk.Upsert(records);

            var positions = GetPositions(records);

            bulk.Upsert(positions);
        }
        finally
        {
            bulk.Disconnect();
        }
    }

    public List<Book> CreateRecords(int white,int draw, int black)
    {
        List<Book> records = new List<Book>(_depth);

        MoveKeyList moveKeyList = stackalloc short[_depth];

        MoveKeyList keyCollection = stackalloc short[_depth];

        _moveHistory.GetSequence(ref moveKeyList);

        records.Add(new Book
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

            records.Add(new Book
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

    public void Upsert(List<Book> records)
    {
        List<Book> recordsToAdd = new List<Book>();
        List<Book> recordsToUpdate = new List<Book>();

        foreach (var record in records)
        {
            Book temp = Connection.Books
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

    public void WaitToData()
    {
        if (!_loadTask.IsCompleted)
        {
            _loadTask.Wait();
        }
    }

    private PopularMoves GetMaxMoves(List<BookMove> item)
    {
        item.Sort();

        var moves = item.Take(_popular).ToArray();

        if (moves.Length > 0)
        {
            return new Popular(moves);
        }

        return PopularMoves.Default;
    }

    private PopularMoves GetMaxMoves(IGrouping<string, BookMove> item)
    {
        var moves = item.OrderByDescending(i => i.Value).Take(_popular).ToArray();

        if (moves.Length > 0)
        {
            return new Popular(moves);
        }

        return PopularMoves.Default;
    }
}
