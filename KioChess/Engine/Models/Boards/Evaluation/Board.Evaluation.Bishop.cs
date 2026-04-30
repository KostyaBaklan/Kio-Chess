using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        /// <summary>
        /// PHASE 1.3: Opening-specific bishop evaluation (skip bad bishop and outposts)
        /// Bad bishop evaluation is only critical in endgame, wastes ~15-20% in opening
        /// Outposts are premature in opening, wastes ~10-15%
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteBishopValueOpening()
        {
            var bits = _boards[Pieces.WhiteBishop];
            int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteBishopFullValue(coordinate);
                value += GetWhiteBishopPinsOpening(coordinate);
                value += GetEvaluationWhiteBishopMobility(coordinate);

                // Check if bishop is trapped with graduated penalty (0-2 moves)
                int mobility = CountTotalWhiteBishopMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedBishopPenalty(mobility);

                // SKIP: Bad bishop evaluation (not relevant in opening)
                // value += EvaluateWhiteBadBishop(coordinate, whitePawns);

                // Accumulate center attacks (optimization: avoid separate iteration)
                _whiteCenterAttacks += (coordinate.BishopAttacks(_occupied) & _centerSquares).Count();

                // Piece coordination: Check if bishop is defended by knight
                if ((_whiteKnightPatterns[coordinate] & _boards[Pieces.WhiteKnight]).Any())
                {
                    value += _evaluationService.GetMinorDefenseBonus();

                    // Extra bonus if centralized bishop is defended
                    if (_centerSquares.IsSet(coordinate))
                        value += _evaluationService.GetCentralPieceDefenseBonus();
                }

                // SKIP: Outpost evaluation (premature in opening)
                // value += EvaluateWhiteBishopOutpost(coordinate);

                value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteBishop);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        /// <summary>
        /// PHASE 1.3: Middle game bishop evaluation (skip bad bishop, keep outposts)
        /// Bad bishop evaluation is only critical in endgame
        /// Outposts are important in middle game
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteBishopValueMiddle()
        {
            var bits = _boards[Pieces.WhiteBishop];
            int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteBishopFullValue(coordinate);
                value += GetWhiteBishopPinsOpening(coordinate);
                value += GetEvaluationWhiteBishopMobility(coordinate);

                // Check if bishop is trapped with graduated penalty (0-2 moves)
                int mobility = CountTotalWhiteBishopMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedBishopPenalty(mobility);

                // SKIP: Bad bishop evaluation (only critical in endgame)
                // value += EvaluateWhiteBadBishop(coordinate, whitePawns);

                // Accumulate center attacks (optimization: avoid separate iteration)
                _whiteCenterAttacks += (coordinate.BishopAttacks(_occupied) & _centerSquares).Count();

                // Piece coordination: Check if bishop is defended by knight
                if ((_whiteKnightPatterns[coordinate] & _boards[Pieces.WhiteKnight]).Any())
                {
                    value += _evaluationService.GetMinorDefenseBonus();

                    // Extra bonus if centralized bishop is defended
                    if (_centerSquares.IsSet(coordinate))
                        value += _evaluationService.GetCentralPieceDefenseBonus();
                }

                // Outpost evaluation (critical in middle game)
                value += EvaluateWhiteBishopOutpost(coordinate);

                value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteBishop);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteBishopEnd()
        {
            var bits = _boards[Pieces.WhiteBishop];
            int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
            BitBoard whitePawns = _boards[Pieces.WhitePawn];

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

                // Evaluate bad bishop (most critical in endgame)
                value += EvaluateWhiteBadBishop(coordinate, whitePawns);

                // Accumulate center attacks (optimization: avoid separate iteration)
                _whiteCenterAttacks += (coordinate.BishopAttacks(_occupied) & _centerSquares).Count();
                // _whiteExtendedCenterAttacks += (coordinate.BishopAttacks(_occupied) & _extendedCenterSquares).Count();

                // Piece coordination: Check if bishop is defended by knight
                if ((_whiteKnightPatterns[coordinate] & _boards[Pieces.WhiteKnight]).Any())
                    value += _evaluationService.GetMinorDefenseBonus();

                // Outpost evaluation (less important in endgame but still valuable)
                value += EvaluateWhiteBishopOutpost(coordinate);

                value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteBishop);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        /// <summary>
        /// PHASE 1.3: Black opening-specific bishop evaluation (skip bad bishop and outposts)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackBishopValueOpening()
        {
            var bits = _boards[Pieces.BlackBishop];
            int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackBishopFullValue(coordinate);
                value += GetBlackBishopPinsOpening(coordinate);
                value += GetEvaluationBlackBishopMobility(coordinate);

                int mobility = CountTotalBlackBishopMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedBishopPenalty(mobility);

                // SKIP: Bad bishop evaluation (not relevant in opening)
                _blackCenterAttacks += (coordinate.BishopAttacks(_occupied) & _centerSquares).Count();

                if ((_blackKnightPatterns[coordinate] & _boards[Pieces.BlackKnight]).Any())
                {
                    value += _evaluationService.GetMinorDefenseBonus();
                    if (_centerSquares.IsSet(coordinate))
                        value += _evaluationService.GetCentralPieceDefenseBonus();
                }

                // SKIP: Outpost evaluation (premature in opening)
                value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackBishop);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        /// <summary>
        /// PHASE 1.3: Black middle game bishop evaluation (skip bad bishop, keep outposts)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackBishopValueMiddle()
        {
            var bits = _boards[Pieces.BlackBishop];
            int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackBishopFullValue(coordinate);
                value += GetBlackBishopPinsOpening(coordinate);
                value += GetEvaluationBlackBishopMobility(coordinate);

                int mobility = CountTotalBlackBishopMobility(coordinate);
                if (mobility < _evaluationService.GetTrappedPieceThreshold())
                    value -= _evaluationService.GetTrappedBishopPenalty(mobility);

                // SKIP: Bad bishop evaluation (only critical in endgame)
                _blackCenterAttacks += (coordinate.BishopAttacks(_occupied) & _centerSquares).Count();

                if ((_blackKnightPatterns[coordinate] & _boards[Pieces.BlackKnight]).Any())
                {
                    value += _evaluationService.GetMinorDefenseBonus();
                    if (_centerSquares.IsSet(coordinate))
                        value += _evaluationService.GetCentralPieceDefenseBonus();
                }

                value += EvaluateBlackBishopOutpost(coordinate);
                value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackBishop);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackBishopEnd()
        {
            var bits = _boards[Pieces.BlackBishop];
            int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
            BitBoard blackPawns = _boards[Pieces.BlackPawn];
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

                // Evaluate bad bishop (most critical in endgame)
                value += EvaluateBlackBadBishop(coordinate, blackPawns);

                // Accumulate center attacks (optimization: avoid separate iteration)
                _blackCenterAttacks += (coordinate.BishopAttacks(_occupied) & _centerSquares).Count();
                //_blackExtendedCenterAttacks += (coordinate.BishopAttacks(_occupied) & _extendedCenterSquares).Count();

                // Piece coordination: Check if bishop is defended by knight
                if ((_blackKnightPatterns[coordinate] & _boards[Pieces.BlackKnight]).Any())
                    value += _evaluationService.GetMinorDefenseBonus();

                // Outpost evaluation (less important in endgame but still valuable)
                value += EvaluateBlackBishopOutpost(coordinate);
                value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackBishop);

                bits = bits.Remove(coordinate);
            }

            return value;
        }


        /// <summary>
        /// Evaluates bad bishop penalty based on own pawns blocking bishop's diagonals.
        /// A bishop is "bad" when many of its own pawns are fixed on the same color squares.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteBadBishop(byte bishopSquare, BitBoard whitePawns)
        {
            int penalty = 0;

            // Determine bishop's square color
            // Count white pawns on same color as bishop
            BitBoard pawnsOnBishopColor = whitePawns & (_isLightSquare[bishopSquare] ? _lightSquares : _darkSquares);
            int pawnCount = pawnsOnBishopColor.Count();

            // Only apply penalty if pawn count exceeds threshold
            if (pawnCount > _evaluationService.GetBadBishopThreshold())
            {
                // Base penalty: -penalty per pawn on bishop's color
                penalty -= pawnCount * _evaluationService.GetBadBishopPenalty();

                // Extra penalty if center pawns are fixed on bishop's color
                // Center light squares: E4 (36), D5 (35)
                // Center dark squares: D4 (27), E5 (36)
                var bits = pawnsOnBishopColor & _centerSquares;
                while (bits.Any())
                {
                    var pawnSquare = bits.BitScanForward();
                    // Pawn is fixed if square in front is occupied or attacked by enemy pawn
                    byte frontSquare = (byte)(pawnSquare + 8);
                    if (_occupied.IsSet(frontSquare) || _blackPawnAttacks.IsSet(frontSquare))
                    {
                        penalty -= _evaluationService.GetFixedCenterPawnPenalty();
                        break;
                    }
                    bits = bits.Remove(pawnSquare);
                }
            }

            return penalty;
        }

        /// <summary>
        /// Evaluates bad bishop penalty for black bishops.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackBadBishop(byte bishopSquare, BitBoard blackPawns)
        {
            int penalty = 0;

            // Determine bishop's square color
            // Count black pawns on same color as bishop
            BitBoard pawnsOnBishopColor = blackPawns & (_isLightSquare[bishopSquare] ? _lightSquares : _darkSquares);
            int pawnCount = pawnsOnBishopColor.Count();

            // Only apply penalty if pawn count exceeds threshold
            if (pawnCount > _evaluationService.GetBadBishopThreshold())
            {
                // Base penalty: -penalty per pawn on bishop's color
                penalty -= pawnCount * _evaluationService.GetBadBishopPenalty();

                // Extra penalty if center pawns are fixed on bishop's color
                var bits = pawnsOnBishopColor & _centerSquares;
                while (bits.Any())
                {
                    var pawnSquare = bits.BitScanForward();
                    // Pawn is fixed if square in front is occupied or attacked by enemy pawn
                    byte frontSquare = (byte)(pawnSquare - 8);
                    if (_occupied.IsSet(frontSquare) || _whitePawnAttacks.IsSet(frontSquare))
                    {
                        penalty -= _evaluationService.GetFixedCenterPawnPenalty();
                        break;
                    }
                    bits = bits.Remove(pawnSquare);
                }
            }

            return penalty;
        }

        /// <summary>
        /// Evaluates bishop outpost bonus (bishops benefit less from outposts than knights).
        /// Applies a reduction factor to knight outpost values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteBishopOutpost(byte square)
        {
            var rank = square / 8;

            // Only ranks 5-6 for bishops
            if (rank < 4 || rank > 5 || (_whiteOutpostSquares[square] & _boards[Pieces.BlackPawn]).Any())
                return 0;

            byte file = (byte)(square % 8);
            // File-indexed lookup - bishops have their own arrays (no factor multiplication!)
            int value = rank == 4
                ? _evaluationService.GetBishopOutpostRank5(file)
                : _evaluationService.GetBishopOutpostRank6(file);

            // Additional bonus if defended by friendly pawn
            if (_whitePawnAttacks.IsSet(square))
                value += _evaluationService.GetOutpostDefendedByPawnBonus();

            return value;
        }

        /// <summary>
        /// Evaluates black bishop outpost bonus.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackBishopOutpost(byte square)
        {
            var rank = square / 8;

            // Only ranks 3-4 for black bishops (indices 2, 3)
            if (rank < 2 || rank > 3 || (_blackOutpostSquares[square] & _boards[Pieces.WhitePawn]).Any())
                return 0;

            byte file = (byte)(square % 8);
            // File-indexed lookup
            int value = rank == 3
                ? _evaluationService.GetBishopOutpostRank5(file)
                : _evaluationService.GetBishopOutpostRank6(file);

            // Additional bonus if defended by friendly pawn
            if (_blackPawnAttacks.IsSet(square))
                value += _evaluationService.GetOutpostDefendedByPawnBonus();

            return value;
        }
    }
}
