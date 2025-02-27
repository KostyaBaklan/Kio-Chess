using Engine.Dal.Models;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Strategies.Models.Contexts;
using Engine.Strategies.Models.Contexts.Book;
using Engine.Strategies.Models.Contexts.Popular;
using Engine.Strategies.Models.Contexts.Regular;
using System.Runtime.CompilerServices;

namespace Engine.Services;

public class DataPoolService 
{
    private readonly MoveList[] _moveLists;
    private readonly SearchContext[] _searchContexts;
    private readonly SortContext[][][] _sortContexts;
    private readonly SortContext[][][] _nullSortContexts;
    private readonly SortContext[][][] _evaluationSortContexts;
    private readonly MoveHistoryService _moveHistory;
    private Position _position;
    private bool[][] _lowSee;

    public DataPoolService(MoveHistoryService moveHistory, 
        IConfigurationProvider configuration, 
        MoveProvider moveProvider, IKillerMoveCollectionFactory killerMoveCollectionFactory)
    {
        var searchDepth = configuration.BookConfiguration.SearchDepth;
        var popularDepth = configuration.BookConfiguration.PopularDepth;
        int gameDepth = configuration.GeneralConfiguration.GameDepth;
        _searchContexts = new SearchContext[gameDepth];
        _moveLists = new MoveList[gameDepth];
        _sortContexts = new SortContext[2][][];
        _nullSortContexts = new SortContext[2][][];
        _evaluationSortContexts = new SortContext[2][][];

        var Moves = killerMoveCollectionFactory.CreateMoves();
        _lowSee = new bool[gameDepth][];

        for (int i = 0; i < _sortContexts.Length; i++)
        {
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _sortContexts[i] = new SortContext[3][];
            _nullSortContexts[i] = new SortContext[3][];
            _evaluationSortContexts[i] = new SortContext[3][];
            for (int j = 0; j < _sortContexts[i].Length; j++)
            {
                _sortContexts[i][j] = new SortContext[gameDepth];
                _nullSortContexts[i][j] = new SortContext[gameDepth];
                _evaluationSortContexts[i][j] = new SortContext[gameDepth];
            }
        }

        for (int i = 0; i < popularDepth; i++)
        {
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = Moves[i], LowSee = _lowSee[i] };
            _moveLists[i] = new MoveList();
            _sortContexts[0][0][i] = new WhitePopularOpeningSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[0][1][i] = new WhitePopularMiddleSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[0][2][i] = new WhitePopularEndSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[1][0][i] = new BlackPopularOpeningSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[1][1][i] = new BlackPopularMiddleSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[1][2][i] = new BlackPopularEndSortContext { Ply = i, CurrentKillers = Moves[i] };

            _evaluationSortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i };
            _evaluationSortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i };
            _evaluationSortContexts[0][2][i] = new WhiteEndSortContext { Ply = i };
            _evaluationSortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i };
            _evaluationSortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i };
            _evaluationSortContexts[1][2][i] = new BlackEndSortContext { Ply = i };
        }

        for (int i = popularDepth; i < searchDepth; i++)
        {
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = Moves[i], LowSee = _lowSee[i] };
            _moveLists[i] = new MoveList();
            _sortContexts[0][0][i] = new WhiteBookOpeningSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[0][1][i] = new WhiteBookMiddleSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[0][2][i] = new WhiteBookEndSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[1][0][i] = new BlackBookOpeningSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[1][1][i] = new BlackBookMiddleSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[1][2][i] = new BlackBookEndSortContext { Ply = i, CurrentKillers = Moves[i] };

            _evaluationSortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i };
            _evaluationSortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i };
            _evaluationSortContexts[0][2][i] = new WhiteEndSortContext { Ply = i };
            _evaluationSortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i };
            _evaluationSortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i };
            _evaluationSortContexts[1][2][i] = new BlackEndSortContext { Ply = i };
        }

        for (int i = 0; i < _searchContexts.Length; i++)
        {
            _nullSortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i, CurrentKillers = Moves[i] };
            _nullSortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i, CurrentKillers = Moves[i] };
            _nullSortContexts[0][2][i] = new WhiteEndSortContext { Ply = i, CurrentKillers = Moves[i] };
            _nullSortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i, CurrentKillers = Moves[i] };
            _nullSortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i, CurrentKillers = Moves[i] };
            _nullSortContexts[1][2][i] = new BlackEndSortContext { Ply = i, CurrentKillers = Moves[i] };
        }

        for (int i = searchDepth; i < _searchContexts.Length; i++)
        {
            _lowSee[i] = new bool[moveProvider.MovesCount];
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = Moves[i], LowSee = _lowSee[i] };
            _moveLists[i] = new MoveList();
            _sortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[0][2][i] = new WhiteEndSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i, CurrentKillers = Moves[i] };
            _sortContexts[1][2][i] = new BlackEndSortContext { Ply = i, CurrentKillers = Moves[i] };

            _evaluationSortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i };
            _evaluationSortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i };
            _evaluationSortContexts[0][2][i] = new WhiteEndSortContext { Ply = i };
            _evaluationSortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i };
            _evaluationSortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i };
            _evaluationSortContexts[1][2][i] = new BlackEndSortContext { Ply = i };

            
        }

        _moveHistory = moveHistory;

        SearchContext.MoveHistory = moveHistory;
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
    public MoveList GetCurrentMoveList() => _moveLists[_moveHistory.GetPly()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortContext GetCurrentSortContext() => _sortContexts[(byte)_position.GetTurn()][_moveHistory.GetPhase()][_moveHistory.GetPly()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortContext GetCurrentNullSortContext() => _nullSortContexts[(byte)_position.GetTurn()][_moveHistory.GetPhase()][_moveHistory.GetPly()];

    public void Initialize(Position position)
    {
        _position = position;

        SortContext.Position = position;
        SortContext.MoveHistory = _moveHistory;
        SortContext.DataPoolService = this;
    }
}
