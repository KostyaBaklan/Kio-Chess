using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;

namespace Engine.Models.Moves
{
    public abstract class SmallCastle : MoveBase
    {
        public SmallCastle()
        {
            IsCastle = true;
        }
    }

    public class WhiteSmallCastle : SmallCastle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.CanDoWhiteSmallCastle();
        }

        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.DoWhiteSmallCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.UndoWhiteSmallCastle();
        }

        #endregion
    }
    public class BlackSmallCastle : SmallCastle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.CanDoBlackSmallCastle();
        }

        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.DoBlackSmallCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.UndoBlackSmallCastle();
        }

        #endregion
    }
}