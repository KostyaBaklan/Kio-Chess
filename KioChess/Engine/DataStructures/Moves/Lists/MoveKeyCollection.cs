using System.Runtime.CompilerServices;
using Engine.Models.Helpers;

namespace Engine.DataStructures.Moves.Lists
{
    public class MoveKeyCollection
    {
        private byte _count;
        private short[] _items;

        public MoveKeyCollection(int size) 
        { 
            _items = new short[size];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short item)
        {
            _items[_count++] = item;
        }

        public void Clear() { _count = 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
        {
            _items.AsSpan(0,_count).Order();
        }

        internal void Remove()
        {
        }
    }
}
