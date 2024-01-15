using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Services.Bits;

namespace Engine.Models.Helpers;

public static class BitBoardExtensions
{
    private static readonly BitServiceBase _bitService;

    static BitBoardExtensions()
    {
        _bitService = ServiceLocator.Current.GetInstance<BitServiceBase>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<byte> BitScan(this BitBoard b) => _bitService.BitScan(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetPositions(this BitBoard b, PositionsList positionsList) => _bitService.GetPositions(b, positionsList);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetPositions(this BitBoard b, ref BitList positionsList) => _bitService.GetPositions(b, ref positionsList);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetPositions(this BitBoard b, SquareList positionsList) => _bitService.GetPositions(b, positionsList);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static byte BitScanReverse(this BitBoard b)
    //{
    //    b |= b >> 1;
    //    b |= b >> 2;
    //    b |= b >> 4;
    //    b |= b >> 8;
    //    b |= b >> 16;
    //    b |= b >> 32;
    //    b = b & ~(b >> 1);
    //    return _magicTable[(b * _magic) >> 58];
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte BitScanForward(this BitBoard b) => _bitService.BitScanForward(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Count(this BitBoard b) => _bitService.Count(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGreaterRank(this int bit, int coordinate) => bit / 8 > coordinate / 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLessRank(this byte bit, byte coordinate) => bit / 8 < coordinate / 8;

    public static string ToBitString(this BitBoard b)
    {
        StringBuilder builder = new StringBuilder();

        BitBoard mask = new BitBoard(1);

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