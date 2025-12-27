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