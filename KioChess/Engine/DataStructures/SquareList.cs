using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public  class SquareList
    {
        private readonly byte[] _squares;

        public SquareList()
        {
            _squares = new byte[10];
        }

        public int Length;

        public byte this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _squares[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { Length = 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Add(byte square)
        {
            _squares[Length++] = square;
        }
    }
}
