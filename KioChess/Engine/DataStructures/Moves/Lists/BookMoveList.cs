using System.Runtime.CompilerServices;
using Engine.Models.Moves;

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
                _items[j + 1] = _items[j];
                j--;
            }
            _items[j + 1] = key;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(MoveBase move)
    {
        byte position = Count;
        _items[Count++] = move;

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
