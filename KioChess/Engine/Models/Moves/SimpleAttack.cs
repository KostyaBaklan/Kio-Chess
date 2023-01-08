using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Helpers;

namespace Engine.Models.Moves
{
    public class SimpleAttack : Attack
    {
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