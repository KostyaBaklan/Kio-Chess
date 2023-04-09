using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;

namespace Engine.Models.Moves
{
    public abstract  class Attack : AttackBase
    {
        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            Piece piece = board.GetPiece(To);
            board.Remove(piece, To);
            figureHistory.Push(piece);
            board.Move(Piece, From,To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.Move(Piece, To, From);
            Piece piece = figureHistory.Pop();
            board.Add(piece, To);
        }

        #endregion

        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegalAttack(IBoard board)
        {
            return board.IsEmpty(EmptyBoard);
        }

        #endregion
    }

    public class WhiteAttack : Attack
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsEmpty(EmptyBoard) && board.IsWhiteOpposite(To) ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool IsQueenCaptured()
        {
            return Captured == Piece.BlackQueen;
        }
    }

    public class BlackAttack : Attack
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsEmpty(EmptyBoard) &&board.IsBlackOpposite(To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool IsQueenCaptured()
        {
            return Captured == Piece.WhiteQueen;
        }
    }
}