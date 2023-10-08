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

namespace Engine.Dal.Services;

public class GameDbService : DbServiceBase, IGameDbService
{
    private readonly int _depth;
    private readonly int _search;
    private readonly int _popular;
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
        _chunk = configurationProvider.BookConfiguration.Chunk;
        _moveHistory = moveHistory;
    }
    public long GetTotalGames()
    {
        return Connection.Books.Where(b => b.History == new byte[0])
            .Sum(x => x.White + x.Draw + x.Black);
    }
    public long GetTotalPopularGames()
    {
        return Connection.Positions.Where(b => b.History == new byte[0])
            .Sum(x => x.Total);
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

    public IEnumerable<PositionTotal> GetPositions()
    {
        return Connection.Books.AsNoTracking()
                .Where(s => (s.White + s.Black + s.Draw) > _games)
                .Select(s => new PositionTotal { History = s.History, NextMove = s.NextMove, Total = s.White + s.Black + s.Draw });
    }

    public IEnumerable<PositionTotal> GetPositions(ICollection<Book> books)
    {
        return from b in books
               let book = Connection.Books.FirstOrDefault(bk => bk.History == b.History && bk.NextMove == b.NextMove)
               where book != null && (book.White + book.Black + book.Draw) > _games
               select new PositionTotal { History = book.History, NextMove = book.NextMove, Total = book.White + book.Black + book.Draw };
    }

    public IEnumerable<SequenceTotalItem> GetPopular(int totalGames)
    {
        return Connection.Positions.AsNoTracking()
                .Where(s => s.Total > totalGames)
                .Select(s => new SequenceTotalItem { Seuquence = Encoding.Unicode.GetString(s.History), Move = new BookMove { Id = s.NextMove, Value = s.Total } });
    }

    public Task LoadAsync()
    {
        _loadTask = Task.Factory.StartNew(() =>
        {
            var items = GetPopular(_games);

            List<SequenceTotalItem> list = new List<SequenceTotalItem>();
            List<BookMove> open = new List<BookMove>();

            foreach (var item in items)
            {
                if (item.Seuquence.Length == 0)
                {
                    open.Add(item.Move);
                }
                else
                {
                    SequenceTotalItem sequenceTotalItem = new SequenceTotalItem
                    {
                        Seuquence = item.Seuquence,
                        Move = item.Move
                    };

                    list.Add(sequenceTotalItem);
                }
            }

            _moveHistory.SetOpening(open);

            Dictionary<string, IPopularMoves> map = list.GroupBy(l => l.Seuquence, v => v.Move)
                    .ToDictionary(k => k.Key, v => GetMaxMoves(v));

            _moveHistory.CreateSequenceCache(map);
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

    private IPopularMoves GetMaxMoves(IGrouping<string, BookMove> item)
    {
        var moves = item.OrderByDescending(i => i.Value).Take(_popular).ToArray();

        return moves.Length switch
        {
            4 => new PopularMoves4(moves),
            3 => new PopularMoves3(moves),
            2 => new PopularMoves2(moves),
            1 => new PopularMoves1(moves),
            _ => new PopularMoves0(),
        };
    }
}
