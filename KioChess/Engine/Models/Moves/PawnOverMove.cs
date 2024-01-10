using System.Runtime.CompilerServices;
using Engine.Models.Boards;

namespace Engine.Models.Moves;

public abstract  class PawnOverMove : MoveBase
{
    public BitBoard OpponentPawns;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        IsEnPassant = false;
        Board.Move(Piece, To, From);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard);
}