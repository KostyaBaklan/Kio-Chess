using Engine.Interfaces;

namespace GameTool.Strategies.Id
{
    public abstract class StepOneIdStrategy : IdStrategyBase
    {
        protected StepOneIdStrategy(short depth, IPosition position) : base(depth, position, 1)
        {
        }
    }
}
