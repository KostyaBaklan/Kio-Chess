using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public sealed class WhiteSimpleAttack : AttackBase
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
    public override bool IsLegal() => Board.IsWhiteOpposite(To);
}
public sealed class BlackSimpleAttack : AttackBase
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
    public override bool IsLegal() => Board.IsBlackOpposite(To);
}