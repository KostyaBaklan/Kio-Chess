using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public class WhiteAttack : AttackBase
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
        Board.AddBlack(_figureHistory.Pop(), To);
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard) && Board.IsWhiteOpposite(To);
}

public class BlackAttack : AttackBase
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
        Board.AddWhite(_figureHistory.Pop(), To);
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard) && Board.IsBlackOpposite(To);
}