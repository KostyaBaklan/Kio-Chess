using System.Runtime.CompilerServices;
using Engine.Models.Helpers;
using Engine.Models.Moves;

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
    internal void Add(PromotionList moves)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            Add(moves._items[i]);
        }
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