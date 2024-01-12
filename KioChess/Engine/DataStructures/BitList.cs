using System.Runtime.CompilerServices;

namespace Engine.DataStructures;

public ref struct BitList
{
    private readonly Span<byte> _items;
    public byte Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitList(byte[] items)
    {
        _items = items;
        Count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitList(Span<byte> items)
    {
        _items = items;
        Count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BitList(byte[] array) => new BitList(array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BitList(Span<byte> array) => new BitList(array);

    public byte this[byte i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return _items[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Count = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(byte square) => _items[Count++] = square;
}
