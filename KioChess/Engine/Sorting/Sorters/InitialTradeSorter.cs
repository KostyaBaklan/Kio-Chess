using Engine.DataStructures.Moves.Collections;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class InitialTradeSorter : InitialSorter
    {
        public InitialTradeSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            InitialMoveCollection = new InitialMoveCollection(comparer);
        }
    }
}