using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections;

public class ComplexMoveCollection : ExtendedMoveCollection
{
    protected readonly MoveList _looseNonCapture;

    public ComplexMoveCollection() : base()
    {
        _looseNonCapture = new MoveList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseNonCapture(MoveBase move) => _looseNonCapture.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList BuildBook()
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

        if (SuggestedBookMoves.Count > 0)
        {
            SuggestedBookMoves.FullSort();
            moves.Add(SuggestedBookMoves);
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
            moves.Add(_counters[0]);
            _counters.Clear();
        }
        if (_suggested.Count > 0)
        {
            moves.SortAndCopy(_suggested, Moves);
            _suggested.Clear();
        }
        if (LooseCaptures.Count > 0)
        {
            LooseCaptures.SortBySee();
            moves.Add(LooseCaptures);
            LooseCaptures.Clear();
        }
        if (_forwardMoves.Count > 0)
        {
            moves.SortAndCopy(_forwardMoves, Moves);
            _forwardMoves.Clear();
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

        return moves;
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
            moves.Add(_counters[0]);
            _counters.Clear();
        }
        if (_suggested.Count > 0)
        {
            moves.SortAndCopy(_suggested, Moves);
            _suggested.Clear();
        }
        if (_forwardMoves.Count > 0)
        {
            moves.SortAndCopy(_forwardMoves, Moves);
            _forwardMoves.Clear();
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

        return moves;
    }
}
