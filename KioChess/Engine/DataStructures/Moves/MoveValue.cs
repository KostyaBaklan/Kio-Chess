using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves;

public struct MoveValue
{
    public short Key;
    public int Value;

    public MoveValue(short key) : this(key,0)
    {
        Key = key;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveValue(short key, int value)
    {
        Key = key;
        Value = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsGreater(MoveValue move)
    {
        return Value > move.Value;
    }

    public override string ToString()
    {
        return $"K={Key}, V={Value}";
    }
}