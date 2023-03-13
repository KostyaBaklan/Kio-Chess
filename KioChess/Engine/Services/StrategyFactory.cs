using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.AB;
using Engine.Strategies.Aspiration;
using Engine.Strategies.Base;
using Engine.Strategies.ID;
using Engine.Strategies.Lmr;
using Engine.Strategies.Null;

namespace Engine.Services
{
    public class StrategyFactory : IStrategyFactory
    {
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
                    ,
                    {"id", (d, p) => new IteretiveDeepingStrategy(d, p)}
                };

        private readonly Dictionary<string, Func<short, IPosition,TranspositionTable, StrategyBase>> _strategyMemoryFactories =
                new Dictionary<string, Func<short, IPosition, TranspositionTable, StrategyBase>>
                {
                    {"lmr", (d, p,t) => new LmrStrategy(d, p,t)},
                    {"lmrd", (d, p,t) => new LmrDeepStrategy(d, p,t)},
                    {"lmr_null", (d, p,t) => new NullLmrStrategy(d, p,t)},
                    {"lmrd_null", (d, p,t) => new NullLmrDeepStrategy(d, p,t)},

                    {"ab", (d, p,t) => new NegaMaxMemoryStrategy(d, p,t)},
                    {"ab_null", (d, p,t) => new NullNegaMaxMemoryStrategy(d, p,t)},
                    {"null_ext", (d, p,t) => new NullExtendedStrategy(d, p,t)}
                };

        public StrategyBase GetStrategy(short depth, IPosition position, string code)
        {
            return _strategyFactories[code](depth, position);
        }

        public StrategyBase GetStrategy(short depth, IPosition position, TranspositionTable table, string code)
        {
            return _strategyMemoryFactories[code](depth, position, table);
        }

        public bool HasMemoryStrategy(string strategy)
        {
            return _strategyMemoryFactories.ContainsKey(strategy);
        }

        public bool HasStrategy(string strategy)
        {
            return _strategyFactories.ContainsKey(strategy);
        }
    }
}
