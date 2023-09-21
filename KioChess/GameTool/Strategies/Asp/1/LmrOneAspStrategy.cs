using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;

namespace GameTool.Strategies.Asp
{
    public class LmrOneAspStrategy : AspOneStrategy
    {
        public LmrOneAspStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override StrategyBase GetStrategy(short depth, IPosition position, TranspositionTable table)
        {
            return new LmrStrategy(depth, position, table);
        }
    }
}
