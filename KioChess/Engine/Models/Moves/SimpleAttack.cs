using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.Models.Moves
{
    public abstract class SimpleAttack : Attack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegalAttack(IBoard board)
        {
            return true;
        }
    }

    public class WhiteSimpleAttack : SimpleAttack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsWhiteOpposite(To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool IsQueenCaptured()
        {
            return Captured == Enums.Pieces.BlackQueen;
        }
    }
    public class BlackSimpleAttack : SimpleAttack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsBlackOpposite(To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool IsQueenCaptured()
        {
            return Captured == Enums.Pieces.WhiteQueen;
        }
    }
}