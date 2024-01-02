using Engine.Interfaces;
using Engine.Strategies.Base.Null;

namespace Engine.Strategies.Null;

public class NullNegaMaxStrategy : NullStrategyBase
{
    public NullNegaMaxStrategy(short depth, IPosition position) : base(depth, position)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));
    }
}
