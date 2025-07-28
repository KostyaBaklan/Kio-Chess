using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKnightValue()
        {
            int value = 0;

            var bits = _boards[Pieces.WhiteKnight];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();

                value += _evaluationService.GetWhiteKnightFullValue(coordinate);
                //if ((_whiteMinorDefense[coordinate] & _boards[Pieces.WhitePawn]).Any())
                //{
                //    value += _evaluationService.GetMinorDefendedByPawnValue();
                //}

                value += GetWhiteKnightMobility(coordinate);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKnightValue()
        {
            int value = 0;
            var bits = _boards[Pieces.BlackKnight];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackKnightFullValue(coordinate);

                //if ((_blackMinorDefense[coordinate] & _boards[Pieces.BlackPawn]).Any())
                //{
                //    value += _evaluationService.GetMinorDefendedByPawnValue();
                //}

                value += GetBlackKnightMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
