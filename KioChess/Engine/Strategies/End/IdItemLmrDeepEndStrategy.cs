using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Strategies.Lmr;
using Engine.Strategies.Models.Contexts;

namespace Engine.Strategies.End
{
    public class IdItemLmrDeepEndStrategy : LmrStrategyBase
    {
        public IdItemLmrDeepEndStrategy(short depth, Position position, TranspositionTable table = null)
            : base(depth, position, table)
        {
        }

        protected override int ReducableDepth => 3;

        protected override int MinimumMaxMoveCount => 5;

        protected override int MinimumMinMoveCount => 4;
        public override IResult GetResult() => GetResult(MinusSearchValue, SearchValue, Depth);
        
        public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
        {
            Result result = new Result();
            if (IsEndGameDraw(result)) return result;

            SortContext sortContext = GetSortContext(depth, pv);
            MoveList moves = sortContext.GetAllMoves(Position);

            SetExtensionThresholds(sortContext.Ply);

            if (CheckEndGame(moves.Count, result)) return result;

            if (IsLateEndGame()) depth++;

            SetLmrResult(alpha, beta, depth, result, moves);

            return result;
        }

        public override int SearchWhite(int alpha, int beta, sbyte depth)
        {
            if (CheckDraw())
                return 0;

            if (depth < 1) return EvaluateWhite(alpha, beta);

            TranspositionContext transpositionContext = GetWhiteTranspositionContext(beta, depth);
            if (transpositionContext.IsBetaExceeded) return beta;

            SearchContext context = transpositionContext.Pv < 0
                ? GetCurrentContext(alpha, beta, ref depth)
                : GetCurrentContext(alpha, beta, ref depth, transpositionContext.Pv);

            if (SetSearchValueWhite(alpha, beta, depth, context) && transpositionContext.ShouldUpdate)
            {
                StoreWhiteValue(depth, (short)context.Value, context.BestMove);
            }
            return context.Value;
        }

        public override int SearchBlack(int alpha, int beta, sbyte depth)
        {
            if (CheckDraw())
                return 0;

            if (depth < 1) return EvaluateBlack(alpha, beta);

            TranspositionContext transpositionContext = GetBlackTranspositionContext(beta, depth);
            if (transpositionContext.IsBetaExceeded) return beta;

            SearchContext context = transpositionContext.Pv < 0
                ? GetCurrentContext(alpha, beta, ref depth)
                : GetCurrentContext(alpha, beta, ref depth, transpositionContext.Pv);

            if (SetSearchValueBlack(alpha, beta, depth, context) && transpositionContext.ShouldUpdate)
            {
                StoreBlackValue(depth, (short)context.Value, context.BestMove);
            }
            return context.Value;
        }

        protected override sbyte[][][] InitializeReductionMinTable()
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
                            if (move > 50)
                            {
                                if (i > 20)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 3);
                                }
                                else if (i > MinimumMinMoveCount)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 2);
                                }
                                else
                                {
                                    result[depth][move][i] = (sbyte)(depth - 1);
                                }
                            }
                            else if (move > 40)
                            {
                                if (i > 16)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 3);
                                }
                                else if (i > MinimumMinMoveCount)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 2);
                                }
                                else
                                {
                                    result[depth][move][i] = (sbyte)(depth - 1);
                                }
                            }
                            else if (move > 30)
                            {
                                if (i > 14)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 3);
                                }
                                else if (i > MinimumMinMoveCount)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 2);
                                }
                                else
                                {
                                    result[depth][move][i] = (sbyte)(depth - 1);
                                }
                            }
                            else if (move > 20)
                            {
                                if (i > 12)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 3);
                                }
                                else if (i > MinimumMinMoveCount)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 2);
                                }
                                else
                                {
                                    result[depth][move][i] = (sbyte)(depth - 1);
                                }
                            }
                            else
                            {
                                if (i > 10)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 3);
                                }
                                else if (i > MinimumMinMoveCount)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 2);
                                }
                                else
                                {
                                    result[depth][move][i] = (sbyte)(depth - 1);
                                }
                            }
                        }
                        else if (depth > ReducableDepth)
                        {
                            if (i > MinimumMinMoveCount)
                            {
                                result[depth][move][i] = (sbyte)(depth - 2);
                            }
                            else
                            {
                                result[depth][move][i] = (sbyte)(depth - 1);
                            }
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

        protected override sbyte[][][] InitializeReductionMaxTable()
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
                            if (move > 50)
                            {
                                if (i > 20)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 3);
                                }
                                else if (i > MinimumMaxMoveCount)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 2);
                                }
                                else
                                {
                                    result[depth][move][i] = (sbyte)(depth - 1);
                                }
                            }
                            else if (move > 40)
                            {
                                if (i > 18)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 3);
                                }
                                else if (i > MinimumMaxMoveCount)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 2);
                                }
                                else
                                {
                                    result[depth][move][i] = (sbyte)(depth - 1);
                                }
                            }
                            else if (move > 30)
                            {
                                if (i > 15)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 3);
                                }
                                else if (i > MinimumMaxMoveCount)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 2);
                                }
                                else
                                {
                                    result[depth][move][i] = (sbyte)(depth - 1);
                                }
                            }
                            else if (move > 20)
                            {
                                if (i > 12)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 3);
                                }
                                else if (i > MinimumMaxMoveCount)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 2);
                                }
                                else
                                {
                                    result[depth][move][i] = (sbyte)(depth - 1);
                                }
                            }
                            else
                            {
                                if (i > 10)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 3);
                                }
                                else if (i > MinimumMaxMoveCount)
                                {
                                    result[depth][move][i] = (sbyte)(depth - 2);
                                }
                                else
                                {
                                    result[depth][move][i] = (sbyte)(depth - 1);
                                }
                            }
                        }
                        else if (depth > ReducableDepth)
                        {
                            if (i > MinimumMaxMoveCount)
                            {
                                result[depth][move][i] = (sbyte)(depth - 2);
                            }
                            else
                            {
                                result[depth][move][i] = (sbyte)(depth - 1);
                            }
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
    }
}
