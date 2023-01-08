using Application.Interfaces;
using Engine.Interfaces;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;

namespace Application.Services
{
    internal class StrategyProvider : IStrategyProvider
    {
        public StrategyBase GetStrategy(short level, IPosition position)
        {
            return new LmrDeepStrategy(level, position);
        }
    }
}
