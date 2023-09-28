using System.Runtime.CompilerServices;

namespace Engine.Book.Models
{
    public struct PopularMoves
    {
        private short _move1;
        private short _move2;
        private short _move3;

        public PopularMoves()
        {
            _move1 = -1;
            _move2 = -1;
            _move3 = -1;
        }

        public PopularMoves(short move1, short move2, short move3)
        {
            _move1 = move1;
            _move2 = move2;
            _move3 = move3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(short move)
        {
            return _move1 == move || _move2 == move || _move3 == move;
        }

        internal bool IsValid()
        {
            return _move1 != -1 || _move2 != -1 || _move3 != -1;
        }
    }
}
