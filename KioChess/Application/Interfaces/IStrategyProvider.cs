using Engine.Models.Boards;
using Engine.Strategies.Base;

namespace Application.Interfaces;

public interface IStrategyProvider
{
    StrategyBase GetStrategy(short level, Position position);
}
