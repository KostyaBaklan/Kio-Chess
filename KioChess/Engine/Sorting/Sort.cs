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
                var min = Math.Min(i / 3, sortConfiguration.SortMinimum);
                if (min == 0)
                {
                    min = 1;
                }
                SortMinimum[i] = min;
            }
            for (var i = sortConfiguration.SortHalfIndex; i < sortConfiguration.SortMoveIndex; i++)
            {
                SortMinimum[i] = Math.Min(i / 3, sortConfiguration.SortMinimum);
            }
            for (var i = sortConfiguration.SortMoveIndex; i < SortMinimum.Length; i++)
            {
                SortMinimum[i] = sortConfiguration.SortMinimum + 1;
            }
        }
    }
}
