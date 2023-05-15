using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.Models.Moves
{
    public class PromotionMove : AttackBase
    {
        public byte PromotionPiece;

        public PromotionMove()
        {
            IsPromotion = true;
            IsAttack = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal()
        {
            return Board.IsEmpty(EmptyBoard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make()
        {
            Board.Remove(Piece, From);
            Board.Add(PromotionPiece, To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake()
        {
            Board.Add(Piece, From);
            Board.Remove(PromotionPiece, To);
        }
    }
}