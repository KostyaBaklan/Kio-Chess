using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Services;
using Engine.Sorting;
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
    protected readonly int OneReplyDepthDifference;
    protected int MaxOneReplyPly;
    protected int[] SortDepth;
    protected readonly int[][] AlphaMargins;
    protected readonly int[][] BetaMargins;

    protected readonly int NullWindow;
    protected readonly sbyte[] NullDepthReduction;
    protected readonly sbyte[] NullDepthExtendedReduction;
    protected readonly int NullDepthThreshold;

    protected const sbyte One = 1;
    protected const sbyte Zero = 0;
    public const short MinusOne = -1;
    protected readonly int Mate;
    protected readonly int MateNegative;
    protected sbyte CutoffDepth;

    // Delta pruning fields for qsearch optimization
    protected int DeltaPruningMargin;

    protected Position Position;
    protected readonly Board _board;
    protected EvaluationSorter EvaluationSorter;
    protected MoveSorterBase BaseSorter;
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

    public static Random Random = new();

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
        CutoffDepth = generalConfiguration.CutoffDepth[Depth];

        RecuptureExtensionOffest = 3;
        ExtensionOffest = depth + algorithmConfiguration.ExtensionConfiguration.DepthDifference;
        ExtensionDepth = algorithmConfiguration.ExtensionConfiguration.ExtensionDepth;
        OneReplyDepthDifference = Math.Min(2 * Depth, algorithmConfiguration.ExtensionConfiguration.OneReplyDepthDifference);

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
        var marginConfiguration = algorithmConfiguration.MarginConfiguration;

        for (byte i = 0; i < ess.Length; i++)
        {
            var es = ess[i];
            AlphaMargins[i] = new int[]
            {
                es.GetPieceValue(Pieces.WhitePawn),
                es.GetPieceValue(Pieces.WhiteBishop)+marginConfiguration.AlphaOffset[i][0],
                es.GetPieceValue(Pieces.WhiteRook)+es.GetPieceValue(Pieces.WhitePawn)+marginConfiguration.AlphaOffset[i][1],
                es.GetPieceValue(Pieces.WhiteQueen)+marginConfiguration.AlphaOffset[i][2]
            };

            BetaMargins[i] = new int[]
            {
                es.GetPieceValue(Pieces.WhitePawn),
                es.GetPieceValue(Pieces.WhiteBishop)+marginConfiguration.BetaOffset[i][0],
                es.GetPieceValue(Pieces.WhiteRook)+es.GetPieceValue(Pieces.WhitePawn)+marginConfiguration.BetaOffset[i][1],
                es.GetPieceValue(Pieces.WhiteQueen)+marginConfiguration.BetaOffset[i][2]
            };
        }

        // Load delta pruning margins from configuration
        DeltaPruningMargin = marginConfiguration.DeltaPruningMargin;

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

        DataPoolService.Resize(Table);

        if (MoveHistory.IsEndPhase())
        {
            return EndGameStrategy.GetResult();
        }
        return GetResult(MinusSearchValue, SearchValue, Depth);
    }

    public IResult GetFirstMove()
    {
        Result result = new();

        var moves = MoveHistory.GetFirstMoves();

        SetExtensionThresholds(0);

        int b = MinusSearchValue;
        sbyte d = (sbyte)(Depth - 1);
        int alpha = MinusSearchValue;

        for (byte i = 0; i < moves.Length; i++)
        {
            var move = MoveProvider.Get(moves[i].Key);

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
        Result result = new();
        if (IsDraw(result))
            return result;

        SortContext sortContext = GetSortContext(depth, pv);

        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();
        sortContext.GetAllMoves(Position, ref context.Moves);

        SetExtensionThresholds(sortContext.Ply);

        if (CheckEndGame(context.Moves.Count, result)) return result;

        if (context.Moves.Count > 1)
        {
            SetResult(alpha, beta, depth, result, ref context.Moves);
        }
        else
        {
            result.Move = MoveProvider.Get(context.Moves[0].Key);
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
    protected void SetResult(int alpha, int beta, sbyte depth, Result result, ref MoveHistoryList moves)
    {
        if (Position.GetTurn() == Turn.White)
        {
            SetResultWhite(alpha, beta, depth, result, ref moves);
        }
        else
        {
            SetResultBlack(alpha, beta, depth, result, ref moves);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetExtensionThresholds(int ply)
    {
        //MaxRecuptureExtensionPly = ply + RecuptureExtensionOffest;
        Ply = ply;
        MaxExtensionPly = ply + ExtensionOffest;
        MaxOneReplyPly = ply + OneReplyDepthDifference;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetResultWhite(int alpha, int beta, sbyte depth, Result result, ref MoveHistoryList moves)
    {
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        for (byte i = 0; i < moves.Count; i++)
        {
            var move = MoveProvider.Get(moves[i].Key);
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
    protected void SetResultBlack(int alpha, int beta, sbyte depth, Result result, ref MoveHistoryList moves)
    {
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);

        for (byte i = 0; i < moves.Count; i++)
        {
            var move = MoveProvider.Get(moves[i].Key);
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
    private bool CanUseNull(int beta)
    {
        return MoveHistory.CanUseNull() && !MoveHistory.IsLastMoveWasCheck()
            && !(beta > SearchValueMinusOne || MoveHistory.GetPly() - Ply < NullDepthThreshold || IsLikelyZugzwangPosition());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private sbyte CalculateBlackDepth(int beta, sbyte depth)
    {
        Position.SetWhiteTurn();
        int nullValue = -NullWindowSearchWhite(NullWindow - beta, NullDepthReduction[depth]);
        Position.SetBlackTurn();

        return GetNullDepth(beta, depth, nullValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private sbyte CalculateWhiteDepth(int beta, sbyte depth)
    {
        Position.SetBlackTurn();
        int nullValue = -NullWindowSearchBlack(NullWindow - beta, NullDepthReduction[depth]);
        Position.SetWhiteTurn();

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

    /// <summary>
    /// Detects positions where Zugzwang is likely based on material composition.
    /// Zugzwang is most common in endgames with limited material.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLikelyZugzwangPosition()
    {
        // Skip Zugzwang detection in opening and early middle game
        switch (MoveHistory.GetPhase())
        {
            case Phase.Middle:
                return IsModerateZugzwangRisk();
            case Phase.End:
                if (_board.IsLateEndGame())
                {
                    // Very late endgame - high Zugzwang risk
                    return _board.GetTotalNonKingPieces() < 5 || _board.IsZugzwangRisk();
                }
                return IsModerateZugzwangRisk();
            default:
                return false;
        }
    }

    /// <summary>
    /// Detects moderate Zugzwang risk scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsModerateZugzwangRisk()
    {
        // Moderate piece count but still endgame
        return _board.GetTotalNonKingPieces() < 7 && (_board.HasAsymmetricMaterial() || _board.IsQueenlessEndgame());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int NullWindowSearchWhite(int beta, int depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateWhite(beta - NullWindow, beta);

        short pv = Table.TryGetWhite(out var entry) ? entry.PvMove : MinusOne;

        ref MoveHistoryList moves = ref GetMovesForNullSearch(depth, pv);

        if (moves.Count < 1)
            return MoveHistory.IsLastMoveWasCheck() ? MateNegative : 0;

        int d = depth - 1;
        int b = NullWindow - beta;
        int best = MinusSearchValue;
        byte i = 0;
        while (i < moves.Count && best < beta)
        {
            Position.MakeWhite(MoveProvider.Get(moves[i++].Key));

            best = -NullWindowSearchBlack(b, d);

            Position.UnMakeWhite();
        }
        return best;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int NullWindowSearchBlack(int beta, int depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateBlack(beta - NullWindow, beta);

        short pv = Table.TryGetBlack(out var entry) ? entry.PvMove : MinusOne;

        ref MoveHistoryList moves = ref GetMovesForNullSearch(depth, pv);

        if (moves.Count < 1)
            return MoveHistory.IsLastMoveWasCheck() ? MateNegative : 0;

        int d = depth - 1;
        int b = NullWindow - beta;
        int best = MinusSearchValue;
        byte i = 0;
        while (i < moves.Count && best < beta)
        {
            Position.MakeBlack(MoveProvider.Get(moves[i++].Key));

            best = -NullWindowSearchWhite(b, d);

            Position.UnMakeBlack();
        }
        return best;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref MoveHistoryList GetMovesForNullSearch(int depth, short pv)
    {
        SortContext sortContext = DataPoolService.GetCurrentNullSortContext();
        if (pv < 0)
        {
            sortContext.Set(Sorters[depth]);
        }
        else
        {
            sortContext.Set(Sorters[depth], pv);
        }

        ref MoveHistoryList moves = ref DataPoolService.GetCurrentMoveHistoryList();
        moves.Clear();
        sortContext.GetAllMoves(Position, ref moves);
        return ref moves;
    }

    #endregion

    #region Search

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual int SearchWhite(int alpha, int beta, sbyte depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateWhite(alpha, beta);

        if (MoveHistory.IsEndPhase())
            return EndGameStrategy.SearchWhite(alpha, beta, ++depth);

        return CommonWhiteSearch(alpha, beta, depth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual int SearchBlack(int alpha, int beta, sbyte depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateBlack(alpha, beta);

        if (MoveHistory.IsEndPhase())
            return EndGameStrategy.SearchBlack(alpha, beta, ++depth);

        return CommonBlackSearch(alpha, beta, depth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int CommonWhiteSearch(int alpha, int beta, sbyte depth)
    {
        if (Table.TryGetWhite(out var entry))
        {
            if (entry.Depth >= depth && entry.Depth < CutoffDepth && (entry.Type == TranspositionEntryType.Exact
                    || (entry.Type == TranspositionEntryType.LowerBound && entry.Value >= beta)
                    || (entry.Type == TranspositionEntryType.UpperBound && entry.Value <= alpha)))
                return entry.Value;

            return CommonWhitePvSearch(alpha, beta, depth, entry);
        }
        return CommonWhiteNonPvSearch(alpha, beta, depth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int CommonBlackSearch(int alpha, int beta, sbyte depth)
    {
        if (Table.TryGetBlack(out var entry))
        {
            if (entry.Depth >= depth && entry.Depth < CutoffDepth && (entry.Type == TranspositionEntryType.Exact
                    || (entry.Type == TranspositionEntryType.LowerBound && entry.Value >= beta)
                    || (entry.Type == TranspositionEntryType.UpperBound && entry.Value <= alpha)))
                return entry.Value;

            return CommonBlackPvSearch(alpha, beta, depth, entry);
        }
        return CommonBlackNonPvSearch(alpha, beta, depth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CommonWhitePvSearch(int alpha, int beta, sbyte depth, TranspositionEntry entry)
    {
        if (CanUseNull(beta))
        {
            depth = CalculateWhiteDepth(beta, depth);

            if (depth < 1)
                return EvaluateWhite(alpha, beta);
        }

        SearchContext context = GetCurrentContext(alpha, beta, depth, entry.PvMove);

        if (SetSearchValueWhite(alpha, beta, ref depth, context) && depth > entry.Depth)
        {
            TranspositionEntryType entryType = ComputeTranspositionEntryType(alpha, beta, context.Value);

            Table.SetWhite(new TranspositionEntry { Depth = depth, Value = (short)context.Value, PvMove = context.BestMove, Type = entryType });
        }
        return context.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CommonWhiteNonPvSearch(int alpha, int beta, sbyte depth)
    {
        if (CanUseNull(beta))
        {
            depth = CalculateWhiteDepth(beta, depth);

            if (depth < 1)
                return EvaluateWhite(alpha, beta);
        }

        SearchContext context = GetCurrentContext(alpha, beta, depth);

        if (SetSearchValueWhite(alpha, beta, ref depth, context))
        {
            TranspositionEntryType entryType = ComputeTranspositionEntryType(alpha, beta, context.Value);

            Table.SetWhite(new TranspositionEntry { Depth = depth, Value = (short)context.Value, PvMove = context.BestMove, Type = entryType });
        }

        return context.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CommonBlackPvSearch(int alpha, int beta, sbyte depth, TranspositionEntry entry)
    {
        if (CanUseNull(beta))
        {
            depth = CalculateBlackDepth(beta, depth);

            if (depth < 1)
                return EvaluateBlack(alpha, beta);
        }

        SearchContext context = GetCurrentContext(alpha, beta, depth, entry.PvMove);

        if (SetSearchValueBlack(alpha, beta, ref depth, context) && depth > entry.Depth)
        {
            TranspositionEntryType entryType = ComputeTranspositionEntryType(alpha, beta, context.Value);

            Table.SetBlack(new TranspositionEntry { Depth = depth, Value = (short)context.Value, PvMove = context.BestMove, Type = entryType });
        }
        return context.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CommonBlackNonPvSearch(int alpha, int beta, sbyte depth)
    {
        if (CanUseNull(beta))
        {
            depth = CalculateBlackDepth(beta, depth);

            if (depth < 1)
                return EvaluateBlack(alpha, beta);
        }

        SearchContext context = GetCurrentContext(alpha, beta, depth);

        if (SetSearchValueBlack(alpha, beta, ref depth, context))
        {
            TranspositionEntryType entryType = ComputeTranspositionEntryType(alpha, beta, context.Value);

            Table.SetBlack(new TranspositionEntry { Depth = depth, Value = (short)context.Value, PvMove = context.BestMove, Type = entryType });
        }
        return context.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TranspositionEntryType ComputeTranspositionEntryType(int alpha, int beta, int value)
    {
        return value <= alpha
            ? TranspositionEntryType.UpperBound
            : value >= beta ? TranspositionEntryType.LowerBound : TranspositionEntryType.Exact;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool SetSearchValueBlack(int alpha, int beta, ref sbyte depth, SearchContext context)
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
            case SearchResultType.OneReply:
                OneReplySearchBlack(alpha, beta, ++depth, context);
                return true;
            case SearchResultType.Check:
                SearchInternalBlack(alpha, beta, ++depth, context);
                return true;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool SetSearchValueWhite(int alpha, int beta, ref sbyte depth, SearchContext context)
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
            case SearchResultType.OneReply:
                OneReplySearchWhite(alpha, beta, ++depth, context);
                return true;
            case SearchResultType.Check:
                SearchInternalWhite(alpha, beta, ++depth, context);
                return true;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OneReplySearchWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        Position.MakeWhite(context.GetMove(0));

        context.Value = -SearchBlack(-beta, -alpha, (sbyte)(depth - 1));

        Position.UnMakeWhite();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OneReplySearchBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        Position.MakeBlack(context.GetMove(0));

        context.Value = -SearchWhite(-beta, -alpha, (sbyte)(depth - 1));

        Position.UnMakeBlack();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FutilitySearchInternalWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;

        var moves = context.Moves.Count;
        for (byte i = 0; i < moves; i++)
        {
            move = context.GetMove(i);

            Position.MakeWhite(move);

            if (!move.IsCheck && move.IsFutile)
            {
                Position.UnMakeWhite();
                continue;
            }

            r = -SearchBlack(b, -alpha, d);

            Position.UnMakeWhite();

            if (move.IsQuiet) move.Butterfly++;

            if (r <= context.Value)
                continue;

            context.Value = r;
            context.BestMove = move.Key;

            if (r >= beta)
            {
                if (move.IsQuiet)
                {
                    context.Add(move.Key);

                    move.History += depth * depth;
                }
                break;
            }
            if (r > alpha) alpha = r;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FutilitySearchInternalBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;

        var moves = context.Moves.Count;
        for (byte i = 0; i < moves; i++)
        {
            move = context.GetMove(i);

            Position.MakeBlack(move);

            if (!move.IsCheck && move.IsFutile)
            {
                Position.UnMakeBlack();
                continue;
            }

            r = -SearchWhite(b, -alpha, d);

            Position.UnMakeBlack();

            if (move.IsQuiet) move.Butterfly++;

            if (r <= context.Value)
                continue;

            context.Value = r;
            context.BestMove = move.Key;

            if (r >= beta)
            {
                if (move.IsQuiet)
                {
                    context.Add(move.Key);

                    move.History += depth * depth;
                }
                break;
            }
            if (r > alpha) alpha = r;
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

        var moves = context.Moves.Count;
        for (byte i = 0; i < moves; i++)
        {
            move = context.GetMove(i);
            Position.MakeWhite(move);

            r = -SearchBlack(b, a, d);

            Position.UnMakeWhite();

            if (move.IsQuiet) move.Butterfly++;

            if (r <= context.Value)
                continue;

            context.Value = r;
            context.BestMove = move.Key;

            if (r >= beta)
            {
                if (move.IsQuiet)
                {
                    context.Add(move.Key);

                    move.History += depth * depth;
                }
                break;
            }

            if (r > alpha)
            {
                alpha = r;
                a = -alpha;
            }
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

        var moves = context.Moves.Count;
        for (byte i = 0; i < moves; i++)
        {
            move = context.GetMove(i);
            Position.MakeBlack(move);

            r = -SearchWhite(b, a, d);

            Position.UnMakeBlack();

            if (move.IsQuiet) move.Butterfly++;

            if (r <= context.Value)
                continue;

            context.Value = r;
            context.BestMove = move.Key;

            if (r >= beta)
            {
                if (move.IsQuiet)
                {
                    context.Add(move.Key);

                    move.History += depth * depth;
                }
                break;
            }

            if (r > alpha)
            {
                alpha = r;
                a = -alpha;
            }
        }
    }

    #endregion

    #region Evaluation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int EvaluateWhite(int alpha, int beta)
    {
        if (MoveHistory.IsLastMoveWasCheck())
            return EvaluationWhiteSearch(alpha, beta);

        int standPat = _board.Evaluate();
        if (standPat >= beta)
            return beta;

        if (alpha < standPat)
            alpha = standPat;

        SortContext sortContext = DataPoolService.GetCurrentEvaluationSortContext();
        sortContext.SetForEvaluation(EvaluationSorter, alpha - standPat);

        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();
        Position.GetAllWhiteForEvaluation(sortContext, ref context.Moves);

        if (context.Moves.Count < 1)
            return alpha;

        int delta = standPat + DeltaPruningMargin;
        int b = -beta;
        int a = -alpha;
        int score;

        for (int i = 0; i < context.Moves.Count; i++)
        {
            MoveBase move = context.GetMove(i);

            // Delta pruning: skip moves unlikely to improve alpha
            if (!move.IsCheck && move is AttackBase attack && delta + attack.See < alpha)
                continue;

            Position.MakeWhite(move);

            score = -EvaluateBlack(b, a);

            Position.UnMakeWhite();

            if (score >= beta)
                return beta;

            if (score <= alpha)
                continue;

            alpha = score;
            a = -alpha;
        }

        return alpha;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int EvaluateBlack(int alpha, int beta)
    {
        if (MoveHistory.IsLastMoveWasCheck())
            return EvaluationBlackSearch(alpha, beta);

        int standPat = _board.EvaluateOpposite();
        if (standPat >= beta)
            return beta;

        if (alpha < standPat)
            alpha = standPat;

        SortContext sortContext = DataPoolService.GetCurrentEvaluationSortContext();
        sortContext.SetForEvaluation(EvaluationSorter, alpha - standPat);

        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();
        Position.GetAllBlackForEvaluation(sortContext, ref context.Moves);

        if (context.Moves.Count < 1)
            return alpha;

        int delta = standPat + DeltaPruningMargin;
        int b = -beta;
        int a = -alpha;
        int score;

        for (int i = 0; i < context.Moves.Count; i++)
        {
            MoveBase move = context.GetMove(i);

            // Delta pruning: skip moves unlikely to improve alpha
            if (!move.IsCheck && move is AttackBase attack && delta + attack.See < alpha)
                continue;

            Position.MakeBlack(move);

            score = -EvaluateWhite(b, a);

            Position.UnMakeBlack();

            if (score >= beta)
                return beta;

            if (score <= alpha)
                continue;

            alpha = score;
            a = -alpha;
        }

        return alpha;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluationWhiteSearch(int alpha, int beta)
    {
        if (CheckDraw())
            return 0;

        SearchContext context = GetCurrentContextForEvaluation();

        if (context.SearchResultType != SearchResultType.EndGame)
        {
            MoveBase move;
            int r;
            int b = -beta;

            var moves = context.Moves.Count;
            for (byte i = 0; i < moves; i++)
            {
                move = context.GetMove(i);
                Position.MakeWhite(move);

                r = -SearchBlack(b, -alpha, 0);

                Position.UnMakeWhite();

                if (move.IsQuiet) move.Butterfly++;

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move.Key;

                if (r >= beta)
                {
                    if (move.IsQuiet)
                    {
                        context.Add(move.Key);

                        move.History++;
                    }
                    break;
                }

                if (r > alpha)
                    alpha = r;
            }
        }

        return context.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluationBlackSearch(int alpha, int beta)
    {
        if (CheckDraw())
            return 0;

        SearchContext context = GetCurrentContextForEvaluation();

        if (context.SearchResultType != SearchResultType.EndGame)
        {
            MoveBase move;
            int r;
            int b = -beta;

            var moves = context.Moves.Count;
            for (byte i = 0; i < moves; i++)
            {
                move = context.GetMove(i);
                Position.MakeBlack(move);

                r = -SearchWhite(b, -alpha, 0);

                Position.UnMakeBlack();

                if (move.IsQuiet) move.Butterfly++;

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move.Key;

                if (r >= beta)
                {
                    if (move.IsQuiet)
                    {
                        context.Add(move.Key);

                        move.History++;
                    }
                    break;
                }

                if (r > alpha)
                    alpha = r;
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
        sortContext.Set(BaseSorter);
        sortContext.GetAllMoves(Position, ref context.Moves);

        if (context.Moves.Count < 1)
        {
            context.SearchResultType = SearchResultType.EndGame;
            context.Value = MateNegative;
        }
        else
        {
            context.SearchResultType = SearchResultType.None;
        }

        return context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SearchContext GetCurrentContext(int alpha, int beta, sbyte depth)
    {
        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[depth]);
        return SetupSearchContext(alpha, beta, depth, sortContext);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SearchContext GetCurrentContext(int alpha, int beta, sbyte depth, short pvKey)
    {
        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[depth], pvKey);
        return SetupSearchContext(alpha, beta, depth, sortContext);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SearchContext SetupSearchContext(int alpha, int beta, sbyte depth, SortContext sortContext)
    {
        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();
        sortContext.GetAllMoves(Position, ref context.Moves);

        if (context.Moves.Count < 1)
        {
            context.SearchResultType = SearchResultType.EndGame;
            context.Value = MoveHistory.IsLastMoveWasCheck() ? MateNegative : 0;
        }
        else if (context.Moves.Count < 2)
        {
            context.SearchResultType = depth < ExtensionDepth && MoveHistory.GetPly() < MaxOneReplyPly
                                        && (MoveHistory.IsLastMoveWasCheck() || !IsLikelyZugzwangPosition())
                ? SearchResultType.OneReply
                : SearchResultType.None;
        }
        else if (MoveHistory.IsLastMoveWasCheck())
        {
            context.SearchResultType = depth < ExtensionDepth && MoveHistory.GetPhase() != Phase.Opening && MoveHistory.GetPly() < MaxExtensionPly
                ? SearchResultType.Check
                : SearchResultType.None;
        }
        else
        {
            context.SearchResultType = depth > RazoringDepth
                ? SearchResultType.None
                : GetSearchResultType(alpha, beta, depth);
        }

        return context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SearchResultType GetSearchResultType(int alpha, int beta, sbyte depth)
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
        EvaluationSorter = MoveSorterProvider.GetAttack(position) as EvaluationSorter;
        BaseSorter = mainSorter;
        List<MoveSorterBase> sorters = [EvaluationSorter];

        var complexSorter = MoveSorterProvider.GetComplex(position);

        for (int i = 0; i < SortDepth[depth]; i++)
        {
            sorters.Add(BaseSorter);
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
    protected bool IsDraw(Result result)
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

        if (_board.IsDraw())
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
    protected bool CheckDraw() => MoveHistory.IsThreefoldRepetition() || MoveHistory.IsFiftyMoves() || _board.IsDraw();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsLateEndGame() => _board.IsLateEndGame();

    public override string ToString() => $"{GetType().Name}[{Depth}]";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Table.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlocked() => Table.IsBlocked();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExecuteAsyncAction() => Table.Update();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual StrategyBase CreateEndGameStrategy() => new IdLmrDeepEndStrategy(EndGameDepth, Position, Table);
}