using Engine.DataStructures.Hash;
using Engine.Models.Boards;
using Engine.Strategies.Base;

namespace Engine.Strategies.AB;

public class NegaMaxMemoryStrategy : MemoryStrategyBase
{
    public NegaMaxMemoryStrategy(int depth, Position position, TranspositionTable table = null) : base(depth, position, table)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));
    }
}
