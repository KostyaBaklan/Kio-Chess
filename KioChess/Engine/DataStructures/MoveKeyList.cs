using Engine.Models.Helpers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures;

public ref struct MoveKeyList
{
    public readonly Span<short> _items;
    public byte Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveKeyList(short[] items)
    {
        _items = items;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveKeyList(Span<short> items)
    {
        _items = items;
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
    internal void Add(Span<short> sequence)
    {
        sequence.CopyTo(_items);
        Count = (byte)sequence.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(short square) => _items[Count++] = square;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Order() => SubSet().Order();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<short> SubSet()
    {
        unsafe
        {
            return new Span<short>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(_items)), Count);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string AsKey() => SubSet().Join('-');

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string AsStringKey() => new string(AsChars());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal byte[] AsByteKey() => AsBytes().ToArray();

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
