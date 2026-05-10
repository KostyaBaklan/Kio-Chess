using Engine.Models.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueenOpening() => EvaluateWhiteQueen();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueenMiddle() => EvaluateWhiteQueen();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueenEnd() => EvaluateWhiteQueen();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueenOpening() => EvaluateBlackQueen();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueenMiddle() => EvaluateBlackQueen();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueenEnd() => EvaluateBlackQueen();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueen()
        {
            int value = 0;
            var bits = Unsafe.Add(ref _boards[0], Pieces.WhiteQueen);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueen()
        {
            int value = 0;
            var bits = Unsafe.Add(ref _boards[0], Pieces.BlackQueen);
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
    }
}
