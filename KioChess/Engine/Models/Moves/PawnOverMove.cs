﻿using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;

namespace Engine.Models.Moves
{
    public abstract  class PawnOverMove : MoveBase
    {
        public BitBoard OpponentPawns;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<byte> figureHistory)
        {
            IsEnPassant = false;
            board.Move(Piece, To, From);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsEmpty(EmptyBoard);
        }
    }
}