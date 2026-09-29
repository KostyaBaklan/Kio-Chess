using Engine.Models.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKnightOpening() => GetWhiteKnightValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKnightMiddle() => GetWhiteKnightValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKnightEnd() => GetWhiteKnightValue();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKnightOpening() => GetBlackKnightValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKnightMiddle() => GetBlackKnightValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKnightEnd() => GetBlackKnightValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKnightValue()
        {
            int value = 0;

            var bits = Unsafe.Add(ref _boards[0], Pieces.WhiteKnight);
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();

                value += _pstService.GetPstValue(Pieces.WhiteKnight, coordinate, _phaseValue) + GetEvaluationWhiteKnightMobility(coordinate);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKnightValue()
        {
            int value = 0;
            var bits = Unsafe.Add(ref _boards[0], Pieces.BlackKnight);
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();

                value += _pstService.GetPstValue(Pieces.BlackKnight, coordinate, _phaseValue) + GetEvaluationBlackKnightMobility(coordinate);

                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
