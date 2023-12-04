using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Strategies.Base;

namespace Engine.Strategies.AB;

public class NegaMaxStrategy : StrategyBase
{
    public NegaMaxStrategy(short depth, IPosition position) : base(depth, position)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position, new HistoryComparer()));
    }

    protected override StrategyBase CreateSubSearchStrategy()
    {
        return new NegaMaxStrategy((short)(Depth - SubSearchDepth), Position);
    }
}
