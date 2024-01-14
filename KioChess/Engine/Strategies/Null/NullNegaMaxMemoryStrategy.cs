using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base.Null;

namespace Engine.Strategies.Null;

public class NullNegaMaxMemoryStrategy : NullMemoryStrategyBase
{
    public NullNegaMaxMemoryStrategy(short depth, IPosition position, TranspositionTable table = null) 
        : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));
    }
}
