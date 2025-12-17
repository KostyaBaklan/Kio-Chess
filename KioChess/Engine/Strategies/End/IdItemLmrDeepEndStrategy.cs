using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Strategies.Lmr;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.End
{
    public class IdItemLmrDeepEndStrategy : LmrStrategyBase
    {
        private readonly sbyte _pawnEndgameDepthExtension;
        private readonly sbyte _lateEndgameDepthExtension;

        public IdItemLmrDeepEndStrategy(short depth, Position position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            ExtensionOffest = depth + configurationProvider.AlgorithmConfiguration.ExtensionConfiguration.EndDepthDifference;
            _pawnEndgameDepthExtension = configurationProvider.EndGameConfiguration.PawnEndgameDepthExtension;
            _lateEndgameDepthExtension = configurationProvider.EndGameConfiguration.LateEndgameDepthExtension;
        }

        public override StrategyType Type => StrategyType.LMRD;

        protected override int[] GetLmrConfig() => configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrEnd;

        protected override int[] GetLmrRatio() => configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrEndRatio;

        public override IResult GetResult() => GetResult(MinusSearchValue, SearchValue, Depth);

        public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
        {
            Result result = new();
            if (IsDraw(result)) return result;

            SortContext sortContext = GetSortContext(depth, pv);
            SearchContext context = DataPoolService.GetCurrentContext();
            context.Clear();
            sortContext.GetAllMoves(Position, ref context.Moves);

            SetExtensionThresholds(sortContext.Ply);

            if (CheckEndGame(context.Moves.Count, result)) return result;

            // Depth extension for endgames:
            // Pure pawn endgames (K+P vs K+P) require much deeper search
            // Late endgames also benefit from extra depth
            //if (_board.IsPawnEndgame())
            //{
            //    depth += _pawnEndgameDepthExtension;
            //}
            //else 
            if (_board.IsLateEndGame())
            {
                depth += _lateEndgameDepthExtension;
            }

            SetLmrResult(alpha, beta, depth, result, ref context.Moves);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int SearchWhite(int alpha, int beta, sbyte depth)
        {
            if (CheckDraw())
                return 0;

            if (depth < 1) return EvaluateWhite(alpha, beta);

            return CommonWhiteSearch(alpha, beta, depth);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int SearchBlack(int alpha, int beta, sbyte depth)
        {
            if (CheckDraw())
                return 0;

            if (depth < 1) return EvaluateBlack(alpha, beta);

            return CommonBlackSearch(alpha, beta, depth);
        }
    }
}
