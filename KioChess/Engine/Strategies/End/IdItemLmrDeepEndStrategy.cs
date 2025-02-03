using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
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
            ExtensionOffest = depth + configurationProvider.AlgorithmConfiguration.ExtensionConfiguration.EndDepthDifference;
        }

        public override StrategyType Type => StrategyType.LMRD;

        protected override int ReducableDepth => 2;

        protected override int MinimumMaxMoveCount => 5;
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

            if (MoveHistory.CanUseNull())
            {
                depth = CalculateWhiteDepth(beta, depth, transpositionContext.Pv);

                if (depth < 1)
                {
                    return EvaluateWhite(alpha, beta);
                } 
            }

            SearchContext context = transpositionContext.Pv < 0
                ? GetCurrentContext(alpha, beta, depth)
                : GetCurrentContext(alpha, beta, depth, transpositionContext.Pv);

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

            if (MoveHistory.CanUseNull())
            {
                depth = CalculateBlackDepth(beta, depth, transpositionContext.Pv);

                if (depth < 1)
                {
                    return EvaluateBlack(alpha, beta);
                } 
            }

            SearchContext context = transpositionContext.Pv < 0
                ? GetCurrentContext(alpha, beta, depth)
                : GetCurrentContext(alpha, beta, depth, transpositionContext.Pv);

            if (SetSearchValueBlack(alpha, beta, depth, context) && transpositionContext.ShouldUpdate)
            {
                StoreBlackValue(depth, (short)context.Value, context.BestMove);
            }
            return context.Value;
        }
    }
}
