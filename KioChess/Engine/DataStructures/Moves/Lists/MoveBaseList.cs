using System.Collections;
using System.Runtime.CompilerServices;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists
{

    public abstract class MoveBaseList<T> : IEnumerable<T> where T : MoveBase
    {
        public readonly T[] _items;

        protected MoveBaseList() : this(128)
        {
        }

        public T this[byte i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _items[i];
            }
        }

        #region Implementation of IReadOnlyCollection<out IMove>

        public byte Count;

        protected MoveBaseList(int capacity)
        {
            _items = new T[capacity];
        }

        #endregion


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T move)
        {
            _items[Count++] = move;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected byte Left(byte i)
        {
            return (byte)(2 * i + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected byte Parent(byte i)
        {
            return (byte)((i - 1) / 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Swap(byte i, byte j)
        {
            var temp = _items[j];
            _items[j] = _items[i];
            _items[i] = temp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _items[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasPv(short pv)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_items[i].Key == pv) return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return $"Count={Count}";
        }
    }
}