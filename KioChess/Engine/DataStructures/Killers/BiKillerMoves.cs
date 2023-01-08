using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.DataStructures.Killers
{
    public class BiKillerMoves : IKillerMoveCollection
    {
        private uint _index;
        private readonly bool[] _moves;

        public BiKillerMoves(int capacity)
        {
            _index = 0;
            _moves = new bool[capacity];
        }

        #region Implementation of IKillerMoveCollection

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short move)
        {
            _moves[_index >> 16] = false;
            _moves[move] = true;
            _index = (uint) move | (_index << 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(short move)
        {
            return _moves[move];
        }

        #endregion
    }
}