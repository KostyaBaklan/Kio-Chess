using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Evaluate()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();
        _whiteKingZone = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];
        _blackKingZone = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];
        var phase = _moveHistory.GetPhase();

        _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);

        return phase == Phase.Middle ? EvaluateMiddle() : phase == Phase.End ? EvaluateEnd() : EvaluateOpening();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EvaluateOpposite()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();
        _whiteKingZone = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];
        _blackKingZone = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];
        var phase = _moveHistory.GetPhase();

        _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);

        return phase == Phase.Middle ? EvaluateMiddleOpposite() : phase == Phase.End ? EvaluateEndOpposite() : EvaluateOpeningOpposite();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateEndOpposite() => EvaluateBlackEnd() - EvaluateWhiteEnd();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateMiddleOpposite() => EvaluateBlackMiddle() - EvaluateWhiteMiddle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateOpeningOpposite() => EvaluateBlackOpening() - EvaluateWhiteOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateEnd() => EvaluateWhiteEnd() - EvaluateBlackEnd();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateMiddle() => EvaluateWhiteMiddle() - EvaluateBlackMiddle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateOpening() => EvaluateWhiteOpening() - EvaluateBlackOpening();

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
            value += EvaluateWhiteRookOpening();

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
                        (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < 16 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                        (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

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

            if ((_whiteFacing[coordinate] & (whites | blacks)).IsZero()
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
                        (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < 16 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                        (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

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
        byte whiteKingPos = _boards[Pieces.WhiteKing].BitScanForward();
        byte blackKingPos = _boards[Pieces.BlackKing].BitScanForward();

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

            if ((_whiteFacing[coordinate] & (whites | blacks)).IsZero()
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
                    value += _evaluationService.GetKingDistanceFactor(coordinate, whiteKingPos, blackKingPos);

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
                    if ((_whiteFacing[coordinate] & _occupied).IsZero() && !_whitePassedPawnSquare[coordinate].IsSet(blackKingPos))
                    {
                        value += _evaluationService.GetUnstoppablePassedPawnValue();
                    }

                    //// Outside passed pawn bonus: pawns on files A, B, G, H are more valuable in endgame
                    //// because they divert the enemy king, allowing friendly king to penetrate
                    //if (_outsideFiles.IsSet(coordinate))
                    //{
                    //    value += _evaluationService.GetOutsidePassedPawnValue();
                    //}
                }
            }


            if ((_whiteIsolatedPawns[coordinate] & whites).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else if (((_whiteBackwardSupportPawns[coordinate] & whites).IsZero() &&
                        (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < 16 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                        (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKnightOpening() => GetWhiteKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKnightMiddle() => GetWhiteKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKnightEnd() => GetWhiteKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteBishopOpening() => GetWhiteBishopValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteBishopMiddle() => GetWhiteBishopValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteBishopEnd()
    {
        var bits = _boards[Pieces.WhiteBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

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
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteRookOpening()
    {
        int i = -1;
        int value = 0;

        var king = _boards[Pieces.BlackKing].BitScanForward();
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

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                }

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

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(_occupied) & whiteRooks).Any()
                    && (_rookFiles[coordinate] & whiteRooks).Any())
                {
                    value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                }
            }
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

            //value += GetWhiteRookMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

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

            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteQueenOpening() => EvaluateWhiteQueen();

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
        var kingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        return _evaluationService.GetWhiteKingFullValue(kingPosition)
            + WhiteKingShieldOpeningValue(kingPosition)
            + WhiteKingZoneAttack();
        //- WhiteKingOpenValue(kingPosition);
        //- WhiteKingAttackValue(kingPosition);
        //+ WhiteDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingMiddle()
    {
        var kingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        return _evaluationService.GetWhiteKingFullValue(kingPosition)
            + WhiteKingShieldMiddleValue(kingPosition)
            + WhiteKingZoneAttack();
        //- WhiteKingOpenValue(kingPosition);
        //- WhiteKingAttackValue(kingPosition)
        //+ WhiteDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingEnd()
    {
        var kingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        return _evaluationService.GetWhiteKingFullValue(kingPosition)
            - KingPawnTrofism(kingPosition);
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
            value += EvaluateBlackRookOpening();

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
                        (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > 47 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                        (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

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

            if ((_blackFacing[coordinate] & (whites | blacks)).IsZero()
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
                        (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > 47 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                        (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

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
        byte blackKingPos = _boards[Pieces.BlackKing].BitScanForward();
        byte whiteKingPos = _boards[Pieces.WhiteKing].BitScanForward();

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

            if ((_blackFacing[coordinate] & (whites | blacks)).IsZero()
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
                    value += _evaluationService.GetKingDistanceFactor(coordinate, blackKingPos, whiteKingPos);

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
                    if ((_blackFacing[coordinate] & _occupied).IsZero() && !_blackPassedPawnSquare[coordinate].IsSet(whiteKingPos))
                    {
                        value += _evaluationService.GetUnstoppablePassedPawnValue();
                    }

                    //// Outside passed pawn bonus: pawns on files A, B, G, H are more valuable in endgame
                    //// because they divert the enemy king, allowing friendly king to penetrate
                    //if (_outsideFiles.IsSet(coordinate))
                    //{
                    //    value += _evaluationService.GetOutsidePassedPawnValue();
                    //}
                }
            }


            if ((_blackIsolatedPawns[coordinate] & blacks).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else if (((_blackBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                        (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > 47 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                        (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
            {
                value -= _evaluationService.GetBackwardPawnValue();
            }
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKnightOpening() => GetBlackKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKnightMiddle() => GetBlackKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKnightEnd() => GetBlackKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackBishopOpening() => GetBlackBishopValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackBishopMiddle() => GetBlackBishopValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackBishopEnd()
    {
        var bits = _boards[Pieces.BlackBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
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

                if ((_whiteKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                }

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

                if ((_whiteKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(_occupied) & blackRooks).Any()
                    && (_rookFiles[coordinate] & blackRooks).Any())
                {
                    value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                }
            }
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

            //value += GetBlackRookMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

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

            bits = bits.Remove(coordinate);
        }

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

            //value += GetBlackQueenMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackQueenOpening() => EvaluateBlackQueen();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackQueenMiddle() => EvaluateBlackQueen();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackQueenEnd() => EvaluateBlackQueen();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingOpening()
    {
        var kingPosition = _boards[Pieces.BlackKing].BitScanForward();
        return _evaluationService.GetBlackKingFullValue(kingPosition)
            + BlackKingShieldOpeningValue(kingPosition)
            + BlackKingZoneAttack();
        //- BlackKingOpenValue(kingPosition);
        //- BlackKingAttackValue(kingPosition)
        // BlackDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingMiddle()
    {
        var kingPosition = _boards[Pieces.BlackKing].BitScanForward();
        return _evaluationService.GetBlackKingFullValue(kingPosition)
            + BlackKingShieldMiddleValue(kingPosition)
            + BlackKingZoneAttack();
        //- BlackKingOpenValue(kingPosition);
        //- BlackKingAttackValue(kingPosition);
        //BlackDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingEnd()
    {
        var kingPosition = _boards[Pieces.BlackKing].BitScanForward();
        return _evaluationService.GetBlackKingFullValue(kingPosition)
            - KingPawnTrofism(kingPosition);
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
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopValue()
    {
        var bits = _boards[Pieces.WhiteBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

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

            bits = bits.Remove(coordinate);
        }

        return value;
    }
}