using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Interfaces
{
    public interface IMoveSorterProvider
    {
        MoveSorter GetBasic(IPosition position, IMoveComparer comparer);
        MoveSorter GetInitial(IPosition position, IMoveComparer comparer);
        MoveSorter GetExtended(IPosition position, IMoveComparer comparer);
        MoveSorter GetAdvanced(IPosition position, IMoveComparer comparer);
        MoveSorter GetHardExtended(IPosition position, IMoveComparer comparer);
        MoveSorter GetDifferenceExtended(IPosition position, IMoveComparer comparer);
    }
}