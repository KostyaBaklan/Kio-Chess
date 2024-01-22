using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Strategies.Lmr;
using Engine.Strategies.Models;
using Engine.Strategies.Models.Contexts;

namespace Engine.Strategies.End
{
    public class IdItemLmrDeepEndStrategy : LmrStrategyBase
    {
        public IdItemLmrDeepEndStrategy(short depth, Position position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            MinReduction = configurationProvider.AlgorithmConfiguration.NullConfiguration.MinReduction + 1;
            MaxReduction = configurationProvider.AlgorithmConfiguration.NullConfiguration.MaxReduction + 1;
        }
        public override IResult GetResult() => GetResult(MinusSearchValue, SearchValue, Depth);
        
        public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
        {
            Result result = new Result();
            if (IsEndGameDraw(result)) return result;

            if (pv == null && Table.TryGet(Position.GetKey(), out var entry))
            {
                pv = GetPv(entry.PvMove);
            }

            if (IsLateEndGame()) depth++;

            return SetLmrResult(alpha, beta, depth, pv);
        }

        public override int Search(int alpha, int beta, sbyte depth)
        {
            if (CheckDraw())
                return 0;

            if (depth < 1) return Evaluate(alpha, beta); 
            
            NullResult nullResult = GetNullResult(depth, beta);

            if (nullResult.ShouldPrune)
            {
                if (nullResult.Value < 1) return Evaluate(alpha, beta);

                depth = (sbyte)nullResult.Value; 
            }

            TranspositionContext transpositionContext = GetTranspositionContext(beta, depth);
            if (transpositionContext.IsBetaExceeded) return beta;

            SearchContext context = GetCurrentContext(alpha, beta, ref depth, transpositionContext.Pv);

            return SetSearchValue(alpha, beta, depth, context) || transpositionContext.NotShouldUpdate
                ? context.Value
                : StoreValue(depth, (short)context.Value, context.BestMove.Key);
        }

        protected override NullResult GetNullResult(sbyte depth, int beta)
        {
            if (NullDepthOffset <= depth || beta >= SearchValue || MoveHistory.IsLastMoveWasCheck())
                return new NullResult { ShouldPrune = false, Value = depth };

            Position.SwapTurn();
            var nullValue = -NullSearch(1 - beta, depth > 6 ? depth - MaxReduction : depth - MinReduction);
            Position.SwapTurn();

            return nullValue < beta
                ? new NullResult { ShouldPrune = false, Value = depth }
                : new NullResult { ShouldPrune = true, Value = depth - 2 };
        }

        protected override bool[] InitializeReducableDepthTable()
        {
            var result = new bool[2 * Depth];
            for (int depth = 0; depth < result.Length; depth++)
            {
                result[depth] = depth > 3;
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
                    if (depth > 6)
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
                    else if (depth > 4)
                    {
                        if (move > 12)
                        {
                            result[depth][move] = (sbyte)(depth - 3);
                        }
                        else if (move > 4)
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
                        if (move > 5)
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
