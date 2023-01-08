using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.DataStructures.Killers
{
    public class KillerMoves : IKillerMoveCollection
    {
        private readonly bool[] _moves;

        public KillerMoves(int capacity)
        {
            _moves = new bool[capacity];
        }

        #region Implementation of IKillerMoveCollection

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short move)
        {
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