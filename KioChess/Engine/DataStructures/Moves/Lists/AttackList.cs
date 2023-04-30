using System.Runtime.CompilerServices;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Lists
{
    public class AttackList : MoveBaseList<AttackBase>
    {
        public AttackList() : base() { }

        public AttackList(int c) : base(c) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(IMoveComparer comparer)
        {
            var count = Count;
            if (count <= 1) return;

            var capturesCount = count < 6 ? count / 2 : Math.Min(count / 3, 10);

            for (var i = 0; i < capturesCount; i++)
            {
                int index = i;
                var min = _items[i];
                for (int j = i + 1; j < count; j++)
                {
                    if (comparer.Compare(min, _items[j]) < 0) continue;

                    min = _items[j];
                    index = j;
                }

                if (index == i) continue;

                var temp = _items[index];
                _items[index] = _items[i];
                _items[i] = temp;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SortBySee()
        {
            var capturesCount = Count / 2;

            for (var i = 0; i < capturesCount; i++)
            {
                int index = i;
                var max = _items[i].See;
                for (int j = i + 1; j < Count; j++)
                {
                    if (_items[j].See <= max) continue;
                    max = _items[j].See;
                    index = j;
                }

                if (index == i) continue;

                var temp = _items[index];
                _items[index] = _items[i];
                _items[i] = temp;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(AttackList moves)
        {
            Array.Copy(moves._items, 0, _items, Count, moves.Count);
            Count += moves.Count;
        }

        internal void Add(PromotionAttackList moves, int attackValue)
        {
            for (byte i = 0; i < moves.Count; i++)
            {
                moves[i].See = attackValue;
                Add(moves[i]);
            }
        }
    }
}