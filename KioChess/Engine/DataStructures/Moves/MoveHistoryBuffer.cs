using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves;

[InlineArray(128)]
public struct MoveHistoryBuffer
{
    public MoveHistory Moves;
}
