using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.AB;
using Engine.Strategies.Base;
using Engine.Strategies.Base.Null;

namespace Engine.Strategies.Null;

public class NullExtendedStrategy : NullExtendedStrategyBase
{
    public NullExtendedStrategy(short depth, IPosition position, TranspositionTable table = null) : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));
    }

    protected override StrategyBase CreateSubSearchStrategy() => new NegaMaxMemoryStrategy((short)(Depth - SubSearchDepth), Position);
}
