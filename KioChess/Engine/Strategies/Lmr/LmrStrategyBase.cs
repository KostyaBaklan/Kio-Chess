using Engine.DataStructures;
using Engine.DataStructures.Moves;
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
    protected LmrTables LmrTables;

    protected int MaxLmr;

    protected int LmrFactor;

    protected int LmrRelation;

    protected int MaxLowLmr;

    protected int LmrLowFactor;

    protected int LmrLowRelation;

    protected int LmrMoveDepth;

    protected int Ratio;

    protected int DeepRatio;

    protected LmrStrategyBase(int depth, Position position, TranspositionTable table = null, LmrTables lmrTables = null)
        : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));

        if(lmrTables == null)
        {
            lmrTables = new LmrTables(configurationProvider, depth, 
                configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.Lmrd
                , configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrRatio);
        }
        else
        {
            LmrTables = lmrTables;
        }

            MaxLmr = configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrMove[2];
        LmrFactor = configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrMove[0];
        LmrRelation = configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrMove[1];
        MaxLowLmr = configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrLowMove[2];
        LmrLowFactor = configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrLowMove[0];
        LmrLowRelation = configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrLowMove[1];
        LmrMoveDepth = configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrMoveDepth;
        LmrTables = lmrTables;
    }

    public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
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

        if (MoveHistory.IsLateMiddleGame()) depth++;

        SetLmrResult(alpha, beta, depth, result, ref context.Moves);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetLmrResult(int alpha, int beta, sbyte depth, Result result, ref MoveHistoryList moves)
    {
        if (MoveHistory.IsLastMoveNotReducible())
        {
            SetResult(alpha, beta, depth, result, ref moves);
        }
        else
        {
            if (Position.GetTurn() == Turn.White)
            {
                SetLmrResultWhite(alpha, beta, depth, result, ref moves);
            }
            else
            {
                SetLmrResultBlack(alpha, beta, depth, result, ref moves);
            }
        }
    }


    private void SetLmrResultWhite(int alpha, int beta, sbyte depth, Result result, ref MoveHistoryList moves)
    {
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        sbyte dr = (sbyte)(depth - 2);
        sbyte ddr = (sbyte)(depth - 3);
        int lmr = Math.Max(GetLmr(moves.Count, depth), moves.LmrIndex - 1);
        //int lmrd = GetLmrd(moves.Count);
        int value;

        for (byte i = 0; i < moves.Count; i++)
        {
            var move = MoveProvider.Get(moves[i].Key);
            Position.MakeWhite(move);
            if (i > lmr && !move.IsCheck && move.CanReduce)
            {
                value = -SearchBlack(b, -alpha, dr);
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

    private void SetLmrResultBlack(int alpha, int beta, sbyte depth, Result result, ref MoveHistoryList moves)
    {
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        sbyte dr = (sbyte)(depth - 2);
        sbyte ddr = (sbyte)(depth - 3);
        int lmr = Math.Max(GetLmr(moves.Count, depth), moves.LmrIndex - 1);
        //int lmrd = GetLmrd(moves.Count);
        int value;

        for (byte i = 0; i < moves.Count; i++)
        {
            var move = MoveProvider.Get(moves[i].Key);
            Position.MakeBlack(move);
            if (i > lmr && !move.IsCheck && move.CanReduce)
            {
                value = -SearchWhite(b, -alpha, dr);
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

    private int GetLmr(int moves, sbyte depth)
    {
        if (depth > LmrMoveDepth)
        {
            return Math.Max(MaxLmr, LmrFactor * moves / LmrRelation);
        }
        else
        {
            return Math.Max(MaxLowLmr, LmrLowFactor * moves / LmrLowRelation);
        }
    }

    //private static int GetLmrd(int moves) => moves < 11 ? moves : Math.Max(moves - 10, 3 * moves / 4);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternalWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        if (!LmrTables.CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
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

            var moves = context.Moves.Count;

            var canReduceMoveMax = LmrTables.CanReduceMoveMax[depth][moves].AsSpan();
            var reduction = LmrTables.ReductionMax[depth][moves].AsSpan();

            var lmr = context.Moves.LmrIndex;

            for (byte i = 0; i < moves; i++)
            {
                move = context.GetMove(i);

                Position.MakeWhite(move);

                if (canReduceMoveMax[i] && i>=lmr && !move.IsCheck && (context.LowSee[move.Key] || move.CanReduce))
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternalBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        if (!LmrTables.CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
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

            var moves = context.Moves.Count;

            var canReduceMoveMax = LmrTables.CanReduceMoveMax[depth][moves].AsSpan();
            var reduction = LmrTables.ReductionMax[depth][moves].AsSpan();

            var lmr = context.Moves.LmrIndex;

            for (byte i = 0; i < moves; i++)
            {
                move = context.GetMove(i);

                Position.MakeBlack(move);

                if (canReduceMoveMax[i] && i>=lmr && !move.IsCheck && (context.LowSee[move.Key] || move.CanReduce))
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
    }
}
