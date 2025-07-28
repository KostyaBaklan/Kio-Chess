using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueen()
        {
            int value = 0;
            var bits = _boards[Pieces.BlackQueen];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackQueenFullValue(coordinate);

                value += GetBlackQueenPins(coordinate);

                //value += GetBlackQueenMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueen()
        {
            int value = 0;
            var bits = _boards[Pieces.WhiteQueen];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteQueenFullValue(coordinate);

                value += GetWhiteQueenPins(coordinate);

                //value += GetWhiteQueenMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
