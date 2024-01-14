using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Lmr;

public abstract class LmrStrategyBase : MemoryStrategyBase
{
    protected readonly bool[] CanReduceDepth;
    protected readonly bool[] CanReduceMove;
    protected readonly sbyte[][] Reduction;

    protected LmrStrategyBase(int depth, IPosition position, TranspositionTable table = null) 
        : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));

        CanReduceDepth = InitializeReducableDepthTable();
        CanReduceMove = InitializeReducableMoveTable();
        Reduction = InitializeReductionTable();
    }
    public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
    {
        Result result = new Result();
        if (IsDraw(result))
            return result;

        if (pv == null && Table.TryGet(Position.GetKey(), out var entry))
        {
            pv = GetPv(entry.PvMove);
        }

        return SetLmrResult(alpha, beta, depth, pv);
    }

    protected IResult SetLmrResult(int alpha, int beta, sbyte depth, MoveBase pv)
    {
        Result result = new Result();
        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[depth], pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        DistanceFromRoot = sortContext.Ply;
        MaxExtensionPly = DistanceFromRoot + depth + ExtensionDepthDifference;

        if (CheckEndGame(moves.Count, result)) return result;

        if (MoveHistory.IsLastMoveNotReducible() || moves.Count < 7)
        {
            SetResult(alpha, beta, depth, result, moves);
        }
        else
        {
            int value;
            sbyte d = (sbyte)(depth - 1);
            sbyte rd = (sbyte)(depth - 2);
            int b = -beta;
            for (byte i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                if (move.CanReduce && !move.IsCheck && i > 4)
                {
                    value = -Search(b, -alpha, rd);
                    if (value > alpha)
                    {
                        value = -Search(b, -alpha, d);
                    }
                }
                else
                {
                    value = -Search(b, -alpha, d);
                }

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

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternal(int alpha, int beta, sbyte depth, SearchContext context)
    {
        if (!CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
        {
            base.SearchInternal(alpha, beta, depth, context);
        }
        else
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

                if (move.CanReduce && !move.IsCheck && CanReduceMove[i])
                {
                    r = -Search(b, -alpha, Reduction[depth][i]);
                    if (r > alpha)
                    {
                        r = -Search(b, -alpha, d);
                    }
                }
                else
                {
                    r = -Search(b, -alpha, d);
                }

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
    }

    protected abstract sbyte[][] InitializeReductionTable();
    protected abstract bool[] InitializeReducableMoveTable();
    protected abstract bool[] InitializeReducableDepthTable();
}
