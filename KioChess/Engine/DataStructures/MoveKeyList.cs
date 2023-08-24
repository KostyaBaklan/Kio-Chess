using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public ref struct MoveKeyList
    {
        private readonly Span<short> _items;
        public byte Count; 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveKeyList(short[] items)
        {
            _items = items;
            Count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveKeyList(Span<short> items)
        {
            _items = items;
            Count = 0;
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
        internal void Sort()
        {
            _items.Order();
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal MoveKeyList Slice(byte i)
        //{
        //    MoveKeyList list = new MoveKeyList(_items.Slice(0,i));
        //    list.Count = Count;
        //    return list;
        //}
    }
}
