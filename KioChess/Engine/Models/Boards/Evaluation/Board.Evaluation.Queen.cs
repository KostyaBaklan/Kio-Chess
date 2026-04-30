using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueenOpening()
        {
            int value = 0;
            var bits = _boards[Pieces.WhiteQueen];

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteQueenFullValue(coordinate);

                value += GetWhiteQueenPins(coordinate);

                // Accumulate center attacks (optimization: avoid separate iteration)
                _whiteCenterAttacks += (coordinate.QueenAttacks(_occupied) & _centerSquares).Count();

                // Penalize early queen development (only in opening)
                if (coordinate != Squares.D1)
                {
                    // Count developed white minor pieces (not on first rank)
                    int developedMinors = ((_boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop]) & ~_whiteFirstRank).Count();

                    // Penalize if queen moved before threshold minor pieces developed
                    if (developedMinors < _evaluationService.GetEarlyQueenMinorPieceThreshold())
                        value -= _evaluationService.GetEarlyQueenPenalty();
                }

                // Check if queen is hanging (attacked but not defended)
                value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteQueen);

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

                // Accumulate center attacks (optimization: avoid separate iteration)
                _whiteCenterAttacks += (coordinate.QueenAttacks(_occupied) & _centerSquares).Count();

                // Check if queen is hanging (attacked but not defended)
                value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteQueen);

                //value += GetWhiteQueenMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueenMiddle() => EvaluateWhiteQueen();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueenEnd() => EvaluateWhiteQueen();

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

                // Accumulate center attacks (optimization: avoid separate iteration)
                _blackCenterAttacks += (coordinate.QueenAttacks(_occupied) & _centerSquares).Count();

                // Check if queen is hanging (attacked but not defended)
                value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackQueen);

                //value += GetBlackQueenMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueenOpening()
        {
            int value = 0;
            var bits = _boards[Pieces.BlackQueen];

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackQueenFullValue(coordinate);

                value += GetBlackQueenPins(coordinate);

                // Accumulate center attacks (optimization: avoid separate iteration)
                _blackCenterAttacks += (coordinate.QueenAttacks(_occupied) & _centerSquares).Count();

                // Penalize early queen development (only in opening)
                if (coordinate != Squares.D8)
                {
                    // Count developed black minor pieces (not on eighth rank)
                    int developedMinors = ((_boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop]) & ~_blackFirstRank).Count();

                    // Penalize if queen moved before threshold minor pieces developed
                    if (developedMinors < _evaluationService.GetEarlyQueenMinorPieceThreshold())
                        value -= _evaluationService.GetEarlyQueenPenalty();
                }

                // Check if queen is hanging (attacked but not defended)
                value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackQueen);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueenMiddle() => EvaluateBlackQueen();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueenEnd() => EvaluateBlackQueen();
    }
}
