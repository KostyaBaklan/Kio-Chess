using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class PawnOverMove : MoveBase
{
    public BitBoard OpponentPawns;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard);
}