using Engine.Models.Helpers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.DataStructures
{
    public ref struct MoveKeyList
    {
        private readonly Span<short> _items;
        public byte Count;
        public byte Size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveKeyList(short[] items)
        {
            _items = items;
            Count = 0;
            Size = (byte)items.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveKeyList(Span<short> items)
        {
            _items = items;
            Count = 0;
            Size = (byte)items.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MoveKeyList(short[] array) => new MoveKeyList(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MoveKeyList(Span<short> array) => new MoveKeyList(array);

        public short this[byte i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _items[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { Count = 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Add(short square)
        {
            _items[Count++] = square;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Order()
        {
            if (Count > 1)
            {
                _items.Slice(0, Count).Order(); 
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string AsKey()
        {
            StringBuilder builder = new StringBuilder();

            byte last = (byte)(Count - 1);
            for (byte i = 0; i < last; i++)
            {
                builder.Append($"{_items[i]}-");
            }

            builder.Append(_items[last]);

            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string AsStringKey()
        {
            return Encoding.Unicode.GetString(AsByteKey());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal byte[] AsByteKey()
        {
            byte[] data = new byte[2*Count];

            Buffer.BlockCopy(_items.Slice(0, Count).ToArray(), 0, data, 0, data.Length);

            return data;
        }
    }
}
