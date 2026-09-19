using Engine.Models.Enums;
using Engine.Models.Helpers;
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
    internal void SetSee() => See = PromotionSee;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard);

    public override string ToUciString() => $"{From.AsString()}{To.AsString()}{PromotionPiece.AsName()}".ToLower();
    
    /// <summary>
    /// Converts promotion to SAN notation (e8=Q).
    /// </summary>
    public override string ToSAN()
    {
        var to = To.AsString().ToLower();
        
        var promoChar = PromotionPiece switch
        {
            Pieces.WhiteQueen or Pieces.BlackQueen => "Q",
            Pieces.WhiteRook or Pieces.BlackRook => "R",
            Pieces.WhiteBishop or Pieces.BlackBishop => "B",
            Pieces.WhiteKnight or Pieces.BlackKnight => "N",
            _ => "Q"
        };
        
        return $"{to}={promoChar}";
    }
}

public sealed class PromotionWhiteMove : PromotionMove
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

public sealed class PromotionBlackMove : PromotionMove
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