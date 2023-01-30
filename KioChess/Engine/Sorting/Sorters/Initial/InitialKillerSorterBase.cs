using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Initial
{
    public abstract class InitialKillerSorterBase : InitialSorter
    {
        protected InitialKillerSorterBase(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
        }
    }
}