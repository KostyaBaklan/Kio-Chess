using System.Collections;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public class ZoobristKeyCollection : ICollection<ulong>
    {
        private ulong[] _keys;

        public ZoobristKeyCollection():this(2)
        {
        }

        public ZoobristKeyCollection(int capacity)
        {
            _keys = new ulong[capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ulong item)
        {
            if (Count == _keys.Length)
            {
                ulong[] keys = new ulong[Count + Count];
                Array.Copy(_keys, keys, Count);
                _keys = keys;
            }

            _keys[Count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(_keys,0,Count);
            Count = 0;
            _keys = new ulong[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<ulong> GetAndClear()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _keys[i];
            }
            Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ulong item)
        {
            return Array.IndexOf(_keys, item) > 0;
        }

        public void CopyTo(ulong[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(ulong item)
        {
            for (var i = 0; i < Count - 1; i++)
            {
                if (item == _keys[i])
                {
                    _keys[i] = _keys[--Count];
                    return true;
                }
            }

            if (item == _keys[Count - 1])
            {
                Count--;
                return true;
            }

            return false;
        }

        public int Count { get; private set; }
        public bool IsReadOnly => false;

        #region Implementation of IEnumerable

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<ulong> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _keys[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return $"Count={Count}";
        }

        #endregion
    }
}