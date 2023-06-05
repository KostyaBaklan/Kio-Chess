using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Strategies.AB;
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
            return new NegaMaxMemoryStrategy((short)(Depth - SubSearchDepth), Position);
        }

        protected override StrategyBase CreateEndGameStrategy()
        {
            return new LmrDeepEndGameStrategy((short)Math.Min(Depth + 1, MaxEndGameDepth), Position, Table);
        }
    }
}
