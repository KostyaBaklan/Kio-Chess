using Engine.Interfaces;
using Engine.Sorting.Sorters;

namespace Engine.Services;

public class MoveSorterProvider: IMoveSorterProvider
{
    #region Implementation of IMoveSorterProvider

    public MoveSorterBase GetSimple(IPosition position)
    {
        return new SimpleSorter(position);
    }

    public MoveSorterBase GetAttack(IPosition position)
    {
        return new AttackSorter(position);
    }

    public MoveSorterBase GetComplex(IPosition position)
    {
        return new ComplexSorter(position);
    }

    #endregion
}
