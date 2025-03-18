using System.Runtime.CompilerServices;
using Engine.Models.Helpers;

namespace Engine.Models.Moves;

public abstract class PromotionAttack : AttackBase
{
    public byte PromotionPiece;
    public int PromotionSee;

    public PromotionAttack()
    {
        IsPromotion = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetSee(byte captured) => See = PromotionSee + CapturedValue[captured];

    public override string ToUciString() => $"{From.AsString()}{To.AsString()}{PromotionPiece.AsName()}".ToLower();
}

public class WhitePromotionAttack : PromotionAttack
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsWhiteOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        Board.RemoveWhite(Piece, From);
        byte piece = Board.GetPiece(To);
        Board.RemoveBlack(piece, To);
        _figureHistory.Push(piece);
        Board.AddWhite(PromotionPiece, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.AddWhite(Piece, From);
        Board.AddBlack(_figureHistory.Pop(), To);
        Board.RemoveWhite(PromotionPiece, To);
    }
}

public class BlackPromotionAttack : PromotionAttack
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsBlackOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        Board.RemoveBlack(Piece, From);
        byte piece = Board.GetPiece(To);
        Board.RemoveWhite(piece, To);
        _figureHistory.Push(piece);
        Board.AddBlack(PromotionPiece, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.AddBlack(Piece, From);
        Board.AddWhite(_figureHistory.Pop(), To);
        Board.RemoveBlack(PromotionPiece, To);
    }
}