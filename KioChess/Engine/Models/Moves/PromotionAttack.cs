using System.Runtime.CompilerServices;
using Engine.Models.Enums;

namespace Engine.Models.Moves;

public abstract  class PromotionAttack : Attack
{
    public byte PromotionPiece;

    public PromotionAttack()
    {
        IsPromotion = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        Board.Remove(Piece, From);
        byte piece = Board.GetPiece(To);
        Board.Remove(piece, To);
        _figureHistory.Push(piece);
        Board.Add(PromotionPiece, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.Add(Piece, From);
        byte piece = _figureHistory.Pop();
        Board.Add(piece, To);
        Board.Remove(PromotionPiece, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegalAttack() => true;
}

public class WhitePromotionAttack : PromotionAttack
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsWhiteOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override bool IsQueenCaptured() => Captured == Pieces.BlackQueen;
}

public class BlackPromotionAttack : PromotionAttack
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsBlackOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override bool IsQueenCaptured() => Captured == Pieces.WhiteQueen;
}