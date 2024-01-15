using Engine.Interfaces;
using Engine.DataStructures.Hash;
using Engine.Strategies.End;
using Engine.DataStructures.Moves.Lists;
using Engine.DataStructures;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using Engine.Strategies.Models.Contexts;
using Engine.Models.Boards;

namespace Engine.Strategies.Base.Null;

public abstract class NullLmrStrategyBase : NullMemoryStrategyBase
{
    protected readonly bool[] CanReduceDepth;
    protected readonly bool[] CanReduceMove;
    protected readonly sbyte[][] Reduction;

    protected NullLmrStrategyBase(short depth, Position position, TranspositionTable table = null) 
        : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));

        CanReduceDepth = InitializeReducableDepthTable();
        CanReduceMove = InitializeReducableMoveTable();
        Reduction = InitializeReductionTable();
    }

    public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pvMove = null)
    {
        Result result = new Result();
        if (IsDraw(result))
        {
            return result;
        }

        MoveBase pv = pvMove;
        if (pv == null)
        {
            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                pv = GetPv(entry.PvMove);
            }
        }

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[Depth], pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        DistanceFromRoot = sortContext.Ply; MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

        if (CheckEndGame(moves.Count, result)) return result;

        if (MoveHistory.IsLastMoveNotReducible())
        {
            SetResult(alpha, beta, depth, result, moves);
        }
        else
        {
            int value;
            sbyte d = (sbyte)(depth - 1);
            int b = -beta;
            for (byte i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                if (move.CanReduce && !move.IsCheck && CanReduceMove[i])
                {
                    value = -Search(b, -alpha, Reduction[depth][i]);
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
        if (IsNull|| !CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
        {
            base.SearchInternal(alpha, beta, depth, context);
        }
        else
        {
            MoveBase move;
            int r;
            sbyte d = (sbyte)(depth - 1);
            int b = -beta;

            for (byte i = 0; i < context.Moves.Count; i++)
            {
                move = context.Moves[i];
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

    protected override StrategyBase CreateEndGameStrategy()
    {
        int depth = Depth + 1;
        if (Depth < MaxEndGameDepth)
        {
            depth++;
        }
        return new IdLmrDeepEndStrategy(depth, Position, Table);
    }

    protected abstract sbyte[][] InitializeReductionTable();
    protected abstract bool[] InitializeReducableMoveTable();
    protected abstract bool[] InitializeReducableDepthTable();
}
