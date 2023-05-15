using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.Models.Moves
{
    public class PawnOverWhiteMove: PawnOverMove
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make()
        {
            IsEnPassant = Board.IsBlackOver(OpponentPawns);
            Board.Move(Piece, From, To);
        }
    }
}