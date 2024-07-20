using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves;

public struct MoveHistory
{
    public short Key;
    public int History;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsGreater(MoveHistory move) => History > move.History;
}