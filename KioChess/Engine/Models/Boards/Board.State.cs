using Engine.Models.Enums;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteCheck(MoveBase move)
    {
        try
        {
            move.Make();

            return IsWhiteAttacksTo(GetBlackKingPosition());
        }
        finally
        {
            move.UnMake();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackCheck(MoveBase move)
    {
        try
        {
            move.Make();

            return IsBlackAttacksTo(GetWhiteKingPosition());
        }
        finally
        {
            move.UnMake();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheck(MoveBase move)
    {
        try
        {
            move.Make();

            return move.IsBlack
                ? IsBlackAttacksTo(GetWhiteKingPosition())
                : IsWhiteAttacksTo(GetBlackKingPosition());
        }
        finally
        {
            move.UnMake();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLateEndGame() => IsLateEndGameForWhite() && IsLateEndGameForBlack();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateEndGameForBlack() => (_boards[Pieces.BlackQueen] | _boards[Pieces.BlackRook]).IsZero() && (_boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop]).Count() < 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateEndGameForWhite() => (_boards[Pieces.WhiteQueen] | _boards[Pieces.WhiteRook]).IsZero() && (_boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop]).Count() < 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLateMiddleGame() => IsLateMiddleGameForWhite() || IsLateMiddleGameForBlack();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateMiddleGameForBlack()
    {
        var wq = _boards[Pieces.BlackQueen].Count();

        if (wq > 1) return (_boards[Pieces.BlackRook] | _boards[Pieces.BlackBishop] | _boards[Pieces.BlackKnight]).IsZero();
        if (wq == 1) return (_boards[Pieces.BlackRook] | _boards[Pieces.BlackBishop] | _boards[Pieces.BlackKnight]).Count() < 2;
        return (_boards[Pieces.BlackRook] | _boards[Pieces.BlackBishop] | _boards[Pieces.BlackKnight]).Count() < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateMiddleGameForWhite()
    {
        var wq = _boards[Pieces.WhiteQueen].Count();

        if (wq > 1) return (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteKnight]).IsZero();
        if (wq == 1) return (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteKnight]).Count() < 2;
        return (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteKnight]).Count() < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEndGame() => IsEndGameForWhite() || IsEndGameForBlack();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEndGameForBlack()
    {
        var bqr = (_boards[Pieces.BlackQueen] | _boards[Pieces.BlackRook]).Count();

        return bqr <= 1 && (bqr == 1
            ? (_boards[Pieces.BlackBishop] | _boards[Pieces.BlackKnight]).Count() < 2
            : (_boards[Pieces.BlackBishop] | _boards[Pieces.BlackKnight]).Count() < 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEndGameForWhite()
    {
        var wqr = (_boards[Pieces.WhiteQueen] | _boards[Pieces.WhiteRook]).Count();

        return wqr <= 1 && (wqr == 1
            ? (_boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteKnight]).Count() < 2
            : (_boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteKnight]).Count() < 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanWhitePromote() => (_rank6 & _boards[Pieces.WhitePawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanBlackPromote() => (_rank1 & _boards[Pieces.BlackPawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDraw()
    {
        if ((_boards[Pieces.WhitePawn] | _boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen] | _boards[Pieces.BlackPawn] | _boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen]).Any())
            return false;

        if ((_boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop]).Count() < 2 && (_boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop]).Count() < 2)
            return true;

        if ((_boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop] | _boards[Pieces.BlackBishop]).IsZero())
            return _boards[Pieces.BlackKnight].Count() < 3;

        if ((_boards[Pieces.BlackKnight] | _boards[Pieces.WhiteBishop] | _boards[Pieces.BlackBishop]).IsZero())
            return _boards[Pieces.WhiteKnight].Count() < 3;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheckToWhite() => IsBlackAttacksTo(_boards[Pieces.WhiteKing].BitScanForward());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheckToBlack() => IsWhiteAttacksTo(_boards[Pieces.BlackKing].BitScanForward());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTotalNonKingPieces() => (_whites | _blacks).Remove(_boards[Pieces.WhiteKing] | _boards[Pieces.BlackKing]).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAsymmetricMaterial()
    {
        int whiteMaterial = _whites.Remove(_boards[Pieces.WhiteKing]).Count();
        int blackMaterial = _blacks.Remove(_boards[Pieces.BlackKing]).Count();

        // One side has significantly more pieces, or very different piece types
        return Math.Abs(whiteMaterial - blackMaterial) > 1 && Math.Min(whiteMaterial, blackMaterial) < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsQueenlessEndgame() => (_boards[Pieces.WhiteQueen] | _boards[Pieces.BlackQueen]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPawnEndgame() => (_boards[Pieces.WhiteQueen] | _boards[Pieces.BlackQueen] |
            _boards[Pieces.WhiteRook] | _boards[Pieces.BlackRook] |
            _boards[Pieces.WhiteBishop] | _boards[Pieces.BlackBishop] |
            _boards[Pieces.WhiteKnight] | _boards[Pieces.BlackKnight]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMinorPieceEndgame() => (_boards[Pieces.WhiteQueen] | _boards[Pieces.BlackQueen] |
            _boards[Pieces.WhiteRook] | _boards[Pieces.BlackRook] |
            _boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsKingAndPawnVsKing()
    {
        if (_whites.Count() < 2)
        {
            return _blacks.Count() - 1 == _boards[Pieces.BlackPawn].Count();
        }
        if (_blacks.Count() < 2)
        {
            return _whites.Count() - 1 == _boards[Pieces.WhitePawn].Count();
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsZugzwangRisk() => (_boards[Pieces.WhiteQueen] | _boards[Pieces.BlackQueen] |
            _boards[Pieces.WhiteRook] | _boards[Pieces.BlackRook]).IsZero() && ((_boards[Pieces.WhiteBishop] | _boards[Pieces.BlackBishop] | _boards[Pieces.WhiteKnight] | _boards[Pieces.BlackKnight]).IsZero() || (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn]).IsZero());
}
