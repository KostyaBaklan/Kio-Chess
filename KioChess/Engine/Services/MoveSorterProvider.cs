using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Services;

public class MoveSorterProvider: IMoveSorterProvider
{
    #region Implementation of IMoveSorterProvider

    public MoveSorterBase GetExtended(IPosition position, IMoveComparer comparer)
    {
        return new ExtendedSorter(position, comparer);
    }

    public MoveSorterBase GetSimple(IPosition position, IMoveComparer comparer)
    {
        return new SimpleSorter(position, comparer);
    }

    public MoveSorterBase GetAttack(IPosition position, IMoveComparer comparer)
    {
        return new AttackSorter(position, comparer);
    }

    public MoveSorterBase GetComplex(IPosition position, IMoveComparer comparer)
    {
        return new ComplexSorter(position, comparer);
    }

    #endregion
}
