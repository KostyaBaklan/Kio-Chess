using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using Engine.DataStructures;
using Engine.Models.Boards;

namespace Engine.Models.Helpers
{
    public static class BitBoardExtensions
    {
        private const ulong _magic = 0x07EDD5E59A4E28C2;

        private static readonly byte[] _magicTable;

        static BitBoardExtensions()
        {
            _magicTable = new byte[64];

            ulong bit = 1;
            byte i = 0;
            do
            {
                _magicTable[(bit * _magic) >> 58] = i;
                i++;
                bit <<= 1;
            } while (bit != 0);

            var set = _magicTable.ToHashSet();
            if (set.Count < 64)
            {
                throw new Exception();
            }

            if (set.Min() != 0)
            {
                throw new Exception();
            }
            if (set.Max() != 63)
            {
                throw new Exception();
            }
        }

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
                positionsList.Add( position);
                b = b.Remove(position);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte BitScanReverse(this BitBoard b)
        {
            b |= b >> 1;
            b |= b >> 2;
            b |= b >> 4;
            b |= b >> 8;
            b |= b >> 16;
            b |= b >> 32;
            b = b & ~(b >> 1);
            return _magicTable[(b * _magic) >> 58];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte BitScanForward(this BitBoard b)
        {
            return _magicTable[(b.Lsb() * _magic) >> 58];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count(this BitBoard b)
        {
            if (Popcnt.X64.IsSupported)
            {
                return (int)Popcnt.X64.PopCount(b.AsValue());
            }

            int count = 0;
            while (b.Any())
            {
                byte position = BitScanForward(b);
                count++;
                b = b.Remove(position);
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGreaterRank(this int bit, int coordinate)
        {
            return bit / 8 > coordinate / 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLessRank(this byte bit, byte coordinate)
        {
            return bit / 8 < coordinate / 8;
        }

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
}