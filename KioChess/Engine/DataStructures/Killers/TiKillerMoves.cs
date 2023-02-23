using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.DataStructures.Killers
{
    public class TiKillerMoves : IKillerMoveCollection
    {
        private long _index;
        private readonly bool[] _moves;

        public TiKillerMoves(int capacity)
        {
            _index = 0;
            _moves = new bool[capacity];
        }

        #region Implementation of IKillerMoveCollection

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short move)
        {
            if (_moves[move])
                return;

            _index = (long)move | (_index << 16);
            _moves[_index >> 48] = false;
            _moves[move] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(short move)
        {
            return _moves[move];
        }

        #endregion
    }
}