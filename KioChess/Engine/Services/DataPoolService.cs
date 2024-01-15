using CommonServiceLocator;
using Engine.Dal.Models;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Interfaces.Evaluation;
using Engine.Strategies.Models.Contexts;
using Engine.Strategies.Models.Contexts.Book;
using Engine.Strategies.Models.Contexts.Popular;
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

    public DataPoolService(IMoveHistoryService moveHistory, 
        IConfigurationProvider configuration, 
        IMoveProvider moveProvider,
        IEvaluationServiceFactory evaluationServiceFactory)
    {
        var searchDepth = configuration.BookConfiguration.SearchDepth;
        var popularDepth = configuration.BookConfiguration.PopularDepth;
        int gameDepth = configuration.GeneralConfiguration.GameDepth;
        _searchContexts = new SearchContext[gameDepth];
        _moveLists = new MoveList[gameDepth];
        _sortContexts = new SortContext[2][][];

        var Moves = ServiceLocator.Current.GetInstance<IKillerMoveCollectionFactory>().CreateMoves();

        for (int i = 0; i < _sortContexts.Length; i++)
        {
            _sortContexts[i] = new SortContext[3][];
            for (int j = 0; j < _sortContexts[i].Length; j++)
            {
                _sortContexts[i][j] = new SortContext[gameDepth];
            }
        }

        for (int i = 0; i < popularDepth; i++)
        {
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = Moves[i] };
            _moveLists[i] = new MoveList();
            _sortContexts[0][0][i] = new WhitePopularOpeningSortContext { Ply = i,EvaluationService = evaluationServiceFactory.GetEvaluationService(0), CurrentKillers = Moves[i] };
            _sortContexts[0][1][i] = new WhitePopularMiddleSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(1), CurrentKillers = Moves[i] };
            _sortContexts[0][2][i] = new WhitePopularEndSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(2), CurrentKillers = Moves[i] };
            _sortContexts[1][0][i] = new BlackPopularOpeningSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(0), CurrentKillers = Moves[i] };
            _sortContexts[1][1][i] = new BlackPopularMiddleSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(1), CurrentKillers = Moves[i] };
            _sortContexts[1][2][i] = new BlackPopularEndSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(2), CurrentKillers = Moves[i] };
        }

        for (int i = popularDepth; i < searchDepth; i++)
        {
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = Moves[i] };
            _moveLists[i] = new MoveList();
            _sortContexts[0][0][i] = new WhiteBookOpeningSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(0), CurrentKillers = Moves[i] };
            _sortContexts[0][1][i] = new WhiteBookMiddleSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(1), CurrentKillers = Moves[i] };
            _sortContexts[0][2][i] = new WhiteBookEndSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(2), CurrentKillers = Moves[i] };
            _sortContexts[1][0][i] = new BlackBookOpeningSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(0), CurrentKillers = Moves[i] };
            _sortContexts[1][1][i] = new BlackBookMiddleSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(1), CurrentKillers = Moves[i] };
            _sortContexts[1][2][i] = new BlackBookEndSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(2), CurrentKillers = Moves[i] };
        }

        for (int i = searchDepth; i < _searchContexts.Length; i++)
        {
            _searchContexts[i] = new SearchContext { Ply = i, CurrentKillers = Moves[i] };
            _moveLists[i] = new MoveList();
            _sortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(0), CurrentKillers = Moves[i] };
            _sortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(1), CurrentKillers = Moves[i] };
            _sortContexts[0][2][i] = new WhiteEndSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(2), CurrentKillers = Moves[i] };
            _sortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(0), CurrentKillers = Moves[i] };
            _sortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(1), CurrentKillers = Moves[i] };
            _sortContexts[1][2][i] = new BlackEndSortContext { Ply = i, EvaluationService = evaluationServiceFactory.GetEvaluationService(2), CurrentKillers = Moves[i] };
        }

        _moveHistory = moveHistory;

        SearchContext.MoveHistory = moveHistory;

        Popular.Initialize(moveProvider.MovesCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchContext GetCurrentContext() => _searchContexts[_moveHistory.GetPly()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetCurrentMoveList() => _moveLists[_moveHistory.GetPly()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortContext GetCurrentSortContext() => _sortContexts[(byte)_position.GetTurn()][_position.GetPhase()][_moveHistory.GetPly()];

    public void Initialize(IPosition position)
    {
        _position = position;

        SortContext.Position = position;
        SortContext.MoveHistory = _moveHistory;
        SortContext.DataPoolService = this;
    }
}
