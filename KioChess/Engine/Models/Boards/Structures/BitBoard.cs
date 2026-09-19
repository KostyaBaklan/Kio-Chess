using Engine.Models.Helpers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Engine.Models.Boards.Structures;

[StructLayout(LayoutKind.Sequential)]
public readonly struct BitBoard : IEquatable<BitBoard>
{
    #region Equality members

    public bool Equals(BitBoard other) => _value == other._value;

    public override bool Equals(object obj) => obj is BitBoard other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    #endregion

    private readonly ulong _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ulong(BitBoard bitBoard) => bitBoard._value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard(int value)
    {
        _value = (ulong)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard(ulong value)
    {
        _value = value;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BitBoard left, BitBoard right)
    {
        return left._value == right._value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BitBoard left, BitBoard right)
    {
        return left._value != right._value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(BitBoard left, BitBoard right)
    {
        return new BitBoard(left._value & right._value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(BitBoard left, BitBoard right)
    {
        return new BitBoard(left._value | right._value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator ^(BitBoard left, BitBoard right)
    {
        return new BitBoard(left._value ^ right._value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator ^(BitBoard left, ulong right)
    {
        return new BitBoard(left._value ^ right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator ~(BitBoard b)
    {
        return new BitBoard(~b._value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(BitBoard left, BitBoard right)
    {
        return left._value > right._value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(BitBoard left, BitBoard right)
    {
        return left._value > right._value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator >>(BitBoard left, int right)
    {
        return new BitBoard(left._value >> right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator <<(BitBoard left, int right)
    {
        return new BitBoard(left._value << right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong operator *(BitBoard left, ulong right)
    {
        return left._value * right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Set(params int[] bits)
    {
        var v = this;
        for (var index = 0; index < bits.Length; index++)
        {
            var bit = bits[index];
            v = v.Add(bit);
        }

        return v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Add(int bit) => new(_value | 1ul << bit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Remove(int bit) => new(_value & ~(1ul << bit));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Remove(BitBoard bit) => new(_value & ~bit._value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSet(BitBoard bitBoard) => (this & bitBoard) == bitBoard;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSet(byte bit) => (_value & 1ul << bit) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOff(byte bit) => (_value & 1ul << bit) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOff(BitBoard bitBoard) => (_value & bitBoard._value) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsZero() => _value == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong And(ulong value) => _value & value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Or(params byte[] squares)
    {
        BitBoard bit = squares[0].AsBitBoard();
        for (int i = 1; i < squares.Length; i++)
        {
            bit |= squares[i].AsBitBoard();
        }

        return bit;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Any() => _value != 0;

    #region Overrides of ValueType

    public override string ToString()
    {
        StringBuilder builder = new();

        for (byte i = 0; i < 64; i++)
        {
            var bit = 1ul << i;
            if ((_value & bit) == bit)
            {
                builder.Append($"{i.AsString()} ");
            }
        }

        return builder.ToString().TrimEnd();
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Count() => BitOperations.PopCount(_value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte BitScanForward() => (byte)BitOperations.TrailingZeroCount(_value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte BitScanReverse() => (byte)(63 - BitOperations.LeadingZeroCount(_value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Lsb() => new BitBoard(1ul << BitOperations.TrailingZeroCount(_value));
}