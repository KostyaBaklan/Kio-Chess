using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Collections;

public class SimpleMoveCollection : AttackCollection
{
    protected readonly MoveHistoryList _killers;
    protected readonly MoveHistoryList _nonCaptures;
    protected readonly MoveHistoryList _counters;
    protected readonly MoveHistoryList _notSuggested;

    public SimpleMoveCollection() : base()
    {
        _killers = new MoveHistoryList();
        _nonCaptures = new MoveHistoryList();
        _counters = new MoveHistoryList();
        _notSuggested = new MoveHistoryList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNonSuggested(MoveBase move) => _notSuggested.Add(new MoveHistory { Key = move.Key, History = move.RelativeHistory });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddKillerMove(MoveBase move) => _killers.Insert(new MoveHistory { Key = move.Key, History = move.RelativeHistory });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCounterMove(MoveBase move) => _counters.Add(new MoveHistory { Key = move.Key});

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNonCapture(MoveBase move) => _nonCaptures.Add(new MoveHistory { Key = move.Key, History = move.RelativeHistory });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveHistoryList BuildBook()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
        moves.Clear(); 
        
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

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveHistoryList Build()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
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

        if (_counters.Count > 0)
        {
            moves.Add(_counters[0]);
            _counters.Clear();
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

        return moves;
    }
}