using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Sorting.Sorters;

namespace Engine.Services;

public class MoveSorterProvider: IMoveSorterProvider
{
    #region Implementation of IMoveSorterProvider

    public MoveSorterBase GetSimple(Position position) => new SimpleSorter(position);

    public MoveSorterBase GetAttack(Position position) => new AttackSorter(position);

    public MoveSorterBase GetComplex(Position position) => new ComplexSorter(position);

    public MoveSorterBase GetComplexQuiet(Position position) => new ComplexQuietSorter(position);

    #endregion
}
