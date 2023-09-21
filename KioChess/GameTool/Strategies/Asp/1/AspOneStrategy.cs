using Engine.Interfaces;

namespace GameTool.Strategies.Asp
{
    public abstract class AspOneStrategy : AspStrategyBase
    {
        protected AspOneStrategy(short depth, IPosition position) : base(depth, position, 1)
        {
        }
    }
}
