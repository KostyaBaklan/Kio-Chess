using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves;

public struct MoveHistory
{
    public short Key;
    public int History;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsGreater(MoveHistory move)
    {
        return History > move.History;
    }
}