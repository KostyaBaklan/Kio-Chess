using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Null;

namespace GameTool.Strategies.Asp
{
    public class LmrDeepNullOneAspStrategy : AspOneStrategy
    {
        public LmrDeepNullOneAspStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override StrategyBase GetStrategy(short depth, IPosition position, TranspositionTable table)
        {
            return new NullLmrDeepStrategy(depth, position, table);
        }
    }
}
