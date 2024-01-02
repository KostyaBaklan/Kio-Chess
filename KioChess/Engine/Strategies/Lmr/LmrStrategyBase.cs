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

    protected LmrStrategyBase(short depth, IPosition position, TranspositionTable table = null) 
        : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));

        CanReduceDepth = InitializeReducableDepthTable();
        CanReduceMove = InitializeReducableMoveTable();
        Reduction = InitializeReductionTable();
    }

    public override IResult GetResult(short alpha, short beta, sbyte depth, MoveBase pv = null)
    {
        Result result = new Result();
        if (IsDraw(result))
        {
            return result;
        }

        if (pv == null && Table.TryGet(Position.GetKey(), out var entry))
        {
            pv = GetPv(entry.PvMove);
        }

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[Depth], pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        DistanceFromRoot = sortContext.Ply; 
        MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

        if (CheckEndGame(moves.Count, result)) return result;

        if (MoveHistory.IsLastMoveNotReducible())
        {
            result.Move = pv;
            SetResult(alpha, beta, depth, result, moves);
        }
        else
        {
            short value;
            sbyte d = (sbyte)(depth - 1);
            short b = (short)-beta;
            for (byte i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                if (move.CanReduce && !move.IsCheck && CanReduceMove[i])
                {
                    value = (short)-Search(b, (short)-alpha, Reduction[depth][i]);
                    if (value > alpha)
                    {
                        value = (short)-Search(b, (short)-alpha, d);
                    }
                }
                else
                {
                    value = (short)-Search(b, (short)-alpha, (IsPvEnabled && i == 0 && pv != null) ? depth : d);
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
    protected override void ExtensibleSearchInternal(short alpha, short beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        short r;
        sbyte d = (sbyte)(depth - 1);
        short b = (short)-beta;

        for (byte i = 0; i < context.Moves.Count; i++)
        {
            move = context.Moves[i];

            Position.Make(move);

            sbyte extension = GetExtension(move);

            if (move.CanReduce && !move.IsCheck && CanReduceMove[i])
            {
                r = (short)-Search(b, (short)-alpha, (sbyte)(Reduction[depth][i] + extension));
                if (r > alpha)
                {
                    r = (short)-Search(b, (short)-alpha, (sbyte)(d + extension));
                }
            }
            else
            {
                r = (short)-Search(b, (short)-alpha, (sbyte)(d + extension));
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
                    Sorters[depth].Add(move.Key);

                    context.BestMove.History += 1 << depth;
                }
                break;
            }
            if (r > alpha)
                alpha = r;
            if (!move.IsAttack) move.Butterfly++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternal(short alpha, short beta, sbyte depth, SearchContext context)
    {
        if (!CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
        {
            if (MaxExtensionPly > context.Ply)
            {
                base.ExtensibleSearchInternal(alpha, beta, depth, context);
            }
            else
            {
                base.RegularSearch(alpha, beta, depth, context);
            }
        }
        else if (MaxExtensionPly > context.Ply)
        {
            ExtensibleSearch(alpha, beta, depth, context);
        }
        else
        {
            RegularSearch(alpha, beta, depth, context);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void RegularSearch(short alpha, short beta, sbyte depth, SearchContext context)
    {
        MoveBase move;
        short r;
        sbyte d = (sbyte)(depth - 1);
        short b = (short)-beta;

        for (byte i = 0; i < context.Moves.Count; i++)
        {
            move = context.Moves[i];

            Position.Make(move);

            if (move.CanReduce && !move.IsCheck && CanReduceMove[i])
            {
                r = (short)-Search(b, (short)-alpha, Reduction[depth][i]);
                if (r > alpha)
                {
                    r = (short)-Search(b, (short)-alpha, d);
                }
            }
            else
            {
                r = (short)-Search(b, (short)-alpha, d);
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
                    Sorters[depth].Add(move.Key);

                    context.BestMove.History += 1 << depth;
                }
                break;
            }
            if (r > alpha)
                alpha = r;
            if (!move.IsAttack) move.Butterfly++;
        }
    }

    protected abstract sbyte[][] InitializeReductionTable();
    protected abstract bool[] InitializeReducableMoveTable();
    protected abstract bool[] InitializeReducableDepthTable();
}
