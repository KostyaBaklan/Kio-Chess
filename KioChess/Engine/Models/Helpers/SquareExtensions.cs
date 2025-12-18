using Engine.Models.Boards.Buffers;
using Engine.Models.Boards.Structures;
using System.Runtime.CompilerServices;

namespace Engine.Models.Helpers;

public static class SquareExtensions
{
    private static readonly string[] _names = new string[64];
    private static readonly byte[] _opponents;
    private static readonly CellBuffer<BitBoard> _cellBoards = new CellBuffer<BitBoard>();

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
            _cellBoards[i] = new BitBoard(1ul << i);
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
    public static string AsString(this byte square) => _names[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard AsBitBoard(this byte square) => _cellBoards[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard AsBitBoard(this int square) => _cellBoards[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetOpponent(this byte square) => _opponents[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetIndex(this string square) => (byte)Array.IndexOf(_names, square.ToUpper());
}