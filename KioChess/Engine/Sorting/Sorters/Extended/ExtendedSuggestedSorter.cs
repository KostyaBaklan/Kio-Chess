using Engine.DataStructures.Moves.Collections.Extended;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Extended
{
    public class ExtendedSuggestedSorter : ExtendedSorter
    {
        public ExtendedSuggestedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            ExtendedMoveCollection = new ExtendedSuggestedMoveCollection(comparer);
        }
    }
}