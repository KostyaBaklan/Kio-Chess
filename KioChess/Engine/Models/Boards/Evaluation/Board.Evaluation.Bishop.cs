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
                value += _pstService.GetPstValue(Pieces.WhiteBishop, coordinate, _phaseValue)
                    + GetWhiteBishopPinsEnd(coordinate)
                    + GetEvaluationWhiteBishopMobility(coordinate);
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
                value += _pstService.GetPstValue(Pieces.BlackBishop, coordinate, _phaseValue)
                    + GetBlackBishopPinsEnd(coordinate)
                    + GetEvaluationBlackBishopMobility(coordinate);
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
                value += _pstService.GetPstValue(Pieces.BlackBishop, coordinate, _phaseValue)
                    + GetBlackBishopPinsOpening(coordinate)
                    + GetEvaluationBlackBishopMobility(coordinate);
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
                value += _pstService.GetPstValue(Pieces.WhiteBishop, coordinate, _phaseValue) 
                    + GetWhiteBishopPinsOpening(coordinate) 
                    + GetEvaluationWhiteBishopMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
