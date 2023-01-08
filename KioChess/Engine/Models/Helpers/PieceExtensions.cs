using System.Runtime.CompilerServices;
using Engine.Models.Enums;

namespace Engine.Models.Helpers
{
    public static class PieceExtensions
    {
        private static readonly int[] _values = new int[12];
        private static readonly string[] _names = new string[12];
        private static readonly int[] _opponents = new int[12];

        static PieceExtensions()
        {
            _values[Piece.WhitePawn.AsByte()] = 8;
            _values[Piece.BlackPawn.AsByte()] = 8;
            _values[Piece.WhiteKnight.AsByte()] = 25;
            _values[Piece.BlackKnight.AsByte()] = 25;
            _values[Piece.WhiteBishop.AsByte()] = 25;
            _values[Piece.BlackBishop.AsByte()] = 25;
            _values[Piece.WhiteKing.AsByte()] = 0;
            _values[Piece.BlackKing.AsByte()] = 0;
            _values[Piece.WhiteRook.AsByte()] = 39;
            _values[Piece.BlackRook.AsByte()] = 39;
            _values[Piece.WhiteQueen.AsByte()] = 79;
            _values[Piece.BlackQueen.AsByte()] = 79;

            _names[(int)Piece.WhitePawn] = "P";
            _names[(int)Piece.BlackPawn] = "P";
            _names[(int)Piece.WhiteKnight] = "N";
            _names[(int)Piece.BlackKnight] = "N";
            _names[(int)Piece.WhiteBishop] = "B";
            _names[(int)Piece.BlackBishop] = "B";
            _names[(int)Piece.WhiteKing] = "K";
            _names[(int)Piece.BlackKing] = "K";
            _names[(int)Piece.WhiteRook] = "R";
            _names[(int)Piece.BlackRook] = "R";
            _names[(int)Piece.WhiteQueen] = "Q";
            _names[(int)Piece.BlackQueen] = "Q";

            for (var i = 0; i < 6; i++)
            {
                _opponents[i] = i + 6;
            }
            for (var i = 6; i < _opponents.Length; i++)
            {
                _opponents[i] = i - 6;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhite(this Piece piece)
        {
            return piece < Piece.BlackPawn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlack(this Piece piece)
        {
            return piece > Piece.WhiteKing;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte AsByte(this Piece piece)
        {
            return (byte)piece;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsValue(this Piece piece)
        {
            return _values[(byte) piece];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece GetOpponent(this Piece piece)
        {
            return (Piece) _opponents[piece.AsByte()];
        }
    }
}
