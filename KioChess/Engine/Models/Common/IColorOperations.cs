using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Services;
using System.Runtime.CompilerServices;

namespace Engine.Models.Common;

/// <summary>
/// Defines color-specific operations for move generation and processing.
/// This interface enables zero-cost generic abstraction for White/Black move processing.
/// </summary>
public interface IColorOperations
{
    /// <summary>
    /// Generates all attacks (captures) for this color.
    /// </summary>
    void GenerateAttacks(MoveProvider moveProvider, Board board, AttackList attacks);

    /// <summary>
    /// Generates all moves (non-captures) for this color.
    /// </summary>
    void GenerateMoves(MoveProvider moveProvider, Board board, MoveList moves);

    /// <summary>
    /// Gets promotion squares for this color.
    /// </summary>
    BitBoard GetPromotionSquares(Board board);

    /// <summary>
    /// Checks if this color can promote.
    /// </summary>
    bool CanPromote(Board board);

    /// <summary>
    /// Gets promotion attacks for a given square.
    /// </summary>
    PromotionAttackList[] GetPromotionAttacks(MoveProvider moveProvider, byte square);

    /// <summary>
    /// Gets promotion moves for a given square.
    /// </summary>
    PromotionList GetPromotions(MoveProvider moveProvider, byte square);

    /// <summary>
    /// Checks if a move is legal for this color.
    /// </summary>
    bool IsMoveLegal(Board board, MoveBase move);

    /// <summary>
    /// Generates single attacks (for attack list generation) for this color.
    /// </summary>
    void GenerateSingleAttacks(MoveProvider moveProvider, Board board, AttackList attacks);

    /// <summary>
    /// Gets the starting piece index for this color (White: 0, Black: 6).
    /// </summary>
    byte PieceStartIndex { get; }

    /// <summary>
    /// Gets the ending piece index for this color (White: 6, Black: 12).
    /// </summary>
    byte PieceEndIndex { get; }

    /// <summary>
    /// Checks if any capture move exists for this color.
    /// </summary>
    bool AnyCapture(MoveProvider moveProvider, Board board);

    /// <summary>
    /// Checks if any non-capture move exists for this color.
    /// </summary>
    bool AnyMove(MoveProvider moveProvider, Board board);

    /// <summary>
    /// Checks if any promotion exists for this color.
    /// </summary>
    bool AnyPromotion(MoveProvider moveProvider, Board board);
}

/// <summary>
/// White color implementation.
/// </summary>
public readonly struct WhiteColor : IColorOperations
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateAttacks(MoveProvider moveProvider, Board board, AttackList attacks)
    {
        moveProvider.GetWhitePawnAttacks(board.GetWhitePawnSquares(), attacks);
        moveProvider.GetWhiteKnightAttacks(board.GetPieceBits(Pieces.WhiteKnight), attacks);
        moveProvider.GetWhiteBishopAttacks(board.GetPieceBits(Pieces.WhiteBishop), attacks);
        moveProvider.GetWhiteRookAttacks(board.GetPieceBits(Pieces.WhiteRook), attacks);
        moveProvider.GetWhiteQueenAttacks(board.GetPieceBits(Pieces.WhiteQueen), attacks);
        moveProvider.GetWhiteKingAttacks(board.GetPieceBits(Pieces.WhiteKing), attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateMoves(MoveProvider moveProvider, Board board, MoveList moves)
    {
        moveProvider.GetWhitePawnMoves(board.GetWhitePawnSquares(), moves);
        moveProvider.GetWhiteKnightMoves(board.GetPieceBits(Pieces.WhiteKnight), moves);
        moveProvider.GetWhiteBishopMoves(board.GetPieceBits(Pieces.WhiteBishop), moves);
        moveProvider.GetWhiteRookMoves(board.GetPieceBits(Pieces.WhiteRook), moves);
        moveProvider.GetWhiteQueenMoves(board.GetPieceBits(Pieces.WhiteQueen), moves);
        moveProvider.GetWhiteKingMoves(board.GetPieceBits(Pieces.WhiteKing), moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetPromotionSquares(Board board) => board.GetWhitePromotionSquares();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPromote(Board board) => board.CanWhitePromote();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionAttackList[] GetPromotionAttacks(MoveProvider moveProvider, byte square)
        => moveProvider.GetWhitePromotionAttacks(square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionList GetPromotions(MoveProvider moveProvider, byte square)
        => moveProvider.GetWhitePromotions(square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveLegal(Board board, MoveBase move) => board.IsWhiteMoveLigal(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateSingleAttacks(MoveProvider moveProvider, Board board, AttackList attacks)
    {
        BitBoard to = new();
        moveProvider.GetWhitePawnSingleAttacks(board.GetWhitePawnSquares(), attacks, ref to);
        moveProvider.GetWhiteKnightSingleAttacks(board.GetPieceBits(Pieces.WhiteKnight), attacks, ref to);
        moveProvider.GetWhiteBishopSingleAttacks(board.GetPieceBits(Pieces.WhiteBishop), attacks, ref to);
        moveProvider.GetWhiteRookSingleAttacks(board.GetPieceBits(Pieces.WhiteRook), attacks, ref to);
        moveProvider.GetWhiteQueenSingleAttacks(board.GetPieceBits(Pieces.WhiteQueen), attacks, ref to);
        moveProvider.GetWhiteKingSingleAttacks(board.GetPieceBits(Pieces.WhiteKing), attacks, ref to);
    }

    public byte PieceStartIndex => 0;
    public byte PieceEndIndex => 6;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyCapture(MoveProvider moveProvider, Board board)
    {
        return moveProvider.AnyWhitePawnAttacks(board.GetWhitePawnSquares()) ||
               moveProvider.AnyWhiteKnightAttacks(board.GetPieceBits(Pieces.WhiteKnight)) ||
               moveProvider.AnyWhiteBishopAttacks(board.GetPieceBits(Pieces.WhiteBishop)) ||
               moveProvider.AnyWhiteRookAttacks(board.GetPieceBits(Pieces.WhiteRook)) ||
               moveProvider.AnyWhiteQueenAttacks(board.GetPieceBits(Pieces.WhiteQueen)) ||
               moveProvider.AnyWhiteKingAttacks(board.GetPieceBits(Pieces.WhiteKing));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyMove(MoveProvider moveProvider, Board board)
    {
        return moveProvider.AnyWhiteKingMoves(board.GetPieceBits(Pieces.WhiteKing)) ||
               moveProvider.AnyWhitePawnMoves(board.GetWhitePawnSquares()) ||
               moveProvider.AnyWhiteKnightMoves(board.GetPieceBits(Pieces.WhiteKnight)) ||
               moveProvider.AnyWhiteBishopMoves(board.GetPieceBits(Pieces.WhiteBishop)) ||
               moveProvider.AnyWhiteRookMoves(board.GetPieceBits(Pieces.WhiteRook)) ||
               moveProvider.AnyWhiteQueenMoves(board.GetPieceBits(Pieces.WhiteQueen));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyPromotion(MoveProvider moveProvider, Board board)
    {
        if (!board.CanWhitePromote())
            return false;

        var promotionBoard = board.GetWhitePromotionSquares();
        while (promotionBoard.Any())
        {
            var f = promotionBoard.BitScanForward();
            var promotions = moveProvider.GetWhitePromotionAttacks(f);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count != 0 && board.IsWhiteMoveLigal(promotions[i][0]))
                    return true;
            }

            var p = moveProvider.GetWhitePromotions(f);
            if (p.Count > 0 && board.IsWhiteMoveLigal(p[0]))
                return true;

            promotionBoard = promotionBoard.Remove(f);
        }
        return false;
    }
}

/// <summary>
/// Black color implementation.
/// </summary>
public readonly struct BlackColor : IColorOperations
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateAttacks(MoveProvider moveProvider, Board board, AttackList attacks)
    {
        moveProvider.GetBlackPawnAttacks(board.GetBlackPawnSquares(), attacks);
        moveProvider.GetBlackKnightAttacks(board.GetPieceBits(Pieces.BlackKnight), attacks);
        moveProvider.GetBlackBishopAttacks(board.GetPieceBits(Pieces.BlackBishop), attacks);
        moveProvider.GetBlackRookAttacks(board.GetPieceBits(Pieces.BlackRook), attacks);
        moveProvider.GetBlackQueenAttacks(board.GetPieceBits(Pieces.BlackQueen), attacks);
        moveProvider.GetBlackKingAttacks(board.GetPieceBits(Pieces.BlackKing), attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateMoves(MoveProvider moveProvider, Board board, MoveList moves)
    {
        moveProvider.GetBlackPawnMoves(board.GetBlackPawnSquares(), moves);
        moveProvider.GetBlackKnightMoves(board.GetPieceBits(Pieces.BlackKnight), moves);
        moveProvider.GetBlackBishopMoves(board.GetPieceBits(Pieces.BlackBishop), moves);
        moveProvider.GetBlackRookMoves(board.GetPieceBits(Pieces.BlackRook), moves);
        moveProvider.GetBlackQueenMoves(board.GetPieceBits(Pieces.BlackQueen), moves);
        moveProvider.GetBlackKingMoves(board.GetPieceBits(Pieces.BlackKing), moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetPromotionSquares(Board board) => board.GetBlackPromotionSquares();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPromote(Board board) => board.CanBlackPromote();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionAttackList[] GetPromotionAttacks(MoveProvider moveProvider, byte square)
        => moveProvider.GetBlackPromotionAttacks(square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionList GetPromotions(MoveProvider moveProvider, byte square)
        => moveProvider.GetBlackPromotions(square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveLegal(Board board, MoveBase move) => board.IsBlackMoveLigal(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateSingleAttacks(MoveProvider moveProvider, Board board, AttackList attacks)
    {
        BitBoard to = new();
        moveProvider.GetBlackPawnSingleAttacks(board.GetBlackPawnSquares(), attacks, ref to);
        moveProvider.GetBlackKnightSingleAttacks(board.GetPieceBits(Pieces.BlackKnight), attacks, ref to);
        moveProvider.GetBlackBishopSingleAttacks(board.GetPieceBits(Pieces.BlackBishop), attacks, ref to);
        moveProvider.GetBlackRookSingleAttacks(board.GetPieceBits(Pieces.BlackRook), attacks, ref to);
        moveProvider.GetBlackQueenSingleAttacks(board.GetPieceBits(Pieces.BlackQueen), attacks, ref to);
        moveProvider.GetBlackKingSingleAttacks(board.GetPieceBits(Pieces.BlackKing), attacks, ref to);
    }

    public byte PieceStartIndex => 6;
    public byte PieceEndIndex => 12;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyCapture(MoveProvider moveProvider, Board board)
    {
        return moveProvider.AnyBlackPawnAttacks(board.GetBlackPawnSquares()) ||
               moveProvider.AnyBlackKnightAttacks(board.GetPieceBits(Pieces.BlackKnight)) ||
               moveProvider.AnyBlackBishopAttacks(board.GetPieceBits(Pieces.BlackBishop)) ||
               moveProvider.AnyBlackRookAttacks(board.GetPieceBits(Pieces.BlackRook)) ||
               moveProvider.AnyBlackQueenAttacks(board.GetPieceBits(Pieces.BlackQueen)) ||
               moveProvider.AnyBlackKingAttacks(board.GetPieceBits(Pieces.BlackKing));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyMove(MoveProvider moveProvider, Board board)
    {
        return moveProvider.AnyBlackKingMoves(board.GetPieceBits(Pieces.BlackKing)) ||
               moveProvider.AnyBlackPawnMoves(board.GetBlackPawnSquares()) ||
               moveProvider.AnyBlackKnightMoves(board.GetPieceBits(Pieces.BlackKnight)) ||
               moveProvider.AnyBlackBishopMoves(board.GetPieceBits(Pieces.BlackBishop)) ||
               moveProvider.AnyBlackRookMoves(board.GetPieceBits(Pieces.BlackRook)) ||
               moveProvider.AnyBlackQueenMoves(board.GetPieceBits(Pieces.BlackQueen));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyPromotion(MoveProvider moveProvider, Board board)
    {
        if (!board.CanBlackPromote())
            return false;

        var promotionBoard = board.GetBlackPromotionSquares();
        while (promotionBoard.Any())
        {
            var f = promotionBoard.BitScanForward();
            var promotions = moveProvider.GetBlackPromotionAttacks(f);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count != 0 && board.IsBlackMoveLigal(promotions[i][0]))
                    return true;
            }

            var p = moveProvider.GetBlackPromotions(f);
            if (p.Count > 0 && board.IsBlackMoveLigal(p[0]))
                return true;

            promotionBoard = promotionBoard.Remove(f);
        }
        return false;
    }
}
