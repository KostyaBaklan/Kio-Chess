using Engine.DataStructures.Moves.Collections.Extended;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Extended
{
    public class ExtendedTradeSorter : ExtendedSorter
    {
        public ExtendedTradeSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            ExtendedMoveCollection = new ExtendedTradeMoveCollection(comparer);
        }
    }
}