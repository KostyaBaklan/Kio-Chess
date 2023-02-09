using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections.Initial
{
    public class InitialSuggestedMoveCollection : InitialMoveCollection
    {
        public InitialSuggestedMoveCollection(IMoveComparer comparer) : base(comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveList Build()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var tradesCount = winCapturesCount + Trades.Count;
            var killersCount = tradesCount + _killers.Count;

            var moves = DataPoolService.GetCurrentMoveList();
            moves.Clear();

            if (killersCount > 0)
            {
                var suggestedCount = killersCount + _suggested.Count;
                var looseCapturesCount = suggestedCount + LooseCaptures.Count;
                var nonCapturesCount = looseCapturesCount + _nonCaptures.Count;
                var notSuggestedCount = nonCapturesCount + _notSuggested.Count;

                if (hashMovesCount > 0)
                {
                    HashMoves.CopyTo(moves, 0);
                    HashMoves.Clear();
                }

                if (WinCaptures.Count > 0)
                {
                    WinCaptures.CopyTo(moves, hashMovesCount);
                    WinCaptures.Clear();
                }

                if (Trades.Count > 0)
                {
                    Trades.CopyTo(moves, winCapturesCount);
                    Trades.Clear();
                }

                if (_killers.Count > 0)
                {
                    _killers.CopyTo(moves, tradesCount);
                    _killers.Clear();
                }

                if (_suggested.Count > 0)
                {
                    _suggested.FullSort();
                    _suggested.CopyTo(moves, killersCount);
                    _suggested.Clear();
                }

                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.CopyTo(moves, suggestedCount);
                    LooseCaptures.Clear();
                }

                if (_nonCaptures.Count > 0)
                {
                    _nonCaptures.FullSort();
                    _nonCaptures.CopyTo(moves, looseCapturesCount);
                    _nonCaptures.Clear();
                }

                if (_notSuggested.Count > 0)
                {
                    _notSuggested.Sort();
                    _notSuggested.CopyTo(moves, nonCapturesCount);
                    _notSuggested.Clear();
                }

                if (_bad.Count > 0)
                {
                    _bad.CopyTo(moves, notSuggestedCount);
                    _bad.Clear();
                }
            }
            else
            {
                _nonCaptures.ExtractMax(Math.Min(1, _nonCaptures.Count), _suggested);

                var suggestedCount = _suggested.Count;
                var looseCapturesCount = suggestedCount + LooseCaptures.Count;
                var nonCapturesCount = looseCapturesCount + _nonCaptures.Count;
                var notSuggestedCount = nonCapturesCount + _notSuggested.Count;

                if (_suggested.Count > 0)
                {
                    _suggested.Sort();
                    _suggested.CopyTo(moves, 0);
                    _suggested.Clear();
                }

                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.CopyTo(moves, suggestedCount);
                    LooseCaptures.Clear();
                }

                if (_nonCaptures.Count > 0)
                {
                    _nonCaptures.Sort();
                    _nonCaptures.CopyTo(moves, looseCapturesCount);
                    _nonCaptures.Clear();
                }

                if (_notSuggested.Count > 0)
                {
                    _notSuggested.Sort();
                    _notSuggested.CopyTo(moves, nonCapturesCount);
                    _notSuggested.Clear();
                }

                if (_bad.Count > 0)
                {
                    _bad.CopyTo(moves, notSuggestedCount);
                    _bad.Clear();
                }
            }

            return moves;
        }
    }
}