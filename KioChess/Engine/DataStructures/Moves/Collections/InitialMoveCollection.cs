using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections
{
    public class InitialMoveCollection : AttackCollection
    {
        protected readonly int _sortThreshold;

        protected readonly MoveList _killers;
        protected readonly MoveList _counters;
        protected readonly MoveList _nonCaptures;
        protected readonly MoveList _notSuggested;
        protected readonly MoveList _suggested;
        protected readonly MoveList _bad;
        protected readonly MoveList _mates;

        public InitialMoveCollection(IMoveComparer comparer) : this(comparer, 6)
        {
        }

        protected InitialMoveCollection(IMoveComparer comparer, int sortThreshold) : base(comparer)
        {
            _sortThreshold = sortThreshold;
            _killers = new MoveList();
            _nonCaptures = new MoveList();
            _notSuggested = new MoveList();
            _suggested = new MoveList();
            _counters = new MoveList();
            _bad = new MoveList();
            _mates = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddMateMove(MoveBase move)
        {
            _mates.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddKillerMove(MoveBase move)
        {
            _killers.Insert(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCounterMove(MoveBase move)
        {
            _counters.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSuggested(MoveBase move)
        {
            _suggested.Insert(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonCapture(MoveBase move)
        {
            _nonCaptures.Insert(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonSuggested(MoveBase move)
        {
            _notSuggested.Insert(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBad(MoveBase move)
        {
            _bad.Insert(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveList Build()
        {
            var moves = DataPoolService.GetCurrentMoveList();
            moves.Clear();

            SetPromisingMoves(moves);

            //SetSugested(moves);

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

            if (NonSuggestedBookMoves.Count > 0)
            {
                moves.Add(NonSuggestedBookMoves);
                NonSuggestedBookMoves.Clear();
            }

            if (LooseCaptures.Count > 0)
            {
                LooseCaptures.SortBySee();
                moves.Add(LooseCaptures);
                LooseCaptures.Clear();
            }

            if (_notSuggested.Count > 0)
            {
                moves.SortAndCopy(_notSuggested, Moves);
                _notSuggested.Clear();
            }

            if (_bad.Count > 0)
            {
                moves.Add(_bad);
                _bad.Clear();
            }

            return moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetSugested(MoveList moves)
        {
            while (_nonCaptures.Count > 0 && _suggested.Count + moves.Count < _sortThreshold)
            {
                _suggested.Insert(_nonCaptures.Maximum());
            }

            while (_notSuggested.Count > 0 && _suggested.Count + moves.Count < _sortThreshold)
            {
                _suggested.Insert(_notSuggested.Maximum());
            }

            if (_suggested.Count > 0)
            {
                moves.SortAndCopy(_suggested, Moves);
                _suggested.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetPromisingMoves(MoveList moves)
        {
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

            if (SuggestedBookMoves.Count > 0)
            {
                moves.SortAndCopy(SuggestedBookMoves, Moves);
                SuggestedBookMoves.Clear();
            }

            if (WinCaptures.Count > 0)
            {
                WinCaptures.SortBySee();
                moves.Add(WinCaptures);
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

            if (_counters.Count > 0)
            {
                moves.Add(_counters);
                _counters.Clear();
            }
        }
    }
}