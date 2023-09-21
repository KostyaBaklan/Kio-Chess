using Engine.Interfaces;

namespace GameTool.Strategies.Id
{
    public abstract class StepTwoIdStrategy : IdStrategyBase
    {
        protected StepTwoIdStrategy(short depth, IPosition position) : base(depth, position, 2)
        {
        }
    }
}
