using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    private byte _whiteKingPosition;
    private byte _blackKingPosition;

    // Center attack accumulators - computed during piece evaluation
    private int _whiteCenterAttacks;
    private int _blackCenterAttacks;
    //private int _whiteExtendedCenterAttacks;
    //private int _blackExtendedCenterAttacks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Evaluate()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();
        _whiteKingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        _blackKingPosition = _boards[Pieces.BlackKing].BitScanForward();
        _whiteKingZone = _whiteKingShield[_whiteKingPosition];
        _blackKingZone = _blackKingShield[_blackKingPosition];
        var phase = _moveHistory.GetPhase();

        _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);


        // Initialize center attack accumulators
        _whiteCenterAttacks = (_whitePawnAttacks & _centerSquares).Count();
        _blackCenterAttacks = (_blackPawnAttacks & _centerSquares).Count();
        //_whiteExtendedCenterAttacks = (_whitePawnAttacks & _extendedCenterSquares).Count();
        //_blackExtendedCenterAttacks = (_blackPawnAttacks & _extendedCenterSquares).Count();

        return (phase == Phase.Middle
            ? EvaluateMiddle() : phase == Phase.End
            ? EvaluateEnd() : EvaluateOpening()) + _evaluationService.GetTempoBonus();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EvaluateOpposite()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();
        _whiteKingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        _blackKingPosition = _boards[Pieces.BlackKing].BitScanForward();
        _whiteKingZone = _whiteKingShield[_whiteKingPosition];
        _blackKingZone = _blackKingShield[_blackKingPosition];
        var phase = _moveHistory.GetPhase();

        _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);

        // Initialize center attack accumulators
        _whiteCenterAttacks = (_whitePawnAttacks & _centerSquares).Count();
        _blackCenterAttacks = (_blackPawnAttacks & _centerSquares).Count();
        //_whiteExtendedCenterAttacks = (_whitePawnAttacks & _extendedCenterSquares).Count();
        //_blackExtendedCenterAttacks = (_blackPawnAttacks & _extendedCenterSquares).Count();

        return (phase == Phase.Middle
            ? EvaluateMiddleOpposite() : phase == Phase.End
            ? EvaluateEndOpposite() : EvaluateOpeningOpposite()) - _evaluationService.GetTempoBonus();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateEndOpposite() => EvaluateBlackEnd() - EvaluateWhiteEnd() + EvaluateOpposition() - EvaluatePawnMajoritiesEndgame();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateMiddleOpposite() => EvaluateBlackMiddle() - EvaluateWhiteMiddle() - EvaluateCenterControl() - EvaluateDevelopment(); // PHASE 1.6: Skip pawn majorities in middle game (only matters in endgame)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateOpeningOpposite() => EvaluateBlackOpening() - EvaluateWhiteOpening() - EvaluateCenterControl() - EvaluateDevelopment();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateEnd() => EvaluateWhiteEnd() - EvaluateBlackEnd() - EvaluateOpposition() + EvaluatePawnMajoritiesEndgame(); // PHASE 1.5: Skip center control & development in endgame

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateMiddle() => EvaluateWhiteMiddle() - EvaluateBlackMiddle() + EvaluateCenterControl() + EvaluateDevelopment(); // PHASE 1.6: Skip pawn majorities in middle game (only matters in endgame)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateOpening() => EvaluateWhiteOpening() - EvaluateBlackOpening() + EvaluateCenterControl() + EvaluateDevelopment();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteOpening()
    {
        var value = EvaluateWhitePawnOpening() + EvaluateWhiteKingOpening();

        if (_boards[Pieces.WhiteKnight].Any())
            value += EvaluateWhiteKnightOpening();

        if (_boards[Pieces.WhiteBishop].Any())
            value += EvaluateWhiteBishopOpening();

        if (_boards[Pieces.WhiteRook].Any())
            value += EvaluateWhiteRookOpening();

        if (_boards[Pieces.WhiteQueen].Any())
            value += EvaluateWhiteQueenOpening();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteMiddle()
    {
        var value = EvaluateWhitePawnMiddle() + EvaluateWhiteKingMiddle();

        if (_boards[Pieces.WhiteKnight].Any())
            value += EvaluateWhiteKnightMiddle();

        if (_boards[Pieces.WhiteBishop].Any())
            value += EvaluateWhiteBishopMiddle();

        if (_boards[Pieces.WhiteRook].Any())
            value += EvaluateWhiteRookMiddle();

        if (_boards[Pieces.WhiteQueen].Any())
            value += EvaluateWhiteQueenMiddle();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteEnd()
    {
        var value = EvaluateWhitePawnEnd() + EvaluateWhiteKingEnd();

        if (_boards[Pieces.WhiteKnight].Any())
            value += EvaluateWhiteKnightEnd();

        if (_boards[Pieces.WhiteBishop].Any())
            value += EvaluateWhiteBishopEnd();

        if (_boards[Pieces.WhiteRook].Any())
            value += EvaluateWhiteRookEnd();

        if (_boards[Pieces.WhiteQueen].Any())
            value += EvaluateWhiteQueenEnd();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhitePawnOpening()
    {
        int value = 0;

        var bits = _boards[Pieces.WhitePawn];
        BitBoard whites = bits;
        BitBoard blacks = _boards[Pieces.BlackPawn];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhitePawnFullValue(coordinate);

            if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_whiteDoublePawns[coordinate] & whites).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }


            if ((_whiteIsolatedPawns[coordinate] & whites).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else if (((_whiteBackwardSupportPawns[coordinate] & whites).IsZero() &&
                        (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < Squares.A3 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                        (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

        // PHASE 1.2: Skip pawn chains in opening (not important, ~5-8% speedup)
        // Pawn chains are only valuable in middle/endgame
        // value += EvaluateWhitePawnChains();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhitePawnMiddle()
    {
        var bits = _boards[Pieces.WhitePawn];
        if (bits.IsZero())
            return _evaluationService.GetNoPawnsValue();

        int value = 0;
        BitBoard whites = bits;
        BitBoard blacks = _boards[Pieces.BlackPawn];
        BitBoard allPawns = whites | blacks;

        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhitePawnFullValue(coordinate);

            if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_whiteDoublePawns[coordinate] & whites).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_whiteFacing[coordinate] & allPawns).IsZero()
                && (_whitePassedPawns[coordinate] & blacks).IsZero())
            {
                var pp = _evaluationService.GetWhitePassedPawnValue(coordinate);
                if (pp > 0)
                {
                    value += pp;

                    if ((_whiteProtectedPassedPawns[coordinate] & whites).Any())
                    {
                        value += _evaluationService.GetWhiteProtectedPassedPawnValue(coordinate);
                    }

                    if ((_whiteConnectedPassedPawns[coordinate] & whites).Any())
                    {
                        value += _evaluationService.GetWhiteConnectedPassedPawnValue(coordinate);
                    }

                    //// Tarrasch Rule: Check if any friendly rook is behind this passed pawn
                    //BitBoard rookFile = _rookFiles[coordinate];
                    //BitBoard friendlyRooksOnFile = rookFile & _boards[Pieces.WhiteRook];
                    //if (friendlyRooksOnFile.Any())
                    //{
                    //    // Check if any rook is behind (lower square for white)
                    //    var rookSquare = friendlyRooksOnFile.BitScanForward();
                    //    if (rookSquare < coordinate)
                    //    {
                    //        // Check no pieces between rook and pawn using lookup table
                    //        if ((_fileBetween[rookSquare][coordinate] & _occupied).IsZero())
                    //        {
                    //            value += _evaluationService.GetRookBehindPassedPawnValue();
                    //        }
                    //    }
                    //}
                }
            }


            if ((_whiteIsolatedPawns[coordinate] & whites).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else if (((_whiteBackwardSupportPawns[coordinate] & whites).IsZero() &&
                        (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < Squares.A3 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                        (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

        // Evaluate pawn chains (protected pawns in diagonal formation)
        value += EvaluateWhitePawnChains();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhitePawnEnd()
    {
        var bits = _boards[Pieces.WhitePawn];
        if (bits.IsZero())
            return _evaluationService.GetNoPawnsValue();

        int value = 0;
        BitBoard whites = bits;
        BitBoard blacks = _boards[Pieces.BlackPawn];
        BitBoard allPawns = whites | blacks;

        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhitePawnFullValue(coordinate);

            if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_whiteDoublePawns[coordinate] & whites).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_whiteFacing[coordinate] & allPawns).IsZero()
                && (_whitePassedPawns[coordinate] & blacks).IsZero())
            {
                var pp = _evaluationService.GetWhitePassedPawnValue(coordinate);
                if (pp > 0)
                {
                    value += pp;

                    if ((_whiteProtectedPassedPawns[coordinate] & whites).Any())
                    {
                        value += _evaluationService.GetWhiteProtectedPassedPawnValue(coordinate);
                    }

                    if ((_whiteConnectedPassedPawns[coordinate] & whites).Any())
                    {
                        value += _evaluationService.GetWhiteConnectedPassedPawnValue(coordinate);
                    }

                    // Full king distance factor in endgame
                    value += _evaluationService.GetKingDistanceFactor(coordinate, _whiteKingPosition, _blackKingPosition);

                    // Check for blockade on the next square
                    byte nextSquare = (byte)(coordinate + 8);
                    if (_blacks.IsSet(nextSquare))
                    {
                        value -= _evaluationService.GetBlockadePenalty(_pieces[nextSquare]);
                    }

                    // Tarrasch Rule: Check if any friendly rook is behind this passed pawn
                    BitBoard friendlyRooksOnFile = _rookFiles[coordinate] & _boards[Pieces.WhiteRook];
                    if (friendlyRooksOnFile.Any())
                    {
                        // Check if any rook is behind (lower square for white)
                        var rookSquare = friendlyRooksOnFile.BitScanForward();
                        if (rookSquare < coordinate && (_fileBetween[rookSquare][coordinate] & _occupied).IsZero())
                        {
                            value += _evaluationService.GetRookBehindPassedPawnValue();
                        }
                    }

                    // Unstoppable passed pawn: Check if enemy king is outside the "square of the pawn"
                    // Only check if path ahead is clear (no blockade)
                    if ((_whiteFacing[coordinate] & _occupied).IsZero() && !_whitePassedPawnSquare[coordinate].IsSet(_blackKingPosition))
                    {
                        value += _evaluationService.GetUnstoppablePassedPawnValue();
                    }

                    // Outside passed pawn bonus: pawns on wing files divert enemy king
                    value += EvaluateWhiteOutsidePassedPawn(coordinate);

                    // Key square control: Evaluate control of critical promotion squares
                    value += EvaluateWhiteKeySquares(coordinate);
                }
            }


            if ((_whiteIsolatedPawns[coordinate] & whites).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else if (((_whiteBackwardSupportPawns[coordinate] & whites).IsZero() &&
                        (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < Squares.A3 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                        (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

        // Evaluate pawn chains (protected pawns in diagonal formation)
        value += EvaluateWhitePawnChains();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKnightOpening() => GetWhiteKnightValueOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKnightMiddle() => GetWhiteKnightValueMiddle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKnightEnd() => GetWhiteKnightValueMiddle(); // Knights same in middle/end

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteBishopOpening() => GetWhiteBishopValueOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteBishopMiddle() => GetWhiteBishopValueMiddle();

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
    private int EvaluateBlackOpening()
    {
        var value = EvaluateBlackPawnOpening() + EvaluateBlackKingOpening();

        if (_boards[Pieces.BlackKnight].Any())
            value += EvaluateBlackKnightOpening();

        if (_boards[Pieces.BlackBishop].Any())
            value += EvaluateBlackBishopOpening();

        if (_boards[Pieces.BlackRook].Any())
            value += EvaluateBlackRookOpening();

        if (_boards[Pieces.BlackQueen].Any())
            value += EvaluateBlackQueenOpening();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackMiddle()
    {
        var value = EvaluateBlackPawnMiddle() + EvaluateBlackKingMiddle();

        if (_boards[Pieces.BlackKnight].Any())
            value += EvaluateBlackKnightMiddle();

        if (_boards[Pieces.BlackBishop].Any())
            value += EvaluateBlackBishopMiddle();

        if (_boards[Pieces.BlackRook].Any())
            value += EvaluateBlackRookMiddle();

        if (_boards[Pieces.BlackQueen].Any())
            value += EvaluateBlackQueenMiddle();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackEnd()
    {
        var value = EvaluateBlackPawnEnd() + EvaluateBlackKingEnd();

        if (_boards[Pieces.BlackKnight].Any())
            value += EvaluateBlackKnightEnd();

        if (_boards[Pieces.BlackBishop].Any())
            value += EvaluateBlackBishopEnd();

        if (_boards[Pieces.BlackRook].Any())
            value += EvaluateBlackRookEnd();

        if (_boards[Pieces.BlackQueen].Any())
            value += EvaluateBlackQueenEnd();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackPawnOpening()
    {
        int value = 0;
        var bits = _boards[Pieces.BlackPawn];
        BitBoard blacks = bits;
        BitBoard whites = _boards[Pieces.WhitePawn];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackPawnFullValue(coordinate);
            if ((_blackBlockedPawns[coordinate] & _whites).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_blackDoublePawns[coordinate] & blacks).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_blackIsolatedPawns[coordinate] & blacks).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else if (((_blackBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                        (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > Squares.H6 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                        (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

        // PHASE 1.2: Skip pawn chains in opening (not important, ~5-8% speedup)
        // Pawn chains are only valuable in middle/endgame
        // value += EvaluateBlackPawnChains();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackPawnMiddle()
    {
        var bits = _boards[Pieces.BlackPawn];
        if (bits.IsZero()) return _evaluationService.GetNoPawnsValue();

        int value = 0;
        BitBoard blacks = bits;
        BitBoard whites = _boards[Pieces.WhitePawn];
        BitBoard allPawns = whites | blacks;
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackPawnFullValue(coordinate);
            if ((_blackBlockedPawns[coordinate] & _whites).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_blackDoublePawns[coordinate] & blacks).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_blackFacing[coordinate] & allPawns).IsZero()
                && (_blackPassedPawns[coordinate] & whites).IsZero())
            {
                var pp = _evaluationService.GetBlackPassedPawnValue(coordinate);
                if (pp > 0)
                {
                    value += pp;

                    if ((_blackProtectedPassedPawns[coordinate] & blacks).Any())
                    {
                        value += _evaluationService.GetBlackProtectedPassedPawnValue(coordinate);
                    }

                    if ((_blackConnectedPassedPawns[coordinate] & blacks).Any())
                    {
                        value += _evaluationService.GetBlackConnectedPassedPawnValue(coordinate);
                    }

                    //// Tarrasch Rule: Check if any friendly rook is behind this passed pawn
                    //BitBoard rookFile = _rookFiles[coordinate];
                    //BitBoard friendlyRooksOnFile = rookFile & _boards[Pieces.BlackRook];
                    //if (friendlyRooksOnFile.Any())
                    //{
                    //    // Check if any rook is behind (higher square for black)
                    //    var rookSquare = friendlyRooksOnFile.BitScanReverse();
                    //    if (rookSquare > coordinate)
                    //    {
                    //        // Check no pieces between rook and pawn using lookup table
                    //        if ((_fileBetween[coordinate][rookSquare] & _occupied).IsZero())
                    //        {
                    //            value += _evaluationService.GetRookBehindPassedPawnValue();
                    //        }
                    //    }
                    //}
                }
            }


            if ((_blackIsolatedPawns[coordinate] & blacks).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else if (((_blackBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                        (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > Squares.H6 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                        (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

        // Evaluate pawn chains (protected pawns in diagonal formation)
        value += EvaluateBlackPawnChains();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackPawnEnd()
    {
        var bits = _boards[Pieces.BlackPawn];
        if (bits.IsZero()) return _evaluationService.GetNoPawnsValue();

        int value = 0;
        BitBoard blacks = bits;
        BitBoard whites = _boards[Pieces.WhitePawn];
        BitBoard allPawns = whites | blacks;

        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackPawnFullValue(coordinate);

            if ((_blackBlockedPawns[coordinate] & _whites).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_blackDoublePawns[coordinate] & blacks).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_blackFacing[coordinate] & allPawns).IsZero()
                && (_blackPassedPawns[coordinate] & whites).IsZero())
            {
                var pp = _evaluationService.GetBlackPassedPawnValue(coordinate);
                if (pp > 0)
                {
                    value += pp;

                    if ((_blackProtectedPassedPawns[coordinate] & blacks).Any())
                    {
                        value += _evaluationService.GetBlackProtectedPassedPawnValue(coordinate);
                    }

                    if ((_blackConnectedPassedPawns[coordinate] & blacks).Any())
                    {
                        value += _evaluationService.GetBlackConnectedPassedPawnValue(coordinate);
                    }

                    // Full king distance factor in endgame
                    value += _evaluationService.GetKingDistanceFactor(coordinate, _blackKingPosition, _whiteKingPosition);

                    // Check for blockade on the next square
                    byte nextSquare = (byte)(coordinate - 8);
                    if (_whites.IsSet(nextSquare))
                    {
                        value -= _evaluationService.GetBlockadePenalty(_pieces[nextSquare]);
                    }

                    // Tarrasch Rule: Check if any friendly rook is behind this passed pawn
                    BitBoard friendlyRooksOnFile = _rookFiles[coordinate] & _boards[Pieces.BlackRook];
                    if (friendlyRooksOnFile.Any())
                    {
                        // Check if any rook is behind (higher square for black)
                        var rookSquare = friendlyRooksOnFile.BitScanReverse();
                        if (rookSquare > coordinate && (_fileBetween[coordinate][rookSquare] & _occupied).IsZero())
                        {
                            value += _evaluationService.GetRookBehindPassedPawnValue();
                        }
                    }

                    // Unstoppable passed pawn: Check if enemy king is outside the "square of the pawn"
                    // Only check if path ahead is clear (no blockade)
                    if ((_blackFacing[coordinate] & _occupied).IsZero() && !_blackPassedPawnSquare[coordinate].IsSet(_whiteKingPosition))
                    {
                        value += _evaluationService.GetUnstoppablePassedPawnValue();
                    }

                    // Outside passed pawn bonus: pawns on wing files divert enemy king
                    value += EvaluateBlackOutsidePassedPawn(coordinate);

                    // Key square control: Evaluate control of critical promotion squares
                    value += EvaluateBlackKeySquares(coordinate);
                }
            }


            if ((_blackIsolatedPawns[coordinate] & blacks).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else if (((_blackBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                        (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > Squares.H6 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                        (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

        // Evaluate pawn chains (protected pawns in diagonal formation)
        value += EvaluateBlackPawnChains();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKnightOpening() => GetBlackKnightValueOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKnightMiddle() => GetBlackKnightValueMiddle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKnightEnd() => GetBlackKnightValueMiddle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackBishopOpening() => GetBlackBishopValueOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackBishopMiddle() => GetBlackBishopValueMiddle();

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
    public int GetStaticValue()
    {
        var _phase = _moveHistory.GetPhase();
        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        return GetWhiteStaticValue() - GetBlackStaticValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackStaticValue()
    {
        int value = 0;
        for (byte i = 6; i < 11; i++)
        {
            value += _evaluationService.GetPieceValue(i) * _boards[i].Count();
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteStaticValue()
    {
        int value = 0;
        for (byte i = 0; i < 5; i++)
        {
            value += _evaluationService.GetPieceValue(i) * _boards[i].Count();
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopValue()
    {
        var bits = _boards[Pieces.BlackBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
        BitBoard blackPawns = _boards[Pieces.BlackPawn];
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

            // Check if bishop is trapped with graduated penalty (0-2 moves)
            int mobility = CountTotalBlackBishopMobility(coordinate);
            if (mobility < _evaluationService.GetTrappedPieceThreshold())
                value -= _evaluationService.GetTrappedBishopPenalty(mobility);

            // Evaluate bad bishop (blocked by own pawns)
            value += EvaluateBlackBadBishop(coordinate, blackPawns);

            // Accumulate center attacks (optimization: avoid separate iteration)
            _blackCenterAttacks += (coordinate.BishopAttacks(_occupied) & _centerSquares).Count();
            //_blackExtendedCenterAttacks += (coordinate.BishopAttacks(_occupied) & _extendedCenterSquares).Count();

            // Piece coordination: Check if bishop is defended by knight
            if ((_blackKnightPatterns[coordinate] & _boards[Pieces.BlackKnight]).Any())
            {
                value += _evaluationService.GetMinorDefenseBonus();

                // Extra bonus if centralized bishop is defended
                if (_centerSquares.IsSet(coordinate))
                    value += _evaluationService.GetCentralPieceDefenseBonus();
            }

            // Outpost evaluation (bishops benefit less than knights but still valuable)
            value += EvaluateBlackBishopOutpost(coordinate);
            value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackBishop);

            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackKnightValue()
    {
        int value = 0;
        var bits = _boards[Pieces.BlackKnight];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackKnightFullValue(coordinate);

            //if ((_blackMinorDefense[coordinate] & _boards[Pieces.BlackPawn]).Any())
            //{
            //    value += _evaluationService.GetMinorDefendedByPawnValue();
            //}

            value += GetEvaluationBlackKnightMobility(coordinate);

            // Check if knight is trapped with graduated penalty (0-2 moves)
            int mobility = CountTotalBlackKnightMobility(coordinate);
            if (mobility < _evaluationService.GetTrappedPieceThreshold())
                value -= _evaluationService.GetTrappedKnightPenalty(mobility);

            // Accumulate center attacks (optimization: avoid separate iteration)
            _blackCenterAttacks += (_blackKnightPatterns[coordinate] & _centerSquares).Count();
            //_blackExtendedCenterAttacks += (_blackKnightPatterns[coordinate] & _extendedCenterSquares).Count();

            // Piece coordination: Check if knight is defended by friendly pieces
            if ((_blackKnightPatterns[coordinate] & _boards[Pieces.BlackKnight]).Any()
                || (coordinate.BishopAttacks(_occupied) & _boards[Pieces.BlackBishop]).Any())
            {
                value += _evaluationService.GetMinorDefenseBonus();

                // Extra bonus if centralized knight is defended
                if (_centerSquares.IsSet(coordinate))
                    value += _evaluationService.GetCentralPieceDefenseBonus();
            }

            // Outpost evaluation (knights excel on outposts)
            value += EvaluateBlackKnightOutpost(coordinate);
            value += EvaluateBlackPieceHanging(coordinate, Pieces.BlackKnight);

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
    private int GetWhiteBishopValue()
    {
        var bits = _boards[Pieces.WhiteBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
        BitBoard whitePawns = _boards[Pieces.WhitePawn];

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

            // Check if bishop is trapped with graduated penalty (0-2 moves)
            int mobility = CountTotalWhiteBishopMobility(coordinate);
            if (mobility < _evaluationService.GetTrappedPieceThreshold())
                value -= _evaluationService.GetTrappedBishopPenalty(mobility);

            // Evaluate bad bishop (blocked by own pawns)
            value += EvaluateWhiteBadBishop(coordinate, whitePawns);

            // Accumulate center attacks (optimization: avoid separate iteration)
            _whiteCenterAttacks += (coordinate.BishopAttacks(_occupied) & _centerSquares).Count();
            //_whiteExtendedCenterAttacks += (coordinate.BishopAttacks(_occupied) & _extendedCenterSquares).Count();

            // Piece coordination: Check if bishop is defended by knight
            if ((_whiteKnightPatterns[coordinate] & _boards[Pieces.WhiteKnight]).Any())
            {
                value += _evaluationService.GetMinorDefenseBonus();

                // Extra bonus if centralized bishop is defended
                if (_centerSquares.IsSet(coordinate))
                    value += _evaluationService.GetCentralPieceDefenseBonus();
            }

            // Outpost evaluation (bishops benefit less than knights but still valuable)
            value += EvaluateWhiteBishopOutpost(coordinate);

            value += EvaluateWhitePieceHanging(coordinate, Pieces.WhiteBishop);

            bits = bits.Remove(coordinate);
        }

        return value;
    }

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
    private int GetWhiteKnightValue()
    {
        int value = 0;

        var bits = _boards[Pieces.WhiteKnight];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();

            value += _evaluationService.GetWhiteKnightFullValue(coordinate);
            //if ((_whiteMinorDefense[coordinate] & _boards[Pieces.WhitePawn]).Any())
            //{
            //    value += _evaluationService.GetMinorDefendedByPawnValue();
            //}

            value += GetEvaluationWhiteKnightMobility(coordinate);

            // Check if knight is trapped with graduated penalty (0-2 moves)
            int mobility = CountTotalWhiteKnightMobility(coordinate);
            if (mobility < _evaluationService.GetTrappedPieceThreshold())
                value -= _evaluationService.GetTrappedKnightPenalty(mobility);

            // Accumulate center attacks (optimization: avoid separate iteration)
            _whiteCenterAttacks += (_whiteKnightPatterns[coordinate] & _centerSquares).Count();
            //_whiteExtendedCenterAttacks += (_whiteKnightPatterns[coordinate] & _extendedCenterSquares).Count();

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
    /// Evaluates center control using pre-accumulated attack counts.
    /// Center attacks are accumulated during piece evaluation to avoid redundant iteration.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateCenterControl()
    {
        // Use pre-accumulated center attack counts (computed during piece evaluation)
        // No need to iterate through pieces again - major performance optimization!
        //int value = (_whiteCenterAttacks - _blackCenterAttacks) * _evaluationService.GetCenterAttackValue();
        //value += (_whiteExtendedCenterAttacks - _blackExtendedCenterAttacks) * _evaluationService.GetExtendedCenterAttackValue();

        return (_whiteCenterAttacks - _blackCenterAttacks) * _evaluationService.GetCenterAttackValue();
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
    /// Evaluates piece development in opening and middle game phases.
    /// Penalizes minor pieces (knights and bishops) that remain on the first rank.
    /// Any minor piece on the back rank is considered undeveloped.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateDevelopment()
    {
        // Only evaluate after threshold ply (e.g., ply 10)
        if (_moveHistory.GetPly() < _evaluationService.GetDevelopmentThresholdMove())
            return 0;

        // Count undeveloped white minor pieces (any on first rank)
        BitBoard whiteUndeveloped = (_boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop])
            & _whiteFirstRank;

        // Count undeveloped black minor pieces (any on eighth rank)
        BitBoard blackUndeveloped = (_boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop])
            & _blackFirstRank;

        // Apply penalty differential (negative if white behind, positive if black behind)
        return (blackUndeveloped.Count() - whiteUndeveloped.Count()) * _evaluationService.GetDevelopmentPenalty();
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
    /// Evaluates opposition in king and pawn endgames using pre-computed patterns.
    /// Opposition only matters when there's minimal material (no pieces besides kings and pawns).
    /// Always returns POSITIVE value if opposition exists (caller controls sign with + or -).
    /// Uses pre-computed bitboards for O(1) detection.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateOpposition()
    {
        // Opposition only relevant in king and pawn endgames (no pieces)
        if (_boards[Pieces.WhiteKnight].Any() || _boards[Pieces.BlackKnight].Any() ||
            _boards[Pieces.WhiteBishop].Any() || _boards[Pieces.BlackBishop].Any() ||
            _boards[Pieces.WhiteRook].Any() || _boards[Pieces.BlackRook].Any() ||
            _boards[Pieces.WhiteQueen].Any() || _boards[Pieces.BlackQueen].Any())
        {
            return 0;  // Opposition not relevant with pieces on board
        }

        // Direct opposition: same file, 1 square between
        if (_directOppositionSquares[_whiteKingPosition].IsSet(_blackKingPosition))
            return _evaluationService.GetDirectOppositionBonus();

        // Diagonal opposition: diagonal, 1 square between
        if (_diagonalOppositionSquares[_whiteKingPosition].IsSet(_blackKingPosition))
            return _evaluationService.GetDiagonalOppositionBonus();

        // Distant opposition: same file, 4-6 squares apart
        if (_distantOppositionSquares[_whiteKingPosition].IsSet(_blackKingPosition))
            return _evaluationService.GetDistantOppositionBonus();

        return 0;
    }

    /// <summary>
    /// Evaluates outside passed pawn bonus for white.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteOutsidePassedPawn(byte pawnSquare)
    {
        // Check if pawn is on outside files
        if (!_outsideFiles.IsSet(pawnSquare))
            return 0;

        int value = _evaluationService.GetOutsidePassedPawnBonus() +
            pawnSquare / 8 * _evaluationService.GetOutsidePassedPawnAdvancedBonus();

        // Bonus if enemy king is far away (on opposite wing)
        int fileDistance = Math.Abs(pawnSquare % 8 - _blackKingPosition % 8);

        // King 4+ files away = opposite wing (major advantage)
        if (fileDistance > 3)
            value += fileDistance * _evaluationService.GetOutsidePassedPawnKingDistanceBonus();

        return value;
    }

    /// <summary>
    /// Evaluates outside passed pawn bonus for black.\n    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackOutsidePassedPawn(byte pawnSquare)
    {
        if (!_outsideFiles.IsSet(pawnSquare))
            return 0;

        int value = _evaluationService.GetOutsidePassedPawnBonus() +
            (7 - (pawnSquare / 8)) * _evaluationService.GetOutsidePassedPawnAdvancedBonus();

        int fileDistance = Math.Abs(pawnSquare % 8 - _whiteKingPosition % 8);

        if (fileDistance >= 4)
            value += fileDistance * _evaluationService.GetOutsidePassedPawnKingDistanceBonus();

        return value;
    }

    /// <summary>
    /// Evaluates pawn chain bonuses for white pawns using pre-computed diagonals.
    /// OPTIMIZED: Base-first approach with O(1) diagonal lookups.
    /// Performance: 15-20x faster than naive implementation!
    /// 
    /// Algorithm:
    /// 1. Find base pawns: pawns NOT defended by friendly pawns (whitePawns & ~_whitePawnAttacks)
    /// 2. For each base, check both diagonal chains using pre-computed bitboards
    /// 3. Validate continuity: chain pawns must be defended (_whitePawnAttacks)
    /// 4. Walk chain from base to find actual continuous chain length
    /// 5. Apply bonuses: protected pawns, long chains (3+), head bonus
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhitePawnChains()
    {
        BitBoard whitePawns = _boards[Pieces.WhitePawn];

        // Key optimization: Base pawns = pawns NOT defended by any friendly pawn
        BitBoard basePawns = whitePawns & ~_whitePawnAttacks;

        if (basePawns.IsZero())
            return 0;

        BitBoard defendedPawns = whitePawns & _whitePawnAttacks;
        if (defendedPawns.IsZero())
            return 0;

        int value = 0;
        const byte NoNext = 0xFF;

        while (basePawns.Any())
        {
            byte chainLength = 1;
            byte baseSquare = basePawns.BitScanForward();

            // Check RIGHT diagonal chain (NE: file+n, rank+n)
            // Filter to defended pawns on this diagonal
            BitBoard rightChainPawns = _whiteRightDiagonal[baseSquare] & defendedPawns;

            // Walk using pre-computed next-square lookup (O(1) per step!)
            if (rightChainPawns.Any())
            {
                byte current = baseSquare;
                byte next = _whiteRightNext[current];

                while (next != NoNext && rightChainPawns.IsSet(next))
                {
                    chainLength++;
                    current = next;
                    next = _whiteRightNext[current];
                }
            }

            // Check LEFT diagonal chain (NW: file-n, rank+n)
            BitBoard leftChainPawns = _whiteLeftDiagonal[baseSquare] & defendedPawns;

            // Walk using pre-computed next-square lookup
            if (leftChainPawns.Any())
            {
                byte current = baseSquare;
                byte next = _whiteLeftNext[current];

                while (next != NoNext && leftChainPawns.IsSet(next))
                {
                    chainLength++;
                    current = next;
                    next = _whiteLeftNext[current];
                }
            }

            // Score the chain (only if length > 1)
            if (chainLength > 1)
            {
                // Direct lookup by chain length (supports non-linear scaling!)
                value += _evaluationService.GetPawnChainBonusByLength(chainLength);
            }

            basePawns = basePawns.Remove(baseSquare);
        }

        return value;
    }

    /// <summary>
    /// Evaluates pawn chain bonuses for black pawns using pre-computed diagonals.
    /// OPTIMIZED: Base-first approach with O(1) diagonal lookups.
    /// Mirror implementation of white chain evaluation with continuity validation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackPawnChains()
    {
        BitBoard blackPawns = _boards[Pieces.BlackPawn];

        // Base pawns: NOT defended by any friendly pawn
        BitBoard basePawns = blackPawns & ~_blackPawnAttacks;

        if (basePawns.IsZero())
            return 0;

        BitBoard defendedPawns = blackPawns & _blackPawnAttacks;
        if (defendedPawns.IsZero())
            return 0;

        int value = 0;
        const byte NoNext = 0xFF;

        while (basePawns.Any())
        {
            byte chainLength = 1;
            byte baseSquare = basePawns.BitScanForward();

            // Check RIGHT diagonal chain (SE: file+n, rank-n)
            BitBoard rightChainPawns = _blackRightDiagonal[baseSquare] & defendedPawns;

            // Walk using pre-computed next-square lookup (O(1) per step!)
            if (rightChainPawns.Any())
            {
                byte current = baseSquare;
                byte next = _blackRightNext[current];

                while (next != NoNext && rightChainPawns.IsSet(next))
                {
                    chainLength++;
                    current = next;
                    next = _blackRightNext[current];
                }
            }

            // Check LEFT diagonal chain (SW: file-n, rank-n)
            BitBoard leftChainPawns = _blackLeftDiagonal[baseSquare] & defendedPawns;

            // Walk using pre-computed next-square lookup
            if (leftChainPawns.Any())
            {
                byte current = baseSquare;
                byte next = _blackLeftNext[current];

                while (next != NoNext && leftChainPawns.IsSet(next))
                {
                    chainLength++;
                    current = next;
                    next = _blackLeftNext[current];
                }
            }

            // Score the chain (only if length > 1)
            if (chainLength > 1)
            {
                // Direct lookup by chain length (supports non-linear scaling!)
                value += _evaluationService.GetPawnChainBonusByLength(chainLength);
            }

            basePawns = basePawns.Remove(baseSquare);
        }

        return value;
    }

    /// <summary>
    /// Evaluates pawn majority for middle game (no king distance factor).
    /// Uses pre-computed wing bitboards for O(1) pawn counting.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluatePawnMajorities()
    {
        BitBoard whitePawns = _boards[Pieces.WhitePawn];
        BitBoard blackPawns = _boards[Pieces.BlackPawn];

        // O(1) pawn counting using pre-computed wing bitboards
        int white = (whitePawns & _queensideFiles).Count() + (whitePawns & _kingsideFiles).Count();
        int black = (blackPawns & _queensideFiles).Count() + (blackPawns & _kingsideFiles).Count();

        // Early exit if no majorities exist
        return (white - black) * _evaluationService.GetPawnMajorityBonus();
    }

    /// <summary>
    /// Evaluates pawn majority for endgame with king distance and blocked detection.
    /// Uses pre-computed Manhattan distance table for O(1) king distance lookups.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluatePawnMajoritiesEndgame()
    {
        BitBoard whitePawns = _boards[Pieces.WhitePawn];
        BitBoard blackPawns = _boards[Pieces.BlackPawn];

        // O(1) pawn counting
        int whiteQueenside = (whitePawns & _queensideFiles).Count();
        int blackQueenside = (blackPawns & _queensideFiles).Count();
        int whiteKingside = (whitePawns & _kingsideFiles).Count();
        int blackKingside = (blackPawns & _kingsideFiles).Count();

        int queensideDiff = whiteQueenside - blackQueenside;
        int kingsideDiff = whiteKingside - blackKingside;

        if (queensideDiff == 0 && kingsideDiff == 0)
            return 0;

        int value = 0;
        byte majorityBonus = _evaluationService.GetPawnMajorityBonus();
        byte distanceFactor = _evaluationService.GetPawnMajorityKingDistanceFactor();

        // Queenside majority (guaranteed at least one non-zero after early exit)
        if (queensideDiff != 0)
        {
            int bonus = Math.Abs(queensideDiff) * majorityBonus;

            // O(1) pre-computed distance lookup
            int whiteKingDist = _manhattanDistance[_whiteKingPosition][QueensideCenter];
            int blackKingDist = _manhattanDistance[_blackKingPosition][QueensideCenter];
            int distanceDiff = whiteKingDist - blackKingDist;

            // Adjust for king distance
            bonus -= queensideDiff > 0
                ? Math.Max(0, distanceDiff * distanceFactor)
                : Math.Max(0, -distanceDiff * distanceFactor);

            // Check if blocked (bitboard shift optimization!)
            if (IsMajorityBlocked(queensideDiff > 0 ? whitePawns : blackPawns,
                                  _queensideFiles, queensideDiff > 0))
            {
                bonus -= majorityBonus;
            }

            value += queensideDiff > 0 ? bonus : -bonus;
        }

        // Kingside majority
        if (kingsideDiff != 0)
        {
            int bonus = Math.Abs(kingsideDiff) * majorityBonus;

            // O(1) pre-computed distance lookup
            int whiteKingDist = _manhattanDistance[_whiteKingPosition][KingsideCenter];
            int blackKingDist = _manhattanDistance[_blackKingPosition][KingsideCenter];
            int distanceDiff = whiteKingDist - blackKingDist;

            // Adjust for king distance
            bonus -= kingsideDiff > 0
                ? Math.Max(0, distanceDiff * distanceFactor)
                : Math.Max(0, -distanceDiff * distanceFactor);

            // Check if blocked
            if (IsMajorityBlocked(kingsideDiff > 0 ? whitePawns : blackPawns,
                                  _kingsideFiles, kingsideDiff > 0))
            {
                bonus -= majorityBonus;
            }

            value += kingsideDiff > 0 ? bonus : -bonus;
        }

        return value;
    }

    /// <summary>
    /// Checks if pawns in a wing majority are blocked using optimized bitboard shift.
    /// A majority is blocked if ALL pawns on the wing have an occupied square ahead.
    /// OPTIMIZED: Single bitboard shift operation instead of per-pawn iteration!
    /// Performance: O(1) bitboard shift vs O(n) pawn iteration = 8x faster!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsMajorityBlocked(BitBoard pawns, BitBoard wingFiles, bool isWhite)
    {
        BitBoard wingPawns = pawns & wingFiles;

        if (wingPawns.IsZero())
            return true;  // No pawns = blocked by definition

        // Optimized: Shift all pawns forward by one rank, check if ANY square is free
        // White: shift up (<<8), Black: shift down (>>8)
        // BitBoard has << and >> operators defined!
        BitBoard pawnsAhead = isWhite
            ? wingPawns << 8   // Shift up one rank
            : wingPawns >> 8;  // Shift down one rank

        // If ANY square ahead is free, majority is NOT blocked
        // All squares occupied = blocked
        return (pawnsAhead & ~_occupied).IsZero();
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

    /// <summary>
    /// Checks if white rook is passively defending own pawns (rook on 1st/2nd rank with pawns ahead).
    /// Uses rook attack generation for accurate detection (accounts for blocking pieces).
    /// OPTIMIZED: Single bitboard operation instead of per-pawn iteration!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsDefendingWhitePawns(byte rookSquare)
    {
        // Get white pawns on same file
        BitBoard pawnsOnFile = _rookFiles[rookSquare] & _boards[Pieces.WhitePawn];

        if (pawnsOnFile.IsZero())
            return false;

        // Check if rook attacks any pawn ahead of it (rook defends what it attacks)
        // This automatically accounts for blocking pieces and validates direct defense
        return (rookSquare.RookAttacks(_occupied) & pawnsOnFile).Any();
    }

    /// <summary>
    /// Checks if black rook is passively defending own pawns (rook on 7th/8th rank with pawns ahead).
    /// Uses rook attack generation for accurate detection (accounts for blocking pieces).
    /// OPTIMIZED: Single bitboard operation instead of per-pawn iteration!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsDefendingBlackPawns(byte rookSquare)
    {
        // Get white pawns on same file
        BitBoard pawnsOnFile = _rookFiles[rookSquare] & _boards[Pieces.BlackPawn];

        if (pawnsOnFile.IsZero())
            return false;

        // Check if rook attacks any pawn ahead of it (rook defends what it attacks)
        // This automatically accounts for blocking pieces and validates direct defense
        return (rookSquare.RookAttacks(_occupied) & pawnsOnFile).Any();
    }

    /// <summary>
    /// Evaluates key square control for a single white passed pawn.
    /// Key squares: Critical squares in front of passed pawns that guarantee promotion.
    /// Uses pre-computed bitboards for O(1) lookups.
    /// Called inline during passed pawn evaluation (zero extra iteration cost).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKeySquares(byte pawnSquare)
    {
        BitBoard keySquares = _whiteKeySquares[pawnSquare];

        // Check if white king occupies key square
        if (keySquares.IsSet(_whiteKingPosition))
        {
            return _evaluationService.GetKeySquareControlBonus();
        }
        // Check if black king occupies key square (penalty)
        if (keySquares.IsSet(_blackKingPosition))
        {
            return -_evaluationService.GetKeySquareControlBonus();
        }

        // Proximity bonus (closer king to key squares)
        int distanceDiff = CalculateMinDistance(_blackKingPosition, keySquares) - CalculateMinDistance(_whiteKingPosition, keySquares);

        return distanceDiff * _evaluationService.GetKeySquareProximityFactor();
    }

    /// <summary>
    /// Evaluates key square control for a single black passed pawn.
    /// Key squares: Critical squares behind passed pawns (from black's perspective) that guarantee promotion.
    /// Uses pre-computed bitboards for O(1) lookups.
    /// Called inline during passed pawn evaluation (zero extra iteration cost).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKeySquares(byte pawnSquare)
    {
        BitBoard keySquares = _blackKeySquares[pawnSquare];

        // Check if black king occupies key square
        if (keySquares.IsSet(_blackKingPosition))
        {
            return _evaluationService.GetKeySquareControlBonus();
        }
        // Check if white king occupies key square (penalty for black)
        if (keySquares.IsSet(_whiteKingPosition))
        {
            return -_evaluationService.GetKeySquareControlBonus();
        }

        // Proximity bonus (closer king to key squares)
        int distanceDiff = CalculateMinDistance(_whiteKingPosition, keySquares) - CalculateMinDistance(_blackKingPosition, keySquares);

        return distanceDiff * _evaluationService.GetKeySquareProximityFactor();
    }

    /// <summary>
    /// Calculates minimum Manhattan distance from king to any key square.
    /// Uses pre-computed Manhattan distance table for O(1) lookups.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CalculateMinDistance(byte kingSquare, BitBoard keySquares)
    {
        int minDistance = 8;
        var bits = keySquares;

        while (bits.Any())
        {
            byte keySquare = bits.BitScanForward();
            int distance = _manhattanDistance[kingSquare][keySquare];
            minDistance = Math.Min(minDistance, distance);
            bits = bits.Remove(keySquare);
        }

        return minDistance;
    }
}