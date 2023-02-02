using System.Runtime.CompilerServices;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.DataStructures
{
    public struct BoardSet
    {
        private byte _count;
        private readonly int _key;
        private readonly ulong _l0,_l1, _l2,_l3;

        public BoardSet(Piece[] pieces)
        {
            _l0 = 0;
            _l1 = 0;
            _l2 = 0;
            _l3 = 0;
            _count = 1;
            for (int i = 0; i < 16; i++)
            {
                _l0 = _l0 | ((ulong) pieces[i].AsByte() << (4 * i % 16));
            }

            for (int i = 16; i < 32; i++)
            {
                _l1 = _l1 | ((ulong) pieces[i].AsByte() << (4 * i % 16));
            }

            for (int i = 32; i < 48; i++)
            {
                _l2 = _l2 | ((ulong) pieces[i].AsByte() << (4 * i % 16));
            }

            for (int i = 48; i < 64; i++)
            {
                _l3 = _l3 | ((ulong) pieces[i].AsByte() << (4 * i % 16));
            }

            _key = (_l0 ^ _l1 ^ _l2 ^ _l3).GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is BoardSet other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BoardSet other)
        {
            return _l0 == other._l0 && _l1 == other._l1 && _l2 == other._l2 && _l3 == other._l3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return _key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoardSet left, BoardSet right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoardSet left, BoardSet right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCount(byte count)
        {
            _count = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCount()
        {
            return _count;
        }
    }
}
