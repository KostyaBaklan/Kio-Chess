using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        /// <summary>
        /// PHASE 2.2: Opening-specific rook evaluation (simplified)
        /// Skip: double rooks, connected rooks, 7th rank features (rare/premature in opening)
        /// Keep: basic file evaluation, pins, trapped check, piece hanging
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookOpening()
        {
            int value = 0;
            var bits = _boards[Pieces.WhiteRook];
            BitBoard whitePawns = _boards[Pieces.WhitePawn];

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteRookFullValue(coordinate);

                // Basic file evaluation (open/half-open)
                if ((coordinate > Squares.H2 && (_whiteFacing[coordinate] & (whitePawns | _boards[Pieces.BlackPawn])).IsZero()) ||
                    (_rookFiles[coordinate] & (whitePawns | _boards[Pieces.BlackPawn])).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();

                    if ((_blackKingPatterns[_blackKingPosition] & _rookFiles[coordinate]).Any())
                        value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                }
                else if ((coordinate > Squares.H2 && (_whiteFacing[coordinate] & whitePawns).IsZero()) ||
                    (_rookFiles[coordinate] & whitePawns).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();

                    if ((_blackKingPatterns[_blackKingPosition] & _rookFiles[coordinate]).Any())
                        value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                }

                value += GetWhiteRookPinsOpening(coordinate);

                // Rook blocked by king (castling not done or king trapped)
                if (coordinate < Squares.A2 && (_whiteRookKingPattern[coordinate] & _boards[Pieces.WhiteKing]).Any() &&
                    (_whiteRookPawnPattern[coordinate] & whitePawns).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }

                // Check if rook is trapped (reduce false positives in opening)
                // In opening, rooks on starting squares (a1/h1 or a8/h8) with low mobility are normal
                // Only penalize if rook has moved from starting square or has 0-1 mobility
                if (coordinate != Squares.A1 && coordinate != Squares.H1)
                {
                    int mobility = CountTotalWhiteRookMobility(coordinate);
                    if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    {
                        value -= _evaluationService.GetTrappedRookPenalty(mobility);
                    }

                    // Accumulate center attacks
                    _whiteCenterAttacks += (coordinate.RookAttacks(_occupied) & _centerSquares).Count();
                }

                // SKIP: Rook on 7th rank (very rare in opening)
                // SKIP: Doubled rooks on files (rare in opening)
                // SKIP: Connected rooks on first rank (rare in opening)

                value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteRook);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        /// <summary>
        /// PHASE 2.2: Middle game rook evaluation (add coordination and 7th rank)
        /// Include: double rooks on files, connected rooks, 7th rank features
        /// Skip: rook activity (endgame only)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookMiddle()
        {
            int i = -1;
            int value = 0;

            var bits = _boards[Pieces.WhiteRook];
            BitBoard whiteRooks = bits;
            BitBoard whitePawns = _boards[Pieces.WhitePawn];

            while (bits.Any())
            {
                i++;
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteRookFullValue(coordinate);

                if ((coordinate > Squares.H2 && (_whiteFacing[coordinate] & (whitePawns | _boards[Pieces.BlackPawn])).IsZero()) ||
                    (_rookFiles[coordinate] & (whitePawns | _boards[Pieces.BlackPawn])).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();

                    if ((_blackKingPatterns[_blackKingPosition] & _rookFiles[coordinate]).Any())
                        value += _evaluationService.GetRookOnOpenFileNextToKingValue();

                    // Double rooks on same open file (important in middle game)
                    if (i > 0 && (coordinate.RookAttacks(_occupied) & whiteRooks).Any()
                        && (_rookFiles[coordinate] & whiteRooks).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnOpenFileValue();
                    }
                }
                else if ((coordinate > Squares.H2 && (_whiteFacing[coordinate] & whitePawns).IsZero()) ||
                    (_rookFiles[coordinate] & whitePawns).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();

                    if ((_blackKingPatterns[_blackKingPosition] & _rookFiles[coordinate]).Any())
                        value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();

                    // Double rooks on same half-open file
                    if (i > 0 && (coordinate.RookAttacks(_occupied) & whiteRooks).Any()
                        && (_rookFiles[coordinate] & whiteRooks).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                    }
                }

                // Connected rooks on first rank (coordination)
                if (i > 0 && coordinate < Squares.A2 && (coordinate.RookAttacks(_occupied) & whiteRooks).Any()
                        && (_rookRanks[coordinate] & whiteRooks).Any())
                {
                    value += _evaluationService.GetConnectedRooksOnFirstRankValue();
                }

                value += GetWhiteRookPinsOpening(coordinate);

                if (coordinate < Squares.A2 && (_whiteRookKingPattern[coordinate] & _boards[Pieces.WhiteKing]).Any() &&
                    (_whiteRookPawnPattern[coordinate] & whitePawns).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }

                int mobility = CountTotalWhiteRookMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedRookPenalty(mobility);

                _whiteCenterAttacks += (coordinate.RookAttacks(_occupied) & _centerSquares).Count();

                // Evaluate rook on 7th rank (important in middle game)
                value += EvaluateWhiteRookOn7thRank(coordinate);

                // SKIP: Rook activity (endgame only)

                value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteRook);

                bits = bits.Remove(coordinate);
            }

            // Check for doubled rooks on 7th rank (powerful in middle game)
            value += EvaluateWhiteDoubledRooksOn7th();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookEnd()
        {
            int value = 0;
            var bits = _boards[Pieces.WhiteRook];
            BitBoard whitePawns = _boards[Pieces.WhitePawn];
            BitBoard blackPawns = _boards[Pieces.BlackPawn];

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteRookFullValue(coordinate);

                BitBoard rookFile = _rookFiles[coordinate];

                if ((rookFile & (whitePawns | blackPawns)).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((rookFile & whitePawns).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                value += GetWhiteRookPinsEnd(coordinate);

                // Check if rook is trapped with graduated penalty (0-2 moves)
                int mobility = CountTotalWhiteRookMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedRookPenalty(mobility);

                // Accumulate center attacks (optimization: avoid separate iteration)
                _whiteCenterAttacks += (coordinate.RookAttacks(_occupied) & _centerSquares).Count();
                //_whiteExtendedCenterAttacks += (coordinate.RookAttacks(_occupied) & _extendedCenterSquares).Count();

                // Evaluate rook on 7th rank (critical in endgame!)
                value += EvaluateWhiteRookOn7thRank(coordinate);

                // Rook activity in endgame
                value += EvaluateWhiteRookActivity(coordinate);

                // Check if rook is hanging (attacked but not defended)
                value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteRook);

                bits = bits.Remove(coordinate);
            }

            // Check for doubled rooks on 7th rank (only once, not per rook)
            value += EvaluateWhiteDoubledRooksOn7th();

            return value;
        }

        /// <summary>
        /// PHASE 2.2: Black opening-specific rook evaluation (simplified)
        /// Skip: double rooks, connected rooks, 7th rank features (rare/premature in opening)
        /// Keep: basic file evaluation, pins, trapped check, piece hanging
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookOpening()
        {
            int value = 0;
            var bits = _boards[Pieces.BlackRook];
            BitBoard blackPawns = _boards[Pieces.BlackPawn];

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackRookFullValue(coordinate);

                // Basic file evaluation (open/half-open)
                if ((coordinate < Squares.A7 && (_blackFacing[coordinate] & (_boards[Pieces.WhitePawn] | blackPawns)).IsZero()) ||
                    (_rookFiles[coordinate] & (_boards[Pieces.WhitePawn] | blackPawns)).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();

                    if ((_whiteKingPatterns[_whiteKingPosition] & _rookFiles[coordinate]).Any())
                        value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                }
                else if ((coordinate < Squares.A7 && (_blackFacing[coordinate] & blackPawns).IsZero()) ||
                    (_rookFiles[coordinate] & blackPawns).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();

                    if ((_whiteKingPatterns[_whiteKingPosition] & _rookFiles[coordinate]).Any())
                        value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                }

                value += GetBlackRookPinsOpening(coordinate);

                // Rook blocked by king
                if (coordinate > Squares.H7 && (_blackRookKingPattern[coordinate] & _boards[Pieces.BlackKing]).Any() &&
                    (_blackRookPawnPattern[coordinate] & blackPawns).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }

                // Check if rook is trapped (reduce false positives in opening)
                // In opening, rooks on starting squares (a8/h8) with low mobility are normal
                // Only penalize if rook has moved from starting square or has 0-1 mobility
                if (coordinate != Squares.A8 && coordinate != Squares.H8)
                {
                    int mobility = CountTotalBlackRookMobility(coordinate);
                    if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    {
                        value -= _evaluationService.GetTrappedRookPenalty(mobility);
                    }

                    // Accumulate center attacks
                    _blackCenterAttacks += (coordinate.RookAttacks(_occupied) & _centerSquares).Count();
                }

                // SKIP: Rook on 7th rank (very rare in opening)
                // SKIP: Doubled rooks on files (rare in opening)
                // SKIP: Connected rooks on first rank (rare in opening)

                value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackRook);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        /// <summary>
        /// PHASE 2.2: Black middle game rook evaluation (add coordination and 7th rank)
        /// Include: double rooks on files, connected rooks, 7th rank features
        /// Skip: rook activity (endgame only)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookMiddle()
        {
            int value = 0;
            int i = -1;
            var bits = _boards[Pieces.BlackRook];
            BitBoard blackPawns = _boards[Pieces.BlackPawn];
            BitBoard blackRooks = bits;

            while (bits.Any())
            {
                i++;
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackRookFullValue(coordinate);

                if ((coordinate < Squares.A7 && (_blackFacing[coordinate] & (_boards[Pieces.WhitePawn] | blackPawns)).IsZero()) ||
                    (_rookFiles[coordinate] & (_boards[Pieces.WhitePawn] | blackPawns)).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();

                    if ((_whiteKingPatterns[_whiteKingPosition] & _rookFiles[coordinate]).Any())
                        value += _evaluationService.GetRookOnOpenFileNextToKingValue();

                    // Double rooks on same open file (important in middle game)
                    if (i > 0 && (coordinate.RookAttacks(_occupied) & blackRooks).Any()
                        && (_rookFiles[coordinate] & blackRooks).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnOpenFileValue();
                    }
                }
                else if ((coordinate < Squares.A7 && (_blackFacing[coordinate] & blackPawns).IsZero()) ||
                    (_rookFiles[coordinate] & blackPawns).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();

                    if ((_whiteKingPatterns[_whiteKingPosition] & _rookFiles[coordinate]).Any())
                        value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();

                    // Double rooks on same half-open file
                    if (i > 0 && (coordinate.RookAttacks(_occupied) & blackRooks).Any()
                        && (_rookFiles[coordinate] & blackRooks).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                    }
                }

                // Connected rooks on eighth rank (coordination)
                if (i > 0 && coordinate > Squares.H7 && (coordinate.RookAttacks(_occupied) & blackRooks).Any()
                        && (_rookRanks[coordinate] & blackRooks).Any())
                {
                    value += _evaluationService.GetConnectedRooksOnFirstRankValue();
                }

                value += GetBlackRookPinsOpening(coordinate);

                if (coordinate > Squares.H7 && (_blackRookKingPattern[coordinate] & _boards[Pieces.BlackKing]).Any() &&
                    (_blackRookPawnPattern[coordinate] & blackPawns).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }

                int mobility = CountTotalBlackRookMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedRookPenalty(mobility);

                _blackCenterAttacks += (coordinate.RookAttacks(_occupied) & _centerSquares).Count();

                // Evaluate rook on 7th rank (important in middle game)
                value += EvaluateBlackRookOn7thRank(coordinate);

                // SKIP: Rook activity (endgame only)

                value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackRook);

                bits = bits.Remove(coordinate);
            }

            // Check for doubled rooks on 7th rank (powerful in middle game)
            value += EvaluateBlackDoubledRooksOn7th();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookEnd()
        {
            int value = 0;
            var bits = _boards[Pieces.BlackRook];
            BitBoard whitePawns = _boards[Pieces.WhitePawn];
            BitBoard blackPawns = _boards[Pieces.BlackPawn];

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackRookFullValue(coordinate);

                BitBoard rookFile = _rookFiles[coordinate];

                if ((rookFile & (whitePawns | blackPawns)).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((rookFile & blackPawns).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                value += GetBlackRookPinsEnd(coordinate);

                // Check if rook is trapped with graduated penalty (0-2 moves)
                int mobility = CountTotalBlackRookMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedRookPenalty(mobility);

                // Accumulate center attacks (optimization: avoid separate iteration)
                _blackCenterAttacks += (coordinate.RookAttacks(_occupied) & _centerSquares).Count();
                //_blackExtendedCenterAttacks += (coordinate.RookAttacks(_occupied) & _extendedCenterSquares).Count();

                // Evaluate rook on 7th rank (critical in endgame!)
                value += EvaluateBlackRookOn7thRank(coordinate);

                // Rook activity in endgame
                value += EvaluateBlackRookActivity(coordinate);

                // Check if rook is hanging (attacked but not defended)
                value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackRook);

                bits = bits.Remove(coordinate);
            }

            // Check for doubled rooks on 7th rank (only once, not per rook)
            value += EvaluateBlackDoubledRooksOn7th();

            return value;
        }

        /// <summary>
        /// Evaluates white rook on 7th rank bonus.
        /// Rook on 7th rank is powerful: attacks enemy pawns, restricts king, controls key squares.
        /// Additional bonus if enemy king is trapped on 8th rank.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookOn7thRank(byte coordinate)
        {
            // Fast bitboard check: is rook on 7th rank?
            if (!_white7thRank.IsSet(coordinate))
                return 0;

            int value = _evaluationService.GetRookOn7thRankBonus();

            // Additional bonus if enemy king trapped on 8th rank (comparison faster than division)
            if (_blackKingPosition > Squares.H7)  // King on rank 8 (squares 56-63)
                value += _evaluationService.GetRookOn7thWithKingOn8thBonus();

            return value;
        }

        /// <summary>
        /// Evaluates black rook on 7th rank bonus (2nd rank from white's perspective).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookOn7thRank(byte coordinate)
        {
            // Fast bitboard check: is rook on black's 7th rank (white's 2nd rank)?
            if (!_black7thRank.IsSet(coordinate))
                return 0;

            int value = _evaluationService.GetRookOn7thRankBonus();

            // Additional bonus if enemy king trapped on 1st rank (squares 0-7)
            if (_whiteKingPosition < Squares.A2)  // King on rank 1
                value += _evaluationService.GetRookOn7thWithKingOn8thBonus();

            return value;
        }

        /// <summary>
        /// Checks if white has doubled rooks on 7th rank (devastating advantage).
        /// Doubled rooks must protect each other for full bonus.
        /// Also checks for queen + rook synergy on 7th rank.
        /// Only checks once for efficiency (not per rook).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteDoubledRooksOn7th()
        {
            // Get all white rooks on 7th rank
            BitBoard rooksOn7th = _boards[Pieces.WhiteRook] & _white7thRank;

            int value = 0;

            // Doubled rooks: check if they protect each other (same file or connected)
            if (rooksOn7th.Count() > 1 && (rooksOn7th.BitScanForward().RookAttacks(_occupied) & rooksOn7th).Any())
            {
                value += _evaluationService.GetDoubledRooksOn7thBonus();
            }

            // Queen + Rook synergy on 7th rank
            //if (rooksOn7th.Any() && (_boards[Pieces.WhiteQueen] & _white7thRank).Any())
            //{
            //    value += _evaluationService.GetQueenRookOn7thBonus();
            //}

            return value;
        }

        /// <summary>
        /// Checks if black has doubled rooks on 7th rank (2nd rank for black).
        /// Doubled rooks must protect each other for full bonus.
        /// Also checks for queen + rook synergy on 7th rank.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackDoubledRooksOn7th()
        {
            // Get all black rooks on 7th rank (2nd rank)
            BitBoard rooksOn7th = _boards[Pieces.BlackRook] & _black7thRank;

            int value = 0;

            // Doubled rooks: check if they protect each other
            if (rooksOn7th.Count() > 1 && (rooksOn7th.BitScanForward().RookAttacks(_occupied) & rooksOn7th).Any())
            {
                value += _evaluationService.GetDoubledRooksOn7thBonus();
            }

            // Queen + Rook synergy on 7th rank
            //if (rooksOn7th.Any() && (_boards[Pieces.BlackQueen] & _black7thRank).Any())
            //{
            //    value += _evaluationService.GetQueenRookOn7thBonus();
            //}

            return value;
        }

        /// <summary>
        /// Evaluates rook activity in endgame. Active rooks dominate endgames, passive rooks are weak.
        /// Three factors: independence (distance from own king), cutting off enemy king, active vs passive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookActivity(byte rookSquare)
        {
            // 1. Independence: Distance from own king (O(1) pre-computed lookup)
            int value = _fileDistance[rookSquare][_whiteKingPosition] * _evaluationService.GetRookIndependenceFactor();

            // 2. Cutting off enemy king (rook controls key files/ranks)
            if (IsCuttingOffKing(rookSquare, _blackKingPosition))
            {
                value += _evaluationService.GetRookCuttingOffKingBonus();
            }

            // 3. Active vs Passive (use coordinate comparison for performance)
            if (rookSquare > Squares.H6) // 7th or 8th rank
            {
                value += _evaluationService.GetActiveRookBonus();
            }
            else if (rookSquare < Squares.A3 && IsDefendingWhitePawns(rookSquare))
            {
                value -= _evaluationService.GetPassiveRookPenalty();
            }

            return value;
        }

        /// <summary>
        /// Evaluates black rook activity in endgame.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookActivity(byte rookSquare)
        {
            // 1. Independence: Distance from own king (O(1) pre-computed lookup)
            int value = _fileDistance[rookSquare][_blackKingPosition] * _evaluationService.GetRookIndependenceFactor();

            // 2. Cutting off enemy king
            if (IsCuttingOffKing(rookSquare, _whiteKingPosition))
            {
                value += _evaluationService.GetRookCuttingOffKingBonus();
            }

            // 3. Active vs Passive (use coordinate comparison for performance)
            if (rookSquare < Squares.A3) // 1st or 2nd rank for black
            {
                value += _evaluationService.GetActiveRookBonus();
            }
            else if (rookSquare > Squares.H6 && IsDefendingBlackPawns(rookSquare))
            {
                value -= _evaluationService.GetPassiveRookPenalty();
            }

            return value;
        }

        /// <summary>
        /// Checks if rook is cutting off enemy king from critical areas.
        /// Rook cuts off king if: 3+ files apart (king can't cross rook's control).
        /// Uses pre-computed file distance table for O(1) lookup.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsCuttingOffKing(byte rookSquare, byte kingSquare)
        {
            return _fileDistance[rookSquare][kingSquare] > 2;
        }
    }
}
