using Engine.Models.Helpers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures;

public ref struct MoveKeyList
{
    private readonly Span<short> _items;
    public byte Count;
    public byte Size;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveKeyList(short[] items)
    {
        _items = items;
        Count = 0;
        Size = (byte)items.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveKeyList(Span<short> items)
    {
        _items = items;
        Count = 0;
        Size = (byte)items.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MoveKeyList(short[] array) => new MoveKeyList(array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MoveKeyList(Span<short> array) => new MoveKeyList(array);

    public short this[byte i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return _items[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() { Count = 0; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(Span<short> sequence)
    {
        sequence.CopyTo(_items);
        Count = (byte)sequence.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(short square)
    {
        _items[Count++] = square;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Order()
    {
        if (Count > 1)
        {
            _items.Slice(0, Count).Order(); 
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string AsKey()
    {
        return _items.Slice(0, Count).Join('-');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string AsStringKey()
    {
        return new string(AsChars());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal byte[] AsByteKey()
    {
        return AsBytes().ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<char> AsChars()
    {
        unsafe
        {
            return new Span<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(_items)), Count);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<byte> AsBytes()
    {
        unsafe
        {
            return new Span<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(_items)), 2 * Count);
        }
    }
}
