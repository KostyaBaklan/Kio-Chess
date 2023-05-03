using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.End
{
    public class LmrDeepEndGameStrategy : LmrDeepStrategy
    {
        public LmrDeepEndGameStrategy(short depth, IPosition position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            ExtensionDepthDifference = configurationProvider
                .AlgorithmConfiguration.EndExtensionDepthDifference[depth];
        }

        public override IResult GetResult()
        {
            return GetResult(-SearchValue, SearchValue, Depth);
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            Result result = new Result();
            if (IsEndGameDraw(result)) return result;

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

            DistanceFromRoot = sortContext.Ply; 
            MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

            if (CheckEndGame(moves.Count, result)) return result;

            if (moves.Count > 1)
            {
                if (MoveHistory.IsLastMoveNotReducible())
                {
                    SetResult(alpha, beta, depth, result, moves);
                }
                else
                {
                    int d = depth - 1;
                    int b = -beta;
                    for (byte i = 0; i < moves.Count; i++)
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
            }
            else
            {
                result.Move = moves[0];
            }

            result.Move.History++;
            return result;
        }

        public override int Search(int alpha, int beta, int depth)
        {
            if (depth < 1) return Evaluate(alpha, beta);

            if (CheckEndGameDraw()) return 0;

            MoveBase pv = null;
            bool shouldUpdate = false;
            bool isInTable = false;

            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                isInTable = true;

                shouldUpdate = entry.Depth < depth;

                pv = GetPv(entry.PvMove);
            }

            SearchContext context = GetCurrentContext(alpha, beta, depth, pv);

            if(SetSearchValue(alpha, beta, depth, context))return context.Value;

            if (isInTable && !shouldUpdate) return context.Value;

            return StoreValue((byte)depth, (short)context.Value, context.BestMove.Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetExtension(MoveBase move)
        {
            return move.IsCheck|| move.IsPromotion || move.IsPromotionExtension ? 1 : 0;
        }

        protected override byte[][] InitializeReductionTable()
        {
            var result = new byte[2 * Depth][];
            for (int depth = 0; depth < result.Length; depth++)
            {
                result[depth] = new byte[128];
                for (int move = 0; move < result[depth].Length; move++)
                {
                    if (depth > 3)
                    {
                        if (move > 11)
                        {
                            result[depth][move] = (byte)(depth - 3);
                        }
                        else if (move > 3)
                        {
                            result[depth][move] = (byte)(depth - 2);
                        }
                        else
                        {
                            result[depth][move] = (byte)(depth - 1);
                        }
                    }
                    else if (depth == 3)
                    {
                        if (move > 3)
                        {
                            result[depth][move] = (byte)(depth - 2);
                        }
                        else
                        {
                            result[depth][move] = (byte)(depth - 1);
                        }
                    }
                    else
                    {
                        result[depth][move] = (byte)(depth - 1);
                    }

                }
            }

            return result;
        }

        protected override StrategyBase CreateSubSearchStrategy()
        {
            return new LmrDeepEndGameStrategy((short)(Depth - SubSearchDepth), Position);
        }
    }
}
