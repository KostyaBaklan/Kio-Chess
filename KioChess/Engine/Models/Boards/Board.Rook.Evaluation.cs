using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookOpening()
        {
            int i = -1;
            int value = 0;

            var king = _boards[Pieces.BlackKing].BitScanForward();
            var bits = _boards[Pieces.WhiteRook];

            while (bits.Any())
            {
                i++;
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteRookFullValue(coordinate);

                if ((coordinate > Squares.H2 && (_whiteFacing[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero()) ||
                    (_rookFiles[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();

                    if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                    {
                        value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                    }

                    if (i > 0 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.WhiteRook]).Any()
                        && (_rookFiles[coordinate] & _boards[Pieces.WhiteRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnOpenFileValue();
                    }
                }
                else if ((coordinate > Squares.H2 && (_whiteFacing[coordinate] & _boards[Pieces.WhitePawn]).IsZero()) ||
                    (_rookFiles[coordinate] & _boards[Pieces.WhitePawn]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();

                    if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                    {
                        value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                    }

                    if (i > 0 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.WhiteRook]).Any()
                        && (_rookFiles[coordinate] & _boards[Pieces.WhiteRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                    }
                }
                if (i > 0 && coordinate < Squares.A2 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.WhiteRook]).Any()
                        && (_rookRanks[coordinate] & _boards[Pieces.WhiteRook]).Any())
                {
                    value += _evaluationService.GetConnectedRooksOnFirstRankValue();
                }

                value += GetWhiteRookPinsOpening(coordinate);

                if ((_whiteRookKingPattern[coordinate] & _boards[Pieces.WhiteKing]).Any() &&
                    (_whiteRookPawnPattern[coordinate] & _boards[Pieces.WhitePawn]).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }

                //value += GetWhiteRookMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookMiddle() => EvaluateWhiteRookOpening();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookEnd()
        {
            int value = 0;
            var bits = _boards[Pieces.WhiteRook];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteRookFullValue(coordinate);

                if ((_rookFiles[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn]))
                    .IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((_rookFiles[coordinate] & _boards[Pieces.WhitePawn]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                value += GetWhiteRookPinsEnd(coordinate);

                //value += GetWhiteRookMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookOpening()
        {
            int value = 0;
            int i = -1;
            var king = _boards[Pieces.WhiteKing].BitScanForward();
            var bits = _boards[Pieces.BlackRook];
            while (bits.Any())
            {
                i++;
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackRookFullValue(coordinate);

                if ((coordinate < Squares.A7 && (_blackFacing[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero()) ||
                    (_rookFiles[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();

                    if ((_whiteKingPatterns[king] & _rookFiles[coordinate]).Any())
                    {
                        value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                    }

                    if (i > 0 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.BlackRook]).Any()
                        && (_rookFiles[coordinate] & _boards[Pieces.BlackRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnOpenFileValue();
                    }
                }
                else if ((coordinate < Squares.A7 && (_blackFacing[coordinate] & _boards[Pieces.BlackPawn]).IsZero()) ||
                    (_rookFiles[coordinate] & _boards[Pieces.BlackPawn]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();

                    if ((_whiteKingPatterns[king] & _rookFiles[coordinate]).Any())
                    {
                        value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                    }

                    if (i > 0 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.BlackRook]).Any()
                        && (_rookFiles[coordinate] & _boards[Pieces.BlackRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                    }
                }
                if (i > 0 && coordinate > Squares.H7 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.BlackRook]).Any()
                        && (_rookRanks[coordinate] & _boards[Pieces.BlackRook]).Any())
                {
                    value += _evaluationService.GetConnectedRooksOnFirstRankValue();
                }

                value += GetBlackRookPinsOpening(coordinate);

                if ((_blackRookKingPattern[coordinate] & _boards[Pieces.BlackKing]).Any() &&
                    (_blackRookPawnPattern[coordinate] & _boards[Pieces.BlackPawn]).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }

                //value += GetBlackRookMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookMiddle() => EvaluateBlackRookOpening();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookEnd()
        {
            int value = 0;
            var bits = _boards[Pieces.BlackRook];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackRookFullValue(coordinate);

                if ((_rookFiles[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn]))
                    .IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((_rookFiles[coordinate] & _boards[Pieces.BlackPawn]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                value += GetBlackRookPinsEnd(coordinate);

                //value += GetBlackRookMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
