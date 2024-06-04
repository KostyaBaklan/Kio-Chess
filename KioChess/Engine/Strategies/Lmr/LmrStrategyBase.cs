using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Lmr;

public abstract class LmrStrategyBase : StrategyBase
{
    protected int MinMax;
    protected readonly bool[] CanReduceDepth;

    protected readonly bool[][][] CanReduceMoveMax;
    protected readonly bool[][][] CanReduceMoveMin;

    protected readonly sbyte[][][] ReductionMax;
    protected readonly sbyte[][][] ReductionMin;

    protected readonly int MaxMoveCount;

    protected LmrStrategyBase(int depth, Position position, TranspositionTable table = null) 
        : base(depth, position, table)
    {
        MinMax = 1;
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));

        MaxMoveCount = configurationProvider.GeneralConfiguration.MaxMoveCount;

        CanReduceDepth = InitializeReducableDepthTable();
        ReductionMax = InitializeReductionMaxTable();
        ReductionMin = InitializeReductionMinTable();
        CanReduceMoveMin = InitializeReducableMinMoveTable();
        CanReduceMoveMax = InitializeReducableMaxMoveTable();
    }

    protected abstract int ReducableDepth { get; }

    protected abstract int MinimumMaxMoveCount { get; }

    protected abstract int MinimumMinMoveCount { get; }

    public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
    {
        Result result = new Result();
        if (IsDraw(result))
            return result;

        SortContext sortContext = GetSortContext(depth, pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        SetExtensionThresholds(sortContext.Ply);

        if (CheckEndGame(moves.Count, result)) return result;

        MinMax = MoveHistory.GetPly() % 2;

        if (MoveHistory.IsLateMiddleGame()) depth++;

        SetResult(alpha, beta, depth, result, moves);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternalWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        if (!CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
        {
            base.SearchInternalWhite(alpha, beta, depth, context);
        }
        else if (MinMax == context.MinMax)
        {
            SearchInternalMinWhite(alpha, beta, depth, context);
        }
        else
        {
            SearchInternalMaxWhite(alpha, beta, depth, context);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternalBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        if (!CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
        {
            base.SearchInternalBlack(alpha, beta, depth, context);
        }
        else if (MinMax == context.MinMax)
        {
            SearchInternalMinBlack(alpha, beta, depth, context);
        }
        else
        {
            SearchInternalMaxBlack(alpha, beta, depth, context);
        }
    }

    private void SearchInternalMaxWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;
        int a = -alpha;

        MoveList moves = context.Moves;

        var canReduceMoveMax = CanReduceMoveMax[depth][moves.Count].AsSpan();
        var reduction = ReductionMax[depth][moves.Count].AsSpan();

        for (byte i = 0; i < moves.Count; i++)
        {
            move = moves[i];

            Position.MakeWhite(move);

            if (canReduceMoveMax[i] && !move.IsCheck && (context.LowSee[move.Key] || move.CanReduce))
            {
                r = -SearchBlack(b, a, reduction[i]);
                if (r > alpha)
                {
                    r = -SearchBlack(b, a, d);
                }
            }
            else
            {
                r = -SearchBlack(b, a, d);
            }

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

    private void SearchInternalMaxBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;
        int a = -alpha;

        MoveList moves = context.Moves;

        var canReduceMoveMax = CanReduceMoveMax[depth][moves.Count].AsSpan();
        var reduction = ReductionMax[depth][moves.Count].AsSpan();

        for (byte i = 0; i < moves.Count; i++)
        {
            move = moves[i];

            Position.MakeBlack(move);

            if (canReduceMoveMax[i] && !move.IsCheck && (context.LowSee[move.Key] || move.CanReduce))
            {
                r = -SearchWhite(b, a, reduction[i]);
                if (r > alpha)
                {
                    r = -SearchWhite(b, a, d);
                }
            }
            else
            {
                r = -SearchWhite(b, a, d);
            }

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

    private void SearchInternalMinWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;
        int a = -alpha;

        MoveList moves = context.Moves;

        var canReduceMoveMin = CanReduceMoveMin[depth][moves.Count].AsSpan(); 
        var reduction = ReductionMin[depth][moves.Count].AsSpan();

        for (byte i = 0; i < moves.Count; i++)
        {
            move = moves[i];

            Position.MakeWhite(move);

            if (canReduceMoveMin[i] && !move.IsCheck && (context.LowSee[move.Key] || move.CanReduce))
            {
                r = -SearchBlack(b, a, reduction[i]);
                if (r > alpha)
                {
                    r = -SearchBlack(b, a, d);
                }
            }
            else
            {
                r = -SearchBlack(b, a, d);
            }

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

    private void SearchInternalMinBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;
        int a = -alpha;

        MoveList moves = context.Moves;

        var canReduceMoveMin = CanReduceMoveMin[depth][moves.Count].AsSpan();
        var reduction = ReductionMin[depth][moves.Count].AsSpan();

        for (byte i = 0; i < moves.Count; i++)
        {
            move = moves[i];

            Position.MakeBlack(move);

            if (canReduceMoveMin[i] && !move.IsCheck && (context.LowSee[move.Key] || move.CanReduce))
            {
                r = -SearchWhite(b, a, reduction[i]);
                if (r > alpha)
                {
                    r = -SearchWhite(b, a, d);
                }
            }
            else
            {
                r = -SearchWhite(b, a, d);
            }

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

    protected abstract sbyte[][][] InitializeReductionMaxTable();

    protected abstract sbyte[][][] InitializeReductionMinTable();

    protected bool[] InitializeReducableDepthTable()
    {
        var result = new bool[2 * Depth];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = depth > ReducableDepth;
        }

        return result;
    }

    protected bool[][][] InitializeReducableMinMoveTable()
    {
        var result = new bool[2 * Depth][][];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = new bool[MaxMoveCount][];
            for (int move = 0; move < result[depth].Length; move++)
            {
                result[depth][move] = new bool[move];
                for (int i = 0; i < result[depth][move].Length; i++)
                {
                    result[depth][move][i] = depth - ReductionMin[depth][move][i] > 1;
                }
            }
        }
        return result;
    }

    protected bool[][][] InitializeReducableMaxMoveTable()
    {
        var result = new bool[2 * Depth][][];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = new bool[MaxMoveCount][];
            for (int move = 0; move < result[depth].Length; move++)
            {
                result[depth][move] = new bool[move];
                for (int i = 0; i < result[depth][move].Length; i++)
                {
                    result[depth][move][i] = depth - ReductionMax[depth][move][i] > 1;
                }
            }
        }
        return result;
    }
}
