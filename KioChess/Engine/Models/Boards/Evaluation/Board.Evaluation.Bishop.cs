using Engine.Models.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteBishopOpening() => GetWhiteBishopValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteBishopMiddle() => GetWhiteBishopValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteBishopEnd()
        {
            var bits = Unsafe.Add(ref _boards[0], Pieces.WhiteBishop);
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

                value += GetEvaluationWhiteBishopMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackBishopOpening() => GetBlackBishopValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackBishopMiddle() => GetBlackBishopValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackBishopEnd()
        {
            var bits = Unsafe.Add(ref _boards[0], Pieces.BlackBishop);
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

                value += GetEvaluationBlackBishopMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackBishopValue()
        {
            var bits = Unsafe.Add(ref _boards[0], Pieces.BlackBishop);
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

                value += GetEvaluationBlackBishopMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteBishopValue()
        {
            var bits = Unsafe.Add(ref _boards[0], Pieces.WhiteBishop);
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

                value += GetEvaluationWhiteBishopMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
