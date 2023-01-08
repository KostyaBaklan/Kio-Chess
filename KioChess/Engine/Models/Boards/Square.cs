using System.Runtime.CompilerServices;
using Engine.Models.Helpers;

namespace Engine.Models.Boards
{
    public struct Square
    {
        #region Equality members

        public bool Equals(Square other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is Square other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion

        private readonly byte _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square(byte value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square(int value)
        {
            _value = (byte) value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AsInt()
        {
            return _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte AsByte()
        {
            return _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Square left, Square right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Square left, Square right)
            => left._value != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong AsLong(Square square)
        {
            return 1ul << _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard AsBitBoard()
        {
            return new BitBoard(1ul << _value);
        }

        public static int operator -(Square left, int right)
        {
            return left._value - right;
        }

        public static int operator +(Square left, int right)
        {
            return left._value + right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Square left, Square right)
            => left._value + right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Square left, Square right)
            => left._value - right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator |(Square left, Square right)
        {
            return new Square(left._value | right._value);
        }

        #region Overrides of ValueType

        public override string ToString()
        {
            return $"[{this.AsString()}, {_value}]";
        }

        #endregion
    }
}
