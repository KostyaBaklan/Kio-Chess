using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Null;

namespace GameTool.Strategies.Id
{
    public class LmrNullTwoIdStrategy : StepTwoIdStrategy
    {
        public LmrNullTwoIdStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override StrategyBase GetStrategy(short d, IPosition position, TranspositionTable table)
        {
            return new NullLmrStrategy(d, position, table);
        }
    }
}
