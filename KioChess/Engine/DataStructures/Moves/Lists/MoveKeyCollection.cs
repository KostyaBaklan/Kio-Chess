using System.Runtime.CompilerServices;
using System.Text;
using Engine.Models.Helpers;

namespace Engine.DataStructures.Moves.Lists;

public class MoveKeyCollection
{
    private byte _count;
    private short[] _items;

    public MoveKeyCollection(int size) 
    { 
        _items = new short[size];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(short item) => _items[_count++] = item;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => _count = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sort() => _items.AsSpan(0, _count).Order();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string AsKey()
    {
        StringBuilder builder = new StringBuilder();

        byte last = (byte)(_count - 1);
        for (byte i = 0; i < last; i++)
        {
            builder.Append($"{_items[i]}-");
        }

        builder.Append(_items[last]);

        return builder.ToString();
    }

    internal void Remove()
    {
    }
}
