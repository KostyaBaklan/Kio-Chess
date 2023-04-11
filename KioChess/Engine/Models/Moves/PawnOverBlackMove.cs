using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;

namespace Engine.Models.Moves
{
    public class PawnOverBlackMove : PawnOverMove
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<byte> figureHistory)
        {
            IsEnPassant = board.IsWhiteOver(OpponentPawns);
            board.Move(Piece, From, To);
        }
    }
}