using Engine.DataStructures;
using Engine.Models.Boards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interfaces
{
    public interface IBitService
    {
        int Count(BitBoard b);
        byte BitScanForward(BitBoard b);
        IEnumerable<byte> BitScan(BitBoard b);
        void GetPositions(BitBoard b, PositionsList positionsList);
    }
}
