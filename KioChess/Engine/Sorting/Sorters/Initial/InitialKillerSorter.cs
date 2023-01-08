using Engine.DataStructures.Moves.Collections.Initial;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Initial
{
    public class InitialKillerSorter : InitialKillerSorterBase
    {
        public InitialKillerSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            InitialMoveCollection = new InitialKillerMoveCollection(comparer);
        }
    }
}