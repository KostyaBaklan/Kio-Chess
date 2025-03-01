﻿using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections;

public class ComplexMoveCollection : SimpleMoveCollection
{
    protected readonly MoveList _looseNonCapture;
    protected readonly MoveList _forward;
    protected readonly MoveList _suggested;
    protected readonly MoveList _bad;
    protected readonly MoveList _mates;
    protected readonly MoveList _looseCheck;
    protected readonly AttackList _looseCheckAttack;

    public ComplexMoveCollection() : base()
    {
        _looseNonCapture = new MoveList();
        _forward = new MoveList();
        _suggested = new MoveList();
        _bad = new MoveList();
        _looseCheck = new MoveList();
        _looseCheckAttack = new AttackList();
        _mates = new MoveList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseCheck(MoveBase move) => _looseCheck.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseCheckAttack(AttackBase move) => _looseCheckAttack.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMateMove(MoveBase move) => _mates.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddForwardMove(MoveBase move) => _forward.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSuggested(MoveBase move) => _suggested.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBad(MoveBase move) => _bad.Insert(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseNonCapture(MoveBase move) => _looseNonCapture.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList BuildBook() => BuildBookOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList Build() => BuildOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildBookEnd()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        if (_mates.Count > 0)
        {
            moves.Add(_mates);
            _mates.Clear();

            HashMoves.Clear();
            SuggestedBookMoves.Clear();
            WinCaptures.Clear();
            Trades.Clear();
            _killers.Clear();
            _counters.Clear();
            _suggested.Clear();
            _forward.Clear();
            _looseCheckAttack.Clear();
            _looseCheck.Clear();
            LooseCaptures.Clear();
            _nonCaptures.Clear();
            _notSuggested.Clear();
            _looseNonCapture.Clear();

            return moves;
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
    internal MoveList BuildEnd()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        if (_mates.Count > 0)
        {
            moves.Add(_mates);
            _mates.Clear();

            HashMoves.Clear();
            WinCaptures.Clear();
            Trades.Clear();
            _killers.Clear();
            _counters.Clear();
            _suggested.Clear();
            _forward.Clear();
            _looseCheckAttack.Clear();
            _looseCheck.Clear();
            LooseCaptures.Clear();
            _nonCaptures.Clear();
            _notSuggested.Clear();
            _looseNonCapture.Clear();

            return moves;
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
    internal MoveList BuildMiddle()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();
        if (_mates.Count > 0)
        {
            moves.Add(_mates);
            _mates.Clear();

            HashMoves.Clear();
            WinCaptures.Clear();
            Trades.Clear();
            _killers.Clear();
            _counters.Clear();
            _suggested.Clear();
            _forward.Clear();
            _looseCheckAttack.Clear();
            _looseCheck.Clear();
            LooseCaptures.Clear();
            _nonCaptures.Clear();
            _notSuggested.Clear();
            _looseNonCapture.Clear();

            return moves;
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
    internal MoveList BuildBookMiddle()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        if (_mates.Count > 0)
        {
            moves.Add(_mates);
            _mates.Clear();

            HashMoves.Clear();
            SuggestedBookMoves.Clear();
            WinCaptures.Clear();
            Trades.Clear();
            _killers.Clear();
            _counters.Clear();
            _suggested.Clear();
            _forward.Clear();
            _looseCheckAttack.Clear();
            _looseCheck.Clear();
            LooseCaptures.Clear();
            _nonCaptures.Clear();
            _notSuggested.Clear();
            _looseNonCapture.Clear();

            return moves;
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
    internal MoveList BuildBookOpening()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        if (_mates.Count > 0)
        {
            moves.Add(_mates);
            _mates.Clear();

            HashMoves.Clear();
            SuggestedBookMoves.Clear();
            WinCaptures.Clear();
            Trades.Clear();
            _killers.Clear();
            _counters.Clear();
            _suggested.Clear();
            _forward.Clear();
            _looseCheckAttack.Clear();
            _looseCheck.Clear();
            LooseCaptures.Clear();
            _nonCaptures.Clear();
            _notSuggested.Clear();
            _looseNonCapture.Clear();
            _bad.Clear();

            return moves;
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
    internal MoveList BuildOpening()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();
        if (_mates.Count > 0)
        {
            moves.Add(_mates);
            _mates.Clear();

            HashMoves.Clear();
            WinCaptures.Clear();
            Trades.Clear();
            _killers.Clear();
            _counters.Clear();
            _suggested.Clear();
            _forward.Clear();
            _looseCheckAttack.Clear();
            _looseCheck.Clear();
            LooseCaptures.Clear();
            _nonCaptures.Clear();
            _notSuggested.Clear();
            _looseNonCapture.Clear();
            _bad.Clear();

            return moves;
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
