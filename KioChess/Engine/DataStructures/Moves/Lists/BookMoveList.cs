﻿using System.Runtime.CompilerServices;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists
{
    public class BookMoveList : MoveBaseList<MoveBase>
    {
        public BookMoveList() : base() { }

        public BookMoveList(int c) : base(c) { }

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
            for (byte i = 0; i < Count; i++)
            {
                history[i] = new MoveHistory { Key = _items[i].Key, History = _items[i].BookValue };
            }
        }
    }
}
