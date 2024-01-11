using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using Engine.Strategies.AB;
using Engine.Strategies.End;
using Engine.Strategies.Models;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Base;

public abstract class StrategyBase
{
    private bool _isBlocked;
    protected bool UseAging;
    protected bool IsPvEnabled;
    protected sbyte Depth;
    protected int SearchValue;
    protected int MinusSearchValue;
    protected int FutilityDepth;
    protected int RazoringDepth;
    protected bool UseFutility;
    protected short MaxEndGameDepth;
    protected int ExtensionDepthDifference;
    protected int EndExtensionDepthDifference;
    protected int DistanceFromRoot;
    protected int MaxExtensionPly;

    protected int[] SortDepth;
    protected readonly short[][] AlphaMargins;
    protected readonly short[][] BetaMargins;
    protected readonly short[] DeltaMargins;

    protected int SubSearchDepthThreshold;
    protected int SubSearchDepth;
    protected int SubSearchLevel;
    protected bool UseSubSearch;

    protected readonly short SuggestedThreshold;
    protected readonly short NonSuggestedThreshold;

    protected const sbyte One = 1;
    protected const sbyte Zero = 0;
    protected readonly int Mate;
    protected readonly int MateNegative;

    protected IPosition Position;
    protected MoveSorterBase[] Sorters;

    protected readonly IMoveHistoryService MoveHistory;
    protected readonly IMoveProvider MoveProvider;
    protected readonly IMoveSorterProvider MoveSorterProvider;
    protected readonly IConfigurationProvider configurationProvider;
    protected readonly IDataPoolService DataPoolService;

    private StrategyBase _endGameStrategy;
    protected StrategyBase EndGameStrategy
    {
        get
        {
            StrategyBase strategyBase = _endGameStrategy ??= CreateEndGameStrategy();
            //strategyBase.MaxExtensionPly = MaxExtensionPly - ExtensionDepthDifference + EndExtensionDepthDifference + 1;
            return strategyBase;
        }
    }

    private StrategyBase _subSearchStrategy;

    protected StrategyBase SubSearchStrategy
    {
        get
        {
            return _subSearchStrategy ??= CreateSubSearchStrategy();
        }
    }

    public static Random Random = new Random();

    protected StrategyBase(int depth, IPosition position)
    {
        configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
        var algorithmConfiguration = configurationProvider.AlgorithmConfiguration;
        var sortingConfiguration = algorithmConfiguration.SortingConfiguration;
        var generalConfiguration = configurationProvider.GeneralConfiguration;
        var bookConfiguration = configurationProvider.BookConfiguration;

        SuggestedThreshold = bookConfiguration.SuggestedThreshold;
        NonSuggestedThreshold = bookConfiguration.NonSuggestedThreshold;

        MaxEndGameDepth = configurationProvider.EndGameConfiguration.MaxEndGameDepth;
        SortDepth = sortingConfiguration.SortDepth;
        Mate = configurationProvider.Evaluation.Static.Mate;
        MateNegative = -Mate;
        SearchValue = Mate - 1;
        MinusSearchValue = -SearchValue;
        UseFutility = generalConfiguration.UseFutility;
        FutilityDepth = generalConfiguration.FutilityDepth;
        RazoringDepth = FutilityDepth + 1;
        UseAging = generalConfiguration.UseAging;
        Depth = (sbyte)depth;
        Position = position;
        IsPvEnabled = algorithmConfiguration.ExtensionConfiguration.IsPvEnabled;
        ExtensionDepthDifference = algorithmConfiguration.ExtensionConfiguration.DepthDifference[depth];
        EndExtensionDepthDifference = algorithmConfiguration.ExtensionConfiguration.EndDepthDifference[depth];

        SubSearchDepthThreshold = configurationProvider
                .AlgorithmConfiguration.SubSearchConfiguration.SubSearchDepthThreshold;
        SubSearchDepth = configurationProvider
                .AlgorithmConfiguration.SubSearchConfiguration.SubSearchDepth;
        SubSearchLevel = configurationProvider
                .AlgorithmConfiguration.SubSearchConfiguration.SubSearchLevel;
        UseSubSearch = configurationProvider
                .AlgorithmConfiguration.SubSearchConfiguration.UseSubSearch;

        MoveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
        MoveSorterProvider = ServiceLocator.Current.GetInstance<IMoveSorterProvider>();
        DataPoolService = ServiceLocator.Current.GetInstance<IDataPoolService>();

        DataPoolService.Initialize(Position);

        AlphaMargins = configurationProvider.AlgorithmConfiguration.MarginConfiguration.AlphaMargins;
        BetaMargins = configurationProvider.AlgorithmConfiguration.MarginConfiguration.BetaMargins;
        DeltaMargins = configurationProvider.AlgorithmConfiguration.MarginConfiguration.DeltaMargins;
    }

    public virtual int Size => 0;

    public virtual IResult GetResult()
    {
        if (MoveHistory.GetPly() < 0)
        {
            return GetFirstMove();
        }
        if (Position.GetPhase() == Phase.End)
        {
            return EndGameStrategy.GetResult();
        }
        return GetResult(MinusSearchValue, SearchValue, Depth);
    }

    public IResult GetFirstMove()
    {
        Result result = new Result();

        var moves = MoveHistory.GetFirstMoves();

        DistanceFromRoot = 0;
        MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

        int b = MinusSearchValue;
        sbyte d = (sbyte)(Depth - 2);
        int alpha = MinusSearchValue;

        for (byte i = 0; i < moves.Length; i++)
        {
            var move = moves[i];

            Position.MakeFirst(move);

            int value = -Search(b, -alpha, d);

            Position.UnMake();

            if (value > result.Value)
            {
                result.Value = value;
                result.Move = move;
            }

            if (value > alpha)
            {
                alpha = value;
            }
        }

        return result;
    }

    public virtual IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
    {
        Result result = new Result();
        if (IsDraw(result))
        {
            return result;
        }

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[Depth], pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        DistanceFromRoot = sortContext.Ply;
        MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

        if (CheckEndGame(moves.Count, result)) return result;

        if (moves.Count > 1)
        {
            SetResult(alpha, beta, depth, result, moves);
        }
        else
        {
            result.Move = moves[0];
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EvaluationSearch(int alpha, int beta)
    {
        if (CheckDraw())
            return 0;

        SearchContext context = GetCurrentContextForEvaluation();

        if (context.SearchResultType != SearchResultType.EndGame)
        {
            MoveBase move;
            int r;
            int b = -beta;

            MoveList moves = context.Moves;

            for (byte i = 0; i < moves.Count; i++)
            {
                move = moves[i];
                Position.Make(move);

                r = -Search(b, -alpha, 0);

                Position.UnMake();

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move;

                if (r >= beta)
                {
                    if (!move.IsAttack)
                    {
                        Sorters[1].Add(move.Key);

                        move.History++;
                    }
                    break;
                }

                if (r > alpha)
                    alpha = r;

                if (!move.IsAttack) move.Butterfly++;
            }
        }

        return context.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual int Search(int alpha, int beta, sbyte depth)
    {
        if (CheckDraw())
        {
            return 0;
        }

        if (depth < 1) return Evaluate(alpha, beta);

        if (Position.GetPhase() == Phase.End)
        {
            if (depth < 6 && MaxExtensionPly > MoveHistory.GetPly())
            {
                depth++;
            }
            return EndGameStrategy.Search(alpha, beta, depth);
        }

        SearchContext context = GetCurrentContext(alpha, beta, depth);

        if (SetSearchValue(alpha, beta, depth, context)) return context.Value;

        return context.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool SetSearchValue(int alpha, int beta, sbyte depth, SearchContext context)
    {
        switch (context.SearchResultType)
        {
            case SearchResultType.EndGame:
                return true;
            case SearchResultType.AlphaFutility:
                FutilitySearchInternal(alpha, beta, depth, context);
                if (context.SearchResultType == SearchResultType.EndGame)
                {
                    context.Value = alpha;
                    return true;
                }
                break;
            case SearchResultType.BetaFutility:
                context.Value = beta;
                return true;
            case SearchResultType.Razoring:
                SearchInternal(alpha, beta, --depth, context);
                break;
            case SearchResultType.None:
                SearchInternal(alpha, beta, depth, context);
                break;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void FutilitySearchInternal(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;

        for (byte i = 0; i < context.Moves.Count; i++)
        {
            move = context.Moves[i];

            Position.Make(move);

            if (!move.IsCheck && move.IsFutile)
            {
                Position.UnMake();
                continue;
            }

            r = -Search(b, -alpha, d);

            Position.UnMake();

            if (r <= context.Value)
                continue;

            context.Value = r;
            context.BestMove = move;

            if (r >= beta)
            {
                if (!move.IsAttack)
                {
                    Sorters[depth].Add(move.Key);

                    move.History += 1 << depth;
                }
                break;
            }
            else if (r > alpha)
            {
                alpha = r;
            }
        }

        if (context.Value == short.MinValue)
        {
            context.SearchResultType = SearchResultType.EndGame;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void SearchInternal(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;

        MoveList moves = context.Moves;

        for (byte i = 0; i < moves.Count; i++)
        {
            move = moves[i];
            Position.Make(move);

            r = -Search(b, -alpha, d);

            Position.UnMake();

            if (r <= context.Value)
                continue;

            context.Value = r;
            context.BestMove = move;

            if (r >= beta)
            {
                if (!move.IsAttack)
                {
                    Sorters[depth].Add(move.Key);

                    move.History += 1 << depth;
                }
                break;
            }

            if (r > alpha)
                alpha = r;

            if (!move.IsAttack) move.Butterfly++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SingleMoveSearch(short alpha, short beta, sbyte depth, SearchContext context)
    {
        Position.Make(context.Moves[0]);
        context.Value = -Search(-beta, -alpha, depth);
        context.BestMove = context.Moves[0];
        Position.UnMake();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void SetResult(int alpha, int beta, sbyte depth, Result result, MoveList moves)
    {
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        for (byte i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            Position.Make(move);

            int value = -Search(b, -alpha,  d);

            Position.UnMake();
            if (value > result.Value)
            {
                result.Value = value;
                result.Move = move;
            }

            if (value > alpha)
            {
                alpha = value;
            }

            if (alpha < beta) continue;
            break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual SearchContext GetCurrentContextForEvaluation()
    {
        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[1]);
        context.Moves = sortContext.GetAllMoves(Position);

        if (context.Moves.Count < 1)
        {
            context.SearchResultType = SearchResultType.EndGame;
            context.Value = MoveHistory.IsLastMoveWasCheck() ? MateNegative : 0;
        }
        else
        {
            context.SearchResultType = SearchResultType.None;
        }

        return context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual SearchContext GetCurrentContext(int alpha, int beta, sbyte depth, MoveBase pv = null)
    {
        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();

        if (Depth - depth > 1 && MaxExtensionPly > context.Ply && MoveHistory.ShouldExtend())
        {
            depth++;
        }

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[depth], pv);
        context.Moves = sortContext.GetAllMoves(Position);

        if (context.Moves.Count < 1)
        {
            context.SearchResultType = SearchResultType.EndGame;
            context.Value = MoveHistory.IsLastMoveWasCheck() ? MateNegative : 0;
        }
        else
        {
            context.SearchResultType = SetEndGameType(alpha, beta, depth);
        }

        return context;
    }

    protected virtual StrategyBase CreateSubSearchStrategy() => new NegaMaxMemoryStrategy(Depth - SubSearchDepth, Position);

    protected virtual StrategyBase CreateEndGameStrategy() => new LmrDeepEndGameStrategy(Math.Min(Depth + 1, MaxEndGameDepth), Position);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected SearchResultType SetEndGameType(int alpha, int beta, sbyte depth)
    {
        if (depth > RazoringDepth || MoveHistory.IsLastMoveWasCheck()) return SearchResultType.None;

        int value = Position.GetStaticValue();

        byte phase = Position.GetPhase();

        if (depth < RazoringDepth)
        {
            if (value + AlphaMargins[phase][depth] < alpha) return SearchResultType.AlphaFutility;
            if (value - BetaMargins[phase][depth] > beta) return SearchResultType.BetaFutility;
            return SearchResultType.None;
        }

        if (value + AlphaMargins[phase][depth] < alpha)
            return SearchResultType.Razoring;

        return SearchResultType.None;
    }

    protected virtual void InitializeSorters(int depth, IPosition position, MoveSorterBase mainSorter)
    {
        List<MoveSorterBase> sorters = new List<MoveSorterBase> { MoveSorterProvider.GetAttack(position) };

        var complexSorter = MoveSorterProvider.GetComplex(position);

        for (int i = 0; i < SortDepth[depth]; i++)
        {
            sorters.Add(mainSorter);
        }
        for (int i = SortDepth[depth]; i < depth + 3; i++)
        {
            sorters.Add(complexSorter);
        }

        Sorters = sorters.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int Evaluate(int alpha, int beta)
    {
        if (MoveHistory.IsLastMoveWasCheck())
            return EvaluationSearch(alpha, beta);

        int standPat = Position.GetValue();
        if (standPat >= beta)
            return beta;

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.SetForEvaluation(Sorters[0]);
        MoveList moves = sortContext.GetAllAttacks(Position);

        if (moves.Count < 1)
            return Math.Max(standPat, alpha);

        int b = -beta;
        int score;

        if (standPat < alpha - DeltaMargins[Position.GetPhase()])
        {
            for (byte i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                if (move.IsCheck || move.IsPromotionToQueen || move.IsQueenCaptured())
                {
                    score = -Evaluate(b, -alpha);

                    Position.UnMake();

                    if (score >= beta)
                        return beta;

                    if (score > alpha)
                        alpha = score;
                }
                else
                {
                    Position.UnMake();
                }
            }
        }
        else
        {
            if (alpha < standPat)
                alpha = standPat;

            for (byte i = 0; i < moves.Count; i++)
            {
                Position.Make(moves[i]);

                score = -Evaluate(b, -alpha);

                Position.UnMake();

                if (score >= beta)
                    return beta;

                if (score > alpha)
                    alpha = score;
            }
        }

        return alpha;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsEndGameDraw(Result result)
    {
        if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
        {
            result.GameResult = GameResult.ThreefoldRepetition;
            result.Value = 0;
            return true;
        }

        if (MoveHistory.IsFiftyMoves())
        {
            result.GameResult = GameResult.FiftyMoves;
            result.Value = 0;
            return true;
        }

        if (Position.IsDraw())
        {
            result.GameResult = GameResult.Draw;
            result.Value = 0;
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsDraw(Result result)
    {
        if (Position.GetPhase() == Phase.Opening) return false;

        if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
        {
            result.GameResult = GameResult.ThreefoldRepetition;
            result.Value = 0;
            return true;
        }

        if (Position.GetPhase() == Phase.Middle) return false;

        if (MoveHistory.IsFiftyMoves())
        {
            result.GameResult = GameResult.FiftyMoves;
            result.Value = 0;
            return true;
        }

        if (Position.IsDraw())
        {
            result.GameResult = GameResult.Draw;
            result.Value = 0;
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool CheckEndGame(int count, Result result)
    {
        if (count > 0) return false;

        if (MoveHistory.IsLastMoveWasCheck())
        {
            result.GameResult = GameResult.Mate;
            result.Value = Mate;
        }
        else
        {
            result.GameResult = GameResult.Pat;
            result.Value = 0;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool CheckEndGameDraw() => MoveHistory.IsThreefoldRepetition(Position.GetKey()) || MoveHistory.IsFiftyMoves() || Position.IsDraw();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool CheckDraw()
    {
        if (Position.GetPhase() == Phase.Opening) return false;

        if (MoveHistory.IsThreefoldRepetition(Position.GetKey())) return true;

        if (Position.GetPhase() == Phase.Middle) return false;

        return MoveHistory.IsFiftyMoves() || Position.IsDraw();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsLateEndGame()
    {
        IBoard board = Position.GetBoard();

        //if ((board.GetPieceBits(Pieces.WhiteQueen) | board.GetPieceBits(Pieces.BlackQueen)).Any()) return false;

        //var wr = board.GetPieceBits(Pieces.WhiteRook);
        //var br = board.GetPieceBits(Pieces.BlackRook);
        //var wb = board.GetPieceBits(Pieces.WhiteBishop);
        //var bb = board.GetPieceBits(Pieces.BlackBishop);
        //var wk = board.GetPieceBits(Pieces.WhiteKnight);
        //var bk = board.GetPieceBits(Pieces.BlackKnight);

        //if ((wr | br).IsZero()) return (wb | wk).Count() < 3 && (bb | bk).Count() < 3;

        //return (wr | wb | wk).Count() < 2 && (br | bb | bk).Count() < 2;

        return board.GetWhites().Remove(board.GetPieceBits(Pieces.WhitePawn)).Count() < 2 &&
            board.GetBlacks().Remove(board.GetPieceBits(Pieces.BlackPawn)).Count() < 2;
    }

    public override string ToString() => $"{GetType().Name}[{Depth}]";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool IsBlocked() => _isBlocked;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void ExecuteAsyncAction()
    {
        _isBlocked = true;
        Task.Factory.StartNew(() => { _isBlocked = false; });
    }

    protected List<MoveBase> GenerateFirstMoves(IMoveProvider provider)
    {
        List<MoveBase> moves = new List<MoveBase>
        {
            provider.GetMoves(Pieces.WhitePawn, Squares.B2).FirstOrDefault(m => m.To == Squares.B3),
            provider.GetMoves(Pieces.WhitePawn, Squares.C2).FirstOrDefault(m => m.To == Squares.C3),
            provider.GetMoves(Pieces.WhitePawn, Squares.C2).FirstOrDefault(m => m.To == Squares.C4),
            provider.GetMoves(Pieces.WhitePawn, Squares.D2).FirstOrDefault(m => m.To == Squares.D4),
            provider.GetMoves(Pieces.WhitePawn, Squares.D2).FirstOrDefault(m => m.To == Squares.D3),
            provider.GetMoves(Pieces.WhitePawn, Squares.E2).FirstOrDefault(m => m.To == Squares.E3),
            provider.GetMoves(Pieces.WhitePawn, Squares.E2).FirstOrDefault(m => m.To == Squares.E4),
            provider.GetMoves(Pieces.WhitePawn, Squares.G2).FirstOrDefault(m => m.To == Squares.G3),
            provider.GetMoves(Pieces.WhiteKnight, Squares.B1).FirstOrDefault(m => m.To == Squares.C3),
            provider.GetMoves(Pieces.WhiteKnight, Squares.G1).FirstOrDefault(m => m.To == Squares.F3)
        };

        return moves;
    }
}
