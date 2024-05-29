﻿using Engine.DataStructures;
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
    protected readonly bool[] CanReduceDepth;
    protected readonly bool[] CanReduceMoveMax;
    protected readonly sbyte[][] ReductionMax;
    protected readonly bool[] CanReduceMoveMin;
    protected readonly sbyte[][] ReductionMin;

    protected LmrStrategyBase(int depth, Position position, TranspositionTable table = null) 
        : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));

        CanReduceDepth = InitializeReducableDepthTable();
        CanReduceMoveMax = InitializeReducableMaxMoveTable();
        ReductionMax = InitializeReductionMaxTable();
        CanReduceMoveMin = InitializeReducableMinMoveTable();
        ReductionMin = InitializeReductionMinTable();
    }

    protected abstract int MinimumMaxMoveCount { get; }

    protected abstract int MinimumMinMoveCount { get; }

    public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
    {
        Result result = new Result();
        if (IsDraw(result))
            return result;

        if (pv == null && Table.TryGet(out var entry))
        {
            pv = GetPv(entry.PvMove);
        }

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[depth], pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        SetExtensionThresholds(sortContext.Ply);

        if (CheckEndGame(moves.Count, result)) return result;

        if (_board.IsLateMiddleGame()) depth++;


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
        else if (Depth % 2 != depth % 2)
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
        else if (Depth % 2 != depth % 2)
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

        for (byte i = 0; i < moves.Count; i++)
        {
            move = moves[i];

            Position.MakeWhite(move);

            if (CanReduceMoveMax[i] && !move.IsCheck && (context.LowSee[move.Key] || move.CanReduce))
            {
                r = -SearchBlack(b, a, ReductionMax[depth][i]);
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

        for (byte i = 0; i < moves.Count; i++)
        {
            move = moves[i];

            Position.MakeBlack(move);

            if (CanReduceMoveMax[i] && !move.IsCheck && (context.LowSee[move.Key] || move.CanReduce))
            {
                r = -SearchWhite(b, a, ReductionMax[depth][i]);
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

        for (byte i = 0; i < moves.Count; i++)
        {
            move = moves[i];

            Position.MakeWhite(move);

            if (CanReduceMoveMin[i] && !move.IsCheck && (context.LowSee[move.Key] || move.CanReduce))
            {
                r = -SearchBlack(b, a, ReductionMin[depth][i]);
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

        for (byte i = 0; i < moves.Count; i++)
        {
            move = moves[i];

            Position.MakeBlack(move);

            if (CanReduceMoveMin[i] && !move.IsCheck && (context.LowSee[move.Key] || move.CanReduce))
            {
                r = -SearchWhite(b, a, ReductionMin[depth][i]);
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
            {
                alpha = r;
                a = -alpha;
            }

            if (!move.IsAttack) move.Butterfly++;
        }
    }

    protected abstract sbyte[][] InitializeReductionMaxTable();

    protected abstract sbyte[][] InitializeReductionMinTable();
    protected abstract bool[] InitializeReducableDepthTable();

    protected bool[] InitializeReducableMinMoveTable()
    {
        var result = new bool[128];
        for (int move = 0; move < result.Length; move++)
        {
            result[move] = move > MinimumMinMoveCount;
        }

        return result;
    }

    protected bool[] InitializeReducableMaxMoveTable()
    {
        var result = new bool[128];
        for (int move = 0; move < result.Length; move++)
        {
            result[move] = move > MinimumMaxMoveCount;
        }

        return result;
    }
}
