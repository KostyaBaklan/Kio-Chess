using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.DataStructures.Killers
{
    public class DoKillerMoves : IKillerMoveCollection
    {
        private int _index;
        private readonly short[] _moves;

        public DoKillerMoves()
        {
            _index = 0;
            _moves = new short[]{-1,-1};
        }

        #region Implementation of IKillerMoveCollection

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short move)
        {
            _moves[_index] = move;
            _index = 1 - _index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(short move)
        {
            return _moves[0] == move || _moves[1] == move;
        }

        #endregion
    }
}