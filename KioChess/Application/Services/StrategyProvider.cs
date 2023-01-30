using Application.Interfaces;
using Engine.Interfaces;
using Engine.Strategies.Aspiration;
using Engine.Strategies.Base;

namespace Application.Services
{
    internal class StrategyProvider : IStrategyProvider
    {
        public StrategyBase GetStrategy(short level, IPosition position)
        {
            return new LmrDeepAspirationStrategy(level, position);
        }
    }
}
