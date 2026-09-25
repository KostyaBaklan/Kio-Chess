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
    private int GetPhaseValue()
    {
        // Queen = 4, Rook = 2, Bishop = Knight = 1
        ref var boardBase = ref _boards[0];
        return ((Unsafe.Add(ref boardBase, Pieces.WhiteQueen) | Unsafe.Add(ref boardBase, Pieces.BlackQueen)).Count() << 2)
            + ((Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.BlackRook)).Count() << 1)
            + (Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | (Unsafe.Add(ref boardBase, Pieces.BlackBishop) | Unsafe.Add(ref boardBase, Pieces.BlackKnight))).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackPhaseValue()
    {
        // Queen = 4, Rook = 2, Bishop = Knight = 1
        ref var boardBase = ref _boards[0];
        return (Unsafe.Add(ref boardBase, Pieces.BlackQueen).Count() << 2)
            + (Unsafe.Add(ref boardBase, Pieces.BlackRook).Count() << 1)
            + (Unsafe.Add(ref boardBase, Pieces.BlackBishop) | Unsafe.Add(ref boardBase, Pieces.BlackKnight)).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhitePhaseValue()
    {
        // Queen = 4, Rook = 2, Bishop = Knight = 1
        ref var boardBase = ref _boards[0];
        return (Unsafe.Add(ref boardBase, Pieces.WhiteQueen).Count() << 2)
            + (Unsafe.Add(ref boardBase, Pieces.WhiteRook).Count() << 1)
            + (Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.WhiteKnight)).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEndMiddleGame() => GetPhaseValue() < _endMiddleGame;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLateEndGame() => GetPhaseValue() < _lateEndGame;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsVeryLateEndGame() => GetPhaseValue() < _veryLateEndGame;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLateMiddleGame() => GetPhaseValue() < _lateMiddleGame;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEndGame() => GetPhaseValue() < _endGame;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte GetEndgamePhaseExtension() => _endGameDepthExtension[GetPhaseValue()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldExtendEndGameSearch() => GetPhaseValue() < _endGameSearchExtension;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanWhitePromote() => (_rank6 & _boards[Pieces.WhitePawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanBlackPromote() => (_rank1 & _boards[Pieces.BlackPawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDraw()
    {
        ref var boardBase = ref _boards[0];
        if ((Unsafe.Add(ref boardBase, Pieces.WhitePawn) | Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen) | Unsafe.Add(ref boardBase, Pieces.BlackPawn) | Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen)).Any())
            return false;

        if ((Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop)).Count() < 2 && (Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackBishop)).Count() < 2)
            return true;

        if ((Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.BlackBishop)).IsZero())
            return Unsafe.Add(ref boardBase, Pieces.BlackKnight).Count() < 3;

        if ((Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.BlackBishop)).IsZero())
            return Unsafe.Add(ref boardBase, Pieces.WhiteKnight).Count() < 3;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheckToWhite() => IsBlackAttacksTo(_boards[Pieces.WhiteKing].BitScanForward());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheckToBlack() => IsWhiteAttacksTo(_boards[Pieces.BlackKing].BitScanForward());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTotalNonKingPieces()
    {
        ref var boardBase = ref _boards[0];
        return Occupied.Remove(Unsafe.Add(ref boardBase, Pieces.WhiteKing) | Unsafe.Add(ref boardBase, Pieces.BlackKing)).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAsymmetricMaterial()
    {
        ref var boardBase = ref _boards[0];
        int whiteMaterial = Whites.Remove(Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Count();
        int blackMaterial = Blacks.Remove(Unsafe.Add(ref boardBase, Pieces.BlackKing)).Count();

        // One side has significantly more pieces, or very different piece types
        return Math.Abs(whiteMaterial - blackMaterial) > 1 && Math.Min(whiteMaterial, blackMaterial) < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsQueenlessEndgame()
    {
        ref var boardBase = ref _boards[0];
        return (Unsafe.Add(ref boardBase, Pieces.WhiteQueen) | Unsafe.Add(ref boardBase, Pieces.BlackQueen)).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPawnEndgame()
    {
        ref var boardBase = ref _boards[0];
        return (Unsafe.Add(ref boardBase, Pieces.WhiteQueen) | Unsafe.Add(ref boardBase, Pieces.BlackQueen) |
            Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.BlackRook) |
            Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.BlackBishop) |
            Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.BlackKnight)).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMinorPieceEndgame()
    {
        ref var boardBase = ref _boards[0];
        return (Unsafe.Add(ref boardBase, Pieces.WhiteQueen) | Unsafe.Add(ref boardBase, Pieces.BlackQueen) |
            Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.BlackRook) |
            Unsafe.Add(ref boardBase, Pieces.WhitePawn) | Unsafe.Add(ref boardBase, Pieces.BlackPawn)).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsKingAndPawnVsKing()
    {
        ref var boardBase = ref _boards[0];
        if (Whites.Count() < 2)
        {
            return Blacks.Count() - 1 == Unsafe.Add(ref boardBase, Pieces.BlackPawn).Count();
        }
        if (Blacks.Count() < 2)
        {
            return Whites.Count() - 1 == Unsafe.Add(ref boardBase, Pieces.WhitePawn).Count();
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsZugzwangRisk()
    {
        ref var boardBase = ref _boards[0];
        return (Unsafe.Add(ref boardBase, Pieces.WhiteQueen) | Unsafe.Add(ref boardBase, Pieces.BlackQueen) |
            Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.BlackRook)).IsZero() && ((Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.BlackBishop) | Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.BlackKnight)).IsZero() || (Unsafe.Add(ref boardBase, Pieces.WhitePawn) | Unsafe.Add(ref boardBase, Pieces.BlackPawn)).IsZero());
    }
}
