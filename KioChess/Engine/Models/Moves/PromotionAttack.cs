using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

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
    
    /// <summary>
    /// Converts promotion attack to SAN notation (dxe8=Q).
    /// </summary>
    public override string ToSAN()
    {
        var from = From.AsString().ToLower();
        var to = To.AsString().ToLower();
        
        var promoChar = PromotionPiece switch
        {
            Pieces.WhiteQueen or Pieces.BlackQueen => "Q",
            Pieces.WhiteRook or Pieces.BlackRook => "R",
            Pieces.WhiteBishop or Pieces.BlackBishop => "B",
            Pieces.WhiteKnight or Pieces.BlackKnight => "N",
            _ => "Q"
        };
        
        // Pawn capture with promotion: origin file + x + destination + = + promotion piece
        return $"{from[0]}x{to}={promoChar}";
    }
}

public sealed class WhitePromotionAttack : PromotionAttack
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

public sealed class BlackPromotionAttack : PromotionAttack
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