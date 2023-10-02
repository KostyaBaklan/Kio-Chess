using System.Runtime.CompilerServices;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists;

public class AttackList : MoveBaseList<AttackBase>
{
    public AttackList() : base() { }

    public AttackList(int c) : base(c) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SortBySee()
    {
        byte count = Count;
        byte capturesCount = Sorting.Sort.SortAttackMinimum[count];

        for (byte i = 0; i < capturesCount; i++)
        {
            byte index = i;
            var max = _items[i];
            for (byte j = (byte)(i + 1); j < count; j++)
            {
                if (!_items[j].IsGreater(max))
                    continue;

                max = _items[j];
                index = j;
            }

            if (index == i) continue;

            _items[index] = _items[i];
            _items[i] = max;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(AttackList moves)
    {
        Array.Copy(moves._items, 0, _items, Count, moves.Count);
        Count += moves.Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(PromotionList moves)
    {
        Array.Copy(moves._items, 0, _items, Count, moves.Count);
        Count += moves.Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InsertByPiece(AttackBase move)
    {
        byte position = Count;
        _items[Count++] = move;

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
        _items[Count++] = move;

        byte parent = Parent(position);

        while (position > 0 && _items[position].IsGreater(_items[parent]))
        {
            Swap(position, parent);
            position = parent;
            parent = Parent(position);
        }
    }
}