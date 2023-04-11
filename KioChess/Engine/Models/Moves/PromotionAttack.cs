using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;

namespace Engine.Models.Moves
{
    public abstract  class PromotionAttack : Attack
    {
        public byte PromotionPiece;

        public PromotionAttack()
        {
            IsPromotion = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<byte> figureHistory)
        {
            board.Remove(Piece, From);
            byte piece = board.GetPiece(To);
            board.Remove(piece, To);
            figureHistory.Push(piece);
            board.Add(PromotionPiece, To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<byte> figureHistory)
        {
            board.Add(Piece, From);
            byte piece = figureHistory.Pop();
            board.Add(piece, To);
            board.Remove(PromotionPiece, To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegalAttack(IBoard board)
        {
            return true;
        }
    }

    public class WhitePromotionAttack : PromotionAttack
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsWhiteOpposite(To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool IsQueenCaptured()
        {
            return Captured == Pieces.BlackQueen;
        }
    }

    public class BlackPromotionAttack : PromotionAttack
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsBlackOpposite(To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool IsQueenCaptured()
        {
            return Captured == Pieces.WhiteQueen;
        }
    }
}