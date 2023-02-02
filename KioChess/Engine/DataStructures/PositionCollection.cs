using System.Runtime.CompilerServices;
using System.Text;
using Engine.Models.Boards;
using Engine.Models.Helpers;

namespace Engine.DataStructures
{
    public class PositionCollection
    {
        private readonly byte[] _index;
        private readonly byte[] _items;
        public byte Count;

        public PositionCollection()
        {
            Count = 0;
            _index = new byte[64];
            _items = new byte[64];
        }

        public byte this[int i] => _items[i];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(byte position)
        {
            _items[Count] = position;
            _index[position] = Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(byte position)
        {
            var index = _index[position];
            --Count;
            if (Count == index) return;

            _items[index] = _items[Count];
            _index[_items[Count]] = index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Replace(byte from, byte to)
        {
            var index = _index[from];
            _index[to] = index;
            _items[index] = to;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square[] ToSquares()
        {
            Square[] squares = new Square[Count];
            for (var i = 0; i < Count; i++)
            {
                squares[i] = new Square(_items[i]);
            }

            return squares;
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (var i = 0; i < Count; i++)
            {
                builder.Append(new Square(_items[i]).AsString()).Append(' ');
            }

            return builder.ToString();
        }

        #endregion
    }
}