using Engine.DataStructures.Moves.Collections.Extended;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Extended
{
    public class ExtendedHardSorter : ExtendedKillerSorter
    {
        public ExtendedHardSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            ExtendedMoveCollection = new ExtendedHardMoveCollection(comparer);
        }
    }
}