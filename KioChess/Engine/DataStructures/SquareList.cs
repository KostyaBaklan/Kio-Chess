using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public  class SquareList
    {
        private readonly Square[] _squares;

        public SquareList()
        {
            _squares = new Square[10];
        }

        public int Length;

        public Square this[int i]
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
        internal void Add(Square square)
        {
            _squares[Length++] = square;
        }
    }
}
