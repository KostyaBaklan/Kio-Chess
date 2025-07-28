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
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhitePawnFullValue(coordinate);

            if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_whiteDoublePawns[coordinate] & _boards[Pieces.WhitePawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }


            if ((_whiteIsolatedPawns[coordinate] & _boards[Pieces.WhitePawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else
            {
                for (byte c = 0; c < _whiteBackwardPawns[coordinate].Count; c++)
                {
                    if ((_whiteBackwardPawns[coordinate][c].Key & _boards[Pieces.WhitePawn]).IsZero() &&
                        (_whiteBackwardPawns[coordinate][c].Value & _boards[Pieces.BlackPawn]).Any())
                    {
                        value -= _evaluationService.GetBackwardPawnValue();
                        break;
                    }
                }
            }
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhitePawnMiddle() => GetWhitePawnValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhitePawnEnd() => GetWhitePawnValue();

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
    private int EvaluateWhiteQueenOpening() => EvaluateWhiteQueen();

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
    private int EvaluateBlackPawnMiddle() => GetBlackPawnValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackPawnEnd() => GetBlackPawnValue();

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
    public int GetKingSafetyValue()
    {
        var _phase = _moveHistory.GetPhase();
        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        return 0; //WhiteMiddleKingSafety(_boards[5].BitScanForward()) - BlackMiddleKingSafety(_boards[11].BitScanForward());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPawnValue()
    {
        var _phase = _moveHistory.GetPhase();
        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        return GetWhitePawnValue() - GetBlackPawnValue();
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
}
