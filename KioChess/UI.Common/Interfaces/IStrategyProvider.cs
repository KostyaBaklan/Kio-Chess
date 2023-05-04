using Engine.Interfaces;
using Engine.Strategies.Base;

namespace UI.Common.Interfaces
{
    public interface IStrategyProvider
    {
        StrategyBase GetStrategy(short level, IPosition position);
    }
}
