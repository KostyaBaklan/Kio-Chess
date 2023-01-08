using System.Runtime.CompilerServices;
using Engine.Models.Moves;

namespace Engine.Sorting.Comparers
{
    public class AttackSeeComparer : IComparer<AttackBase>
    {
        #region Implementation of IComparer<in IAttack>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(AttackBase x, AttackBase y)
        {
            return y.See.CompareTo(x.See);
        }

        #endregion
    }
}