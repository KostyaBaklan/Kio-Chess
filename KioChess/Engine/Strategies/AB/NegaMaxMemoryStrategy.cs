using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;

namespace Engine.Strategies.AB;

public class NegaMaxMemoryStrategy : MemoryStrategyBase
{
    public NegaMaxMemoryStrategy(int depth, IPosition position, TranspositionTable table = null) : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));
    }

    protected override StrategyBase CreateSubSearchStrategy() => new NegaMaxMemoryStrategy(Depth - SubSearchDepth, Position);
}
