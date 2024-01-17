using Engine.Models.Boards;
using Engine.Strategies.Base;

namespace Application.Interfaces;

public interface IStrategyProvider
{
    MemoryStrategyBase GetStrategy(short level, Position position);
}
