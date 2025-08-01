using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures.Moves;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct MoveHistory:IComparable<MoveHistory>
{
    public readonly short Key;
    public readonly int History;

    public MoveHistory(short key, int history)
    {
        Key = key;
        History = history;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(MoveHistory other) => other.History.CompareTo(History);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsGreater(MoveHistory move) => History > move.History;

    public override string ToString() => $"{Key} {History}";
}