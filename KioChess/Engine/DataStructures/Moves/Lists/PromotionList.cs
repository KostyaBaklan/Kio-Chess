using System.Runtime.CompilerServices;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists
{
    public class PromotionList : MoveBaseList<PromotionMove>
    {
        public PromotionList() : base(128)
        {
        }

        public PromotionList(int capacity) : base(capacity)
        {
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CopyTo(PromotionList moves, int index)
        {
            Array.Copy(_items, 0, moves._items, index, Count);
            moves.Count += Count;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Add(PromotionList moves)
        {
            Array.Copy(moves._items, 0, _items, Count, moves.Count);
            Count += moves.Count;
        }
    }
}