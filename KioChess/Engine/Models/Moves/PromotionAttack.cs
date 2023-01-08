using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Moves
{
    public class PromotionAttack : Attack
    {
        public Piece PromotionPiece;

        public PromotionAttack()
        {
            IsPromotion = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.Remove(Piece, From);
            Piece piece = board.GetPiece(To);
            board.Remove(piece, To);
            figureHistory.Push(piece);
            board.Add(PromotionPiece, To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.Add(Piece, From);
            Piece piece = figureHistory.Pop();
            board.Add(piece, To);
            board.Remove(PromotionPiece, To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return Piece.IsWhite() ? board.IsWhiteOpposite(To) : board.IsBlackOpposite(To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegalAttack(IBoard board)
        {
            return true;
        }
    }
}