using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Models.Helpers
{
    public static class MoveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCheckToBlack(this IMoveProvider moveProvider,IBoard board)
        {
            var kingPosition = board.GetBlackKingPosition().AsBitBoard();
            return moveProvider.IsWhiteAttackTo(board, kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCheckToWhite(this IMoveProvider moveProvider, IBoard board)
        {
            var kingPosition = board.GetWhiteKingPosition().AsBitBoard();
            return moveProvider.IsBlackAttackTo(board, kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            return moveProvider.IsBlackBishopAttackTo(board, to)
                   || moveProvider.IsBlackKnightAttackTo(board, to)
                   || moveProvider.IsBlackQueenAttackTo(board, to)
                   || moveProvider.IsBlackRookAttackTo(board, to)
                   || moveProvider.IsBlackPawnAttackTo(board, to)
                   || moveProvider.IsBlackKingAttackTo(board, to);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static bool IsBlackAttackTo(this IMoveProvider moveProvider, IBoard board, int to)
        //{
        //    return moveProvider.IsBlackBishopAttackTo(board, to)
        //           || moveProvider.IsBlackKnightAttackTo(board, to)
        //           || moveProvider.IsBlackQueenAttackTo(board, to)
        //           || moveProvider.IsBlackRookAttackTo(board, to)
        //           || moveProvider.IsBlackPawnAttackTo(board, to)
        //           || moveProvider.IsBlackKingAttackTo(board, to);
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhiteKingAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return moveProvider.GetAttackPattern(Piece.BlackKing.AsByte(), to) &
                   board.GetPieceBits(Piece.WhiteKing);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhitePawnAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), to) &
                   board.GetPieceBits(Piece.WhitePawn);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhiteRookAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return to.RookAttacks(board.GetOccupied()) & board.GetPieceBits(Piece.WhiteRook);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhiteQueenAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return to.QueenAttacks(board.GetOccupied()) & board.GetPieceBits(Piece.WhiteQueen);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhiteKnightAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return moveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), to) &
                   board.GetPieceBits(Piece.WhiteKnight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhiteBishopAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return to.BishopAttacks(board.GetOccupied()) & board.GetPieceBits(Piece.WhiteBishop);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackKingAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return moveProvider.GetAttackPattern(Piece.WhiteKing.AsByte(), to) &
                board.GetPieceBits(Piece.BlackKing);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackPawnAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), to) &
                   board.GetPieceBits(Piece.BlackPawn);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackRookAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return to.RookAttacks(board.GetOccupied()) & board.GetPieceBits(Piece.BlackRook);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackQueenAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return to.QueenAttacks(board.GetOccupied()) & board.GetPieceBits(Piece.BlackQueen);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackKnightAttackTo(this IMoveProvider moveProvider, IBoard board, byte to)
        {
            return moveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), to) &
                   board.GetPieceBits(Piece.BlackKnight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackBishopAttackTo(this IMoveProvider moveProvider, IBoard board, int to)
        {
            return to.BishopAttacks(board.GetOccupied()) & board.GetPieceBits(Piece.BlackBishop);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            return moveProvider.IsWhiteBishopAttackTo(board, to)
                || moveProvider.IsWhiteKnightAttackTo(board, to)
                || moveProvider.IsWhiteQueenAttackTo(board, to)
                || moveProvider.IsWhiteRookAttackTo(board, to)
                || moveProvider.IsWhitePawnAttackTo(board, to)
                || moveProvider.IsWhiteKingAttackTo(board, to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteKnightAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.WhiteKnight.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhiteKnightAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteBishopAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.WhiteBishop.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhiteBishopAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteQueenAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.WhiteQueen.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhiteQueenAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteRookAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.WhiteRook.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhiteRookAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhitePawnAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.WhitePawn.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhitePawnAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteKingAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.WhiteKing.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhiteKingAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackKnightAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.BlackKnight.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackKnightAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackBishopAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.BlackBishop.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackBishopAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackQueenAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.BlackQueen.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackQueenAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackRookAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.BlackRook.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackRookAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackPawnAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.BlackPawn.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackPawnAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackKingAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPiecePositions(Piece.BlackKing.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackKingAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackPawnAttackPattern(this IMoveProvider moveProvider, byte position)
        {
            return moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackRookAttackPattern(this byte position, BitBoard occupied)
        {
            return position.RookAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackQueenAttackPattern(this byte position, BitBoard occupied)
        {
            return position.QueenAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackBishopAttackPattern(this byte position, BitBoard occupied)
        {
            return position.BishopAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackKingAttackPattern(this IMoveProvider moveProvider, byte position)
        {
            return moveProvider.GetAttackPattern(Piece.BlackKing.AsByte(), position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetBlackKnightAttackPattern(this IMoveProvider moveProvider, byte position)
        {
            return moveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhitePawnAttackPattern(this IMoveProvider moveProvider, byte position)
        {
            return moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhiteRookAttackPattern(this byte position, BitBoard occupied)
        {
            return position.RookAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhiteQueenAttackPattern(this byte position, BitBoard occupied)
        {
            return position.QueenAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhiteBishopAttackPattern(this byte position, BitBoard occupied)
        {
            return position.BishopAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhiteKnightAttackPattern(this IMoveProvider moveProvider, byte position)
        {
            return moveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetWhiteKingAttackPattern(this IMoveProvider moveProvider, byte position)
        {
            return moveProvider.GetAttackPattern(Piece.WhiteKing.AsByte(), position);
        }
    }
}
