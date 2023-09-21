using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Null;

namespace GameTool.Strategies.Id
{
    public class LmrDeepNullOneIdStrategy : StepOneIdStrategy
    {
        public LmrDeepNullOneIdStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override StrategyBase GetStrategy(short d, IPosition position, TranspositionTable table)
        {
            return new NullLmrDeepStrategy(d, position, table);
        }
    }
}
