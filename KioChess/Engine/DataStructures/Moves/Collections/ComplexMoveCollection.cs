using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections;

public class ComplexMoveCollection : SimpleMoveCollection
{
    protected readonly MoveList _looseNonCapture;
    protected readonly MoveList _suggested;
    protected readonly MoveList _bad;
    protected readonly MoveList _mates;
    protected readonly AttackList _smallLooseCaptures;

    public ComplexMoveCollection() : base()
    {
        _looseNonCapture = new MoveList();
        _suggested = new MoveList();
        _bad = new MoveList();
        _mates = new MoveList();
        _smallLooseCaptures = new AttackList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMateMove(MoveBase move) => _mates.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSmallLooseCapture(AttackBase move) => _smallLooseCaptures.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSuggested(MoveBase move) => _suggested.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBad(MoveBase move) => _bad.Insert(move);

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
            moves.SortAndCopy(_suggested);
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
            moves.SortAndCopy(_nonCaptures);
            _nonCaptures.Clear();
        }
        if (_notSuggested.Count > 0)
        {
            moves.SortAndCopy(_notSuggested);
            _notSuggested.Clear();
        }
        if (_looseNonCapture.Count > 0)
        {
            moves.SortAndCopy(_looseNonCapture);
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
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
        if (LooseCaptures.Count > 0)
        {
            if (moves.Count < 1)
            {
                while (moves.Count < 3 && _nonCaptures.Count > 0)
                {
                    moves.Add(_nonCaptures.ExtractMax());
                }
            }
            LooseCaptures.SortBySee();
            moves.Add(LooseCaptures);
            LooseCaptures.Clear();
        }
        if (_nonCaptures.Count > 0)
        {
            moves.SortAndCopy(_nonCaptures);
            _nonCaptures.Clear();
        }
        if (_notSuggested.Count > 0)
        {
            moves.SortAndCopy(_notSuggested);
            _notSuggested.Clear();
        }
        if (_looseNonCapture.Count > 0)
        {
            moves.SortAndCopy(_looseNonCapture);
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
    internal MoveList BuildBookEnd()
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
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
        if (_smallLooseCaptures.Count > 0)
        {
            _smallLooseCaptures.SortBySee();
            moves.Add(_smallLooseCaptures);
            _smallLooseCaptures.Clear();
        }
        if (_nonCaptures.Count > 0)
        {
            moves.SortAndCopy(_nonCaptures);
            _nonCaptures.Clear();
        }
        if (LooseCaptures.Count > 0)
        {
            LooseCaptures.SortBySee();
            moves.Add(LooseCaptures);
            LooseCaptures.Clear();
        }
        if (_looseNonCapture.Count > 0)
        {
            moves.SortAndCopy(_looseNonCapture);
            _looseNonCapture.Clear();
        }

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildEnd()
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
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
        if (_smallLooseCaptures.Count > 0)
        {
            _smallLooseCaptures.SortBySee();
            moves.Add(_smallLooseCaptures);
            _smallLooseCaptures.Clear();
        }
        if (_nonCaptures.Count > 0)
        {
            moves.SortAndCopy(_nonCaptures);
            _nonCaptures.Clear();
        }
        if (LooseCaptures.Count > 0)
        {
            LooseCaptures.SortBySee();
            moves.Add(LooseCaptures);
            LooseCaptures.Clear();
        }
        if (_looseNonCapture.Count > 0)
        {
            moves.SortAndCopy(_looseNonCapture);
            _looseNonCapture.Clear();
        }

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildMiddle()
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
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
        if (_smallLooseCaptures.Count > 0)
        {
            if (moves.Count < 1)
            {
                while (moves.Count < 3 && _nonCaptures.Count > 0)
                {
                    moves.Add(_nonCaptures.ExtractMax());
                }
            }
            _smallLooseCaptures.SortBySee();
            moves.Add(_smallLooseCaptures);
            _smallLooseCaptures.Clear();
        }

        if (_nonCaptures.Count > 0)
        {
            moves.SortAndCopy(_nonCaptures);
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
            moves.SortAndCopy(_notSuggested);
            _notSuggested.Clear();
        }
        if (_looseNonCapture.Count > 0)
        {
            moves.SortAndCopy(_looseNonCapture);
            _looseNonCapture.Clear();
        }

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildBookMiddle()
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
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
        if (_smallLooseCaptures.Count > 0)
        {
            _smallLooseCaptures.SortBySee();
            moves.Add(_smallLooseCaptures);
            _smallLooseCaptures.Clear();
        }
        if (_nonCaptures.Count > 0)
        {
            moves.SortAndCopy(_nonCaptures);
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
            moves.SortAndCopy(_notSuggested);
            _notSuggested.Clear();
        }
        if (_looseNonCapture.Count > 0)
        {
            moves.SortAndCopy(_looseNonCapture);
            _looseNonCapture.Clear();
        }

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildBookOpening()
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
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
        if (_smallLooseCaptures.Count > 0)
        {
            _smallLooseCaptures.SortBySee();
            moves.Add(_smallLooseCaptures);
            _smallLooseCaptures.Clear();
        }
        if (_nonCaptures.Count > 0)
        {
            moves.SortAndCopy(_nonCaptures);
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
            moves.SortAndCopy(_notSuggested);
            _notSuggested.Clear();
        }
        if (_looseNonCapture.Count > 0)
        {
            moves.SortAndCopy(_looseNonCapture);
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
    internal MoveList BuildOpening()
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
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
        if (_smallLooseCaptures.Count > 0)
        {
            _smallLooseCaptures.SortBySee();
            moves.Add(_smallLooseCaptures);
            _smallLooseCaptures.Clear();
        }
        if (_nonCaptures.Count > 0)
        {
            moves.SortAndCopy(_nonCaptures);
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
            moves.SortAndCopy(_notSuggested);
            _notSuggested.Clear();
        }
        if (_looseNonCapture.Count > 0)
        {
            moves.SortAndCopy(_looseNonCapture);
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
