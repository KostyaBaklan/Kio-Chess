using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;

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
        public override void Make(IBoard board, ArrayStack<byte> figureHistory)
        {
            board.DoWhiteBigCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<byte> figureHistory)
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
        public override void Make(IBoard board, ArrayStack<byte> figureHistory)
        {
            board.DoBlackBigCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<byte> figureHistory)
        {
            board.UndoBlackBigCastle();
        }

        #endregion
    }
}