using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections
{
    public class EndGameMoveCollection : InitialMoveCollection
    {
        protected readonly MoveList _passed;
        public EndGameMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _passed = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPassed(MoveBase move)
        {
            _passed.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveList Build()
        {
            var moves = DataPoolService.GetCurrentMoveList();
            moves.Clear();

            if (_mates.Count > 0)
            {
                moves.Add(_mates);
                _mates.Clear();
            }

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

            if (_passed.Count > 0)
            {
                moves.SortAndCopy(_passed, Moves);
                _passed.Clear();
            }

            if (_suggested.Count > 0)
            {
                moves.SortAndCopy(_suggested, Moves);
                _suggested.Clear();
            }

            if (_nonCaptures.Count > 0)
            {
                moves.SortAndCopy(_nonCaptures, Moves);
                _nonCaptures.Clear();
            }

            if (LooseCaptures.Count > 0)
            {
                moves.SortAndCopy(LooseCaptures, Moves);
                LooseCaptures.Clear();
            }

            if (_notSuggested.Count > 0)
            {
                moves.SortAndCopy(_notSuggested, Moves);
                _notSuggested.Clear();
            }

            return moves;
        }
    }
}
