using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Services
{
    public class MoveSorterProvider: IMoveSorterProvider
    {
        #region Implementation of IMoveSorterProvider

        public MoveSorterBase GetInitial(IPosition position, IMoveComparer comparer)
        {
            return new InitialSorter(position, comparer);
        }

        public MoveSorterBase GetAdvanced(IPosition position, IMoveComparer comparer)
        {
            return new AdvancedSorter(position, comparer);
        }

        public MoveSorterBase GetAttack(IPosition position, IMoveComparer comparer)
        {
            return new AttackSorter(position, comparer);
        }

        public MoveSorterBase GetComplex(IPosition position, IMoveComparer comparer)
        {
            return new ComplexSorter(position, comparer);
        }

        public MoveSorterBase GetEndGame(IPosition position, IMoveComparer comparer)
        {
            return new EndGameSorter(position, comparer);
        }

        #endregion
    }
}
