using Application.Interfaces;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Strategies.Aspiration;
using Engine.Strategies.Base;

namespace Application.Services;

internal class StrategyProvider : IStrategyProvider
{
    private readonly string _strategy;
    private readonly IStrategyFactory _strategyFactory;

    public StrategyProvider(IConfigurationProvider configuration, IStrategyFactory strategyFactory)
    {
        _strategy = configuration.GeneralConfiguration.Strategy;
        _strategyFactory = strategyFactory;
    }

    public StrategyBase GetStrategy(short level, Position position)
    {
        if (_strategyFactory.HasStrategy(_strategy))
        {
            return _strategyFactory.GetStrategy(level, position, _strategy);
        }

        return new LmrDeepAspirationStrategy(level, position);
    }
}
