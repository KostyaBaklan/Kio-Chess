using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteBishopValue()
        {
            var bits = _boards[Pieces.WhiteBishop];
            int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteBishopFullValue(coordinate);

                value += GetWhiteBishopPinsOpening(coordinate);

                //if ((_whiteMinorDefense[coordinate] & _boards[Pieces.WhitePawn]).Any())
                //{
                //    value += _evaluationService.GetMinorDefendedByPawnValue();
                //}

                value += GetWhiteBishopMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackBishopValue()
        {
            var bits = _boards[Pieces.BlackBishop];
            int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackBishopFullValue(coordinate);

                value += GetBlackBishopPinsOpening(coordinate);

                //if ((_blackMinorDefense[coordinate] & _boards[Pieces.BlackPawn]).Any())
                //{
                //    value += _evaluationService.GetMinorDefendedByPawnValue();
                //}

                value += GetBlackBishopMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteBishopEnd()
        {
            var bits = _boards[Pieces.WhiteBishop];
            int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteBishopFullValue(coordinate);

                value += GetWhiteBishopPinsEnd(coordinate);

                //if ((_whiteMinorDefense[coordinate] & _boards[Pieces.WhitePawn]).Any())
                //{
                //    value += _evaluationService.GetMinorDefendedByPawnValue();
                //}

                value += GetWhiteBishopMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackBishopEnd()
        {
            var bits = _boards[Pieces.BlackBishop];
            int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackBishopFullValue(coordinate);

                value += GetBlackBishopPinsEnd(coordinate);

                //if ((_blackMinorDefense[coordinate] & _boards[Pieces.BlackPawn]).Any())
                //{
                //    value += _evaluationService.GetMinorDefendedByPawnValue();
                //}

                value += GetBlackBishopMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
