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

    private SortContext[][][] _sortContexts;
    private SortContext[][][] _evaluationSortContexts;

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
        _sortContexts = new SortContext[2][][];
        _evaluationSortContexts = new SortContext[2][][];

        _lowSee = new bool[_capacity][];

        for (int i = 0; i < _sortContexts.Length; i++)
        {
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _sortContexts[i] = new SortContext[3][];
            _evaluationSortContexts[i] = new SortContext[3][];
            for (int j = 0; j < _sortContexts[i].Length; j++)
            {
                if (j > 0)
                {
                    _sortContexts[i][j] = new SortContext[_capacity];
                    _evaluationSortContexts[i][j] = new SortContext[_capacity];
                }
                else
                {
                    _sortContexts[i][j] = new SortContext[searchDepth];
                    _evaluationSortContexts[i][j] = new SortContext[searchDepth];
                }
            }
        }

        for (int i = 0; i < popularDepth; i++)
        {
            var killer = new KillerMoves();
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = killer, LowSee = _lowSee[i] };
            _sortContexts[0][0][i] = new WhitePopularOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[0][1][i] = new WhitePopularMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[0][2][i] = new WhitePopularEndSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][0][i] = new BlackPopularOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][1][i] = new BlackPopularMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][2][i] = new BlackPopularEndSortContext { Ply = i, CurrentKillers = killer };

            _evaluationSortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[0][2][i] = new WhiteEndSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[1][2][i] = new BlackEndSortContext { Ply = i, CurrentKillers = killer };
        }

        for (int i = popularDepth; i < searchDepth; i++)
        {
            var killer = new KillerMoves();
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = killer, LowSee = _lowSee[i] };
            _sortContexts[0][0][i] = new WhiteBookOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[0][1][i] = new WhiteBookMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[0][2][i] = new WhiteBookEndSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][0][i] = new BlackBookOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][1][i] = new BlackBookMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][2][i] = new BlackBookEndSortContext { Ply = i, CurrentKillers = killer };

            _evaluationSortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[0][2][i] = new WhiteEndSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i, CurrentKillers = killer };
            _evaluationSortContexts[1][2][i] = new BlackEndSortContext { Ply = i, CurrentKillers = killer };
        }

        for (int i = searchDepth; i < _searchContexts.Length; i++)
        {
            var killer = new KillerMoves();
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = killer, LowSee = _lowSee[i] };
            //_sortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[0][2][i] = new WhiteEndSortContext { Ply = i, CurrentKillers = killer };
            //_sortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][2][i] = new BlackEndSortContext { Ply = i, CurrentKillers = killer };

            //_evaluationSortContexts[0][0][i] = _sortContexts[0][0][i];
            _evaluationSortContexts[0][1][i] = _sortContexts[0][1][i];
            _evaluationSortContexts[0][2][i] = _sortContexts[0][2][i];
            //_evaluationSortContexts[1][0][i] = _sortContexts[1][0][i];
            _evaluationSortContexts[1][1][i] = _sortContexts[1][1][i];
            _evaluationSortContexts[1][2][i] = _sortContexts[1][2][i];
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
    public SortContext GetCurrentEvaluationSortContext() => _evaluationSortContexts[(byte)_position.GetTurn()][_moveHistory.GetPhase()][_moveHistory.GetPly()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool[] GetCurrentLowSee() => _lowSee[_moveHistory.GetPly()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortContext GetCurrentSortContext() => _sortContexts[(byte)_position.GetTurn()][_moveHistory.GetPhase()][_moveHistory.GetPly()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortContext GetCurrentNullSortContext() => _evaluationSortContexts[(byte)_position.GetTurn()][_moveHistory.GetPhase()][_moveHistory.GetPly()];

    public void Initialize(Position position)
    {
        _position = position;

        SortContext.MoveHistory = _moveHistory;
        SortContext.DataPoolService = this;
    }

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
                for (int i = 0; i < _sortContexts.Length; i++)
                {
                    for (int j = 1; j < _sortContexts[i].Length; j++)
                    {
                        EnumerableExtensions.Resize(ref _sortContexts[i][j], _offset);

                        EnumerableExtensions.Resize(ref _evaluationSortContexts[i][j], _offset);
                    }
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

            //_sortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[0][2][i] = new WhiteEndSortContext { Ply = i, CurrentKillers = killer };
            //_sortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i, CurrentKillers = killer };
            _sortContexts[1][2][i] = new BlackEndSortContext { Ply = i, CurrentKillers = killer };
            //_evaluationSortContexts[0][0][i] = _sortContexts[0][0][i];
            _evaluationSortContexts[0][1][i] = _sortContexts[0][1][i];
            _evaluationSortContexts[0][2][i] = _sortContexts[0][2][i];
            //_evaluationSortContexts[1][0][i] = _sortContexts[1][0][i];
            _evaluationSortContexts[1][1][i] = _sortContexts[1][1][i];
            _evaluationSortContexts[1][2][i] = _sortContexts[1][2][i];
        }
    }
}
