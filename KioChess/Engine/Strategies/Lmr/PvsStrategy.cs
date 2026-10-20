using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Lmr;

public class PvsStrategy : LmrStrategyBase
{
    public PvsStrategy(int depth, Position position, TranspositionTable table = null, LmrTables lmrTables = null) : base(depth, position, table, lmrTables)
    {
        NullOnlyNonPv = true;
        PruneOnlyNonPv = true;
    }

    public override StrategyType Type => StrategyType.PVS;

    public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
    {
        Result result = new();
        if (IsDraw(result))
            return result;

        SortContext sortContext = GetSortContext(depth, pv);
        SearchContext context = DataPoolService.GetCurrentContext();
        context.Clear();
        sortContext.GetAllMoves(Position, ref context.Moves);

        if (CheckEndGame(context.Moves.Count, result)) return result;

        if (MoveHistory.IsLateMiddleGame()) depth++;

        SetExtensionThresholds(sortContext.Ply, depth);

        if (MoveHistory.IsLastMoveNotReducible())
        {
            SetPvsResult(alpha, beta, depth, result, ref context.Moves);
        }
        else
        {
            SetPvsLmrResult(alpha, beta, depth, result, ref context.Moves);
        }

        return result;
    }

    #region Root

    private void SetPvsResult(int alpha, int beta, sbyte depth, Result result, ref MoveHistoryList moves)
    {
        bool white = Position.GetTurn() == Turn.White;
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        int value;

        for (byte i = 0; i < moves.Count; i++)
        {
            var move = MoveProvider.Get(moves[i].Key);
            int a = -alpha;

            if (white)
            {
                Position.MakeWhite(move);
                value = -SearchBlack(i == 0 ? b : a - 1, a, d);
                if (i > 0 && value > alpha && value < beta)
                    value = -SearchBlack(b, a, d);
                Position.UnMakeWhite();
            }
            else
            {
                Position.MakeBlack(move);
                value = -SearchWhite(i == 0 ? b : a - 1, a, d);
                if (i > 0 && value > alpha && value < beta)
                    value = -SearchWhite(b, a, d);
                Position.UnMakeBlack();
            }

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

    private void SetPvsLmrResult(int alpha, int beta, sbyte depth, Result result, ref MoveHistoryList moves)
    {
        bool white = Position.GetTurn() == Turn.White;
        int b = -beta;
        sbyte d = (sbyte)(depth - 1);
        sbyte dr = (sbyte)(depth - 2);
        int lmr = Math.Max(GetLmr(moves.Count, depth), moves.LmrIndex - 1);
        int value;

        for (byte i = 0; i < moves.Count; i++)
        {
            var move = MoveProvider.Get(moves[i].Key);
            int a = -alpha;

            if (white)
            {
                Position.MakeWhite(move);
                if (i == 0)
                {
                    value = -SearchBlack(b, a, d);
                }
                else
                {
                    if (i > lmr && !move.IsCheck && move.CanReduce)
                    {
                        value = -SearchBlack(a - 1, a, dr);
                        if (value > alpha)
                            value = -SearchBlack(a - 1, a, d);
                    }
                    else
                    {
                        value = -SearchBlack(a - 1, a, d);
                    }

                    if (value > alpha && value < beta)
                        value = -SearchBlack(b, a, d);
                }
                Position.UnMakeWhite();
            }
            else
            {
                Position.MakeBlack(move);
                if (i == 0)
                {
                    value = -SearchWhite(b, a, d);
                }
                else
                {
                    if (i > lmr && !move.IsCheck && move.CanReduce)
                    {
                        value = -SearchWhite(a - 1, a, dr);
                        if (value > alpha)
                            value = -SearchWhite(a - 1, a, d);
                    }
                    else
                    {
                        value = -SearchWhite(a - 1, a, d);
                    }

                    if (value > alpha && value < beta)
                        value = -SearchWhite(b, a, d);
                }
                Position.UnMakeBlack();
            }

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

    #region In-tree

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternalWhite(int alpha, int beta, sbyte depth, SearchContext context)
    {
        if (!LmrTables.CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
        {
            SearchPvsWhite(alpha, beta, depth, context);
            return;
        }

        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;
        int a = -alpha;

        var moves = context.Moves.Count;

        var canReduceMoveMax = LmrTables.CanReduceMoveMax[depth][moves].AsSpan();
        var reduction = LmrTables.ReductionMax[depth][moves].AsSpan();
        var lowSee = context.LowSee;
        var lmr = context.Moves.LmrIndex;

        for (byte i = 0; i < moves; i++)
        {
            move = context.GetMove(i);

            Position.MakeWhite(move);

            if (i == 0)
            {
                r = -SearchBlack(b, a, d);
            }
            else
            {
                if (canReduceMoveMax[i] && i >= lmr && !move.IsCheck && (lowSee[move.Key] || move.CanReduce))
                {
                    r = -SearchBlack(a - 1, a, reduction[i]);
                    if (r > alpha)
                        r = -SearchBlack(a - 1, a, d);
                }
                else
                {
                    r = -SearchBlack(a - 1, a, d);
                }

                if (r > alpha && r < beta)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternalBlack(int alpha, int beta, sbyte depth, SearchContext context)
    {
        if (!LmrTables.CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
        {
            SearchPvsBlack(alpha, beta, depth, context);
            return;
        }

        MoveBase move;
        int r;
        sbyte d = (sbyte)(depth - 1);
        int b = -beta;
        int a = -alpha;

        var moves = context.Moves.Count;

        var canReduceMoveMax = LmrTables.CanReduceMoveMax[depth][moves].AsSpan();
        var reduction = LmrTables.ReductionMax[depth][moves].AsSpan();
        var lowSee = context.LowSee;
        var lmr = context.Moves.LmrIndex;

        for (byte i = 0; i < moves; i++)
        {
            move = context.GetMove(i);

            Position.MakeBlack(move);

            if (i == 0)
            {
                r = -SearchWhite(b, a, d);
            }
            else
            {
                if (canReduceMoveMax[i] && i >= lmr && !move.IsCheck && (lowSee[move.Key] || move.CanReduce))
                {
                    r = -SearchWhite(a - 1, a, reduction[i]);
                    if (r > alpha)
                        r = -SearchWhite(a - 1, a, d);
                }
                else
                {
                    r = -SearchWhite(a - 1, a, d);
                }

                if (r > alpha && r < beta)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SearchPvsWhite(int alpha, int beta, sbyte depth, SearchContext context)
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

            if (i == 0)
            {
                r = -SearchBlack(b, a, d);
            }
            else
            {
                r = -SearchBlack(a - 1, a, d);
                if (r > alpha && r < beta)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SearchPvsBlack(int alpha, int beta, sbyte depth, SearchContext context)
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

            if (i == 0)
            {
                r = -SearchWhite(b, a, d);
            }
            else
            {
                r = -SearchWhite(a - 1, a, d);
                if (r > alpha && r < beta)
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

    #endregion
}
