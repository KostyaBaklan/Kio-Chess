using Engine.DataStructures.Hash;
using Engine.Models.Boards;
using Engine.Strategies.Base.Null;

namespace Engine.Strategies.Null;

public class NullNegaMaxMemoryStrategy : NullStrategyBase
{
    public NullNegaMaxMemoryStrategy(short depth, Position position, TranspositionTable table = null) 
        : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));
    }
}
