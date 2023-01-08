using System.Runtime.CompilerServices;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections.Initial
{
    public class InitialTradeMoveCollection : InitialMoveCollection
    {
        public InitialTradeMoveCollection(IMoveComparer comparer) : base(comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveBase[] Build()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var tradesCount = winCapturesCount + Trades.Count;
            var killersCount = tradesCount + _killers.Count;
            var suggestedCount = killersCount + _suggested.Count;
            var looseCapturesCount = suggestedCount + LooseCaptures.Count;
            var nonCapturesCount = looseCapturesCount + _nonCaptures.Count;
            var notSuggestedCount = nonCapturesCount + _notSuggested.Count;
            Count = notSuggestedCount + _bad.Count;

            MoveBase[] moves = new MoveBase[Count];
            if (killersCount > 0)
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
                int suggested = 0;
                if (suggestedCount > 0)
                {
                    if (suggestedCount < 3)
                    {
                        _nonCaptures.Add(_suggested);
                    }
                    else
                    {
                        suggested = suggestedCount;
                        _suggested.FullSort();
                        _suggested.CopyTo(moves, 0);
                    }
                    _suggested.Clear(); 
                }

                var capturesCount = _nonCaptures.Count;
                if (capturesCount > 0)
                {
                    _nonCaptures.FullSort();
                    _nonCaptures.CopyTo(moves, suggested);
                    _nonCaptures.Clear();
                }
                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.CopyTo(moves, capturesCount+suggested);
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

            Count = 0;
            return moves;
        }
    }
}