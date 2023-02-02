using System.Runtime.CompilerServices;
using Engine.Models.Moves;

namespace Engine.Sorting.Comparers
{
    public class HistoryDifferenceComparer : IMoveComparer
    {
        #region Implementation of IComparer<in IMove>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(MoveBase x, MoveBase y)
        {
            var comparision = y.History.CompareTo(x.History);
            return comparision != 0 ? comparision : y.Difference.CompareTo(x.Difference);
        }

        #endregion
    }
}