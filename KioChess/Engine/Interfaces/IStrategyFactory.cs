using Engine.DataStructures.Hash;
using Engine.Models.Boards;
using Engine.Strategies.Base;

namespace Engine.Interfaces;

public interface IStrategyFactory
{
    MemoryStrategyBase GetStrategy(short depth, Position position, string code);
    MemoryStrategyBase GetStrategy(short depth, Position position, TranspositionTable table, string code);
    bool HasStrategy(string strategy);
    bool HasMemoryStrategy(string strategy);
}
