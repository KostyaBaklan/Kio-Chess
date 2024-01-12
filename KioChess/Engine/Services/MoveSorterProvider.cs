using Engine.Interfaces;
using Engine.Sorting.Sorters;

namespace Engine.Services;

public class MoveSorterProvider: IMoveSorterProvider
{
    #region Implementation of IMoveSorterProvider

    public MoveSorterBase GetSimple(IPosition position) => new SimpleSorter(position);

    public MoveSorterBase GetAttack(IPosition position) => new AttackSorter(position);

    public MoveSorterBase GetComplex(IPosition position) => new ComplexSorter(position);

    public MoveSorterBase GetComplexQuiet(IPosition position) => new ComplexQuietSorter(position);

    #endregion
}
