using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;

namespace GameTool.Strategies.Id
{
    public class LmrOneIdStrategy : StepOneIdStrategy
    {
        public LmrOneIdStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override StrategyBase GetStrategy(short d, IPosition position, TranspositionTable table)
        {
            return new LmrStrategy(d, position, table);
        }
    }
}
