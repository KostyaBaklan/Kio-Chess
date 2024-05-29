using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.Models;

namespace Engine.Strategies.End
{
    public class IdLmrDeepEndStrategy : StrategyBase
    {
        protected List<IterativeDeepingModel> Models;

        public IdLmrDeepEndStrategy(int depth, Position position, TranspositionTable table = null) 
            : base(depth, position,table)
        {
            Stack<IterativeDeepingModel> models = new Stack<IterativeDeepingModel>();
            models.Push(new IterativeDeepingModel { Depth = Depth, Strategy = new IdItemLmrDeepEndStrategy(Depth, position, Table) });

            sbyte d = (sbyte)(Depth - 1);
            while (d > 5)
            {
                models.Push(new IterativeDeepingModel { Depth = d, Strategy = new IdItemLmrDeepEndStrategy(d, position, Table) });
                d--;
            }

            Models = models.ToList();
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
                if(result.GameResult == GameResult.Continue)
                {
                    result = model.Strategy.GetResult(MinusSearchValue, SearchValue, model.Depth, result.Move);
                }
            }

            return result;
        }

        public override int SearchWhite(int alpha, int beta, sbyte depth) => Models.Last().Strategy.SearchWhite(alpha, beta, depth);

        public override int SearchBlack(int alpha, int beta, sbyte depth) => Models.Last().Strategy.SearchBlack(alpha, beta, depth);
    }
}
