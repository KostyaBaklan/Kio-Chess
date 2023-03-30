using CommonServiceLocator;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting
{
    public static class Sort
    {
        public static readonly int[] SortMinimum;
        public static readonly IComparer<MoveBase> Comparer;
        public static readonly IMoveComparer DifferenceComparer;
        public static readonly IMoveComparer HistoryComparer;

        static Sort()
        {
            SortingConfiguration sortConfiguration;
            try
            {
                sortConfiguration = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                        .AlgorithmConfiguration.SortingConfiguration;
            }
            catch (Exception)
            {
                sortConfiguration = new SortingConfiguration { SortMinimum = 10, SortMoveIndex = 41, SortHalfIndex = 11 };
            }
            var historyComparer = new HistoryComparer();
            Comparer = historyComparer;
            HistoryComparer = historyComparer;

            DifferenceComparer = new DifferenceComparer();

            SortMinimum = new int[128];
            for (var i = 0; i < sortConfiguration.SortHalfIndex; i++)
            {
                SortMinimum[i] = Math.Min(GetSortCount(i), sortConfiguration.SortMinimum);
            }
            for (var i = sortConfiguration.SortHalfIndex; i < sortConfiguration.SortMoveIndex; i++)
            {
                SortMinimum[i] = Math.Min(GetSortCount(i), sortConfiguration.SortMinimum);
            }
            for (var i = sortConfiguration.SortMoveIndex; i < SortMinimum.Length; i++)
            {
                SortMinimum[i] = Math.Min(GetSortCount(i), sortConfiguration.SortMinimum + 1);
            }
        }

        private static int GetSortCount(int i, int factor = 0, int offset = 0)
        {
            return i < 4 ? 1 : (i + factor) / 3 + offset;
        }
    }
}
