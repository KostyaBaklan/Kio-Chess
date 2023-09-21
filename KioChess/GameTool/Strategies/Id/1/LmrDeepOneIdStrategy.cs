using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;

namespace GameTool.Strategies.Id
{
    public class LmrDeepOneIdStrategy : StepOneIdStrategy
    {
        public LmrDeepOneIdStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override StrategyBase GetStrategy(short d, IPosition position, TranspositionTable table)
        {
            return new LmrDeepStrategy(d, position, table);
        }
    }
}
