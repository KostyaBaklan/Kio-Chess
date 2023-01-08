using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Moves
{
    public class BigCastle : MoveBase
    {
        public BigCastle()
        {
            Type = MoveType.BigCastle;
            IsCastle = true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return Piece.IsWhite() ? board.CanDoWhiteBigCastle() : board.CanDoBlackBigCastle();
        }

        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsReversable()
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            if (Piece.IsWhite())
                board.DoWhiteBigCastle();
            else
                board.DoBlackBigCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece> figureHistory)
        {
            if (Piece.IsWhite())
                board.UndoWhiteBigCastle();
            else
                board.UndoBlackBigCastle();
        }

        #endregion
    }
}