using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Interfaces
{
    public interface IMoveSorterProvider
    {
        MoveSorterBase GetAttack(IPosition position, IMoveComparer comparer);
        MoveSorterBase GetInitial(IPosition position, IMoveComparer comparer);
        MoveSorterBase GetAdvanced(IPosition position, IMoveComparer comparer);
        MoveSorterBase GetComplex(IPosition position, IMoveComparer comparer);
        MoveSorterBase GetEndGame(IPosition position, IMoveComparer comparer);
    }
}