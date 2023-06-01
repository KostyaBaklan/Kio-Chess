using System.Runtime.CompilerServices;

namespace Engine.Models.Moves
{
    public class PawnOverBlackMove : PawnOverMove
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make()
        {
            IsEnPassant = Board.IsWhiteOver(OpponentPawns);
            Board.Move(Piece, From, To);
        }
    }
}