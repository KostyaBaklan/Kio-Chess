﻿using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Services;
using Engine.Sorting.Sorters;
using Engine.Strategies.End;
using Engine.Strategies.Models;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Base;

public abstract class StrategyBase 
{
    protected sbyte AlphaDepth;
    protected readonly TranspositionTable Table; protected bool UseAging;
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
    protected static int MaxExtensionPly;
    protected static int MaxRecuptureExtensionPly;

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

    protected int NullDepthReduction;
    protected int MinReduction;
    protected int MaxReduction;
    protected int NullWindow;
    protected int NullDepthOffset;
    protected int NullDepthThreshold;

    protected Position Position;
    protected readonly Board _board;
    protected MoveSorterBase[] Sorters;
    protected NullMoveSorter NullSorter;

    protected readonly MoveHistoryService MoveHistory;
    protected readonly MoveProvider MoveProvider;
    protected readonly IMoveSorterProvider MoveSorterProvider;
    protected readonly IConfigurationProvider configurationProvider;
    protected readonly IDataPoolService DataPoolService;

    private StrategyBase _endGameStrategy;
    protected StrategyBase EndGameStrategy
    {
        get
        {
            return _endGameStrategy ??= CreateEndGameStrategy();
        }
    }

    public static Random Random = new Random();

    protected StrategyBase(int depth, Position position, TranspositionTable table = null)
    {
        configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
        var algorithmConfiguration = configurationProvider.AlgorithmConfiguration;
        var sortingConfiguration = algorithmConfiguration.SortingConfiguration;
        var generalConfiguration = configurationProvider.GeneralConfiguration;
        var bookConfiguration = configurationProvider.BookConfiguration;

        SuggestedThreshold = bookConfiguration.SuggestedThreshold;
        NonSuggestedThreshold = bookConfiguration.NonSuggestedThreshold;

        NullDepthThreshold = configurationProvider.AlgorithmConfiguration.NullConfiguration.NullDepthReduction;
        NullDepthReduction = NullDepthThreshold + 1;
        NullDepthOffset = depth - configurationProvider.AlgorithmConfiguration.NullConfiguration.NullDepthOffset;

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
        _board = position.GetBoard();
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

        MoveHistory = ServiceLocator.Current.GetInstance<MoveHistoryService>();
        MoveProvider = ServiceLocator.Current.GetInstance<MoveProvider>();
        MoveSorterProvider = ServiceLocator.Current.GetInstance<IMoveSorterProvider>();
        DataPoolService = ServiceLocator.Current.GetInstance<IDataPoolService>();

        DataPoolService.Initialize(Position);

        NullSorter = new NullMoveSorter(Position);

        AlphaMargins = configurationProvider.AlgorithmConfiguration.MarginConfiguration.AlphaMargins;
        BetaMargins = configurationProvider.AlgorithmConfiguration.MarginConfiguration.BetaMargins;
        DeltaMargins = configurationProvider.AlgorithmConfiguration.MarginConfiguration.DeltaMargins;
        if (table == null)
        {
            var service = ServiceLocator.Current.GetInstance<ITranspositionTableService>();

            Table = service.Create(depth);
        }
        else
        {
            Table = table;
        }

        AlphaDepth = (sbyte)(depth - 2);
    }
    public int Size => Table.Count; 
    
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

        SetExtensionThresholds(0, 0);

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
            return result;

        if (pv == null && Table.TryGet(Position.GetKey(), out var entry))
        {
            pv = GetPv(entry.PvMove);
        }

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[depth], pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        SetExtensionThresholds(depth, sortContext.Ply);

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

    protected void SetExtensionThresholds(sbyte depth, int ply)
    {
        DistanceFromRoot = ply;
        MaxRecuptureExtensionPly = DistanceFromRoot + 4;
        MaxExtensionPly = DistanceFromRoot + depth + ExtensionDepthDifference;
    }

    public virtual int Search(int alpha, int beta, sbyte depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return Evaluate(alpha, beta);

        if (Position.GetPhase() == Phase.End) return EndGameStrategy.Search(alpha, beta, ++depth);

        if (NullDepthOffset > depth && beta < SearchValue && !MoveHistory.IsLastMoveWasCheck())
        {
            Position.SwapTurn();
            var nullValue = -NullSearch(1 - beta, depth - NullDepthReduction);
            Position.SwapTurn();
            if (nullValue >= beta)
            {
                return nullValue;
            }
        }

        TranspositionContext transpositionContext = GetTranspositionContext(beta, depth);
        if (transpositionContext.IsBetaExceeded) return beta;

        SearchContext context = GetCurrentContext(alpha, beta, ref depth, transpositionContext.Pv);

        return SetSearchValue(alpha, beta, depth, context) || transpositionContext.NotShouldUpdate
            ? context.Value
            : StoreValue(depth, (short)context.Value, context.BestMove.Key);
    }

    private int NullSearch(int beta, int depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return Evaluate(beta - 1, beta);

        int alpha = 1-beta;
        MoveBase move;
        int r;
        int d = depth - 1;

        SortContext sortContext = DataPoolService.GetCurrentNullSortContext();
        sortContext.SetNull(NullSorter);
        MoveList moves = sortContext.GetAllMovesForNull(Position);

        for (byte i = 0; i < moves.Count; i++)
        {
            move = moves[i];
            Position.Make(move);

            r = -NullSearch(alpha, d);

            Position.UnMake();

            if (r >= beta)
                return beta;
        }

        return beta - 1;
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
                        context.Add(move.Key);

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
    private void FutilitySearchInternal(int alpha, int beta, sbyte depth, SearchContext context)
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
                    context.Add(move.Key);

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
                    context.Add(move.Key);

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
    protected virtual void SetResult(int alpha, int beta, sbyte depth, Result result, MoveList moves)
    {
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        for (byte i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            Position.Make(move);

            int value = -Search(b, -alpha, d);

            Position.UnMake();
            if (value > result.Value)
            {
                result.Value = value;
                result.Move = move;
            }

            if (value > alpha)
                alpha = value;

            if (alpha < beta) continue;
            break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SearchContext GetCurrentContextForEvaluation()
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
    protected SearchContext GetCurrentContext(int alpha, int beta, ref sbyte depth, MoveBase pv = null)
    {
        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();

        if (MaxExtensionPly > context.Ply && (MoveHistory.ShouldExtend() || MaxRecuptureExtensionPly > context.Ply && MoveHistory.IsRecapture()))
            depth++;

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

    protected void InitializeSorters(int depth, Position position, MoveSorterBase mainSorter)
    {
        List<MoveSorterBase> sorters = new List<MoveSorterBase> { MoveSorterProvider.GetAttack(position) };

        var complexSorter = MoveSorterProvider.GetComplex(position);

        for (int i = 0; i < SortDepth[depth]; i++)
        {
            sorters.Add(mainSorter);
        }
        for (int i = 0; i < depth - SortDepth[depth] - 1; i++)
        {
            sorters.Add(complexSorter);
        }
        for (int i = 0; i < 3; i++)
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
    protected bool CheckDraw() => MoveHistory.IsThreefoldRepetition(Position.GetKey()) || MoveHistory.IsFiftyMoves() || Position.IsDraw();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsLateEndGame() => _board.GetWhites().Remove(_board.GetPieceBits(Pieces.WhitePawn)).Count() < 2 &&
            _board.GetBlacks().Remove(_board.GetPieceBits(Pieces.BlackPawn)).Count() < 2;

    public override string ToString() => $"{GetType().Name}[{Depth}]";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TranspositionContext GetTranspositionContext(int beta, sbyte depth)
    {
        TranspositionContext context = new TranspositionContext();

        if (!Table.TryGet(Position.GetKey(), out var entry)) return context;

        context.Pv = GetPv(entry.PvMove);

        if (context.Pv == null || entry.Depth < depth) return context;

        if (entry.Value >= beta)
            context.IsBetaExceeded = true;
        else
            context.NotShouldUpdate = true;

        return context;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MoveBase GetHashMove() => !Table.TryGet(Position.GetKey(), out var entry) ? null : GetPv(entry.PvMove);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected short StoreValue(sbyte depth, short value, short bestMove)
    {
        Table.Set(Position.GetKey(), new TranspositionEntry { Depth = depth, Value = value, PvMove = bestMove });

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Table.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlocked() => Table.IsBlocked();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExecuteAsyncAction() => Table.Update();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected MoveBase GetPv(short entry)
    {
        var pv = MoveProvider.Get(entry);

        return pv.Turn != Position.GetTurn()
            ? null
            : pv;
    }

    protected StrategyBase CreateEndGameStrategy()
    {
        int depth = Depth + 1;
        if (Depth < MaxEndGameDepth)
            depth++;
        return new IdLmrDeepEndStrategy(depth, Position, Table);
    }
}
