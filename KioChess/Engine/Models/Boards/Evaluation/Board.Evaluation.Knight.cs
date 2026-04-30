using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKnightMiddle() => GetBlackKnightValueMiddle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKnightEnd() => GetBlackKnightValueMiddle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKnightMiddle() => GetWhiteKnightValueMiddle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKnightEnd() => GetWhiteKnightValueMiddle(); // Knights same in middle/end

        /// <summary>
        /// PHASE 1.4: Opening-specific knight evaluation (skip outposts)
        /// Outposts are premature in opening, wastes ~10-15%
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKnightValueOpening()
        {
            int value = 0;

            var bits = _boards[Pieces.WhiteKnight];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();

                value += _evaluationService.GetWhiteKnightFullValue(coordinate);
                value += GetEvaluationWhiteKnightMobility(coordinate);

                // Check if knight is trapped with graduated penalty (0-2 moves)
                int mobility = CountTotalWhiteKnightMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedKnightPenalty(mobility);

                // Accumulate center attacks (optimization: avoid separate iteration)
                _whiteCenterAttacks += (_whiteKnightPatterns[coordinate] & _centerSquares).Count();

                // Piece coordination: Check if knight is defended by friendly pieces
                if ((_whiteKnightPatterns[coordinate] & _boards[Pieces.WhiteKnight]).Any()
                    || (coordinate.BishopAttacks(_occupied) & _boards[Pieces.WhiteBishop]).Any())
                {
                    value += _evaluationService.GetMinorDefenseBonus();

                    // Extra bonus if centralized knight is defended
                    if (_centerSquares.IsSet(coordinate))
                        value += _evaluationService.GetCentralPieceDefenseBonus();
                }

                // SKIP: Outpost evaluation (premature in opening)
                // value += EvaluateWhiteKnightOutpost(coordinate);

                value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteKnight);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        /// <summary>
        /// PHASE 1.4: Middle/End game knight evaluation (with outposts)
        /// Outposts are critical in middle game
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKnightValueMiddle()
        {
            int value = 0;

            var bits = _boards[Pieces.WhiteKnight];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();

                value += _evaluationService.GetWhiteKnightFullValue(coordinate);
                value += GetEvaluationWhiteKnightMobility(coordinate);

                // Check if knight is trapped with graduated penalty (0-2 moves)
                int mobility = CountTotalWhiteKnightMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedKnightPenalty(mobility);

                // Accumulate center attacks (optimization: avoid separate iteration)
                _whiteCenterAttacks += (_whiteKnightPatterns[coordinate] & _centerSquares).Count();

                // Piece coordination: Check if knight is defended by friendly pieces
                if ((_whiteKnightPatterns[coordinate] & _boards[Pieces.WhiteKnight]).Any()
                    || (coordinate.BishopAttacks(_occupied) & _boards[Pieces.WhiteBishop]).Any())
                {
                    value += _evaluationService.GetMinorDefenseBonus();

                    // Extra bonus if centralized knight is defended
                    if (_centerSquares.IsSet(coordinate))
                        value += _evaluationService.GetCentralPieceDefenseBonus();
                }

                // Outpost evaluation (knights excel on outposts)
                value += EvaluateWhiteKnightOutpost(coordinate);
                value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteKnight);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        /// <summary>
        /// PHASE 1.4: Black opening-specific knight evaluation (skip outposts)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKnightValueOpening()
        {
            int value = 0;
            var bits = _boards[Pieces.BlackKnight];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackKnightFullValue(coordinate);
                value += GetEvaluationBlackKnightMobility(coordinate);

                int mobility = CountTotalBlackKnightMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedKnightPenalty(mobility);

                _blackCenterAttacks += (_blackKnightPatterns[coordinate] & _centerSquares).Count();

                if ((_blackKnightPatterns[coordinate] & _boards[Pieces.BlackKnight]).Any()
                    || (coordinate.BishopAttacks(_occupied) & _boards[Pieces.BlackBishop]).Any())
                {
                    value += _evaluationService.GetMinorDefenseBonus();
                    if (_centerSquares.IsSet(coordinate))
                        value += _evaluationService.GetCentralPieceDefenseBonus();
                }

                // SKIP: Outpost evaluation (premature in opening)
                value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackKnight);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        /// <summary>
        /// PHASE 1.4: Black middle/end game knight evaluation (with outposts)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKnightValueMiddle()
        {
            int value = 0;
            var bits = _boards[Pieces.BlackKnight];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackKnightFullValue(coordinate);
                value += GetEvaluationBlackKnightMobility(coordinate);

                int mobility = CountTotalBlackKnightMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedKnightPenalty(mobility);

                _blackCenterAttacks += (_blackKnightPatterns[coordinate] & _centerSquares).Count();

                if ((_blackKnightPatterns[coordinate] & _boards[Pieces.BlackKnight]).Any()
                    || (coordinate.BishopAttacks(_occupied) & _boards[Pieces.BlackBishop]).Any())
                {
                    value += _evaluationService.GetMinorDefenseBonus();
                    if (_centerSquares.IsSet(coordinate))
                        value += _evaluationService.GetCentralPieceDefenseBonus();
                }

                value += EvaluateBlackKnightOutpost(coordinate);
                value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackKnight);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        /// <summary>
        /// Evaluates knight outpost bonus. An outpost is a square that:
        /// 1. Cannot be attacked by enemy pawns (no enemy pawn can reach it)
        /// 2. Is on an advanced rank (ranks 4-6 for white, ranks 3-5 for black)
        /// 3. Optionally defended by a friendly pawn (more valuable)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKnightOutpost(byte square)
        {
            var rank = square / 8;

            // Only evaluate outposts on ranks 5-6 (indices 4, 5) - deep penetration only
            if (rank < 4 || rank > 5 || (_whiteOutpostSquares[square] & _boards[Pieces.BlackPawn]).Any())
                return 0;

            byte file = (byte)(square % 8);

            // File-indexed lookup (no multiply-divide needed!)
            int value = rank == 4
                ? _evaluationService.GetKnightOutpostRank5(file)  // Rank 5
                : _evaluationService.GetKnightOutpostRank6(file); // Rank 6

            // Additional bonus if defended by friendly pawn (secure outpost)
            if (_whitePawnAttacks.IsSet(square))
                value += _evaluationService.GetOutpostDefendedByPawnBonus();

            return value;
        }

        /// <summary>
        /// Evaluates black knight outpost bonus.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKnightOutpost(byte square)
        {
            var rank = square / 8;

            // Black outposts on ranks 3-4 (indices 2, 3) - deep penetration only
            if (rank < 2 || rank > 3 || (_blackOutpostSquares[square] & _boards[Pieces.WhitePawn]).Any())
                return 0;

            byte file = (byte)(square % 8);
            // File-indexed lookup (rank values inverted for black)
            int value = rank == 3
                ? _evaluationService.GetKnightOutpostRank5(file)  // Rank 4 (black's rank 5)
                : _evaluationService.GetKnightOutpostRank6(file); // Rank 3 (black's rank 6)

            // Additional bonus if defended by friendly pawn
            if (_blackPawnAttacks.IsSet(square))
                value += _evaluationService.GetOutpostDefendedByPawnBonus();

            return value;
        }
    }
}
