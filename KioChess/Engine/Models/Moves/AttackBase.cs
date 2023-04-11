using System.Runtime.CompilerServices;

namespace Engine.Models.Moves
{
    public abstract class AttackBase : MoveBase,IComparable<AttackBase>
    {
        public byte Captured;
        public int See;

        protected AttackBase()
        {
            IsAttack = true;
        }

        #region Implementation of IComparable<in AttackBase>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(AttackBase other)
        {
            return other.See.CompareTo(See);
        }

        #endregion
    }
}