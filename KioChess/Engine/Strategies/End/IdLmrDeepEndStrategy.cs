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
            Models = new List<IterativeDeepingModel>();

            if(depth > 7)
            {
                Models.Add(new IterativeDeepingModel { Depth = (sbyte)(Depth - 3), Strategy = new IdItemLmrDeepEndStrategy((sbyte)(Depth - 3),position,Table) });
                Models.Add(new IterativeDeepingModel { Depth = (sbyte)(Depth - 1), Strategy = new IdItemLmrDeepEndStrategy((sbyte)(Depth - 1), position, Table) });
                Models.Add(new IterativeDeepingModel { Depth = Depth, Strategy = new IdItemLmrDeepEndStrategy(Depth, position, Table) });
            }
            else if(depth > 5)
            {
                Models.Add(new IterativeDeepingModel { Depth = (sbyte)(Depth - 1), Strategy = new IdItemLmrDeepEndStrategy((sbyte)(Depth - 1), position, Table) });
                Models.Add(new IterativeDeepingModel { Depth = Depth, Strategy = new IdItemLmrDeepEndStrategy(Depth, position, Table) });
            }
            else
            {
                Models.Add(new IterativeDeepingModel { Depth = Depth, Strategy = new IdItemLmrDeepEndStrategy(Depth, position, Table) });
            }
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
