using Engine.DataStructures.Hash;
using Engine.Models.Boards;
using Engine.Strategies.Base;

namespace Engine.Interfaces;

public interface IStrategyFactory
{
    StrategyBase GetStrategy(short depth, Position position, string code);
    StrategyBase GetStrategy(short depth, Position position, TranspositionTable table, string code);
    bool HasStrategy(string strategy);
    bool HasMemoryStrategy(string strategy);
}
