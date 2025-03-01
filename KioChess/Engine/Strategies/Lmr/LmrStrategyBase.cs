﻿using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Lmr;

public abstract class LmrStrategyBase : StrategyBase
{
    protected readonly bool[] CanReduceDepth;

    protected readonly bool[][][] CanReduceMoveMax;

    protected readonly sbyte[][][] ReductionMax;

    protected readonly int MaxMoveCount;

    protected int ReducableDepth;

    protected int NonLmrOffset;

    protected int LmrOffset;

    protected LmrStrategyBase(int depth, Position position, TranspositionTable table = null) 
        : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));

        MaxMoveCount = configurationProvider.GeneralConfiguration.MaxMoveCount;

        int[] lmrConfig = GetLmrConfig();

        ReducableDepth = lmrConfig[0];
        NonLmrOffset = lmrConfig[1];
        LmrOffset = lmrConfig[2];

        CanReduceDepth = InitializeReducableDepthTable();
        ReductionMax = InitializeReductionMaxTable();
        CanReduceMoveMax = InitializeReducableMaxMoveTable();
    }

    protected abstract int[] GetLmrConfig();

    public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
    {
        Result result = new Result();
        if (IsDraw(result))
            return result;

        SortContext sortContext = GetSortContext(depth, pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        SetExtensionThresholds(sortContext.Ply);

        if (CheckEndGame(moves.Count, result)) return result;

        if (MoveHistory.IsLateMiddleGame()) depth++;

        SetLmrResult(alpha, beta, depth, result, moves);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetLmrResult(int alpha, int beta, sbyte depth, Result result, MoveList moves)
    {
        if (MoveHistory.IsLastMoveNotReducible())
        {
            SetResult(alpha, beta, depth, result, moves);
        }
        else
        {
            if (Position.GetTurn() == Turn.White)
            {
                SetLmrResultWhite(alpha, beta, depth, result, moves);
            }
            else
            {
                SetLmrResultBlack(alpha, beta, depth, result, moves);
            }
        }
    }


    private void SetLmrResultWhite(int alpha, int beta, sbyte depth, Result result, MoveList moves)
    {
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        sbyte dr = (sbyte)(depth - 2);
        sbyte ddr = (sbyte)(depth - 3);
        int lmr = GetLmr(moves.Count, depth);
        //int lmrd = GetLmrd(moves.Count);
        int value;

        for (byte i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            Position.MakeWhite(move);
            if (i > lmr && !move.IsCheck && move.CanReduce)
            {
                value = -SearchBlack(b, -alpha,  dr);
                if (value > alpha)
                {
                    value = -SearchBlack(b, -alpha, d);
                }
            }
            else
            {
                value = -SearchBlack(b, -alpha, d);
            }

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

    private void SetLmrResultBlack(int alpha, int beta, sbyte depth, Result result, MoveList moves)
    {
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        sbyte dr = (sbyte)(depth - 2);
        sbyte ddr = (sbyte)(depth - 3);
        int lmr = GetLmr(moves.Count, depth);
        //int lmrd = GetLmrd(moves.Count);
        int value;

        for (byte i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            Position.MakeBlack(move);
            if (i > lmr && !move.IsCheck && move.CanReduce)
            {
                value = -SearchWhite(b, -alpha,  dr);
                if (value > alpha)
                {
                    value = -SearchWhite(b, -alpha, d);
                }
            }
            else
            {
                value = -SearchWhite(b, -alpha, d);
            }

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

    private static int GetLmr(int moves, sbyte depth)
    {
        if (depth > 8)
        {
            return Math.Max(8, 3 * moves / 5);
        }
        else
        {
            return Math.Max(8, moves / 2);
        }
    }

    //private static int GetLmrd(int moves) => moves < 11 ? moves : Math.Max(moves - 10, 3 * moves / 4);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternalWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        if (!CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
        {
            base.SearchInternalWhite(alpha, beta, depth, context);
        }
        else
        {
            MoveBase move;
            int r;
            sbyte d = (sbyte)(depth - 1);
            int b = -beta;
            int a = -alpha;

            var moves = context.Moves.AsSpan();

            var canReduceMoveMax = CanReduceMoveMax[depth][moves.Length].AsSpan();
            var reduction = ReductionMax[depth][moves.Length].AsSpan();

            for (byte i = 0; i < moves.Length; i++)
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternalBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        if (!CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
        {
            base.SearchInternalBlack(alpha, beta, depth, context);
        }
        else
        {
            MoveBase move;
            int r;
            sbyte d = (sbyte)(depth - 1);
            int b = -beta;
            int a = -alpha;

            var moves = context.Moves.AsSpan();

            var canReduceMoveMax = CanReduceMoveMax[depth][moves.Length].AsSpan();
            var reduction = ReductionMax[depth][moves.Length].AsSpan();

            for (byte i = 0; i < moves.Length; i++)
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
    }

    protected sbyte[][][] InitializeReductionMaxTable()
    {
        var result = new sbyte[2 * Depth][][];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = new sbyte[MaxMoveCount][];
            for (int move = 0; move < result[depth].Length; move++)
            {
                result[depth][move] = new sbyte[move];
                for (int i = 0; i < result[depth][move].Length; i++)
                {
                    if (depth > ReducableDepth + 1)
                    {
                        result[depth][move][i] = GetOnReducableDepth(depth,move, i);
                    }
                    else if (depth > ReducableDepth)
                    {
                        result[depth][move][i] = GetReducableDepth(depth,move,i);
                    }
                    else
                    {
                        result[depth][move][i] = (sbyte)(depth - 1);
                    }
                }
            }
        }

        return result;
    }

    protected virtual sbyte GetOnReducableDepth(int depth, int move, int i)
    {
        return i > LmrOffset + GetDeepOffset(depth, move) ? (sbyte)(depth - 3) : GetReducableDepth(depth, move, i);
    }

    protected virtual sbyte GetReducableDepth(int depth, int move, int i)
    {
        return i > NonLmrOffset + GetOffset(depth, move) ? (sbyte)(depth - 2) : (sbyte)(depth - 1);
    }

    private int GetOffset(int depth, int move)
    {
        if (depth < 7) return 0;
        if (depth < 10) return move / 14 - 1;
        return move / 15;
    }

    private static int GetDeepOffset(int depth, int move)
    {
        if (depth < 7) return move / 4;
        if (depth < 9) return move / 5;
        return move / 6;
    }

    protected bool[] InitializeReducableDepthTable()
    {
        var result = new bool[2 * Depth];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = depth > ReducableDepth;
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
