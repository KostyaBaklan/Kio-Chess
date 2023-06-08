using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections
{
    public class AdvancedMoveCollection : AttackCollection
    {
        private readonly MoveList _killers;
        private readonly MoveList _nonCaptures;
        private readonly MoveList _suggested;

        public AdvancedMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _killers = new MoveList();
            _nonCaptures = new MoveList();
            _suggested = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddKillerMove(MoveBase move)
        {
            _killers.Insert(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonCapture(MoveBase move)
        {
            _nonCaptures.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveList Build()
        {
            var moves = DataPoolService.GetCurrentMoveList();
            moves.Clear();

            if (HashMoves.Count > 0)
            {
                moves.Add(HashMoves);
                HashMoves.Clear();
            }

            if (WinCaptures.Count > 0)
            {
                moves.SortAndCopy(WinCaptures, Moves);
                WinCaptures.Clear();
            }

            if (Trades.Count > 0)
            {
                moves.Add(Trades);
                Trades.Clear();
            }

            if (_killers.Count > 0)
            {
                moves.Add(_killers);
                _killers.Clear();
            }

            if (_nonCaptures.Count > 0)
            {
                moves.SortAndCopy(_nonCaptures,Moves);
                _nonCaptures.Clear();
            }

            if (LooseCaptures.Count > 0)
            {
                moves.SortAndCopy(LooseCaptures, Moves);
                LooseCaptures.Clear();
            }

            return moves;
        }
    }
}