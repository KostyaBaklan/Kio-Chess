using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.End
{
    public class IdLmrDeepEndStrategy : StrategyBase
    {
        protected List<IterativeDeepingModel> Models;

        public IdLmrDeepEndStrategy(int depth, Position position, TranspositionTable table = null) 
            : base(depth, position,table)
        {
            Models = new List<IterativeDeepingModel>();

            for (sbyte d = 3; d <= Depth; d++)
            {
                Models.Add(new IterativeDeepingModel { Depth = d, Strategy = new IdItemLmrDeepEndStrategy(d, position, Table) });
            }
        }

        public override StrategyType Type => StrategyType.LMRD;

        public override IResult GetResult()
        {
            IResult result = new Result
            {
                GameResult = GameResult.Continue,
                Move = null,
                Value = 0
            };

            foreach (var model in Models)
            {
                result = model.Strategy.GetResult(MinusSearchValue, SearchValue, model.Depth, result.Move);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int SearchWhite(int alpha, int beta, sbyte depth) => Models[GetStrategy(depth)].Strategy.SearchWhite(alpha, beta, depth);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int SearchBlack(int alpha, int beta, sbyte depth) => Models[GetStrategy(depth)].Strategy.SearchBlack(alpha, beta, depth);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetStrategy(sbyte depth)
        {
            int i = 0;
            while (i < Models.Count && Models[i].Depth < depth)
            {
                i++;
            }

            return i;
        }
    }
}
