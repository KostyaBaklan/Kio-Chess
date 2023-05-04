using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Strategies.Aspiration;
using Engine.Strategies.Base;
using Engine.Strategies.Null;
using UI.Common.Interfaces;

namespace UI.Common.Services
{
    public class StrategyProvider : IStrategyProvider
    {
        private readonly string _strategy;
        private readonly IStrategyFactory _strategyFactory;

        public StrategyProvider(IConfigurationProvider configuration, IStrategyFactory strategyFactory)
        {
            _strategy = configuration.GeneralConfiguration.Strategy;
            _strategyFactory = strategyFactory;
        }

        public StrategyBase GetStrategy(short level, IPosition position)
        {
            if (_strategyFactory.HasStrategy(_strategy))
            {
                return _strategyFactory.GetStrategy(level, position, _strategy);
            }

            if (level < 8)
            {
                return new NullLmrDeepStrategy(level, position);
            }

            return new LmrDeepAspirationStrategy(level, position);
        }
    }
}
