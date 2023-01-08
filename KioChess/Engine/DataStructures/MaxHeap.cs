using System.Runtime.CompilerServices;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures
{
    public class MaxHeap
    {
        private MoveBase[] _elements;
        private int _size;
        private readonly IMoveComparer _comparer;

        public MaxHeap() : this(128, new HistoryComparer())
        {
        }

        public MaxHeap(int size, IMoveComparer comparer)
        {
            _comparer = comparer;
            _elements = new MoveBase[size];
        }

        public MoveBase this[int index] => _elements[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Parent(int i)
        {
            return (i - 1) / 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Left(int i)
        {
            return i * 2 + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int i, int j)
        {
            var t = _elements[i];
            _elements[i] = _elements[j];
            _elements[j] = t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Heapify(int index)
        {
            var left = Left(index);
            if (left >= _size) return;

            var largest = index;
            if (_comparer.Compare(_elements[left], _elements[largest]) < 0)
            {
                largest = left;
            }

            var right = left + 1;
            if (right >= _size) return;

            if (_comparer.Compare(_elements[right], _elements[largest]) < 0)
            {
                largest = right;
            }

            if (largest == index) return;

            Swap(index, largest);
            Heapify(largest);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCount()
        {
            return _size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(MoveBase element)
        {
            if (_elements.Length == _size)
            {
                MoveBase[] elements = new MoveBase[_size + _size];
                Array.Copy(_elements, elements, _size);
                _elements = elements;
            }

            _elements[_size] = element;
            var i = _size++;
            var parent = Parent(i);
            while (i > 0 && _comparer.Compare(_elements[i], _elements[parent]) < 0)
            {
                Swap(i, parent);
                i = parent;
                parent = Parent(parent);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveBase Maximum()
        {
            _size--;
            Swap(0, _size);
            Heapify(0);
            return _elements[_size];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveBase[] GetOrderedItems()
        {
            var elements = new MoveBase[_size];
            if (_size <= 0) return elements;

            if (_size < 6)
            {
                for (var i = 0; i < elements.Length - 1; i++)
                {
                    elements[i] = Maximum();
                }

                elements[elements.Length - 1] = _elements[0];
            }
            else
            {
                var count = elements.Length / 3;
                for (var i = 0; i < count; i++)
                {
                    elements[i] = Maximum();
                }

                for (var i = count; i < elements.Length; i++)
                {
                    elements[i] = _elements[i - count];
                }
            }

            return elements;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _size = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetItems()
        {
            var count = _size;
            if (count <= 0) yield break;

            for (var index = 0; index < count; index++)
            {
                yield return _elements[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetItems(List<MoveBase> moves)
        {
            var count = _size;
            if (count <= 0) return;

            for (var index = 0; index < count; index++)
            {
                moves.Add(_elements[index]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetOrderedItems(List<MoveBase> moves)
        {
            if (_size <= 0) return;

            if (_size > 1)
            {
                var count = _size;
                int last = count < 6 ? count / 2 : count / 3;
                for (var i = 0; i < last; i++)
                {
                    moves.Add(Maximum());
                }

                count = count - last;
                for (var i = 0; i < count; i++)
                {
                    moves.Add(_elements[i]);
                }
            }

            else
            {
                moves.Add(_elements[0]); 
            }
        }
    }
}
