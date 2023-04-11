using System.Runtime.CompilerServices;
using Engine.Models.Boards;

namespace Engine.Models.Helpers
{
    public static class SquareExtensions
    {
        private static readonly string[] _names = new string[64];
        private static readonly BitBoard[] _values = new BitBoard[64];
        private static readonly byte[] _opponents;

        static SquareExtensions()
        {
            string ab = "ABCDEFGH";
            string n = "12345678";

            int index = 0;
            foreach (var d in n)
            {
                foreach (var t in ab)
                {
                    _names[index] = $"{t}{d}";
                    index++;
                }
            }

            for (int i = 0; i < 64; i++)
            {
                _values[i] = new BitBoard(1ul << i);
            }

            _opponents = new byte[64];
            for (int i = 0; i < 64; i++)
            {
                var file = 7 - i / 8;
                var rank = i % 8;
                _opponents[i] = (byte)(file * 8 + rank);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AsString(this byte square)
        {
            return _names[square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard AsBitBoard(this byte square)
        {
            return _values[square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard AsBitBoard(this int square)
        {
            return _values[square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetOpponent(this byte square)
        {
            return _opponents[square];
        }
    }
}