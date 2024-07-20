using System.Runtime.CompilerServices;
using Engine.Models.Enums;

namespace Engine.Models.Moves;

public abstract  class Attack : AttackBase
{
    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegalAttack() => Board.IsEmpty(EmptyBoard);

    #endregion
}

public class WhiteAttack : Attack
{
    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        byte piece = Board.GetPiece(To);
        Board.RemoveBlack(piece, To);
        _figureHistory.Push(piece);
        Board.MoveWhite(Piece, From, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.MoveWhite(Piece, To, From);
        byte piece = _figureHistory.Pop();
        Board.AddBlack(piece, To);
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard) && Board.IsWhiteOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override bool IsQueenCaptured() => Captured == Pieces.BlackQueen;
}

public class BlackAttack : Attack
{
    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        byte piece = Board.GetPiece(To);
        Board.RemoveWhite(piece, To);
        _figureHistory.Push(piece);
        Board.MoveBlack(Piece, From, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.MoveBlack(Piece, To, From);
        byte piece = _figureHistory.Pop();
        Board.AddWhite(piece, To);
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard) && Board.IsBlackOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override bool IsQueenCaptured() => Captured == Pieces.WhiteQueen;
}