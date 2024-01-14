using Engine.Interfaces;
using Engine.Strategies.Base;

namespace Engine.Strategies.AB;

public class NegaMaxStrategy : StrategyBase
{
    public NegaMaxStrategy(int depth, IPosition position) : base(depth, position)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));
    }
}
