using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Hash
{
    public class DynamicHash<T>: IDynamicHash<T>
    {
        private struct HashEntry<T>: IEquatable<HashEntry<T>>
        {
            public readonly ulong Key;
            public readonly T Item;

            public HashEntry(ulong key, T item)
            {
                Key = key;
                Item = item;
            }

            public HashEntry(ulong key)
            {
                Key = key;
                Item = default(T);
            }

            #region Overrides of ValueType

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj)
            {
                return obj is HashEntry<T> other && Equals(other);
            }

            #region Equality members

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(HashEntry<T> other)
            {
                return Key == other.Key;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(HashEntry<T> left, HashEntry<T> right)
            {
                return left.Equals(right);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(HashEntry<T> left, HashEntry<T> right)
            {
                return !left.Equals(right);
            }

            #endregion

            #endregion
        }

        private readonly ulong _capacity;
        private readonly DynamicCollection<HashEntry<T>>[] _set;

        public DynamicHash(int capacity)
        {
            _capacity = (ulong) capacity;

            _set = new DynamicCollection<HashEntry<T>>[_capacity];
            for (ulong i = 0; i < _capacity; i++)
            {
                _set[i] = new DynamicCollection<HashEntry<T>>();
            }
        }

        #region Implementation of IDynamicHash<T>

        public int Count { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ulong key, T item)
        {
            //var dynamicCollection = _set[key % _capacity];
            //foreach (var hashEntry in dynamicCollection)
            //{
            //    if (hashEntry.Key == key) return;
            //}

            _set[key % _capacity].Add(new HashEntry<T>(key,item));
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(ulong key)
        {
            foreach (var hashEntry in _set[key % _capacity])
            {
                if (hashEntry.Key == key) return hashEntry.Item;
            }

            return default(T);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(ulong key, out T item)
        {
            foreach (var hashEntry in _set[key % _capacity])
            {
                if (hashEntry.Key == key)
                {
                    item = hashEntry.Item;
                    return true;
                }
            }

            item = default(T);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(ulong key)
        {
            if (!_set[key % _capacity].Remove(new HashEntry<T>(key))) return false;

            Count--;
            return true;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (ulong i = 0; i < _capacity; i++)
            {
                _set[i] = new DynamicCollection<HashEntry<T>>();
            }
        }

        #endregion
    }
}