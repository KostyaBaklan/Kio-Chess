using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Strategies.Base;
using Engine.Strategies.Base.Null;
using Engine.Strategies.End;

namespace Engine.Strategies.Null
{
    public class NullNegaMaxMemoryStrategy : NullMemoryStrategyBase
    {
        public NullNegaMaxMemoryStrategy(short depth, IPosition position, TranspositionTable table = null) 
            : base(depth, position, table)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetAdvanced(position, new HistoryComparer()));
        }

        protected override StrategyBase CreateSubSearchStrategy()
        {
            return new LmrDeepEndGameStrategy((short)(Depth - SubSearchDepth), Position);
        }
    }
}
