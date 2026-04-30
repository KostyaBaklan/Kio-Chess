using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    /// <summary>
    /// Checks if a square is attacked by white pieces.
    /// Uses existing attack patterns and magic bitboards for efficiency.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSquareAttackedByWhite(byte square)
    {
        // Pawn attacks
        if (_whitePawnAttacks.IsSet(square))
            return true;

        // Knight attacks  
        if ((_whiteKnightPatterns[square] & _boards[Pieces.WhiteKnight]).Any())
            return true;

        // Bishop/Queen diagonal attacks (use extension method)
        if ((square.BishopAttacks(_occupied) & (_boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteQueen])).Any())
            return true;

        // Rook/Queen straight attacks (use extension method)
        if ((square.RookAttacks(_occupied) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).Any())
            return true;

        // King attacks
        if (_whiteKingPatterns[_whiteKingPosition].IsSet(square))
            return true;

        return false;
    }

    /// <summary>
    /// Checks if a square is attacked by black pieces.
    /// Uses existing attack patterns and magic bitboards for efficiency.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSquareAttackedByBlack(byte square)
    {
        // Pawn attacks
        if (_blackPawnAttacks.IsSet(square))
            return true;

        // Knight attacks
        if ((_blackKnightPatterns[square] & _boards[Pieces.BlackKnight]).Any())
            return true;

        // Bishop/Queen diagonal attacks (use extension method)
        if ((square.BishopAttacks(_occupied) & (_boards[Pieces.BlackBishop] | _boards[Pieces.BlackQueen])).Any())
            return true;

        // Rook/Queen straight attacks (use extension method)
        if ((square.RookAttacks(_occupied) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).Any())
            return true;

        // King attacks
        if (_blackKingPatterns[_blackKingPosition].IsSet(square))
            return true;

        return false;
    }

    /// <summary>
    /// Checks if a square is defended by white pieces.
    /// Defensive check is same as attack check (a piece defends what it can attack).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSquareDefendedByWhite(byte square) => IsSquareAttackedByWhite(square);

    /// <summary>
    /// Checks if a square is defended by black pieces.
    /// Defensive check is same as attack check (a piece defends what it can attack).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSquareDefendedByBlack(byte square) => IsSquareAttackedByBlack(square);

    /// <summary>
    /// Evaluates if a white piece is hanging (attacked but not defended).
    /// Returns penalty if piece is undefended and under attack.
    /// Uses pre-calculated penalty from configuration (50% of piece value).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhitePieceHanging(byte square, byte pieceType)
    {
        var attack = GetBlackAttackToForCheck(square);
        if (attack == null)
        {
            return 0;
        }

        attack.Captured = pieceType;

        var seeValue = StaticExchangeWithPins(attack);

        if (seeValue > 0)
        {
            return Math.Max(-seeValue, _evaluationService.GetHangingPiecePenalty(pieceType));
        }

        return 0;
    }

    /// <summary>
    /// Evaluates if a black piece is hanging (attacked but not defended).
    /// Returns penalty if piece is undefended and under attack.
    /// Uses pre-calculated penalty from configuration (50% of piece value).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackPieceHanging(byte square, byte pieceType)
    {
        var attack = GetWhiteAttackToForCheck(square);
        if (attack == null)
        {
            return 0;
        }

        attack.Captured = pieceType;

        var seeValue = StaticExchangeWithPins(attack);

        if (seeValue > 0)
        {
            return Math.Max(-seeValue, _evaluationService.GetHangingPiecePenalty(pieceType));
        }

        return 0;
    }
}