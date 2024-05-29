using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class PromotionMove : AttackBase
{
    public byte PromotionPiece;
    public int PromotionSee;

    public PromotionMove()
    {
        IsPromotion = true;
        IsAttack = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetSee() => See = PromotionSee;
}

public class PromotionWhiteMove : PromotionMove
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        Board.RemoveWhite(Piece, From);
        Board.AddWhite(PromotionPiece, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.AddWhite(Piece, From);
        Board.RemoveWhite(PromotionPiece, To);
    }
}

public class PromotionBlackMove : PromotionMove
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        Board.RemoveBlack(Piece, From);
        Board.AddBlack(PromotionPiece, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.AddBlack(Piece, From);
        Board.RemoveBlack(PromotionPiece, To);
    }
}