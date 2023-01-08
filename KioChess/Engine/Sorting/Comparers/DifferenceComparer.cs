using System.Runtime.CompilerServices;
using Engine.Models.Moves;

namespace Engine.Sorting.Comparers
{
    public class DifferenceComparer : IMoveComparer
    {
        #region Implementation of IComparer<in IMove>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(MoveBase x, MoveBase y)
        {
            return y.Difference.CompareTo(x.Difference);
        }

        #endregion
    }
}