using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Null;

namespace GameTool.Strategies.Asp
{
    public class LmrNullTwoAspStrategy : AspTwoStrategy
    {
        public LmrNullTwoAspStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override StrategyBase GetStrategy(short depth, IPosition position, TranspositionTable table)
        {
            return new NullLmrStrategy(depth, position, table);
        }
    }
}
