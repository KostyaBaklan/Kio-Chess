using CommonServiceLocator;
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
    protected bool UseAging;
    protected bool IsPvEnabled;
    protected sbyte Depth;
    protected int SearchValue;
    protected int MinusSearchValue;
    protected sbyte RazoringDepth;
    protected bool UseFutility;
    protected short MaxEndGameDepth;

    protected static int MaxExtensionPly;
    protected static int MaxRecuptureExtensionPly;
    protected readonly int RecuptureExtensionOffest;
    protected readonly int ExtensionOffest;

    protected int[] SortDepth;
    protected readonly int[][] AlphaMargins;
    protected readonly int[][] BetaMargins;
    protected readonly int[] DeltaMargins;

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

    protected Position Position;
    protected readonly Board _board;
    protected MoveSorterBase[] Sorters;
    protected readonly TranspositionTable Table;

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

        MaxEndGameDepth = configurationProvider.EndGameConfiguration.MaxEndGameDepth;
        SortDepth = sortingConfiguration.SortDepth;
        Mate = configurationProvider.Evaluation.Static.Mate;
        MateNegative = -Mate;
        SearchValue = Mate - 1;
        MinusSearchValue = -SearchValue;
        UseFutility = generalConfiguration.UseFutility;
        RazoringDepth = (sbyte)(generalConfiguration.FutilityDepth + 1);
        UseAging = generalConfiguration.UseAging;
        Depth = (sbyte)depth;
        Position = position;
        _board = position.GetBoard();
        IsPvEnabled = algorithmConfiguration.ExtensionConfiguration.IsPvEnabled;

        RecuptureExtensionOffest = 3;
        ExtensionOffest = depth * 2 / 3;

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
        if (pv!=null)
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
        MaxRecuptureExtensionPly = ply + RecuptureExtensionOffest;
        MaxExtensionPly = ply + ExtensionOffest;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual int SearchWhite(int alpha, int beta, sbyte depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateWhite(alpha, beta);

        if (MoveHistory.IsEndPhase())
            return EndGameStrategy.SearchWhite(alpha, beta, ++depth);

        TranspositionContext transpositionContext = GetWhiteTranspositionContext(beta, depth);
        if (transpositionContext.IsBetaExceeded) return beta;

        SearchContext context = transpositionContext.Pv < 0
            ? GetCurrentContext(alpha, beta, ref depth)
            : GetCurrentContext(alpha, beta, ref depth, transpositionContext.Pv);

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

        SearchContext context = transpositionContext.Pv < 0
            ? GetCurrentContext(alpha, beta, ref depth)
            : GetCurrentContext(alpha, beta, ref depth, transpositionContext.Pv);

        if (SetSearchValueBlack(alpha, beta, depth, context) && transpositionContext.ShouldUpdate)
        {
            StoreBlackValue(depth, (short)context.Value, context.BestMove);
        }
        return context.Value;
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

            MoveList moves = context.Moves;

            for (byte i = 0; i < moves.Count; i++)
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

            MoveList moves = context.Moves;

            for (byte i = 0; i < moves.Count; i++)
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
                if (context.SearchResultType == SearchResultType.EndGame)
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
                if (context.SearchResultType == SearchResultType.EndGame)
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

        MoveList moves = context.Moves;
        for (byte i = 0; i < moves.Count; i++)
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
    private void FutilitySearchInternalBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;

        MoveList moves = context.Moves;
        for (byte i = 0; i < moves.Count; i++)
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
    protected virtual void SearchInternalWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;
        int a = -alpha;

        MoveList moves = context.Moves;

        for (byte i = 0; i < moves.Count; i++)
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

        MoveList moves = context.Moves;

        for (byte i = 0; i < moves.Count; i++)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void SetResultWhite(int alpha, int beta, sbyte depth, Result result, MoveList moves)
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
    protected virtual void SetResultBlack(int alpha, int beta, sbyte depth, Result result, MoveList moves)
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
    protected SearchContext GetCurrentContext(int alpha, int beta, ref sbyte depth)
    {
        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();

        //if (MaxExtensionPly > context.Ply && (MoveHistory.ShouldExtend() || MaxRecuptureExtensionPly > context.Ply && MoveHistory.IsRecapture()))
        //    depth++;

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
    protected SearchContext GetCurrentContext(int alpha, int beta, ref sbyte depth, short pvKey)
    {
        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();

        //if (MaxExtensionPly > context.Ply && (MoveHistory.ShouldExtend() || MaxRecuptureExtensionPly > context.Ply && MoveHistory.IsRecapture()))
        //    depth++;

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
        int value = Position.GetStaticValue();

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
        sortContext.SetForEvaluation(Sorters[0],alpha, standPat);
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
        if (MoveHistory.GetPhase() == Phase.Opening) return false;

        if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
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
    protected bool CheckDraw() => MoveHistory.IsThreefoldRepetition(Position.GetKey()) || MoveHistory.IsFiftyMoves() || Position.IsDraw();

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

    protected virtual StrategyBase CreateEndGameStrategy()
    {
        int depth = Depth + 1;
        if (Depth < MaxEndGameDepth)
            depth++;
        return new IdLmrDeepEndStrategy(depth, Position, Table);
    }
}
