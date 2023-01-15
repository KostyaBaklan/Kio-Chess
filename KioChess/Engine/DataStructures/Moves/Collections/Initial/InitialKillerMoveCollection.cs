using System.Runtime.CompilerServices;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections.Initial
{
    public class InitialKillerMoveCollection : InitialMoveCollection
    {
        public InitialKillerMoveCollection(IMoveComparer comparer) : base(comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveList Build()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var killersCount = winCapturesCount + _killers.Count;
            var tradesCount = killersCount + Trades.Count;
            var suggestedCount = tradesCount + _suggested.Count;
            var looseCapturesCount = suggestedCount + LooseCaptures.Count;
            var nonCapturesCount = looseCapturesCount + _nonCaptures.Count;
            var notSuggestedCount = nonCapturesCount + _notSuggested.Count;

            var moves = DataPoolService.GetCurrentMoveList();
            moves.Clear();

            if (tradesCount > 0)
            {
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

                if (_killers.Count > 0)
                {
                    _killers.CopyTo(moves, winCapturesCount);
                    _killers.Clear();
                }

                if (Trades.Count > 0)
                {
                    Trades.CopyTo(moves, killersCount);
                    Trades.Clear();
                }

                if (_suggested.Count > 0)
                {
                    _suggested.FullSort();
                    _suggested.CopyTo(moves, tradesCount);
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
                    _notSuggested.FullSort();
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
                if (_suggested.Count > 0)
                {
                    _nonCaptures.Add(_suggested);
                    _suggested.Clear();
                }
                var capturesCount = _nonCaptures.Count;
                if (capturesCount > 0)
                {
                    _nonCaptures.FullSort();
                    _nonCaptures.CopyTo(moves, 0);
                    _nonCaptures.Clear();
                }
                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.CopyTo(moves, capturesCount);
                    LooseCaptures.Clear();
                }

                if (_notSuggested.Count > 0)
                {
                    _notSuggested.FullSort();
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