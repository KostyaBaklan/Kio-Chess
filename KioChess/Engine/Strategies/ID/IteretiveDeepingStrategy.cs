using Engine.Interfaces;

namespace Engine.Strategies.ID
{
    public class IteretiveDeepingStrategy : IteretiveDeepingStrategyBase
    {
        public IteretiveDeepingStrategy(short depth, IPosition position) : base(depth, position)
        {
        }
    }
}
