using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;

namespace GameTool.Strategies.Asp
{
    public class LmrDeepOneAspStrategy : AspOneStrategy
    {
        public LmrDeepOneAspStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override StrategyBase GetStrategy(short depth, IPosition position, TranspositionTable table)
        {
            return new LmrDeepStrategy(depth, position, table);
        }
    }
}
