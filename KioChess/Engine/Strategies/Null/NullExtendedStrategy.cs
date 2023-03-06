using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Strategies.AB;
using Engine.Strategies.Base;
using Engine.Strategies.Base.Null;

namespace Engine.Strategies.Null
{
    public class NullExtendedStrategy : NullExtendedStrategyBase
    {
        public NullExtendedStrategy(short depth, IPosition position, TranspositionTable table = null) : base(depth, position, table)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetAdvanced(position, new HistoryComparer()));
        }

        protected override StrategyBase CreateSubSearchStrategy()
        {
            return new NegaMaxMemoryStrategy((short)(Depth - SubSearchDepth), Position);
        }
    }
}
