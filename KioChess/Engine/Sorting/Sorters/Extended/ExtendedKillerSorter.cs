using Engine.DataStructures.Moves.Collections.Extended;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Extended
{
    public class ExtendedKillerSorter : ExtendedSorter
    {
        public ExtendedKillerSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            ExtendedMoveCollection = new ExtendedKillerMoveCollection(comparer);
        }
    }
}