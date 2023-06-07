using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves
{
    public struct AttackSee
    {
        public short Key;
        public short See;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsGreater(AttackSee move)
        {
            return See > move.See;
        }
    }
}