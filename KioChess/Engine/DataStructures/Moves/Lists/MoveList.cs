using System.Runtime.CompilerServices;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists
{

    public class MoveList : MoveBaseList<MoveBase>
    {
        public MoveList() : base() { }

        public MoveList(int c) : base(c) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
        {
            var count = Count;
            var capturesCount = Sorting.Sort.SortMinimum[count];

            for (var i = 0; i < capturesCount; i++)
            {
                int index = i;
                var max = _items[i];
                for (int j = i + 1; j < count; j++)
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
        public void FullSort()
        {
            for (byte i = 1; i < Count; i++)
            {
                var key = _items[i];
                int j = i - 1;

                while (j > -1 && key.IsGreater(_items[j]))
                {
                    _items[j + 1] = _items[j];
                    j--;
                }
                _items[j + 1] = key;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(AttackList moves)
        {
            Array.Copy(moves._items, 0, _items, Count, moves.Count);
            Count += moves.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(MoveList moves)
        {
            Array.Copy(moves._items, 0, _items, Count, moves.Count);
            Count += moves.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(PromotionList moves)
        {
            Array.Copy(moves._items, 0, _items, Count, moves.Count);
            Count += moves.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(PromotionAttackList moves)
        {
            Array.Copy(moves._items, 0, _items, Count, moves.Count);
            Count += moves.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(BookMoveList moves)
        {
            Array.Copy(moves._items, 0, _items, Count, moves.Count);
            Count += moves.Count;
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

            _items[index] = _items[--Count];
            return max;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fill(Span<MoveHistory> history)
        {
            for (byte i = 0; i < Count; i++)
            {
                history[i] = new MoveHistory { Key = _items[i].Key, History = _items[i].History };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SortAndCopy(MoveBase[] moves)
        {
            Span<MoveHistory> history = stackalloc MoveHistory[Count];

            Fill(history);

            history.InsertionSort();

            for (byte i = 0; i < history.Length; i++)
            {
                _items[i] = moves[history[i].Key];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SortAndCopy(MoveList moveList, MoveBase[] moves)
        {
            Span<MoveHistory> history = stackalloc MoveHistory[moveList.Count];

            moveList.Fill(history);

            history.InsertionSort();

            for (int i = 0; i < history.Length; i++)
            {
                Add(moves[history[i].Key]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SortAndCopy(BookMoveList moveList, MoveBase[] moves)
        {
            Span<MoveHistory> history = stackalloc MoveHistory[moveList.Count];

            moveList.Fill(history);

            history.InsertionSort();

            for (int i = 0; i < history.Length; i++)
            {
                Add(moves[history[i].Key]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(MoveBase move)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveBase Maximum()
        {
            var max = _items[0];
            _items[0] = _items[--Count];
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
}
