using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class SimpleAttack : Attack
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegalAttack() => true;
}

public class WhiteSimpleAttack : SimpleAttack
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
    public override bool IsLegal() => Board.IsWhiteOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override bool IsQueenCaptured() => Captured == Enums.Pieces.BlackQueen;
}
public class BlackSimpleAttack : SimpleAttack
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
    public override bool IsLegal() => Board.IsBlackOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override bool IsQueenCaptured() => Captured == Enums.Pieces.WhiteQueen;
}