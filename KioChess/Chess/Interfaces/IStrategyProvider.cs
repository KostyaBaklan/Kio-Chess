using Engine.Interfaces;
using Engine.Strategies.Base;

namespace Chess.Interfaces
{
    public interface IStrategyProvider
    {
        StrategyBase GetStrategy(short level, IPosition position);
    }
}
