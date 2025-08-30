using Engine.DataStructures;
using Engine.Models.Boards.Structures;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Engine.Models.Helpers;

public static class BitBoardExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<byte> BitScan(this BitBoard b)
    {
        while (b.Any())
        {
            byte position = BitScanForward(b);
            yield return position;
            b = b.Remove(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetPositions(this BitBoard b, PositionsList positionsList)
    {
        positionsList.Clear();
        while (b.Any())
        {
            byte position = BitScanForward(b);
            positionsList.Add(position);
            b = b.Remove(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte BitScanReverse(this BitBoard b) =>
#if BMI
        (byte)(63 - Lzcnt.X64.LeadingZeroCount(b.AsValue()));
#else
    (byte)BitOperations.LeadingZeroCount(b.AsValue());
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte BitScanForward(this BitBoard b) =>
#if BMI
        (byte)Bmi1.X64.TrailingZeroCount(b.AsValue());
#else
    (byte)BitOperations.TrailingZeroCount(b.AsValue());
#endif


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Count(this BitBoard b)
    {
#if BMI
        return (byte)Popcnt.X64.PopCount(b.AsValue());
#else
        return (byte)BitOperations.PopCount(b.AsValue());
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Lsb(this BitBoard b)
    {
#if BMI
        return new BitBoard(1ul << (int)Bmi1.X64.TrailingZeroCount(b.AsValue()));
#else
        return new BitBoard(1ul << BitOperations.TrailingZeroCount(b.AsValue()));
#endif
    }

    public static string ToBitString(this BitBoard b)
    {
        StringBuilder builder = new();

        BitBoard mask = new(1);

        for (int i = 63; i >= 0; i--)
        {
            var x = b >> i;
            x = x & mask;
            if (x.IsZero())
            {
                builder.Append('0');
            }
            else if (x == mask)
            {
                builder.Append('1');
            }
            else
            {
                throw new Exception("Dabeg");
            }
        }

        return builder.ToString();
    }
}