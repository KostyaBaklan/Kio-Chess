using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections
{
    public class InitialMoveCollection : AttackCollection
    {
        protected readonly MoveList _killers;
        protected readonly MoveList _nonCaptures;
        protected readonly MoveList _notSuggested;
        protected readonly MoveList _suggested;
        protected readonly MoveList _bad;

        public InitialMoveCollection(IMoveComparer comparer) : base(comparer)
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
            _killers.Insert(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSuggested(MoveBase move)
        {
            _suggested.Insert(move);
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
            _bad.Insert(move);
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

            if (moves.Count > 0)
            {
                if (_suggested.Count > 0)
                {
                    _suggested.FullSort();
                    moves.Add(_suggested);
                    _suggested.Clear();
                }

                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.SortBySee();
                    moves.Add(LooseCaptures);
                    LooseCaptures.Clear();
                }

                if (_nonCaptures.Count > 0)
                {
                    moves.SortAndCopy(_nonCaptures, Moves);
                    _nonCaptures.Clear();
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
            }
            else
            {
                if (_suggested.Count > 0)
                {
                    if (_suggested.Count < 3)
                    {
                        _nonCaptures.Add(_suggested);
                    }
                    else
                    {
                        _suggested.FullSort();
                        moves.Add(_suggested);
                    }
                    _suggested.Clear();
                }

                if (_nonCaptures.Count > 0)
                {
                    moves.SortAndCopy(_nonCaptures, Moves);
                    _nonCaptures.Clear();
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
            }

            return moves;
        }
    }
}