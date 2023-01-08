using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;

namespace Engine.Models.Moves
{
    public class PawnOverBlackMove : PawnOverMove
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            IsEnPassant = board.IsWhiteOver(OpponentPawns);
            board.Move(Piece, From, To);
        }
    }
}