using Engine.DataStructures.Moves.Collections.Extended;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Extended
{
    public class ExtendedDifferenceSorter : ExtendedKillerSorter
    {
        public ExtendedDifferenceSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            ExtendedMoveCollection = new ExtendedDifferenceMoveCollection(comparer);
        }
    }
}