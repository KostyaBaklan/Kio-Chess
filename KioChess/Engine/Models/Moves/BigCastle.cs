using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Moves
{
    public abstract class BigCastle : MoveBase
    {
        public BigCastle()
        {
            IsCastle = true;
        }
    }
    public class WhiteBigCastle : BigCastle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.CanDoWhiteBigCastle();
        }

        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.DoWhiteBigCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.UndoWhiteBigCastle();
        }

        #endregion
    }
    public class BlackBigCastle : BigCastle
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.CanDoBlackBigCastle();
        }

        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.DoBlackBigCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.UndoBlackBigCastle();
        }

        #endregion
    }
}