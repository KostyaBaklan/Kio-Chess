using System.Runtime.CompilerServices;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists
{
    public class AttackList : MoveBaseList<AttackBase>
    {
        public AttackList() : base() { }

        public AttackList(int c) : base(c) { }

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
        internal void Fill(Span<AttackSee> history)
        {
            for (byte i = 0; i < Count; i++)
            {
                history[i] = new AttackSee { Key = _items[i].Key, See = _items[i].See };
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
}