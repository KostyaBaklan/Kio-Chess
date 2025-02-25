using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
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
    protected bool IsPvEnabled;
    protected sbyte Depth;
    protected sbyte EndGameDepth;
    protected int SearchValue;
    protected int SearchValueMinusOne;
    protected int MinusSearchValue;
    protected sbyte RazoringDepth;
    protected int Ply;

    protected static int MaxExtensionPly;
    protected static int MaxRecuptureExtensionPly;
    protected readonly int RecuptureExtensionOffest;
    protected int ExtensionOffest;
    protected readonly int ExtensionDepth;
    protected int[] SortDepth;
    protected readonly int[][] AlphaMargins;
    protected readonly int[][] BetaMargins;

    protected readonly int NullWindow;
    protected readonly sbyte[] NullDepthReduction;
    protected readonly sbyte[] NullDepthExtendedReduction;
    protected readonly int NullDepthThreshold;

    protected const sbyte One = 1;
    protected const sbyte Zero = 0;
    protected readonly int Mate;
    protected readonly int MateNegative;

    protected Position Position;
    protected readonly Board _board;
    protected MoveSorterBase[] Sorters;
    protected readonly TranspositionTable Table;

    protected readonly MoveHistoryService MoveHistory;
    protected readonly MoveProvider MoveProvider;
    protected readonly IMoveSorterProvider MoveSorterProvider;
    protected readonly IConfigurationProvider configurationProvider;
    protected readonly DataPoolService DataPoolService;
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
        configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var algorithmConfiguration = configurationProvider.AlgorithmConfiguration;
        var sortingConfiguration = algorithmConfiguration.SortingConfiguration;
        var generalConfiguration = configurationProvider.GeneralConfiguration;

        SortDepth = sortingConfiguration.SortDepth;
        Mate = configurationProvider.Evaluation.Static.Mate;
        MateNegative = -Mate;
        SearchValue = Mate - 1;
        SearchValueMinusOne = SearchValue - 1;
        MinusSearchValue = -SearchValue;
        RazoringDepth = (sbyte)(generalConfiguration.FutilityDepth + 1);
        Depth = (sbyte)depth;
        EndGameDepth = configurationProvider.EndGameConfiguration.EndGameDepth[Depth];
        Position = position;
        _board = position.GetBoard();
        IsPvEnabled = algorithmConfiguration.ExtensionConfiguration.IsPvEnabled;

        RecuptureExtensionOffest = 3;
        ExtensionOffest = depth + algorithmConfiguration.ExtensionConfiguration.DepthDifference;
        ExtensionDepth = algorithmConfiguration.ExtensionConfiguration.ExtensionDepth;

        NullConfiguration nullConfiguration = configurationProvider.AlgorithmConfiguration.NullConfiguration;

        NullWindow = nullConfiguration.NullWindow;
        NullDepthReduction = nullConfiguration.NullDepthReduction;
        NullDepthExtendedReduction = nullConfiguration.NullDepthExtendedReduction;
        NullDepthThreshold = nullConfiguration.NullDepthThreshold + 1;

        MoveHistory = ContainerLocator.Current.Resolve<MoveHistoryService>();
        MoveProvider = ContainerLocator.Current.Resolve<MoveProvider>();
        MoveSorterProvider = ContainerLocator.Current.Resolve<IMoveSorterProvider>();
        DataPoolService = ContainerLocator.Current.Resolve<DataPoolService>();

        DataPoolService.Initialize(Position);

        var esf = ContainerLocator.Current.Resolve<IEvaluationServiceFactory>();

        AlphaMargins = new int[3][];
        BetaMargins = new int[3][];

        var ess = esf.GetEvaluationServices();

        for (byte i = 0; i < ess.Length; i++)
        {
            var es = ess[i];
            AlphaMargins[i] = new int[]
            {
                es.GetPieceValue(Pieces.WhitePawn),
                es.GetPieceValue(Pieces.WhiteBishop),
                es.GetPieceValue(Pieces.WhiteRook)+es.GetPieceValue(Pieces.WhitePawn),
                es.GetPieceValue(Pieces.WhiteQueen)
            };

            BetaMargins[i] = new int[]
            {
                es.GetPieceValue(Pieces.WhitePawn),
                es.GetPieceValue(Pieces.WhiteBishop)+25,
                es.GetPieceValue(Pieces.WhiteRook)+es.GetPieceValue(Pieces.WhitePawn)+25,
                es.GetPieceValue(Pieces.WhiteQueen)+25
            };
        }

        if (table == null)
        {
            var service = ContainerLocator.Current.Resolve<ITranspositionTableService>();

            Table = service.Create(depth);
        }
        else
        {
            Table = table;
        }

        AlphaDepth = (sbyte)(depth - 2);
    }
    public int Size => Table.Count;

    public abstract StrategyType Type { get; }

    #region Get Result

    public virtual IResult GetResult()
    {
        if (MoveHistory.GetPly() < 0)
        {
            return GetFirstMove();
        }
        if (MoveHistory.IsEndPhase())
        {
            return EndGameStrategy.GetResult();
        }
        return GetResult(MinusSearchValue, SearchValue, Depth);
    }

    public IResult GetFirstMove()
    {
        Result result = new Result();

        var moves = MoveHistory.GetFirstMoves();

        SetExtensionThresholds(0);

        int b = MinusSearchValue;
        sbyte d = (sbyte)(Depth - 2);
        int alpha = MinusSearchValue;

        for (byte i = 0; i < moves.Length; i++)
        {
            var move = moves[i];

            Position.MakeFirst(move);

            int value = -SearchBlack(b, -alpha, d);

            Position.UnMakeWhite();

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

        SortContext sortContext = GetSortContext(depth, pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        SetExtensionThresholds(sortContext.Ply);

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

    protected SortContext GetSortContext(sbyte depth, MoveBase pv)
    {
        pv = TryGetPv(pv);

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        if (pv != null)
        {
            sortContext.Set(Sorters[depth], pv.Key);
        }
        else
        {
            sortContext.Set(Sorters[depth]);
        }

        return sortContext;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected MoveBase TryGetPv(MoveBase pv)
    {
        if (pv == null)
        {
            var turn = Position.GetTurn();
            if (turn == Turn.White && Table.TryGetWhite(out var entry))
            {
                pv = MoveProvider.Get(entry.PvMove);
            }
            else if (turn == Turn.Black && Table.TryGetBlack(out entry))
            {
                pv = MoveProvider.Get(entry.PvMove);
            }
        }

        return pv;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetResult(int alpha, int beta, sbyte depth, Result result, MoveList moves)
    {
        if (Position.GetTurn() == Turn.White)
        {
            SetResultWhite(alpha, beta, depth, result, moves);
        }
        else
        {
            SetResultBlack(alpha, beta, depth, result, moves);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetExtensionThresholds(int ply)
    {
        //MaxRecuptureExtensionPly = ply + RecuptureExtensionOffest;
        Ply = ply;
        MaxExtensionPly = ply + ExtensionOffest;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetResultWhite(int alpha, int beta, sbyte depth, Result result, MoveList moves)
    {
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        for (byte i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            Position.MakeWhite(move);

            int value = -SearchBlack(b, -alpha, d);

            Position.UnMakeWhite();
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
    protected void SetResultBlack(int alpha, int beta, sbyte depth, Result result, MoveList moves)
    {
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        for (byte i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            Position.MakeBlack(move);

            int value = -SearchWhite(b, -alpha, d);

            Position.UnMakeBlack();
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

    #endregion

    #region Null Search

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected sbyte CalculateBlackDepth(int beta, sbyte depth, short pv)
    {
        if (ShouldExtend(beta, depth, pv, out var d)) return d;

        DoBlackNullMove();
        int nullValue = -NullWindowSerachWhite(NullWindow - beta, NullDepthReduction[depth]);
        UnDoBlackNullMove();

        return GetNullDepth(beta, depth, nullValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected sbyte CalculateWhiteDepth(int beta, sbyte depth, short pv)
    {
        if (ShouldExtend(beta, depth, pv, out var d)) return d;

        DoWhiteNullMove();
        int nullValue = -NullWindowSerachBlack(NullWindow - beta, NullDepthReduction[depth]);
        UnDoWhiteNullMove();

        return GetNullDepth(beta, depth, nullValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private sbyte GetNullDepth(int beta, sbyte depth, int nullValue)
    {
        if (nullValue < beta)
            return depth;

        MoveHistory.SetNull();
        return NullDepthExtendedReduction[depth];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ShouldExtend(int beta, sbyte depth, short pv, out sbyte newDepth)
    {
        newDepth = depth;

        if (MoveHistory.IsLastMoveWasCheck())
        {
            if (depth < ExtensionDepth && MoveHistory.GetPhase() != Phase.Opening && MoveHistory.GetPly() < MaxExtensionPly)
            {
                newDepth++;
            }

            return true;
        }

        return pv > -1 || beta > SearchValueMinusOne || MoveHistory.GetPly() - Ply < NullDepthThreshold;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int NullWindowSerachWhite(int beta, int depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateWhite(beta - NullWindow, beta);

        var moves = GetMovesForNullSearch(depth).AsSpan();

        if (moves.Length < 1)
            return MoveHistory.IsLastMoveWasCheck() ? MateNegative : 0;

        int d = depth - 1;
        int b = NullWindow - beta;
        int best = MinusSearchValue;
        byte i = 0;
        while (i < moves.Length && best < beta)
        {
            Position.MakeWhite(moves[i++]);

            best = -NullWindowSerachBlack(b, d);

            Position.UnMakeWhite();
        }
        return best;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int NullWindowSerachBlack(int beta, int depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateBlack(beta - NullWindow, beta);

        var moves = GetMovesForNullSearch(depth).AsSpan();

        if (moves.Length < 1)
            return MoveHistory.IsLastMoveWasCheck() ? MateNegative : 0;

        int d = depth - 1;
        int b = NullWindow - beta;
        int best = MinusSearchValue;
        byte i = 0;
        while (i < moves.Length && best < beta)
        {
            Position.MakeBlack(moves[i++]);

            best = -NullWindowSerachWhite(b, d);

            Position.UnMakeBlack();
        }
        return best;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MoveList GetMovesForNullSearch(int depth)
    {
        SortContext sortContext = DataPoolService.GetCurrentNullSortContext();
        sortContext.Set(Sorters[depth]);
        return sortContext.GetAllMoves(Position);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnDoWhiteNullMove() => Position.SetWhiteTurn();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoWhiteNullMove() => Position.SetBlackTurn();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnDoBlackNullMove() => Position.SetBlackTurn();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoBlackNullMove() => Position.SetWhiteTurn();

    #endregion

    #region Search

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual int SearchWhite(int alpha, int beta, sbyte depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateWhite(alpha, beta);

        if (MoveHistory.IsEndPhase())
            return EndGameStrategy.SearchWhite(alpha, beta, ++depth);

        TranspositionContext transpositionContext = GetWhiteTranspositionContext(beta, depth);
        if (transpositionContext.IsBetaExceeded) return beta;

        if (MoveHistory.CanUseNull())
        {
            depth = CalculateWhiteDepth(beta, depth, transpositionContext.Pv);

            if (depth < 1)
                return EvaluateWhite(alpha, beta);
        }

        SearchContext context = transpositionContext.Pv < 0
            ? GetCurrentContext(alpha, beta, depth)
            : GetCurrentContext(alpha, beta, depth, transpositionContext.Pv);

        if (SetSearchValueWhite(alpha, beta, depth, context) && transpositionContext.ShouldUpdate)
        {
            StoreWhiteValue(depth, (short)context.Value, context.BestMove);
        }
        return context.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual int SearchBlack(int alpha, int beta, sbyte depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateBlack(alpha, beta);

        if (MoveHistory.IsEndPhase())
            return EndGameStrategy.SearchBlack(alpha, beta, ++depth);

        TranspositionContext transpositionContext = GetBlackTranspositionContext(beta, depth);
        if (transpositionContext.IsBetaExceeded) return beta;

        if (MoveHistory.CanUseNull())
        {
            depth = CalculateBlackDepth(beta, depth, transpositionContext.Pv);

            if (depth < 1)
                return EvaluateBlack(alpha, beta);
        }

        SearchContext context = transpositionContext.Pv < 0
            ? GetCurrentContext(alpha, beta, depth)
            : GetCurrentContext(alpha, beta, depth, transpositionContext.Pv);

        if (SetSearchValueBlack(alpha, beta, depth, context) && transpositionContext.ShouldUpdate)
        {
            StoreBlackValue(depth, (short)context.Value, context.BestMove);
        }
        return context.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool SetSearchValueBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        switch (context.SearchResultType)
        {
            case SearchResultType.None:
                SearchInternalBlack(alpha, beta, depth, context);
                break;
            case SearchResultType.EndGame:
                return false;
            case SearchResultType.AlphaFutility:
                FutilitySearchInternalBlack(alpha, beta, depth, context);
                if (context.Value == short.MinValue)
                {
                    context.Value = alpha;
                    return false;
                }
                break;
            case SearchResultType.BetaFutility:
                context.Value = beta;
                return false;
            case SearchResultType.Razoring:
                SearchInternalBlack(alpha, beta, --depth, context);
                break;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool SetSearchValueWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        switch (context.SearchResultType)
        {
            case SearchResultType.None:
                SearchInternalWhite(alpha, beta, depth, context);
                break;
            case SearchResultType.EndGame:
                return false;
            case SearchResultType.AlphaFutility:
                FutilitySearchInternalWhite(alpha, beta, depth, context);
                if (context.Value == short.MinValue)
                {
                    context.Value = alpha;
                    return false;
                }
                break;
            case SearchResultType.BetaFutility:
                context.Value = beta;
                return false;
            case SearchResultType.Razoring:
                SearchInternalWhite(alpha, beta, --depth, context);
                break;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FutilitySearchInternalWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;

        var moves = context.Moves.AsSpan();
        for (int i = 0; i < moves.Length; i++)
        {
            move = moves[i];

            Position.MakeWhite(move);

            if (!move.IsCheck && move.IsFutile)
            {
                Position.UnMakeWhite();
                continue;
            }

            r = -SearchBlack(b, -alpha, d);

            Position.UnMakeWhite();

            if (r <= context.Value)
                continue;

            context.Value = r;
            context.BestMove = move.Key;

            if (r >= beta)
            {
                if (!move.IsAttack)
                {
                    context.Add(move.Key);

                    move.History += 1 << depth;
                }
                break;
            }
            if (r > alpha) alpha = r;

            if (!move.IsAttack) move.Butterfly++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FutilitySearchInternalBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;

        var moves = context.Moves.AsSpan();
        for (int i = 0; i < moves.Length; i++)
        {
            move = moves[i];

            Position.MakeBlack(move);

            if (!move.IsCheck && move.IsFutile)
            {
                Position.UnMakeBlack();
                continue;
            }

            r = -SearchWhite(b, -alpha, d);

            Position.UnMakeBlack();

            if (r <= context.Value)
                continue;

            context.Value = r;
            context.BestMove = move.Key;

            if (r >= beta)
            {
                if (!move.IsAttack)
                {
                    context.Add(move.Key);

                    move.History += 1 << depth;
                }
                break;
            }
            if (r > alpha) alpha = r;

            if (!move.IsAttack) move.Butterfly++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void SearchInternalWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;
        int a = -alpha;

        var moves = context.Moves.AsSpan();

        for (byte i = 0; i < moves.Length; i++)
        {
            move = moves[i];
            Position.MakeWhite(move);

            r = -SearchBlack(b, a, d);

            Position.UnMakeWhite();

            if (r <= context.Value)
                continue;

            context.Value = r;
            context.BestMove = move.Key;

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
            {
                alpha = r;
                a = -alpha;
            }

            if (!move.IsAttack) move.Butterfly++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void SearchInternalBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;
        int a = -alpha;

        var moves = context.Moves.AsSpan();

        for (byte i = 0; i < moves.Length; i++)
        {
            move = moves[i];
            Position.MakeBlack(move);

            r = -SearchWhite(b, a, d);

            Position.UnMakeBlack();

            if (r <= context.Value)
                continue;

            context.Value = r;
            context.BestMove = move.Key;

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
            {
                alpha = r;
                a = -alpha;
            }

            if (!move.IsAttack) move.Butterfly++;
        }
    }

    #endregion

    #region Evaluation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int EvaluateWhite(int alpha, int beta)
    {
        if (MoveHistory.IsLastMoveWasCheck())
            return EvaluationWhiteSearch(alpha, beta);

        int standPat = Position.GetWhiteValue();
        if (standPat >= beta)
            return beta;

        if (alpha < standPat)
            alpha = standPat;

        SortContext sortContext = DataPoolService.GetCurrentEvaluationSortContext();
        sortContext.SetForEvaluation(Sorters[0], alpha, standPat);
        MoveList moves = sortContext.GetAllForEvaluation(Position);

        if (moves.Count < 1)
            return Math.Max(standPat, alpha);

        int b = -beta;
        int a = -alpha;
        int score;

        for (byte i = 0; i < moves.Count; i++)
        {
            Position.MakeWhite(moves[i]);

            score = -EvaluateBlack(b, a);

            Position.UnMakeWhite();

            if (score >= beta)
                return beta;

            if (score > alpha)
            {
                alpha = score;
                a = -alpha;
            }
        }

        return alpha;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int EvaluateBlack(int alpha, int beta)
    {
        if (MoveHistory.IsLastMoveWasCheck())
            return EvaluationBlackSearch(alpha, beta);

        int standPat = Position.GetBlackValue();
        if (standPat >= beta)
            return beta;

        if (alpha < standPat)
            alpha = standPat;

        SortContext sortContext = DataPoolService.GetCurrentEvaluationSortContext();
        sortContext.SetForEvaluation(Sorters[0], alpha, standPat);
        MoveList moves = sortContext.GetAllForEvaluation(Position);

        if (moves.Count < 1)
            return Math.Max(standPat, alpha);

        int b = -beta;
        int a = -alpha;
        int score;

        for (byte i = 0; i < moves.Count; i++)
        {
            Position.MakeBlack(moves[i]);

            score = -EvaluateWhite(b, a);

            Position.UnMakeBlack();

            if (score >= beta)
                return beta;

            if (score > alpha)
            {
                alpha = score;
                a = -alpha;
            }
        }

        return alpha;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EvaluationWhiteSearch(int alpha, int beta)
    {
        if (CheckDraw())
            return 0;

        SearchContext context = GetCurrentContextForEvaluation();

        if (context.SearchResultType != SearchResultType.EndGame)
        {
            MoveBase move;
            int r;
            int b = -beta;

            var moves = context.Moves.AsSpan();
            for (int i = 0; i < moves.Length; i++)
            {
                move = moves[i];
                Position.MakeWhite(move);

                r = -SearchBlack(b, -alpha, 0);

                Position.UnMakeWhite();

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move.Key;

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
    public int EvaluationBlackSearch(int alpha, int beta)
    {
        if (CheckDraw())
            return 0;

        SearchContext context = GetCurrentContextForEvaluation();

        if (context.SearchResultType != SearchResultType.EndGame)
        {
            MoveBase move;
            int r;
            int b = -beta;

            var moves = context.Moves.AsSpan();
            for (int i = 0; i < moves.Length; i++)
            {
                move = moves[i];
                Position.MakeBlack(move);

                r = -SearchWhite(b, -alpha, 0);

                Position.UnMakeBlack();

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move.Key;

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

    #endregion

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
    protected SearchContext GetCurrentContext(int alpha, int beta, sbyte depth)
    {
        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[depth]);
        context.Moves = sortContext.GetAllMoves(Position);

        if (context.Moves.Count < 1)
        {
            context.SearchResultType = SearchResultType.EndGame;
            context.Value = MoveHistory.IsLastMoveWasCheck() ? MateNegative : 0;
        }
        else
        {
            context.SearchResultType = depth > RazoringDepth || MoveHistory.IsLastMoveWasCheck()
                ? SearchResultType.None
                : SetEndGameType(alpha, beta, depth);
        }

        return context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected SearchContext GetCurrentContext(int alpha, int beta, sbyte depth, short pvKey)
    {
        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[depth], pvKey);
        context.Moves = sortContext.GetAllMoves(Position);

        if (context.Moves.Count < 1)
        {
            context.SearchResultType = SearchResultType.EndGame;
            context.Value = MoveHistory.IsLastMoveWasCheck() ? MateNegative : 0;
        }
        else
        {
            context.SearchResultType = depth > RazoringDepth || MoveHistory.IsLastMoveWasCheck()
                ? SearchResultType.None
                : SetEndGameType(alpha, beta, depth);
        }

        return context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected SearchResultType SetEndGameType(int alpha, int beta, sbyte depth)
    {
        int value = Position.GetValue();

        byte phase = MoveHistory.GetPhase();

        if (depth < RazoringDepth)
        {
            if (value + AlphaMargins[phase][depth] < alpha) return SearchResultType.AlphaFutility;
            if (value - BetaMargins[phase][depth] > beta) return SearchResultType.BetaFutility;
            return SearchResultType.None;
        }

        return value + AlphaMargins[phase][depth] < alpha ? SearchResultType.Razoring : SearchResultType.None;
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
    protected bool IsEndGameDraw(Result result)
    {
        if (MoveHistory.IsThreefoldRepetition())
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
        if (MoveHistory.GetPhase() == Phase.Opening) return false;

        if (MoveHistory.IsThreefoldRepetition())
        {
            result.GameResult = GameResult.ThreefoldRepetition;
            result.Value = 0;
            return true;
        }

        if (MoveHistory.GetPhase() == Phase.Middle) return false;

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
    protected bool CheckDraw() => MoveHistory.IsThreefoldRepetition() || MoveHistory.IsFiftyMoves() || Position.IsDraw();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsLateEndGame() => _board.IsLateEndGame();

    public override string ToString() => $"{GetType().Name}[{Depth}]";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TranspositionContext GetWhiteTranspositionContext(int beta, sbyte depth)
    {
        if (!Table.TryGetWhite(out var entry)) return new TranspositionContext(-1);

        TranspositionContext context = new TranspositionContext(entry.PvMove);

        if (entry.Depth < depth) return context;

        if (entry.Value < beta)
            context.ShouldUpdate = false;
        else
            context.IsBetaExceeded = true;

        return context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TranspositionContext GetBlackTranspositionContext(int beta, sbyte depth)
    {
        if (!Table.TryGetBlack(out var entry)) return new TranspositionContext(-1);

        TranspositionContext context = new TranspositionContext(entry.PvMove);

        if (entry.Depth < depth) return context;

        if (entry.Value < beta)
            context.ShouldUpdate = false;
        else
            context.IsBetaExceeded = true;

        return context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void StoreWhiteValue(sbyte depth, short value, short bestMove)
        => Table.SetWhite(new TranspositionEntry { Depth = depth, Value = value, PvMove = bestMove });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void StoreBlackValue(sbyte depth, short value, short bestMove)
        => Table.SetBlack(new TranspositionEntry { Depth = depth, Value = value, PvMove = bestMove });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Table.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlocked() => Table.IsBlocked();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExecuteAsyncAction() => Table.Update();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual StrategyBase CreateEndGameStrategy() => new IdLmrDeepEndStrategy(EndGameDepth, Position, Table);
}
