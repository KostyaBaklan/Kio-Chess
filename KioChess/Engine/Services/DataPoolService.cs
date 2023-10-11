using Engine.Dal.Models;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Strategies.Models;
using Engine.Strategies.Models.Contexts;
using Engine.Strategies.Models.Contexts.Book;
using Engine.Strategies.Models.Contexts.Regular;
using System.Runtime.CompilerServices;

namespace Engine.Services;

public class DataPoolService : IDataPoolService
{
    private readonly MoveList[] _moveLists;
    private readonly SearchContext[] _searchContexts;
    private readonly SortContext[][][] _sortContexts;
    private readonly IMoveHistoryService _moveHistory;
    private IPosition _position;

    public DataPoolService(IMoveHistoryService moveHistory, IConfigurationProvider configuration, IMoveProvider moveProvider)
    {
        var SearchDepth = configuration.BookConfiguration.SearchDepth;
        int gameDepth = configuration.GeneralConfiguration.GameDepth;
        _searchContexts = new SearchContext[gameDepth];
        _moveLists = new MoveList[gameDepth];
        _sortContexts = new SortContext[2][][];

        for (int i = 0; i < _sortContexts.Length; i++)
        {
            _sortContexts[i] = new SortContext[3][];
            for (int j = 0; j < _sortContexts[i].Length; j++)
            {
                _sortContexts[i][j] = new SortContext[gameDepth];
            }
        }

        for (int i = 0; i < SearchDepth; i++)
        {
            _searchContexts[i] = new SearchContext { Ply = i };
            _moveLists[i] = new MoveList();
            _sortContexts[0][0][i] = new WhiteBookOpeningSortContext { Ply = i };
            _sortContexts[0][1][i] = new WhiteBookMiddleSortContext { Ply = i };
            _sortContexts[0][2][i] = new WhiteBookEndSortContext { Ply = i};
            _sortContexts[1][0][i] = new BlackBookOpeningSortContext { Ply = i};
            _sortContexts[1][1][i] = new BlackBookMiddleSortContext { Ply = i };
            _sortContexts[1][2][i] = new BlackBookEndSortContext { Ply = i };
        }

        for (int i = SearchDepth; i < _searchContexts.Length; i++)
        {
            _searchContexts[i] = new SearchContext { Ply = i };
            _moveLists[i] = new MoveList();
            _sortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i };
            _sortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i };
            _sortContexts[0][2][i] = new WhiteEndSortContext { Ply = i};
            _sortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i, };
            _sortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i, };
            _sortContexts[1][2][i] = new BlackEndSortContext { Ply = i};
        }

        _moveHistory = moveHistory;

        Popular.Initialize(moveProvider.MovesCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchContext GetCurrentContext()
    {
        return _searchContexts[_moveHistory.GetPly()];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetCurrentMoveList()
    {
        return _moveLists[_moveHistory.GetPly()];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortContext GetCurrentSortContext()
    {
        return _sortContexts[(byte)_position.GetTurn()][_position.GetPhase()][_moveHistory.GetPly()];
    }

    public void Initialize(IPosition position)
    {
        _position = position;

        SortContext.Position = position;
        SortContext.MoveHistory = _moveHistory;
    }
}
