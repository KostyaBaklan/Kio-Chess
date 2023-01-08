using Engine.Interfaces;
using Engine.Strategies.Base;

namespace Application.Interfaces
{
    public interface IStrategyProvider
    {
        StrategyBase GetStrategy(short level, IPosition position);
    }
}
