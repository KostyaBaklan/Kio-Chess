using Engine.Models.Boards.Buffers;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures;

public record AttackStack
{
    private byte _count;
    private AttackBuffer _items;

    public AttackStack()
    {
        _count = 0;
        _items = new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(byte item) => _items[_count++] = item;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Pop() => _items[--_count];
}
