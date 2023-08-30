using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Null;

namespace GameTool.Strategies.Asp
{
    public class NullOneAspStrategy : AspOneStrategy
    {
        public NullOneAspStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override StrategyBase GetStrategy(short depth, IPosition position, TranspositionTable table)
        {
            return new NullNegaMaxMemoryStrategy(depth, position, table);
        }
    }
}
