using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Models;

namespace Engine.Strategies.End
{
    public class IdLmrDeepEndStrategy : MemoryStrategyBase
    {
        protected List<IterativeDeepingModel> Models;

        public IdLmrDeepEndStrategy(short depth, IPosition position, TranspositionTable table = null) 
            : base(depth, position,table)
        {
            Stack<IterativeDeepingModel> models = new Stack<IterativeDeepingModel>();

            var d = Depth;
            while (d > 3)
            {
                models.Push(new IterativeDeepingModel { Depth = d, Strategy = new IdItemLmrDeepEndStrategy(d, position, Table) });
                d -= 2;
            }

            Models = models.ToList();
        }

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
                    result = model.Strategy.GetResult((short)-SearchValue, SearchValue, model.Depth, result.Move);
                }
            }

            return result;
        }

        public override short Search(short alpha, short beta, sbyte depth)
        {
            return Models.Last().Strategy.Search(alpha, beta, depth);
        }
    }
}
