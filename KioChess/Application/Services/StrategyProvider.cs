using Application.Interfaces;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Strategies.Aspiration;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Engine.Strategies.Null;
using System.Collections.Generic;
using System;
using Engine.Strategies.AB;

namespace Application.Services
{
    internal class StrategyProvider : IStrategyProvider
    {
        private readonly string _strategy;
        private readonly Dictionary<string, Func<short, IPosition, StrategyBase>> _strategyFactories =
                new Dictionary<string, Func<short, IPosition, StrategyBase>>
                {
                    {"lmr", (d, p) => new LmrStrategy(d, p)},
                    {"lmrd", (d, p) => new LmrDeepStrategy(d, p)},
                    {"lmr_null", (d, p) => new NullLmrStrategy(d, p)},
                    {"lmrd_null", (d, p) => new NullLmrDeepStrategy(d, p)},

                    {"ab", (d, p) => new NegaMaxMemoryStrategy(d, p)},
                    {"ab_null", (d, p) => new NullNegaMaxMemoryStrategy(d, p)},
                    {"null_ext", (d, p) => new NullExtendedStrategy(d, p)},

                    {"lmr_asp", (d, p) => new LmrAspirationStrategy(d, p)},
                    {"lmrd_asp", (d, p) => new LmrDeepAspirationStrategy(d, p)}
                };

        public StrategyProvider(IConfigurationProvider configuration)
        {
            _strategy = configuration.GeneralConfiguration.Strategy;
        }

        public StrategyBase GetStrategy(short level, IPosition position)
        {
            if (_strategyFactories.TryGetValue(_strategy, out var factory))
            {
                return factory(level, position);
            }

            if (level < 8)
            {
                return new NullLmrDeepStrategy(level, position); 
            }

            return new LmrDeepAspirationStrategy(level, position);
        }
    }
}
