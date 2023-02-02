using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Models.Moves;

namespace Engine.DataStructures
{
    public class PriorityQueue
    {
        private int _size;
        private MoveWrapper[] _elements;

        public PriorityQueue() : this( 128) { }

        public PriorityQueue( int size)
        {
            _elements = new MoveWrapper[size];
        }

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
        private void BubbleUp(int i, int newKey)
        {
            var parent = Parent(i);
            while (i > 0 && newKey.CompareTo(_elements[parent].Value) > 0)
            {
                Swap(i, parent);
                i = parent;
                parent = Parent(parent);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Heapify(int index)
        {
            var left = Left(index);
            if (left >= _size) return;

            var largest = index;
            if (_elements[left].CompareTo(_elements[largest]) > 0)
            {
                largest = left;
            }

            var right = left + 1;
            if (right >= _size) return;

            if (_elements[right].CompareTo(_elements[largest]) > 0)
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
        public void Insert(MoveWrapper element)
        {
            if (_elements.Length == _size)
            {
                MoveWrapper[] elements = new MoveWrapper[_size + _size];
                Array.Copy(_elements, elements, _size);
                _elements = elements;
            }

            _elements[_size] = element;
            var i = _size++;
            var parent = Parent(i);
            while (i > 0 && element.Value >  _elements[parent].Value)
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
            return _elements[_size].Move;
        }

        public void Clear()
        {
            _size = 0;
        }

        public MoveBase this[int index]
        {
            get { return _elements[index].Move; }
        }
    }
}