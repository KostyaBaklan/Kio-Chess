using Engine.Models.Boards;
using Engine.Strategies.Base;

namespace KioChess.App.Interfaces;

public interface IStrategyProvider
{
    StrategyBase GetStrategy(short level, Position position);
}
