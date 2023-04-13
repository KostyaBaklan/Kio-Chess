﻿using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;

namespace Engine.Models.Moves
{
    public class Move : MoveBase
    {
        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsEmpty(EmptyBoard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<byte> figureHistory)
        {
            board.Move(Piece, From,To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<byte> figureHistory)
        {
            board.Move(Piece, To, From);
        }

        #endregion
    }
}