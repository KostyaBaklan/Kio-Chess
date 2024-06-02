using System.Runtime.CompilerServices;
using Engine.Models.Enums;

namespace Engine.Models.Moves;

public abstract  class PromotionAttack : Attack
{
    public byte PromotionPiece;
    public int PromotionSee;

    public PromotionAttack()
    {
        IsPromotion = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegalAttack() => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetSee(byte captured) => See = PromotionSee + CapturedValue[captured];
}

public class WhitePromotionAttack : PromotionAttack
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsWhiteOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override bool IsQueenCaptured() => Captured == Pieces.BlackQueen;

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
        byte piece = _figureHistory.Pop();
        Board.AddBlack(piece, To);
        Board.RemoveWhite(PromotionPiece, To);
    }
}

public class BlackPromotionAttack : PromotionAttack
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsBlackOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override bool IsQueenCaptured() => Captured == Pieces.WhiteQueen;

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
        byte piece = _figureHistory.Pop();
        Board.AddWhite(piece, To);
        Board.RemoveBlack(PromotionPiece, To);
    }
}