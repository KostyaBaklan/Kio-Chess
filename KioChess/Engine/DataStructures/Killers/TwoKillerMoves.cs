using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.DataStructures.Killers
{
    public class TwoKillerMoves : IKillerMoveCollection
    {
        private bool _isOne = true;
        private short _move1;
        private short _move2;

        public TwoKillerMoves()
        {
            _move1 = _move2 = -1;
        }

        #region Implementation of IKillerMoveCollection

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short move)
        {
            if (_isOne)
            {
                _move1 = move;
                _isOne = false;
            }
            else
            {
                _move2 = move;
                _isOne = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(short move)
        {
            return _move1 == move || _move2 == move;
        }

        #endregion
    }
}