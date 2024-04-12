using System.Runtime.CompilerServices;

namespace Engine.DataStructures;

public  class SquareList
{
    private static byte _zero = 0;
    private readonly byte[] _squares;

    public SquareList()
    {
        _squares = new byte[10];
    }

    public byte Length;

    public byte this[byte i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return _squares[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Length = _zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(byte square) => _squares[Length++] = square;
}
