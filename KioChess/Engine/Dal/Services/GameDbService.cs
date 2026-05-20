using DataAccess.Entities;
using DataAccess.Models;
using DataAccess.Services;
using Engine.Dal.Interfaces;
using Engine.Dal.Models;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces.Config;
using Engine.Models.Helpers;
using Engine.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Dal.Services;

public class GameDbService : DbServiceBase, IGameDbService
{
    private readonly int _depth;
    private readonly int _search;
    private readonly int _popular;
    private readonly int _popularDepth;
    private readonly int _minimumPopular;
    private readonly int _minimumPopularThreshold;
    private readonly int _maximumPopularThreshold;
    private readonly int _chunk;
    private readonly short _games;

    private readonly object _sync = new();
    private Task _loadTask;
    private readonly MoveHistoryService _moveHistory;
    private readonly MoveProvider _moveProvider;

    public GameDbService(IConfigurationProvider configurationProvider, MoveHistoryService moveHistory, MoveProvider moveProvider) : base()
    {
        _depth = configurationProvider.BookConfiguration.SaveDepth;
        _search = configurationProvider.BookConfiguration.SearchDepth;
        _games = configurationProvider.BookConfiguration.GamesThreshold;
        _popular = configurationProvider.BookConfiguration.PopularThreshold;
        _popularDepth = configurationProvider.BookConfiguration.PopularDepth;
        _minimumPopular = configurationProvider.BookConfiguration.MinimumPopular;
        _minimumPopularThreshold = configurationProvider.BookConfiguration.MinimumPopularThreshold;
        _maximumPopularThreshold = configurationProvider.BookConfiguration.MaximumPopularThreshold;
        _chunk = configurationProvider.BookConfiguration.Chunk;
        _moveHistory = moveHistory;
        _moveProvider = moveProvider;
    }
    protected override void OnConnected() => Connection.Database.ExecuteSqlRaw("PRAGMA journal_mode=wal");

    public long GetTotalGames() => Connection.Books.Where(b => b.History == new byte[0])
            .Sum(x => x.White + x.Draw + x.Black);

    public HistoryValue Get(byte[] history)
    {
        HistoryValue value = [];

        var books = Connection.Books.AsNoTracking()
            .Where(x => x.History == history)
            .Select(x => new { x.NextMove, x.White, x.Draw, x.Black });

        foreach (var book in books)
        {
            value.Add(book.NextMove, book.White, book.Draw, book.Black);
        }

        return value;
    }

    public IEnumerable<PositionEntity> LoadPositions()
    {
        string sql = $@"SELECT History, NextMove, (White+Black+Draw) AS Total
                        from Books
                        where White+Black+Draw > @total and length(History) < @length";

        var parameters = new List<SqliteParameter>
        {
            new("@total",_games-1),
            new("@length",2*_search+1)
        };

        return Execute(sql, r =>
        {
            return new PositionEntity
            {
                Sequence = Encoding.Unicode.GetString(r[0] as byte[]),
                NextMove = r.GetInt16(1),
                Total = r.GetInt32(2)
            };
        }, parameters, 300);
    }

    public Task LoadAsync()
    {
        _loadTask = Task.Factory.StartNew(() =>
        {
            ILocalDbService localDbService = ContainerLocator.Current.Resolve<ILocalDbService>();

            var positions = localDbService.GetPositionTotalList();

            var groups = positions.GroupBy(
                p => OrderIndependentSequenceHasher.ComputeOrderIndependentHashFromString(p.Sequence),
                g => new PositionItem
                {
                    Id = g.NextMove,
                    Total = g.Total
                });

            Dictionary<ulong, PopularMoves> map = new(positions.Count);

            foreach (var item in groups)
            {
                map[item.Key] = GetMaxItems(item);
            }

            _moveHistory.CreateSequenceCache(map);

            Dictionary<ulong, MoveHistory[]> popularMap = new(10000);

            groups = positions.Where(p => p.Sequence.Length <= _popularDepth && p.Total >= _minimumPopular)
                .GroupBy(
                    p => OrderIndependentSequenceHasher.ComputeOrderIndependentHashFromString(p.Sequence),
                    g => new PositionItem
                {
                    Id = g.NextMove,
                    Total = g.Total
                })
                .Where(g => g.Count() >= _minimumPopularThreshold);


            foreach (var gr in groups)
            {
                var item = gr.OrderByDescending(x => x.Total);

                if (gr.Key != 0UL)
                {
                    popularMap[gr.Key] = [.. item
                    .Take(_maximumPopularThreshold)
                    .Select(x => new MoveHistory(x.Id, x.Total))];
                }
                else
                {
                    var data = item.Take(_maximumPopularThreshold).ToArray();
                    data.Shuffle();
                    popularMap[gr.Key] = [.. data.Select(x => new MoveHistory(x.Id, 0))];
                }
            }

            _moveHistory.CreatePopularCache(popularMap);

        });

        return _loadTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PopularMoves GetMaxItems(IGrouping<ulong, PositionItem> item)
    {
        var moves = item
            .OrderByDescending(x => x.Total)          
            .Take(_popular)
            .Select(p => new BookMove
            {
                Id = p.Id,
                Value = p.Total
            })
            .ToArray();

        if (moves.Length > 0)
        {

            return new Popular(moves);
        }

        return PopularMoves.Default;
    }

    public void UpdateHistory(GameValue value)
    {
        List<Book> records = value switch
        {
            GameValue.WhiteWin => CreateRecords(1, 0, 0),
            GameValue.BlackWin => CreateRecords(0, 0, 1),
            _ => CreateRecords(0, 1, 0),
        };

        Upsert(records);
    }

    public List<Book> CreateRecords(int white, int draw, int black)
    {
        List<Book> records = new(_depth);

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
        List<Book> recordsToAdd = [];
        List<Book> recordsToUpdate = [];

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
}
