using Engine.Sorting.Sorters;

namespace Engine.Interfaces;

public interface IMoveSorterProvider
{
    MoveSorterBase GetAttack(IPosition position);
    MoveSorterBase GetSimple(IPosition position);
    MoveSorterBase GetComplex(IPosition position);
}