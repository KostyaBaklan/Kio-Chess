using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections;

public class ComplexMoveCollection : SimpleMoveCollection
{
    protected readonly MoveHistoryList _looseNonCapture;
    protected readonly MoveHistoryList _forward;
    protected readonly MoveHistoryList _suggested;
    protected readonly MoveHistoryList _bad;
    protected readonly MoveHistoryList _mates;
    protected readonly MoveHistoryList _looseCheck;
    protected readonly MoveHistoryList _looseCheckAttack;

    public ComplexMoveCollection() : base()
    {
        _looseNonCapture = new MoveHistoryList();
        _forward = new MoveHistoryList();
        _suggested = new MoveHistoryList();
        _bad = new MoveHistoryList();
        _looseCheck = new MoveHistoryList();
        _looseCheckAttack = new MoveHistoryList();
        _mates = new MoveHistoryList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseCheck(MoveBase move) => _looseCheck.Add(new MoveHistory { Key = move.Key, History = move.RelativeHistory });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseCheckAttack(AttackBase move) => _looseCheckAttack.Add(new MoveHistory { Key = move.Key, History = move.See });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMateMove(MoveBase move) => _mates.Add(new MoveHistory { Key = move.Key});

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddForwardMove(MoveBase move) => _forward.Add(new MoveHistory { Key = move.Key, History = move.RelativeHistory });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSuggested(MoveBase move) => _suggested.Add(new MoveHistory { Key = move.Key, History = move.RelativeHistory });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBad(MoveBase move) => _bad.Insert(new MoveHistory { Key = move.Key, History = move.RelativeHistory });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseNonCapture(MoveBase move) => _looseNonCapture.Add(new MoveHistory { Key = move.Key, History = move.RelativeHistory });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveHistoryList BuildBook()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
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
            moves.SortAndCopy(SuggestedBookMoves);
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
        if (_looseCheckAttack.Count > 0)
        {
            _looseCheckAttack.SortBySee();
            moves.Add(_looseCheckAttack);
            _looseCheckAttack.Clear();
        }
        if (_looseCheck.Count > 0)
        {
            moves.SortAndCopy(_looseCheck);
            _looseCheck.Clear();
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
    public override MoveHistoryList Build()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
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
        if (_looseCheckAttack.Count > 0)
        {
            _looseCheckAttack.SortBySee();
            moves.Add(_looseCheckAttack);
            _looseCheckAttack.Clear();
        }
        if (_looseCheck.Count > 0)
        {
            moves.SortAndCopy(_looseCheck);
            _looseCheck.Clear();
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
    internal MoveHistoryList BuildBookEnd()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
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
            moves.SortAndCopy(SuggestedBookMoves);
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
        if (_forward.Count > 0)
        {
            moves.SortAndCopy(_forward);
            _forward.Clear();
        }
        if (_looseCheckAttack.Count > 0)
        {
            _looseCheckAttack.SortBySee();
            moves.Add(_looseCheckAttack);
            _looseCheckAttack.Clear();
        }
        if (_looseCheck.Count > 0)
        {
            moves.SortAndCopy(_looseCheck);
            _looseCheck.Clear();
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
    internal MoveHistoryList BuildEnd()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
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
        if (_forward.Count > 0)
        {
            moves.SortAndCopy(_forward);
            _forward.Clear();
        }
        if (_looseCheckAttack.Count > 0)
        {
            _looseCheckAttack.SortBySee();
            moves.Add(_looseCheckAttack);
            _looseCheckAttack.Clear();
        }
        if (_looseCheck.Count > 0)
        {
            moves.SortAndCopy(_looseCheck);
            _looseCheck.Clear();
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
    internal MoveHistoryList BuildMiddle()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
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
        if (_forward.Count > 0)
        {
            moves.SortAndCopy(_forward);
            _forward.Clear();
        }
        if (_looseCheckAttack.Count > 0)
        {
            _looseCheckAttack.SortBySee();
            moves.Add(_looseCheckAttack);
            _looseCheckAttack.Clear();
        }
        if (_looseCheck.Count > 0)
        {
            moves.SortAndCopy(_looseCheck);
            _looseCheck.Clear();
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

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveHistoryList BuildBookMiddle()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
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
            moves.SortAndCopy(SuggestedBookMoves);
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
        if (_forward.Count > 0)
        {
            moves.SortAndCopy(_forward);
            _forward.Clear();
        }
        if (_looseCheckAttack.Count > 0)
        {
            _looseCheckAttack.SortBySee();
            moves.Add(_looseCheckAttack);
            _looseCheckAttack.Clear();
        }
        if (_looseCheck.Count > 0)
        {
            moves.SortAndCopy(_looseCheck);
            _looseCheck.Clear();
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

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveHistoryList BuildBookOpening()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
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
            moves.SortAndCopy(SuggestedBookMoves);
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
        if (_forward.Count > 0)
        {
            moves.SortAndCopy(_forward);
            _forward.Clear();
        }
        if (_looseCheckAttack.Count > 0)
        {
            _looseCheckAttack.SortBySee();
            moves.Add(_looseCheckAttack);
            _looseCheckAttack.Clear();
        }
        if (_looseCheck.Count > 0)
        {
            moves.SortAndCopy(_looseCheck);
            _looseCheck.Clear();
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
    internal MoveHistoryList BuildOpening()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
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
        if (_forward.Count > 0)
        {
            moves.SortAndCopy(_forward);
            _forward.Clear();
        }
        if (_looseCheckAttack.Count > 0)
        {
            _looseCheckAttack.SortBySee();
            moves.Add(_looseCheckAttack);
            _looseCheckAttack.Clear();
        }
        if (_looseCheck.Count > 0)
        {
            moves.SortAndCopy(_looseCheck);
            _looseCheck.Clear();
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
