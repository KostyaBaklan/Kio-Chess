using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.DataStructures.Killers
{
    public class TriKillerMoves : IKillerMoveCollection
    {
        private int _index;
        private readonly short[] _moves;

        public TriKillerMoves()
        {
            _index = 0;
            _moves = new short[] { -1, -1, -1 };
        }

        #region Implementation of IKillerMoveCollection

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short move)
        {
            _moves[_index] = move;
            _index = (1 + _index)%3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(short move)
        {
            return _moves[0] == move || _moves[1] == move || _moves[2] == move;
        }

        #endregion
    }
}