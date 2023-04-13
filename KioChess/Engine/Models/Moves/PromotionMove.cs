using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;

namespace Engine.Models.Moves
{
    public class PromotionMove : Move
    {
        public byte PromotionPiece;

        public PromotionMove()
        {
            IsPromotion = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<byte> figureHistory)
        {
            board.Remove(Piece, From);
            board.Add(PromotionPiece, To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<byte> figureHistory)
        {
            board.Add(Piece, From);
            board.Remove(PromotionPiece, To);
        }
    }
}