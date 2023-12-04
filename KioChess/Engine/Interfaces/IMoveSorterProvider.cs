using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Interfaces;

public interface IMoveSorterProvider
{
    MoveSorterBase GetAttack(IPosition position, IMoveComparer comparer);
    MoveSorterBase GetExtended(IPosition position, IMoveComparer comparer);
    MoveSorterBase GetSimple(IPosition position, IMoveComparer comparer);
    MoveSorterBase GetComplex(IPosition position, IMoveComparer comparer);
    MoveSorterBase GetRiskComplex(IPosition position, IMoveComparer comparer);
}