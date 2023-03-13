using Engine.DataStructures.Hash;
using Engine.Strategies.Base;

namespace Engine.Interfaces
{
    public interface IStrategyFactory
    {
        StrategyBase GetStrategy(short depth, IPosition position, string code);
        StrategyBase GetStrategy(short depth, IPosition position, TranspositionTable table, string code);
        bool HasStrategy(string strategy);
        bool HasMemoryStrategy(string strategy);
    }
}
