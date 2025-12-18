using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures.Moves.Lists;

public class AttackList : MoveBaseList<AttackBase>
{
    public AttackList() : base() { }

    public AttackList(int c) : base(c) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SortBySee()
    {
        if (Count < 2) return;

        AsSpan().InsertionSort();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(PromotionList moves, int see)
    {
        var items = moves.AsSpan();
        for (int i = 0; i < items.Length; i++)
        {
            items[i].See = see;
            Add(items[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(PromotionAttackList moves, int see)
    {
        var items = moves.AsSpan();
        for (int i = 0; i < items.Length; i++)
        {
            items[i].See = see;
            Add(items[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InsertByPiece(AttackBase move)
    {
        byte position = Count;

        // Unsafe optimized assignment - eliminates StelemRef_Helper overhead
        ref AttackBase slot = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), Count);
        slot = move;
        Count++;

        byte parent = Parent(position);

        while (position > 0 && _items[position].IsLess(_items[parent]))
        {
            Swap(position, parent);
            position = parent;
            parent = Parent(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(AttackBase move)
    {
        byte position = Count;

        // Unsafe optimized assignment - eliminates StelemRef_Helper overhead
        ref AttackBase slot = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), Count);
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

    /// <summary>
    /// Fast MVV-LVA (Most Valuable Victim - Least Valuable Attacker) sorting.
    /// Prioritizes capturing high-value pieces with low-value attackers.
    /// Uses hybrid approach: insertion sort for small counts, counting sort for larger counts.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void MvvLvaSort()
    {
        if (Count < 2) return;

        // MVV-LVA score: victim_value * 12 - attacker_value
        // Multiply by 12 (max piece index + 1) to ensure no overlap
        // Higher scores = better captures (capture valuable pieces with cheap attackers)

        for (byte i = 1; i < Count; i++)
        {
            var key = _items[i];
            int keyScore = key.Captured * 12 - key.Piece;
            int j = i - 1;

            // Move elements with lower MVV-LVA scores to the right
            while (j >= 0)
            {
                var current = _items[j];
                if (current.Captured * 12 - current.Piece >= keyScore)
                    break;

                _items[j + 1] = current;
                j--;
            }

            _items[j + 1] = key;
        }
    }
}