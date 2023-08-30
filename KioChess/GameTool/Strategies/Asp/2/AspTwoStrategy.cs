using Engine.Interfaces;

namespace GameTool.Strategies.Asp
{
    public abstract class AspTwoStrategy : AspStrategyBase
    {
        protected AspTwoStrategy(short depth, IPosition position) : base(depth, position, 2)
        {
        }
    }
}
