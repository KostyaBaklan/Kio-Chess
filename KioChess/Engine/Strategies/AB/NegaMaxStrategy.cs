using Engine.Models.Boards;
using Engine.Strategies.Base;

namespace Engine.Strategies.AB;

public class NegaMaxStrategy : StrategyBase
{
    public NegaMaxStrategy(int depth, Position position) : base(depth, position)
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));
    }
}
