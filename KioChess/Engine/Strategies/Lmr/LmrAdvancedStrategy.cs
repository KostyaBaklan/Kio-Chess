using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.Lmr
{
    public class LmrAdvancedStrategy : LmrStrategy
    {
        public LmrAdvancedStrategy(short depth, IPosition position, TranspositionTable table = null) : base(depth, position, table)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetInitial(position, new HistoryComparer()));
        }
    }
}
