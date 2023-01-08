using System.Runtime.CompilerServices;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections.Initial
{
    public abstract class InitialMoveCollection : AttackCollection
    {
        protected readonly MoveList _killers;
        protected readonly MoveList _nonCaptures;
        protected readonly MoveList _notSuggested;
        protected readonly MoveList _suggested;
        protected readonly MoveList _bad;

        protected InitialMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _killers = new MoveList();
            _nonCaptures = new MoveList();
            _notSuggested = new MoveList();
            _suggested = new MoveList();
            _bad = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddKillerMove(MoveBase move)
        {
            _killers.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSuggested(MoveBase move)
        {
            _suggested.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonCapture(MoveBase move)
        {
            _nonCaptures.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonSuggested(MoveBase move)
        {
            _notSuggested.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBad(MoveBase move)
        {
            _bad.Add(move);
        }
    }
}