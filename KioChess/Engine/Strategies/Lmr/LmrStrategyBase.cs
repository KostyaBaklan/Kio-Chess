﻿using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Strategies.Base;
using Engine.Strategies.End;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Lmr
{
    public abstract class LmrStrategyBase : MemoryStrategyBase
    {
        protected readonly bool[] CanReduceDepth;
        protected readonly bool[] CanReduceMove;
        protected readonly byte[][] Reduction;

        protected LmrStrategyBase(short depth, IPosition position, TranspositionTable table = null) 
            : base(depth, position, table)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetAdvanced(position, new HistoryComparer()));

            CanReduceDepth = InitializeReducableDepthTable();
            CanReduceMove = InitializeReducableMoveTable();
            Reduction = InitializeReductionTable();
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
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
            MoveList moves = Position.GetAllMoves(sortContext);

            DistanceFromRoot = sortContext.Ply; MaxExtensionPly = DistanceFromRoot + Depth + 1;

            if (CheckEndGame(moves.Count, result)) return result;

            if (MoveHistory.IsLastMoveNotReducible())
            {
                SetResult(alpha, beta, depth, result, moves);
            }
            else
            {
                int d = depth - 1;
                int b = -beta;
                for (var i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    int value;
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

            result.Move.History++;
            return result;
        }

        protected override void SearchInternal(int alpha, int beta, int depth, SearchContext context)
        {
            if (!CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
            {
                base.SearchInternal(alpha, beta, depth, context);
            }
            else
            {
                MoveBase move;
                int r;
                int d = depth - 1;
                int b = -beta;

                for (var i = 0; i < context.Moves.Count; i++)
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
                        if (!move.IsAttack) Sorters[depth].Add(move.Key);
                        break;
                    }
                    if (r > alpha)
                        alpha = r;
                }

                context.BestMove.History += 1 << depth;
            }
        }

        protected override StrategyBase CreateEndGameStrategy()
        {
            return new LmrDeepEndGameStrategy((short)Math.Min(Depth + 1, MaxEndGameDepth), Position, Table);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetExtension(MoveBase move)
        {
            return move.IsCheck  ? 1 : 0;
        }

        protected abstract byte[][] InitializeReductionTable();
        protected abstract bool[] InitializeReducableMoveTable();
        protected abstract bool[] InitializeReducableDepthTable();
    }
}
