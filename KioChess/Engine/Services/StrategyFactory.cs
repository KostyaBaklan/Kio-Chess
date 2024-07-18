using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Strategies.AB;
using Engine.Strategies.Aspiration;
using Engine.Strategies.Base;
using Engine.Strategies.ID;
using Engine.Strategies.Lmr;

namespace Engine.Services;

public class StrategyFactory : IStrategyFactory
{
    private readonly Dictionary<string, Func<short, Position, StrategyBase>> _strategyFactories =
            new Dictionary<string, Func<short, Position, StrategyBase>>
            {
                {"lmr", (d, p) => new LmrStrategy(d, p)},
                {"lmrd", (d, p) => new LmrDeepStrategy(d, p)},
                //{"lmr_null", (d, p) => new NullLmrStrategy(d, p)},
                //{"lmrd_null", (d, p) => new NullLmrDeepStrategy(d, p)},

                {"ab", (d, p) => new NegaMaxMemoryStrategy(d, p)},
                //{"ab_null", (d, p) => new NullNegaMaxMemoryStrategy(d, p)},
                //{"null_ext", (d, p) => new NullExtendedStrategy(d, p)},

                {"asp", (d, p) => new AspirationStrategy(d, p)},
                {"id", (d, p) => new IteretiveDeepingStrategy(d, p)}
            };

    private readonly Dictionary<string, Func<short, Position,TranspositionTable, StrategyBase>> _strategyMemoryFactories =
            new Dictionary<string, Func<short, Position, TranspositionTable, StrategyBase>>
            {
                {"lmr", (d, p,t) => new LmrStrategy(d, p,t)},
                {"lmrd", (d, p,t) => new LmrDeepStrategy(d, p,t)},
                //{"lmr_null", (d, p,t) => new NullLmrStrategy(d, p,t)},
                //{"lmrd_null", (d, p,t) => new NullLmrDeepStrategy(d, p,t)},

                {"ab", (d, p,t) => new NegaMaxMemoryStrategy(d, p,t)},
                {"asp", (d, p,t) => new AspirationStrategy(d, p,t)},
                //{"ab_null", (d, p,t) => new NullNegaMaxMemoryStrategy(d, p,t)},
                //{"null_ext", (d, p,t) => new NullExtendedStrategy(d, p,t)}
            };

    public StrategyBase GetStrategy(short depth, Position position, string code) => _strategyFactories[code](depth, position);

    public StrategyBase GetStrategy(short depth, Position position, TranspositionTable table, string code) => _strategyMemoryFactories[code](depth, position, table);

    public bool HasMemoryStrategy(string strategy) => _strategyMemoryFactories.ContainsKey(strategy);

    public bool HasStrategy(string strategy) => _strategyFactories.ContainsKey(strategy);
}
