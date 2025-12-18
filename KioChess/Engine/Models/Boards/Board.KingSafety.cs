using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingZoneAttack()
    {
        int valueOfAttacks = 0;
        BitBoard attackPattern;
        BitBoardList boards = stackalloc BitBoard[8];

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
        BitBoardList boards = stackalloc BitBoard[8];

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackKingShieldOpeningValue(byte kingPosition) => _moveHistory.CanDoBlackCastle() ? 0 : BlackKingShieldMiddleValue(kingPosition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackKingShieldMiddleValue(byte kingPosition)
    {
        var pawns = _boards[Pieces.BlackPawn];

        return (_blackPawnShield7[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield2Value() +
            (_blackPawnShield6[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield3Value() +
            (_blackPawnShield5[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield4Value() +
            (_blackPawnKingShield7[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield2Value() +
            (_blackPawnKingShield6[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield3Value() +
            (_blackPawnKingShield5[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield4Value();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingShieldOpeningValue(byte kingPosition) => _moveHistory.CanDoWhiteCastle() ? 0 : WhiteKingShieldMiddleValue(kingPosition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingShieldMiddleValue(byte kingPosition)
    {
        var pawns = _boards[Pieces.WhitePawn];

        return (_whitePawnShield2[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield2Value() +
            (_whitePawnShield3[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield3Value() +
            (_whitePawnShield4[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield4Value() +
            (_whitePawnKingShield2[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield2Value() +
            (_whitePawnKingShield3[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield3Value() +
            (_whitePawnKingShield4[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield4Value();
    }
}
