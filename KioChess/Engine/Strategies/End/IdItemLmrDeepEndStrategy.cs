﻿using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Strategies.Lmr;
using Engine.Strategies.Models.Contexts;

namespace Engine.Strategies.End
{
    public class IdItemLmrDeepEndStrategy : LmrStrategyBase
    {
        public IdItemLmrDeepEndStrategy(short depth, IPosition position, TranspositionTable table = null)
            : base(depth, position, table)
        {
        }
        public override IResult GetResult()
        {
            return GetResult((short)-SearchValue, SearchValue, Depth);
        }

        public override IResult GetResult(short alpha, short beta, sbyte depth, MoveBase pv = null)
        {
            Result result = new Result();
            if (IsEndGameDraw(result)) return result;

            if (pv == null && Table.TryGet(Position.GetKey(), out var entry))
            {
                pv = GetPv(entry.PvMove);
            }

            if (IsLateEndGame()) depth++;

            SortContext sortContext = DataPoolService.GetCurrentSortContext();
            sortContext.Set(Sorters[depth], pv);
            MoveList moves = sortContext.GetAllMoves(Position);

            DistanceFromRoot = sortContext.Ply;
            MaxExtensionPly = DistanceFromRoot + depth + ExtensionDepthDifference;

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

            result.Move.History++;
            return result;
        }

        public override short Search(short alpha, short beta, sbyte depth)
        {
            if (CheckEndGameDraw())
                return 0;

            if (depth < 1) return Evaluate(alpha, beta);

            MoveBase pv = null;
            bool shouldUpdate = false;
            bool isInTable = false;

            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                isInTable = true;
                pv = GetPv(entry.PvMove);

                if (pv == null || entry.Depth < depth)
                {
                    shouldUpdate = true;
                }
                else
                {
                    if (entry.Value >= beta)
                        return entry.Value;

                    if (entry.Value > alpha)
                        alpha = entry.Value;
                }
            }

            SearchContext context = GetCurrentContext(alpha, beta, depth, pv);

            if (SetSearchValue(alpha, beta, depth, context)) return context.Value;

            if (isInTable && !shouldUpdate) return context.Value;

            return StoreValue(depth, context.Value, context.BestMove.Key);
        }

        protected override bool[] InitializeReducableDepthTable()
        {
            var result = new bool[2 * Depth];
            for (int depth = 0; depth < result.Length; depth++)
            {
                result[depth] = depth > 2;
            }

            return result;
        }

        protected override bool[] InitializeReducableMoveTable()
        {
            var result = new bool[128];
            for (int move = 0; move < result.Length; move++)
            {
                result[move] = move > 3;
            }

            return result;
        }

        protected override sbyte[][] InitializeReductionTable()
        {
            var result = new sbyte[2 * Depth][];
            for (int depth = 0; depth < result.Length; depth++)
            {
                result[depth] = new sbyte[128];
                for (int move = 0; move < result[depth].Length; move++)
                {
                    if (depth > 4)
                    {
                        if (move > 10)
                        {
                            result[depth][move] = (sbyte)(depth - 3);
                        }
                        else if (move > 3)
                        {
                            result[depth][move] = (sbyte)(depth - 2);
                        }
                        else
                        {
                            result[depth][move] = (sbyte)(depth - 1);
                        }
                    }
                    else if (depth == 4)
                    {
                        if (move > 3)
                        {
                            result[depth][move] = (sbyte)(depth - 2);
                        }
                        else
                        {
                            result[depth][move] = (sbyte)(depth - 1);
                        }
                    }
                    else
                    {
                        result[depth][move] = (sbyte)(depth - 1);
                    }

                }
            }

            return result;
        }
    }
}