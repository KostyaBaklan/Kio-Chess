using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.Services.Bits
{
    public abstract class BitServiceBase : IBitService
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<byte> BitScan(BitBoard b)
        {
            while (b.Any())
            {
                byte position = BitScanForward(b);
                yield return position;
                b = b.Remove(position);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract byte BitScanForward(BitBoard b);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract byte Count(BitBoard b);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetPositions(BitBoard b, SquareList positionsList)
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
        public void GetPositions(BitBoard b, PositionsList positionsList)
        {
            positionsList.Clear();
            while (b.Any())
            {
                byte position = BitScanForward(b);
                positionsList.Add(position);
                b = b.Remove(position);
            }
        }
    }
}
