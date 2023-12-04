using System.Runtime.CompilerServices;
using Engine.Models.Enums;

namespace Engine.Models.Helpers;

public static class PieceExtensions
{
    private static readonly string[] _names = new string[12];
    private static readonly string[] _strings = new string[12];
    private static readonly string[] _keys = new string[12];
    private static readonly byte[] _opponents = new byte[12];

    static PieceExtensions()
    {
        _names[Pieces.WhitePawn] = "P";
        _names[Pieces.BlackPawn] = "P";
        _names[Pieces.WhiteKnight] = "N";
        _names[Pieces.BlackKnight] = "N";
        _names[Pieces.WhiteBishop] = "B";
        _names[Pieces.BlackBishop] = "B";
        _names[Pieces.WhiteKing] = "K";
        _names[Pieces.BlackKing] = "K";
        _names[Pieces.WhiteRook] = "R";
        _names[Pieces.BlackRook] = "R";
        _names[Pieces.WhiteQueen] = "Q";
        _names[Pieces.BlackQueen] = "Q";

        _keys[Pieces.WhitePawn] = "WP";
        _keys[Pieces.BlackPawn] = "BP";
        _keys[Pieces.WhiteKnight] = "WN";
        _keys[Pieces.BlackKnight] = "BN";
        _keys[Pieces.WhiteBishop] = "WB";
        _keys[Pieces.BlackBishop] = "BB";
        _keys[Pieces.WhiteKing] = "WK";
        _keys[Pieces.BlackKing] = "BK";
        _keys[Pieces.WhiteRook] = "WR";
        _keys[Pieces.BlackRook] = "BR";
        _keys[Pieces.WhiteQueen] = "WQ";
        _keys[Pieces.BlackQueen] = "BQ";

        _strings[Pieces.WhitePawn] = "WhitePawn";
        _strings[Pieces.BlackPawn] = "BlackPawn";
        _strings[Pieces.WhiteKnight] = "WhiteKnight";
        _strings[Pieces.BlackKnight] = "BlackKnight";
        _strings[Pieces.WhiteBishop] = "WhiteBishop";
        _strings[Pieces.BlackBishop] = "BlackBishop";
        _strings[Pieces.WhiteKing] = "WhiteKing";
        _strings[Pieces.BlackKing] = "BlackKing";
        _strings[Pieces.WhiteRook] = "WhiteRook";
        _strings[Pieces.BlackRook] = "BlackRook";
        _strings[Pieces.WhiteQueen] = "WhiteQueen";
        _strings[Pieces.BlackQueen] = "BlackQueen";

        for (var i = 0; i < 6; i++)
        {
            _opponents[i] = (byte)(i + 6);
        }
        for (var i = 6; i < _opponents.Length; i++)
        {
            _opponents[i] = (byte)(i - 6);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWhite(this byte piece)
    {
        return piece < Pieces.BlackPawn;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBlack(this byte piece)
    {
        return piece > Pieces.WhiteKing;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsKeyName(this byte piece)
    {
        return _keys[piece];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsEnumString(this byte piece)
    {
        return _strings[piece];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Opponent(this byte piece)
    {
        return _opponents[piece];
    }
}
