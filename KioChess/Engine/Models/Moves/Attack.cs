using System.Runtime.CompilerServices;
using Engine.Models.Enums;

namespace Engine.Models.Moves
{
    public abstract  class Attack : AttackBase
    {
        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make()
        {
            byte piece = Board.GetPiece(To);
            Board.Remove(piece, To);
            _figureHistory.Push(piece);
            Board.Move(Piece, From,To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake()
        {
            Board.Move(Piece, To, From);
            byte piece = _figureHistory.Pop();
            Board.Add(piece, To);
        }

        #endregion

        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegalAttack()
        {
            return Board.IsEmpty(EmptyBoard);
        }

        #endregion
    }

    public class WhiteAttack : Attack
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal()
        {
            return Board.IsEmpty(EmptyBoard) && Board.IsWhiteOpposite(To) ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool IsQueenCaptured()
        {
            return Captured == Pieces.BlackQueen;
        }
    }

    public class BlackAttack : Attack
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal()
        {
            return Board.IsEmpty(EmptyBoard) &&Board.IsBlackOpposite(To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool IsQueenCaptured()
        {
            return Captured == Pieces.WhiteQueen;
        }
    }
}