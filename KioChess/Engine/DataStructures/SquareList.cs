using System.Runtime.CompilerServices;

namespace Engine.DataStructures;

public  class SquareList
{
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
    public void Clear() => Length = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(byte square) => _squares[Length++] = square;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(byte[] items, byte count)
    {
        Array.Copy(items,_squares,count);
        Length = count;
    }
}
