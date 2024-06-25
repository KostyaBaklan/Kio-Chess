using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Collections;

public class SimpleMoveCollection : AttackCollection
{
    protected readonly MoveList _killers;
    protected readonly MoveList _nonCaptures;
    protected readonly MoveList _counters;
    protected readonly MoveList _notSuggested;
    protected readonly MoveList _mates;
    protected readonly MoveList _checks;

    public SimpleMoveCollection() : base()
    {
        _killers = new MoveList();
        _nonCaptures = new MoveList();
        _counters = new MoveList();
        _notSuggested = new MoveList();
        _checks = new MoveList();
        _mates = new MoveList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMateMove(MoveBase move) => _mates.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCheck(MoveBase move) => _checks.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNonSuggested(MoveBase move) => _notSuggested.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddKillerMove(MoveBase move) => _killers.Insert(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCounterMove(MoveBase move) => _counters.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNonCapture(MoveBase move) => _nonCaptures.Add(move);

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
        if (_checks.Count > 0)
        {
            moves.SortAndCopy(_checks);
            _checks.Clear();
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
        if (_checks.Count > 0)
        {
            moves.SortAndCopy(_checks);
            _checks.Clear();
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

        return moves;
    }
}