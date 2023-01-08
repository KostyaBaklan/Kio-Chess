using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Moves
{
    public class SmallCastle : MoveBase
    {
        public SmallCastle()
        {
            Type = MoveType.SmallCastle;
            IsCastle = true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return Piece.IsWhite() ? board.CanDoWhiteSmallCastle() : board.CanDoBlackSmallCastle();
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
                board.DoWhiteSmallCastle();
            else
                board.DoBlackSmallCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece> figureHistory)
        {
            if (Piece.IsWhite())
                board.UndoWhiteSmallCastle();
            else
                board.UndoBlackSmallCastle();
        }

        #endregion
    }
}