using Engine.Interfaces.Config;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures.Moves.Lists;

public class MoveList : MoveBaseList<MoveBase>
{
    public MoveList() : base() { }

    public MoveList(int c) : base(c) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(AttackList moves) => AddSpan(moves.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(MoveList moves) => AddSpan(moves.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(PromotionList moves) => AddSpan(moves.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(PromotionAttackList moves) => AddSpan(moves.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(BookMoveList moves) => AddSpan(moves.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddSpan<T>(Span<T> items) where T : MoveBase
    {
        for (int i = 0; i < items.Length; i++)
        {
            Add(items[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase ExtractMax()
    {
        int index = 0;
        var max = _items[0];
        for (int j = 1; j < Count; j++)
        {
            if (!_items[j].IsGreater(max))
                continue;

            max = _items[j];
            index = j;
        }

        // Unsafe optimized assignment - eliminates StelemRef_Helper overhead
        ref MoveBase itemsRef = ref MemoryMarshal.GetArrayDataReference(_items);
        Unsafe.Add(ref itemsRef, index) = Unsafe.Add(ref itemsRef, --Count);
        return max;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Fill(Span<MoveHistory> history)
    {
        var items = _items.AsSpan();
        for (byte i = Zero; i < Count; i++)
            history[i] = items[i].ToMoveHistory();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SortAndCopy(MoveList moveList)
    {
        if (moveList.Count < 2)
        {
            Add(moveList[0]);
        }
        else
        {
            Span<MoveHistory> history = stackalloc MoveHistory[moveList.Count];

            moveList.Fill(history);

            history.InsertionSort();

            var moves = Moves.AsSpan();

            for (byte i = Zero; i < history.Length; i++)
            {
                Add(moves[history[i].Key]);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(MoveBase move)
    {
        byte position = Count;

        // Unsafe optimized assignment - eliminates StelemRef_Helper overhead
        ref MoveBase slot = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), Count);
        slot = move;
        Count++;

        byte parent = Parent(position);

        while (position > 0 && _items[position].IsGreater(_items[parent]))
        {
            Swap(position, parent);
            position = parent;
            parent = Parent(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase Maximum()
    {
        var max = _items[0];

        // Unsafe optimized assignment - eliminates StelemRef_Helper overhead
        ref MoveBase itemsRef = ref MemoryMarshal.GetArrayDataReference(_items);
        itemsRef = Unsafe.Add(ref itemsRef, --Count);

        MoveDown(0);
        return max;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveDown(byte i)
    {
        byte left = Left(i);
        byte largest = i;

        if (left < Count && _items[left].IsGreater(_items[largest]))
            largest = left;

        if (++left < Count && _items[left].IsGreater(_items[largest]))
            largest = left;

        if (i == largest)
            return;

        Swap(i, largest);
        MoveDown(largest);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Heapify(byte i)
    {
        byte left;

        do
        {
            byte largest = i;
            left = Left(i);

            if (left < Count && _items[left].IsGreater(_items[largest]))
                largest = left;

            if (++left < Count && _items[left].IsGreater(_items[largest]))
                largest = left;

            if (i < largest)
            {
                Swap(i, largest);
                i = largest;
            }
            else break;

        } while (i < Count);
    }
}
