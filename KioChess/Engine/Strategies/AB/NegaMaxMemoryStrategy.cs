using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Strategies.Base;

namespace Engine.Strategies.AB;

public class NegaMaxMemoryStrategy : MemoryStrategyBase
{
    public NegaMaxMemoryStrategy(short depth, IPosition position, TranspositionTable table = null) : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position, new HistoryComparer()));
    }

    protected override StrategyBase CreateSubSearchStrategy()
    {
        return new NegaMaxMemoryStrategy((short)(Depth - SubSearchDepth), Position);
    }
}
