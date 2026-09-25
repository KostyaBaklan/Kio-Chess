using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.End
{
    public class IdLmrDeepEndStrategy : StrategyBase
    {
        private readonly List<AspirationModel> Models;
        private readonly StrategyBase _strategy;


        public IdLmrDeepEndStrategy(int depth, Position position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            Models = [];

            var lmrTables = new LmrTables(configurationProvider, depth,
                configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrEnd
                , configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.LmrEndRatio);

            var EndGameDepthOffset = configurationProvider.EndGameConfiguration.EndGameDepthOffset[depth];
            for (sbyte d = EndGameDepthOffset; d <= Depth+3; d++)
            {
                Models.Add(new AspirationModel { Depth = d , Window = configurationProvider.AlgorithmConfiguration.AspirationConfiguration.AspirationEndWindow });
            }
            _strategy = new IdItemLmrDeepEndStrategy(Models.Last().Depth, position, table, lmrTables);
        }

        public override StrategyType Type => StrategyType.LMRD;

        public override IResult GetResult()
        {
            DataPoolService.Resize(Table);

            IResult result = new Result
            {
                GameResult = GameResult.Continue,
                Move = null,
                Value = 0
            };

            sbyte depth = (sbyte)(Depth + _board.GetEndgamePhaseExtension());

            var window = SearchValue;

            foreach (var model in Models.TakeWhile(m => m.Depth <= depth))
            {
                int alpha = result.Value - window;
                int beta = result.Value + window;

                var move = result.Move;
                window = model.Window;

                result = _strategy.GetResult(alpha, beta, model.Depth, move);
                if (result.GameResult != GameResult.Continue) break;

                if (result.Value < beta && result.Value > alpha)
                    continue;

                result = _strategy.GetResult(MinusSearchValue, SearchValue, model.Depth, move);
            }

            //foreach (var model in Models.TakeWhile(m => m.Depth <= depth))
            //{
            //    result = _strategy.GetResult(MinusSearchValue, SearchValue, model.Depth, result.Move);
            //    if (result.GameResult != GameResult.Continue) break;
            //}

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int SearchWhite(int alpha, int beta, sbyte depth) => _board.ShouldExtendEndGameSearch()
                ? _strategy.SearchWhite(alpha, beta, (sbyte)(depth + 1))
                : _strategy.SearchWhite(alpha, beta, depth);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int SearchBlack(int alpha, int beta, sbyte depth) => _board.ShouldExtendEndGameSearch()
                ? _strategy.SearchBlack(alpha, beta, (sbyte)(depth + 1))
                : _strategy.SearchBlack(alpha, beta, depth);

        protected override StrategyBase CreateEndGameStrategy() => null;
    }
}
