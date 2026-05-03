using Engine.DataStructures;
using Engine.Models.Boards.Structures;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Engine.Models.Helpers;

public static class BitBoardExtensions
{
    private static readonly bool _isBmi2Supported = Bmi2.X64.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<byte> BitScan(this BitBoard b)
    {
        while (b.Any())
        {
            byte position = b.BitScanForward();
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
            byte position = b.BitScanForward();
            positionsList.Add(position);
            b = b.Remove(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ExtractBits(this BitBoard bits, BitBoard mask)
    {
        if (_isBmi2Supported)
            return (short)Bmi2.X64.ParallelBitExtract(bits, mask);
        return (short)ParallelBitExtract(bits, mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ParallelBitExtract(ulong value, ulong mask)
    {
        ulong result = 0;
        ulong outputBit = 0;

        while (mask != 0)
        {
            ulong lowestSetBit = mask & unchecked((ulong)-(long)mask);
            if ((value & lowestSetBit) != 0)
            {
                result |= 1u << (int)outputBit;
            }

            mask &= mask - 1;   // clear lowest set bit
            outputBit++;
        }

        return result;
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