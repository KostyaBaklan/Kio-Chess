using Engine.DataStructures;
using Engine.Models.Boards;

namespace Engine.Interfaces
{
    public interface IBitService
    {
        int Count(BitBoard b);
        byte BitScanForward(BitBoard b);
        IEnumerable<byte> BitScan(BitBoard b);
        void GetPositions(BitBoard b, PositionsList positionsList);
        void GetPositions(BitBoard b, SquareList positionsList);
    }
}
