using System;
using System.Runtime.CompilerServices;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

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

                Swap(i, index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HeapSort()
        {
            var capturesCount = Sorting.Sort.SortMinimum[Count];

            MoveList list = new MoveList(Count);
            list.Add(this);

            for (var i = 0; i < capturesCount; i++)
            {
                _items[i] = list.Maximum();
            }
            for(int i = capturesCount;i < Count; i++)
            {
                _items[i] = list._items[i-capturesCount];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FullSort()
        {
            for (int i = 1; i < Count; i++)
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
        public void Insert(MoveBase move)
        {
            int position = Count;
            _items[Count++] = move;

            int parent = Parent(position);

            while (position > 0 && _items[position].IsGreater(_items[parent]))
            {
                Swap(position, parent);
                position = parent;
                parent = Parent(position);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ArraySort()
        {
            Array.Sort(_items, 0, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(MoveList moves)
        {
            Array.Copy(moves._items, 0, _items, Count, moves.Count);
            Count += moves.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FullSort(IMoveComparer differenceComparer)
        {
            Array.Sort(_items, 0, Count, differenceComparer);
        }

        public void DifferenceSort()
        {
            var count = Count;
            if (count < 3) return;

            var comparer = Sorting.Sort.HistoryComparer;

            var capturesCount = 3;

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

                Swap(i, index);
            }

            if (count < 10) return;

            int left = capturesCount, right = Count - 1;

            while (left < right)
            {
                while (_items[left].Difference > 0 && left < right)
                    left++;

                while (_items[right].Difference <= 0 && left < right)
                    right--;

                if (left >= right) continue;


                Swap(left, right);

                left++;
                right--;
            }

            Maximum(capturesCount, right, comparer);

            Maximum(left, count, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Maximum(int i, int right, IMoveComparer comparer)
        {
            int index = i;
            var min = _items[i];
            for (int j = i + 1; j < right; j++)
            {
                if (comparer.Compare(min, _items[j]) < 0) continue;

                min = _items[j];
                index = j;
            }

            if (index == i) return;

            Swap(i, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExtractMax(int count, MoveList suggested)
        {
            for (int i = 0; i < count; i++)
            {
                MoveBase max = ExtractMax();
                suggested.Add(max);
            }
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
        public MoveBase Maximum()
        {
            var max = _items[0];
            _items[0] = _items[--Count];
            MoveDown(0);
            return max;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveDown(int i)
        {
            int left = Left(i);
            int right = left + 1;
            int largest = i;

            if(left < Count && _items[left].IsGreater(_items[largest]))
            {
                largest = left;
            }
            if (right < Count && _items[right].IsGreater(_items[largest]))
            {
                largest = right;
            }

            if (i != largest)
            {
                Swap(i, largest);
                MoveDown(largest);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Left(int i)
        {
            return 2*i+1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Parent(int i)
        {
            return (i - 1) / 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int i, int j)
        {
            var temp = _items[j];
            _items[j] = _items[i];
            _items[i] = temp;
        }
    }
}
