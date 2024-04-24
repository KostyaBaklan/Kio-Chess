using Engine.DataStructures;
using Engine.Interfaces.Config;
using System.Data;
using System.Text;
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
using Engine.Services;
using DataAccess.Contexts;

namespace Engine.Dal.Services;

public class GameDbService : DbServiceBase, IGameDbService
{
    private readonly int _depth;
    private readonly int _search;
    private readonly int _popular;
    private readonly int _populrDepth;
    private readonly int _minimumPopular;
    private readonly int _minimumPopularThreshold;
    private readonly int _maximumPopularThreshold;
    private readonly int _chunk;
    private readonly short _games;
    protected LiteContext AdditionalConnection;

    private Task _loadTask;
    private readonly MoveHistoryService _moveHistory;

    public GameDbService(IConfigurationProvider configurationProvider, MoveHistoryService moveHistory) : base()
    {
        _depth = configurationProvider.BookConfiguration.SaveDepth;
        _search = configurationProvider.BookConfiguration.SearchDepth;
        _games = configurationProvider.BookConfiguration.GamesThreshold;
        _popular = configurationProvider.BookConfiguration.PopularThreshold;
        _populrDepth = configurationProvider.BookConfiguration.PopularDepth;
        _minimumPopular = configurationProvider.BookConfiguration.MinimumPopular;
        _minimumPopularThreshold = configurationProvider.BookConfiguration.MinimumPopularThreshold;
        _maximumPopularThreshold = configurationProvider.BookConfiguration.MaximumPopularThreshold;
        _chunk = configurationProvider.BookConfiguration.Chunk;
        _moveHistory = moveHistory;
    }
    protected override void OnConnected()
    {
        AdditionalConnection = new LiteContext();
        Connection.Database.ExecuteSqlRaw(@"PRAGMA journal_mode = 'wal'");
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
                        Value = (short)s.Total
                    }
                });
    }

    public Task LoadAsync()
    {
        _loadTask = Task.Factory.StartNew(() =>
        {
            Action ProcessMap = () =>
            {
                Dictionary<string, PopularMoves> popularMap = GetPopularPositions()
                                .GroupBy(p => p.History, v => new BookMove { Id = v.NextMove, Value = v.Value })
                                .ToDictionary(k => k.Key, v => new Popular(v.ToArray()) as PopularMoves);

                _moveHistory.CreateSequenceCache(popularMap);
            };

            Action ProcessPopular = () =>
            {
                var moveProvider = ServiceLocator.Current.GetInstance<MoveProvider>();

                Dictionary<string, MoveBase[]> veryPopularMap = new Dictionary<string, MoveBase[]>();

                var groups = GetVeryPopularPositions()
                    .GroupBy(g => g.History, v => new BookMove { Id = v.NextMove, Value = v.Value });

                foreach (var item in groups)
                {
                    var list = item.ToList();
                    list.Sort();

                    if (item.Key.Length < 1)
                    {
                        veryPopularMap[item.Key] = list
                        .Select(x => moveProvider.Get(x.Id))
                        .ToArray();
                    }
                    else
                    {
                        var data = list.ToArray();
                        data.Shuffle();
                        veryPopularMap[item.Key] = data.Select(x => moveProvider.Get(x.Id)).ToArray();
                    }
                }

                _moveHistory.CreatePopularCache(veryPopularMap);
            };

            Parallel.Invoke(ProcessPopular,ProcessMap);
        });

        return _loadTask;
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

    public void AddDebuts(IEnumerable<Debut> debuts)
    {
        Connection.Debuts.AddRange(debuts);
        Connection.SaveChanges();
    }

    public void AddPopularPositions(IEnumerable<PopularPosition> records)
    {
        var bulk = ServiceLocator.Current.GetInstance<IBulkDbService>();
        bulk.Connect();

        foreach (var chunk in records.Chunk(10000))
        {
            bulk.AddRange(chunk);
        }

        bulk.Disconnect();
    }

    public void AddVeryPopularPositions(IEnumerable<VeryPopularPosition> records)
    {
        var bulk = ServiceLocator.Current.GetInstance<IBulkDbService>();
        bulk.Connect();

        foreach (var chunk in records.Chunk(10000))
        {
            bulk.AddRange(chunk);
        }

        bulk.Disconnect();
    }

    public void RemovePopularPositions()
    {
        Connection.PopularPositions.ExecuteDelete();
        Connection.SaveChanges();
    }

    public void RemoveVeryPopularPositions()
    {
        Connection.VeryPopularPositions.ExecuteDelete();
        Connection.SaveChanges();
    }

    public IEnumerable<PopularPosition> GetPopularPositions()
    {
        return Connection.PopularPositions.AsNoTracking();
    }

    public IEnumerable<VeryPopularPosition> GetVeryPopularPositions()
    {
        return AdditionalConnection.VeryPopularPositions.AsNoTracking();
    }

    public PopularPositions GeneratePopularPositions()
    {
        PopularPositions pps = new PopularPositions();
        IEnumerable<PositionValue> positionValues = GetPositionValues(_games, _search);
        List<PositionValue> pvs = new List<PositionValue>(2000000);

        pvs.AddRange(positionValues);

        List<PopularPosition> popularPositions = new List<PopularPosition>(pvs.Count);

        foreach (var positionValue in pvs.GroupBy(p => p.Sequence))
        {
            var maxMoves = GetMaxMoves(positionValue, _popular);

            var list = maxMoves.Select(v => new PopularPosition { History = positionValue.Key, NextMove = v.Key, Value = v.Value }).ToList();

            popularPositions.AddRange(list);
        }

        pps.Popular = popularPositions;

        positionValues = pvs.Where(p => p.Book.GetTotal() > _minimumPopular).ToList();//GetPositionValues(_minimumPopular, _populrDepth);
        List<VeryPopularPosition> vpvs = new List<VeryPopularPosition>(20000);

        foreach (var positionValue in positionValues.GroupBy(p => p.Sequence).Where(g => g.Count() >= _minimumPopularThreshold))
        {
            var maxMoves = GetMaxMoves(positionValue, _maximumPopularThreshold);

            var list = maxMoves.Select(v => new VeryPopularPosition { History = positionValue.Key, NextMove = v.Key, Value = v.Value }).ToList();

            vpvs.AddRange(list);
        }

        pps.VeryPopular = vpvs;

        return pps;
    }

    private IEnumerable<PositionValue> GetPositionValues(int total, int depth)
    {
        return Connection.Books.AsNoTracking()
                .Where(s => (s.White + s.Black + s.Draw) > total && s.History.Length < 2 * depth + 1)
                .Select(s => new PositionValue { Sequence = Encoding.Unicode.GetString(s.History), NextMove = s.NextMove, Book = new BookValue { Black = s.Black, Draw = s.Draw, White = s.White } });
    }

    private KeyValuePair<short, short>[] GetMaxMoves(IGrouping<string, PositionValue> item, int count)
    {
        KeyValuePair<short, short>[] moves = item.OrderBy(g => g)
            .Take(count)
            .Select(t => new KeyValuePair<short, short>(t.NextMove, t.Book.GetPercentageDifference(t.Book.GetTotal(), item.Key.Length % 2 == 0)))
            .ToArray();

        return moves;
    }

    public object GetPopularSize()
    {
        return Connection.PopularPositions.Count();
    }

    public object GetVeryPopularSize()
    {
        return Connection.VeryPopularPositions.Count();
    }
}
