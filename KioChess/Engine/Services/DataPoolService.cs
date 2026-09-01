using Engine.DataStructures.Moves;
using Engine.Dal.Models;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Strategies.Models.Contexts;
using Engine.Strategies.Models.Contexts.Book;
using Engine.Strategies.Models.Contexts.Popular;
using Engine.Strategies.Models.Contexts.Regular;
using System.Runtime.CompilerServices;
using Engine.Models.Helpers;
using Engine.DataStructures;

namespace Engine.Services;

[SkipLocalsInit]
public class DataPoolService
{
    private int _capacity;
    private readonly int _threshold;
    private readonly int _offset;
    private Position _position;

    private SearchContext[] _searchContexts;
    private bool[][] _lowSee;

    private SortContext[][] _sortContexts;
    private SortContext[][] _evaluationSortContexts;

    private SortContext[] _currentSortRow;
    private SortContext[] _currentEvalSortRow;
    private byte _cachedRow = 255;

    private readonly MoveHistoryService _moveHistory;

    public DataPoolService(MoveHistoryService moveHistory,
        IConfigurationProvider configuration,
        MoveProvider moveProvider)
    {
        _capacity = configuration.GeneralConfiguration.DynamicGameDepth;
        _threshold = configuration.GeneralConfiguration.ResizeThreshold;
        _offset = configuration.GeneralConfiguration.ResizeDepth;

        var searchDepth = configuration.BookConfiguration.SearchDepth;
        var popularDepth = configuration.BookConfiguration.PopularDepth;
        _searchContexts = new SearchContext[_capacity];
        _sortContexts = new SortContext[6][];
        _evaluationSortContexts = new SortContext[6][];

        _lowSee = new bool[_capacity][];

        // phase<<1|turn mapping: Opening(0,1) Middle(2,3) End(4,5); White=even, Black=odd
        // rows 0,1 = Opening (searchDepth); rows 2,3,4,5 = Middle/End (_capacity)
        for (int i = 0; i < 6; i++)
        {
            if (i < 2)
            {
                _sortContexts[i] = new SortContext[searchDepth];
                _evaluationSortContexts[i] = new SortContext[searchDepth];
            }
            else
            {
                _sortContexts[i] = new SortContext[_capacity];
                _evaluationSortContexts[i] = new SortContext[_capacity];
            }
        }

        for (int i = 0; i < popularDepth; i++)
        {
            var killer = new KillerMoves();
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = killer, LowSee = _lowSee[i] };
            _sortContexts[0][i] = new WhitePopularOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][i] = new BlackPopularOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[2][i] = new WhitePopularMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[3][i] = new BlackPopularMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[4][i] = new WhitePopularEndSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[5][i] = new BlackPopularEndSortContext { Ply = i, CurrentKillers = killer };

            _evaluationSortContexts[0][i] = new WhiteOpeningSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[1][i] = new BlackOpeningSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[2][i] = new WhiteMiddleSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[3][i] = new BlackMiddleSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[4][i] = new WhiteEndSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[5][i] = new BlackEndSortContext { Ply = i, CurrentKillers = killer };
        }

        for (int i = popularDepth; i < searchDepth; i++)
        {
            var killer = new KillerMoves();
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = killer, LowSee = _lowSee[i] };
            _sortContexts[0][i] = new WhiteBookOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][i] = new BlackBookOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[2][i] = new WhiteBookMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[3][i] = new BlackBookMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[4][i] = new WhiteBookEndSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[5][i] = new BlackBookEndSortContext { Ply = i, CurrentKillers = killer };

            _evaluationSortContexts[0][i] = new WhiteOpeningSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[1][i] = new BlackOpeningSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[2][i] = new WhiteMiddleSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[3][i] = new BlackMiddleSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[4][i] = new WhiteEndSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[5][i] = new BlackEndSortContext { Ply = i, CurrentKillers = killer };
        }

        for (int i = searchDepth; i < _searchContexts.Length; i++)
        {
            var killer = new KillerMoves();
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = killer, LowSee = _lowSee[i] };
            //_sortContexts[0][i] = new WhiteOpeningSortContext { Ply = i, CurrentKillers = killer };
            //_sortContexts[1][i] = new BlackOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[2][i] = new WhiteMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[3][i] = new BlackMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[4][i] = new WhiteEndSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[5][i] = new BlackEndSortContext { Ply = i, CurrentKillers = killer };

            //_evaluationSortContexts[0][i] = _sortContexts[0][i];
            //_evaluationSortContexts[1][i] = _sortContexts[1][i];
            _evaluationSortContexts[2][i] = _sortContexts[2][i];
            _evaluationSortContexts[3][i] = _sortContexts[3][i];
            _evaluationSortContexts[4][i] = _sortContexts[4][i];
            _evaluationSortContexts[5][i] = _sortContexts[5][i];
        }

        _moveHistory = moveHistory;

        SearchContext.MoveHistory = moveHistory;
        SearchContext.MoveProvider = moveProvider;
        SortContext.MoveProvider = moveProvider;

        Popular.Initialize(moveProvider.MovesCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchContext GetCurrentContext() => _searchContexts[_moveHistory.GetPly()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortContext GetCurrentEvaluationSortContext()
    {
        EnsureRows();
        return _currentEvalSortRow[_moveHistory.GetPly()];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool[] GetCurrentLowSee() => _lowSee[_moveHistory.GetPly()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortContext GetCurrentSortContext()
    {
        EnsureRows();
        return _currentSortRow[_moveHistory.GetPly()];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortContext GetCurrentNullSortContext()
    {
        EnsureRows();
        return _currentEvalSortRow[_moveHistory.GetPly()];
    }

    public void Initialize(Position position)
    {
        _position = position;

        SortContext.MoveHistory = _moveHistory;
        SortContext.DataPoolService = this;

        RefreshRows(0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RefreshRows(byte row)
    {
        _currentSortRow     = _sortContexts[row];
        _currentEvalSortRow = _evaluationSortContexts[row];
        _cachedRow = row;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RefreshRows()
    {
        byte row = (byte)(_moveHistory.GetPhase() << 1 | (byte)_position.GetTurn());
        RefreshRows(row);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureRows()
    {
        byte row = (byte)(_moveHistory.GetPhase() << 1 | (byte)_position.GetTurn());
        if (row != _cachedRow)
            RefreshRows(row);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetCapacity() => _capacity;


    public void Resize(TranspositionTable table)
    {
        if (_moveHistory.GetPly() + _threshold < _capacity)
            return;

        Parallel.Invoke(
            () => { table.Resize(_offset); _moveHistory.Resize(_offset); },
            () =>
            {
                EnumerableExtensions.Resize(ref _searchContexts, _offset);
                EnumerableExtensions.Resize(ref _lowSee, _offset);
            },
            () =>
            {
                for (int i = 0; i < 6; i++)
                {
                    if (i < 2) continue; // Opening rows have fixed searchDepth, not resized

                    EnumerableExtensions.Resize(ref _sortContexts[i], _offset);
                    EnumerableExtensions.Resize(ref _evaluationSortContexts[i], _offset);
                }
            }
        );

        PostResizeInitialization();
    }

    private void PostResizeInitialization()
    {
        int previousCapacity = _capacity;

        _capacity += _offset;

        for (int i = previousCapacity; i < _capacity; i++)
        {
            var killer = new KillerMoves();

            _lowSee[i] = new bool[SortContext.MoveProvider.MovesCount];
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = killer, LowSee = _lowSee[i] };

            //_sortContexts[0][i] = new WhiteOpeningSortContext { Ply = i, CurrentKillers = killer };
            //_sortContexts[1][i] = new BlackOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[2][i] = new WhiteMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[3][i] = new BlackMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[4][i] = new WhiteEndSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[5][i] = new BlackEndSortContext { Ply = i, CurrentKillers = killer };
            //_evaluationSortContexts[0][i] = _sortContexts[0][i];
            //_evaluationSortContexts[1][i] = _sortContexts[1][i];
            _evaluationSortContexts[2][i] = _sortContexts[2][i];
            _evaluationSortContexts[3][i] = _sortContexts[3][i];
            _evaluationSortContexts[4][i] = _sortContexts[4][i];
            _evaluationSortContexts[5][i] = _sortContexts[5][i];
        }

        RefreshRows();
    }
}
