using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public class PositionsList
    {
        private readonly byte[] _items;
        public int Count;

        public PositionsList()
        {
            _items = new byte[64];
        }

        public byte this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _items[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(byte x)
        {
            _items[Count++] = x;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Count = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(byte[] items, int index)
        {
            Array.Copy(_items, 0, items, index, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(PositionsList positions)
        {
            Array.Copy(positions._items, 0, _items, Count, positions.Count);
            Count += positions.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return $"Count={Count}";
        }
    }
}
