using System.Collections;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public class DynamicCollection<T> : ICollection<T>
    {
        private Node<T> _root;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            _root = new Node<T>(item) { Next = _root };
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var root = _root;
            _root = null;
            Count = 0;

            var next = root.Next;
            while (root != null)
            {
                root.Next = null;
                root = next;
                next = next.Next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAndClear()
        {
            var root = _root;
            if (root == null) yield break;
            _root = null;
            Count = 0;

            var next = root.Next;
            while (root != null)
            {
                yield return root.Value;
                root.Next = null;
                root = next;
                next = next.Next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            var current = _root;
            while (current != null)
            {
                if (current.Value.Equals(item)) return true;
                current = current.Next;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            Node<T> previous = null;
            var current = _root;
            while (current != null)
            {
                if (current.Value.Equals(item))
                {
                    if (previous == null)
                    {
                        _root = current.Next;
                    }
                    else
                    {
                        previous.Next = current.Next;
                    }
                    Count--;
                    return true;
                }

                previous = current;
                current = current.Next;
            }

            return false;
        }

        public int Count { get; private set; }
        public bool IsReadOnly => false;

        #region Implementation of IEnumerable

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
        {
            var current = _root;
            while (current != null)
            {
                yield return current.Value;
                current = current.Next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}