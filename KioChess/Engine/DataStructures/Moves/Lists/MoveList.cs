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

                var temp = _items[index];
                _items[index] = _items[i];
                _items[i] = temp;
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
        public void InsertionSort()
        {
            for (int i = 1; i < Count; ++i)
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

                var temp = _items[index];
                _items[index] = _items[i];
                _items[i] = temp;
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

                var t = _items[left];
                _items[left] = _items[right];
                _items[right] = t;
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

            var temp = _items[index];
            _items[index] = _items[i];
            _items[i] = temp;
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
        private MoveBase ExtractMax()
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
    }
}
