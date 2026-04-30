using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingOpening()
    {
        int value = _evaluationService.GetWhiteKingFullValue(_whiteKingPosition)
            + WhiteKingShieldValue(_whiteKingPosition)
            + EvaluateWhiteFianchetto()
            + EvaluateWhiteCastleRights()
            - EvaluateWhiteOpenFilesNearKing();

        // White attacking black king = positive for white
        value += WhiteKingZoneAttack();

        // PHASE 2.1: Conditional escape square evaluation (only if white king under pressure)
        // BlackKingZoneAttack() measures black attacking white king (positive = good for black = bad for white)
        int whiteKingUnderAttack = BlackKingZoneAttack();
        value -= whiteKingUnderAttack;

        // Only check escape squares if white king zone is under significant attack
        if (whiteKingUnderAttack > _evaluationService.GetKingZoneAttackThreshold())
            value -= EvaluateWhiteKingEscapeSquares();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingMiddle()
    {
        int value = _evaluationService.GetWhiteKingFullValue(_whiteKingPosition)
            + WhiteKingShieldValue(_whiteKingPosition)
            + EvaluateWhiteFianchetto()
            + EvaluateWhiteCastleRights()
            - EvaluateWhiteOpenFilesNearKing();

        // PHASE 2.1: Conditional escape square evaluation (only if white king under pressure)
        // BlackKingZoneAttack() measures black attacking white king (positive = good for black = bad for white)
        int whiteKingUnderAttack = BlackKingZoneAttack();
        value -= whiteKingUnderAttack;

        // Only check escape squares if white king zone is under significant attack
        if (whiteKingUnderAttack > _evaluationService.GetKingZoneAttackThreshold())
            value -= EvaluateWhiteKingEscapeSquares();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingEnd()
    {
        return _evaluationService.GetWhiteKingFullValue(_whiteKingPosition)
            - KingPawnTrofism(_whiteKingPosition);
        //+ WhiteDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingOpening()
    {
        int value = _evaluationService.GetBlackKingFullValue(_blackKingPosition)
            + BlackKingShieldValue(_blackKingPosition)
            + EvaluateBlackFianchetto()
            + EvaluateBlackCastleRights()
            - EvaluateBlackOpenFilesNearKing();

        // PHASE 2.1: Conditional escape square evaluation (only if black king under pressure)
        // WhiteKingZoneAttack() measures white attacking black king (positive = good for white = bad for black)
        int blackKingUnderAttack = WhiteKingZoneAttack();
        value -= blackKingUnderAttack;

        // Only check escape squares if black king zone is under significant attack
        if (blackKingUnderAttack > _evaluationService.GetKingZoneAttackThreshold())
            value -= EvaluateBlackKingEscapeSquares();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingMiddle()
    {
        int value = _evaluationService.GetBlackKingFullValue(_blackKingPosition)
            + BlackKingShieldValue(_blackKingPosition)
            + EvaluateBlackFianchetto()
            + EvaluateBlackCastleRights()
            - EvaluateBlackOpenFilesNearKing();

        // PHASE 2.1: Conditional escape square evaluation (only if black king under pressure)
        // WhiteKingZoneAttack() measures white attacking black king (positive = good for white = bad for black)
        int blackKingUnderAttack = WhiteKingZoneAttack();
        value -= blackKingUnderAttack;

        // Only check escape squares if black king zone is under significant attack
        if (blackKingUnderAttack > _evaluationService.GetKingZoneAttackThreshold())
            value -= EvaluateBlackKingEscapeSquares();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingEnd()
    {
        return _evaluationService.GetBlackKingFullValue(_blackKingPosition)
            - KingPawnTrofism(_blackKingPosition);
        //+ BlackDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingZoneAttack()
    {
        int valueOfAttacks = 0;
        BitBoard attackPattern;
        BitBoardList boards = stackalloc BitBoard[10];

        var bits = _boards[Pieces.WhiteKnight];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = _whiteKnightPatterns[position] & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetKnightAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.WhiteBishop];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.BishopAttacks(_occupied) & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetBishopAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.WhiteRook];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.RookAttacks(_occupied) & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetRookAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.WhiteQueen];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.QueenAttacks(_occupied) & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetQueenAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        if (boards.Count < 1) return 0;

        attackPattern = _whitePawnAttacks & _blackKingZone;
        if (attackPattern.Any())
        {
            valueOfAttacks++;
            boards.Add(attackPattern);
            return boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
        }

        return boards.Count < 2
            ? 0
            : boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackKingZoneAttack()
    {
        int valueOfAttacks = 0;
        BitBoard attackPattern;
        BitBoardList boards = stackalloc BitBoard[10];

        var bits = _boards[Pieces.BlackKnight];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = _blackKnightPatterns[position] & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetKnightAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.BlackBishop];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.BishopAttacks(_occupied) & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetBishopAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.BlackRook];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.RookAttacks(_occupied) & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetRookAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.BlackQueen];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.QueenAttacks(_occupied) & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetQueenAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        if (boards.Count < 1) return 0;

        attackPattern = _blackPawnAttacks & _whiteKingZone;
        if (attackPattern.Any())
        {
            valueOfAttacks++;
            boards.Add(attackPattern);
            return boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
        }

        return boards.Count < 2
            ? 0
            : boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetDistance(byte from, BitBoard bits)
    {
        int value = 0;
        var distance = _evaluationService.Distance(from);
        while (bits.Any())
        {
            byte position = bits.BitScanForward();
            value += distance[position];
            bits = bits.Remove(position);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int KingPawnTrofism(byte kingPosition) => _trofismCoefficient * GetDistance(kingPosition, _boards[0] | _boards[6]);

    /// <summary>
    /// Evaluates black king's pawn shield at current position.
    /// Always evaluates regardless of castle rights - king safety matters whether castled or not.
    /// Evaluates pawns on ranks 5, 6, 7 relative to king position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackKingShieldValue(byte kingPosition)
    {
        var pawns = _boards[Pieces.BlackPawn];

        return (_blackPawnShield7[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield2Value() +
            (_blackPawnShield6[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield3Value() +
            (_blackPawnShield5[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield4Value() +
            (_blackPawnKingShield7[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield2Value() +
            (_blackPawnKingShield6[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield3Value() +
            (_blackPawnKingShield5[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield4Value();
    }

    /// <summary>
    /// Evaluates white king's pawn shield at current position.
    /// Always evaluates regardless of castle rights - king safety matters whether castled or not.
    /// Evaluates pawns on ranks 2, 3, 4 relative to king position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingShieldValue(byte kingPosition)
    {
        var pawns = _boards[Pieces.WhitePawn];

        return (_whitePawnShield2[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield2Value() +
            (_whitePawnShield3[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield3Value() +
            (_whitePawnShield4[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield4Value() +
            (_whitePawnKingShield2[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield2Value() +
            (_whitePawnKingShield3[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield3Value() +
            (_whitePawnKingShield4[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield4Value();
    }

    /// <summary>
    /// Evaluates white's castling rights bonus.
    /// Castling ability is a significant strategic advantage in opening/middle game.
    /// Both sides available = maximum bonus
    /// One side available = partial bonus
    /// No sides available = no bonus
    /// Called as part of king evaluation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteCastleRights()
    {
        // Check if white can castle both sides
        if (_moveHistory.CanDoBothWhiteCastle())
        {
            // White can castle both sides (ideal!)
            return _evaluationService.GetCastleRightsBothBonus();
        }

        // Check if white can castle one side
        if (_moveHistory.CanDoWhiteCastle())
        {
            // White can castle one side (still good)
            return _evaluationService.GetCastleRightsOneBonus();
        }

        // No castling rights = 0 bonus
        return 0;
    }

    /// <summary>
    /// Evaluates black's castling rights bonus.
    /// Castling ability is a significant strategic advantage in opening/middle game.
    /// Both sides available = maximum bonus
    /// One side available = partial bonus
    /// No sides available = no bonus
    /// Called as part of king evaluation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackCastleRights()
    {
        // Check if black can castle both sides
        if (_moveHistory.CanDoBothBlackCastle())
        {
            // Black can castle both sides (ideal!)
            return _evaluationService.GetCastleRightsBothBonus();
        }

        // Check if black can castle one side
        if (_moveHistory.CanDoBlackCastle())
        {
            // Black can castle one side (still good)
            return _evaluationService.GetCastleRightsOneBonus();
        }

        // No castling rights = 0 bonus
        return 0;
    }

    /// <summary>
    /// Evaluates white fianchetto structure using pre-computed bitboards.
    /// Complete structure: Bishop on b2/g2 + pawn on b3/g3 = bonus
    /// Incomplete structure: Pawn advanced but no bishop = penalty
    /// Bishop traded: Extra penalty if king castled to that side (weakened structure)
    /// OPTIMIZED: O(1) bitboard checks for instant evaluation!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteFianchetto()
    {
        int value = 0;
        BitBoard whiteBishops = _boards[Pieces.WhiteBishop];
        BitBoard whitePawns = _boards[Pieces.WhitePawn];

        // Check queenside fianchetto (B-file)
        if (whitePawns.IsSet(Squares.B3))
        {
            // Pawn on B3 - check if bishop on B2
            if (whiteBishops.IsSet(Squares.B2))
            {
                value += _evaluationService.GetFianchettoBonus();  // +15-20cp

                // Extra bonus if king castled queenside (defensive structure)
                if (_whiteQueensideCastle.IsSet(_whiteKingPosition))
                    value += _evaluationService.GetFianchettoKingSafetyBonus();  // +10-12cp
            }
            else if (_whiteQueensideCastle.IsSet(_whiteKingPosition))
            {
                // Pawn advanced but no bishop = weakness!
                value -= _evaluationService.GetFianchettoWithoutBishopPenalty();  // -10-15cp

                // Extra penalty if (bishop was traded!)
                if ((_darkSquares & whiteBishops).IsZero())
                    value -= _evaluationService.GetFianchettoBishopTradedPenalty();  // -20-25cp 
            }
        }

        // Check kingside fianchetto (G-file)
        if (whitePawns.IsSet(Squares.G3))
        {
            // Pawn on G3 - check if bishop on G2
            if (whiteBishops.IsSet(Squares.G2))
            {
                value += _evaluationService.GetFianchettoBonus();  // +15-20cp

                // Extra bonus if king castled kingside (defensive structure)
                if (_whiteKingsideCastle.IsSet(_whiteKingPosition))
                    value += _evaluationService.GetFianchettoKingSafetyBonus();  // +10-12cp
            }
            else if (_whiteKingsideCastle.IsSet(_whiteKingPosition))
            {
                // Pawn advanced but no bishop = weakness!
                value -= _evaluationService.GetFianchettoWithoutBishopPenalty();  // -10-15cp

                // Extra penalty if king castled kingside (bishop was traded!)
                if ((_lightSquares & whiteBishops).IsZero())
                    value -= _evaluationService.GetFianchettoBishopTradedPenalty();  // -20-25cp
            }
        }

        return value;
    }

    /// <summary>
    /// Evaluates black fianchetto structure using pre-computed bitboards.
    /// OPTIMIZED: O(1) bitboard checks for instant evaluation!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackFianchetto()
    {
        int value = 0;
        BitBoard blackBishops = _boards[Pieces.BlackBishop];
        BitBoard blackPawns = _boards[Pieces.BlackPawn];

        // Check queenside fianchetto (B-file)
        if (blackPawns.IsSet(Squares.B6))
        {
            if (blackBishops.IsSet(Squares.B7))
            {
                value += _evaluationService.GetFianchettoBonus();

                if (_blackQueensideCastle.IsSet(_blackKingPosition))
                    value += _evaluationService.GetFianchettoKingSafetyBonus();
            }
            else if (_blackQueensideCastle.IsSet(_blackKingPosition))
            {
                value -= _evaluationService.GetFianchettoWithoutBishopPenalty();

                if ((_lightSquares & blackBishops).IsZero())
                    value -= _evaluationService.GetFianchettoBishopTradedPenalty();
            }
        }

        // Check kingside fianchetto (G-file)
        if (blackPawns.IsSet(Squares.G6))
        {
            if (blackBishops.IsSet(Squares.G7))
            {
                value += _evaluationService.GetFianchettoBonus();

                if (_blackKingsideCastle.IsSet(_blackKingPosition))
                    value += _evaluationService.GetFianchettoKingSafetyBonus();
            }
            else if (_blackKingsideCastle.IsSet(_blackKingPosition))
            {
                value -= _evaluationService.GetFianchettoWithoutBishopPenalty();

                if ((_darkSquares & blackBishops).IsZero())
                    value -= _evaluationService.GetFianchettoBishopTradedPenalty();
            }
        }


        return value;
    }

    /// <summary>
    /// Evaluates open and half-open files near white king.
    /// Open files adjacent to the king are highways for enemy rooks/queens to attack.
    /// Uses pre-computed _rookFiles bitboards for O(1) file lookup.
    /// Only evaluates in opening and middle game (endgame king should be active).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteOpenFilesNearKing()
    {
        int penalty = 0;

        // Get king file (0-7 for files a-h)
        byte kingFile = (byte)(_whiteKingPosition % 8);

        BitBoard whitePawns = _boards[Pieces.WhitePawn];
        BitBoard blackPawns = _boards[Pieces.BlackPawn];

        // Check file to the left of king (if not on a-file)
        if (kingFile > 0)
        {
            byte leftFileSquare = (byte)(_whiteKingPosition - 1);  // Any square on the left file
            BitBoard leftFileSquares = _rookFiles[leftFileSquare];

            BitBoard whitePawnsOnFile = leftFileSquares & whitePawns;
            BitBoard blackPawnsOnFile = leftFileSquares & blackPawns;

            if (whitePawnsOnFile.IsZero() && blackPawnsOnFile.IsZero())
            {
                // Open file (no pawns from either side)
                penalty += _evaluationService.GetOpenFileNearKingPenalty();
            }
            else if (whitePawnsOnFile.IsZero())
            {
                // Half-open file (only black pawns - dangerous for white king)
                penalty += _evaluationService.GetHalfOpenFileNearKingPenalty();
            }
        }

        // Check file to the right of king (if not on h-file)
        if (kingFile < 7)
        {
            byte rightFileSquare = (byte)(_whiteKingPosition + 1);  // Any square on the right file
            BitBoard rightFileSquares = _rookFiles[rightFileSquare];

            BitBoard whitePawnsOnFile = rightFileSquares & whitePawns;
            BitBoard blackPawnsOnFile = rightFileSquares & blackPawns;

            if (whitePawnsOnFile.IsZero() && blackPawnsOnFile.IsZero())
            {
                // Open file
                penalty += _evaluationService.GetOpenFileNearKingPenalty();
            }
            else if (whitePawnsOnFile.IsZero())
            {
                // Half-open file
                penalty += _evaluationService.GetHalfOpenFileNearKingPenalty();
            }
        }

        return penalty;
    }

    /// <summary>
    /// Evaluates open and half-open files near black king.
    /// Open files adjacent to the king are highways for enemy rooks/queens to attack.
    /// Uses pre-computed _rookFiles bitboards for O(1) file lookup.
    /// Only evaluates in opening and middle game (endgame king should be active).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackOpenFilesNearKing()
    {
        int penalty = 0;

        // Get king file (0-7 for files a-h)
        byte kingFile = (byte)(_blackKingPosition % 8);

        BitBoard whitePawns = _boards[Pieces.WhitePawn];
        BitBoard blackPawns = _boards[Pieces.BlackPawn];

        // Check file to the left of king (if not on a-file)
        if (kingFile > 0)
        {
            byte leftFileSquare = (byte)(_blackKingPosition - 1);  // Any square on the left file
            BitBoard leftFileSquares = _rookFiles[leftFileSquare];

            BitBoard whitePawnsOnFile = leftFileSquares & whitePawns;
            BitBoard blackPawnsOnFile = leftFileSquares & blackPawns;

            if (whitePawnsOnFile.IsZero() && blackPawnsOnFile.IsZero())
            {
                // Open file (no pawns from either side)
                penalty += _evaluationService.GetOpenFileNearKingPenalty();
            }
            else if (blackPawnsOnFile.IsZero())
            {
                // Half-open file (only white pawns - dangerous for black king)
                penalty += _evaluationService.GetHalfOpenFileNearKingPenalty();
            }
        }

        // Check file to the right of king (if not on h-file)
        if (kingFile < 7)
        {
            byte rightFileSquare = (byte)(_blackKingPosition + 1);  // Any square on the right file
            BitBoard rightFileSquares = _rookFiles[rightFileSquare];

            BitBoard whitePawnsOnFile = rightFileSquares & whitePawns;
            BitBoard blackPawnsOnFile = rightFileSquares & blackPawns;

            if (whitePawnsOnFile.IsZero() && blackPawnsOnFile.IsZero())
            {
                // Open file
                penalty += _evaluationService.GetOpenFileNearKingPenalty();
            }
            else if (blackPawnsOnFile.IsZero())
            {
                // Half-open file
                penalty += _evaluationService.GetHalfOpenFileNearKingPenalty();
            }
        }

        return penalty;
    }

    /// <summary>
    /// Evaluates white king escape squares - critical for detecting back-rank mate threats.
    /// Distinguishes between dangerous traps (attacked squares) vs safe structures (blocked by own pieces).
    /// Two-tier evaluation:
    /// 1) Safe escapes: Empty AND not attacked (preferred)
    /// 2) Protected squares: Occupied by own pieces (still provides safety)
    /// Only penalizes when BOTH safe escapes AND protected squares are scarce.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingEscapeSquares()
    {
        // Compute all squares attacked by black pieces
        BitBoard blackAttacks = _blackPawnAttacks;

        // Knights
        var bits = _boards[Pieces.BlackKnight];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            blackAttacks |= _blackKnightPatterns[position];
            bits = bits.Remove(position);
        }

        // Bishops
        bits = _boards[Pieces.BlackBishop];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            blackAttacks |= position.BishopAttacks(_occupied);
            bits = bits.Remove(position);
        }

        // Rooks
        bits = _boards[Pieces.BlackRook];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            blackAttacks |= position.RookAttacks(_occupied);
            bits = bits.Remove(position);
        }

        // Queens
        bits = _boards[Pieces.BlackQueen];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            blackAttacks |= position.QueenAttacks(_occupied);
            bits = bits.Remove(position);
        }

        // King
        blackAttacks |= _blackKingPatterns[_blackKingPosition];

        // Get squares around king using pre-computed king pattern
        BitBoard surroundingSquares = _whiteKingPatterns[_whiteKingPosition];

        if ((blackAttacks & surroundingSquares).IsZero())
            return 0;

        // Tier 1: Safe escape squares (empty AND not attacked) - PREFERRED
        BitBoard safeEscapes = surroundingSquares & _empty & ~blackAttacks;
        int safeEscapeCount = safeEscapes.Count();

        // Tier 2: Protected squares (occupied by own pieces) - STILL PROVIDES SAFETY
        BitBoard protectedSquares = surroundingSquares & _whites;
        int protectedCount = protectedSquares.Count();

        // Combined safety metric: safe escapes + half credit for protected squares
        int totalSafety = safeEscapeCount + (protectedCount / 2);

        // Only penalize if BOTH safe escapes AND protected squares are scarce
        // Example: King on g1, Rook f1, Bishop g2, Knight f3 = 3 protected squares = safe!
        if (safeEscapeCount == 0 && protectedCount < 2)
        {
            // Critical: No safe escapes AND few protected squares (true back-rank mate danger)
            return _evaluationService.GetNoEscapeSquaresPenalty();  // -50cp
        }

        if (totalSafety < 2)
        {
            // Dangerous: Very limited mobility
            return _evaluationService.GetOneEscapeSquarePenalty();  // -25cp
        }

        if (safeEscapeCount == 0 && protectedCount == 2)
        {
            // Marginal: No safe escapes but reasonably protected (e.g., castled position)
            return _evaluationService.GetProtectedEscapeSquaresPenalty();  // -5cp (pre-computed)
        }

        if (totalSafety == 2)
        {
            // Slight concern: Limited but not critical
            return _evaluationService.GetTwoEscapeSquaresPenalty();  // -10cp
        }

        return 0;  // King has adequate escape options or protection
    }

    /// <summary>
    /// Evaluates black king escape squares - critical for detecting back-rank mate threats.
    /// Distinguishes between dangerous traps (attacked squares) vs safe structures (blocked by own pieces).
    /// Two-tier evaluation:
    /// 1) Safe escapes: Empty AND not attacked (preferred)
    /// 2) Protected squares: Occupied by own pieces (still provides safety)
    /// Only penalizes when BOTH safe escapes AND protected squares are scarce.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingEscapeSquares()
    {
        // Compute all squares attacked by white pieces
        BitBoard whiteAttacks = _whitePawnAttacks;

        // Knights
        var bits = _boards[Pieces.WhiteKnight];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            whiteAttacks |= _whiteKnightPatterns[position];
            bits = bits.Remove(position);
        }

        // Bishops
        bits = _boards[Pieces.WhiteBishop];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            whiteAttacks |= position.BishopAttacks(_occupied);
            bits = bits.Remove(position);
        }

        // Rooks
        bits = _boards[Pieces.WhiteRook];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            whiteAttacks |= position.RookAttacks(_occupied);
            bits = bits.Remove(position);
        }

        // Queens
        bits = _boards[Pieces.WhiteQueen];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            whiteAttacks |= position.QueenAttacks(_occupied);
            bits = bits.Remove(position);
        }

        // King
        whiteAttacks |= _whiteKingPatterns[_whiteKingPosition];

        // Get squares around king using pre-computed king pattern
        BitBoard surroundingSquares = _blackKingPatterns[_blackKingPosition];

        if ((whiteAttacks & surroundingSquares).IsZero())
            return 0;

        // Tier 1: Safe escape squares (empty AND not attacked) - PREFERRED
        BitBoard safeEscapes = surroundingSquares & _empty & ~whiteAttacks;
        int safeEscapeCount = safeEscapes.Count();

        // Tier 2: Protected squares (occupied by own pieces) - STILL PROVIDES SAFETY
        BitBoard protectedSquares = surroundingSquares & _blacks;
        int protectedCount = protectedSquares.Count();

        // Combined safety metric: safe escapes + half credit for protected squares
        int totalSafety = safeEscapeCount + (protectedCount / 2);

        // Only penalize if BOTH safe escapes AND protected squares are scarce
        // Example: King on g8, Rook f8, Bishop g7, Knight f6 = 3 protected squares = safe!
        if (safeEscapeCount == 0 && protectedCount < 2)
        {
            // Critical: No safe escapes AND few protected squares (true back-rank mate danger)
            return _evaluationService.GetNoEscapeSquaresPenalty();  // -50cp
        }

        if (totalSafety < 2)
        {
            // Dangerous: Very limited mobility
            return _evaluationService.GetOneEscapeSquarePenalty();  // -25cp
        }

        if (safeEscapeCount == 0 && protectedCount == 2)
        {
            // Marginal: No safe escapes but reasonably protected (e.g., castled position)
            return _evaluationService.GetProtectedEscapeSquaresPenalty();  // -5cp (pre-computed)
        }

        if (totalSafety == 2)
        {
            // Slight concern: Limited but not critical
            return _evaluationService.GetTwoEscapeSquaresPenalty();  // -10cp
        }

        return 0;  // King has adequate escape options or protection
    }
}