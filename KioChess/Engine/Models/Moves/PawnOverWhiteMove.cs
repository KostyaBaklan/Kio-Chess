using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;

namespace Engine.Models.Moves
{
    public class PawnOverWhiteMove: PawnOverMove
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<byte> figureHistory)
        {
            IsEnPassant = board.IsBlackOver(OpponentPawns);
            board.Move(Piece, From, To);
        }
    }
}