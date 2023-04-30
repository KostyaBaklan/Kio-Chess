using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections
{
    public class ComplexMoveCollection : InitialMoveCollection
    {
        protected readonly MoveList _looseNonCapture;
        public ComplexMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _looseNonCapture = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLooseNonCapture(MoveBase move)
        {
            _looseNonCapture.Add(move);
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

            if (moves.Count < 1)
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
                    moves.Add(LooseCaptures);
                    LooseCaptures.Clear();
                }

                if (_notSuggested.Count > 0)
                {
                    moves.SortAndCopy(_notSuggested, Moves);
                    _notSuggested.Clear();
                }

                if (_looseNonCapture.Count > 0)
                {
                    moves.SortAndCopy(_looseNonCapture, Moves);
                    _looseNonCapture.Clear();
                }

                if (_bad.Count > 0)
                {
                    moves.Add(_bad);
                    _bad.Clear();
                }
            }
            else
            {
                if (moves.Count < 2)
                {
                    while (_nonCaptures.Count > 0 && _suggested.Count < 2)
                    {
                        _suggested.Add(_nonCaptures.ExtractMax());
                    } 
                }

                if (_suggested.Count > 0)
                {
                    _suggested.FullSort();
                    moves.Add(_suggested);
                    _suggested.Clear();
                }

                if (LooseCaptures.Count > 0)
                {
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

                if (_looseNonCapture.Count > 0)
                {
                    moves.SortAndCopy(_looseNonCapture, Moves);
                    _looseNonCapture.Clear();
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
