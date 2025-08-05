using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures.Moves.Lists;

public class BookMoveList : MoveBaseList<MoveBase>
{
    public BookMoveList() : base() { }

    public BookMoveList(int c) : base(c) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FullSort()
    {
        for (byte i = 1; i < Count; i++)
        {
            var key = _items[i];
            int j = i - 1;

            while (j > -1 && key.IsBookGreater(_items[j]))
            {
                // Unsafe optimized assignment - eliminates StelemRef_Helper overhead
                ref MoveBase itemsRef = ref MemoryMarshal.GetArrayDataReference(_items);
                Unsafe.Add(ref itemsRef, j + 1) = Unsafe.Add(ref itemsRef, j);
                j--;
            }
            // Unsafe optimized assignment - eliminates StelemRef_Helper overhead
            ref MoveBase slot = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), j + 1);
            slot = key;
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

        while (position > 0 && _items[position].IsBookGreater(_items[parent]))
        {
            Swap(position, parent);
            position = parent;
            parent = Parent(position);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Fill(Span<MoveHistory> history)
    {
        for (byte i = Zero; i < Count; i++)
        {
            history[i] = _items[i].ToMoveHistory();
        }
    }
}
