using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public ref struct BitBoardList
{
    private readonly Span<BitBoard> _items;
    public byte Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoardList(BitBoard[] items)
    {
        _items = items;
        Count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoardList(Span<BitBoard> items)
    {
        _items = items;
        Count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BitBoardList(BitBoard[] array) => new BitBoardList(array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BitBoardList(Span<BitBoard> array) => new BitBoardList(array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(BitBoard bit) => _items[Count++] = bit;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int GetKingZoneWeight(int value)
    {
        for (int i = 0; i < Count - 1; i++)
        {
            for (int j = i + 1; j <Count; j++)
            {
                if ((_items[i] & _items[j]).Any())
                    value += 5;
            }
        }
        return value;
    }
}
