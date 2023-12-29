using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections;

public class SimpleMoveCollection : AttackCollection
{
    protected byte PromisingCount;

    protected readonly MoveList _killers;
    protected readonly MoveList _nonCaptures;
    protected readonly MoveList _counters;
    protected readonly MoveList _forwardMoves;

    public SimpleMoveCollection(IMoveComparer comparer) : base(comparer)
    {
        _killers = new MoveList();
        _nonCaptures = new MoveList();
        _counters = new MoveList();
        _forwardMoves = new MoveList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddForwardMove(MoveBase move)
    {
        _forwardMoves.Add(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddKillerMove(MoveBase move)
    {
        _killers.Insert(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCounterMove(MoveBase move)
    {
        _counters.Add(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNonCapture(MoveBase move)
    {
        _nonCaptures.Add(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList BuildBook()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        SetBestBookMoves(moves);

        SetPromisingMoves(moves);

        ProcessNonCaptures(moves);

        ProcessOtherMoves(moves);

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList Build()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        SetBestMoves(moves);

        SetPromisingMoves(moves);

        ProcessNonCaptures(moves);

        ProcessOtherMoves(moves);

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void SetBestBookMoves(MoveList moves)
    {
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void SetBestMoves(MoveList moves)
    {
        if (HashMoves.Count > 0)
        {
            moves.Add(HashMoves);
            HashMoves.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void SetPromisingMoves(MoveList moves)
    {
        if (WinCaptures.Count > 0)
        {
            WinCaptures.SortBySee();
            moves.Add(WinCaptures);
            WinCaptures.Clear();
        }

        PromisingCount = moves.Count;

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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void ProcessNonCaptures(MoveList moves)
    {
        if (_forwardMoves.Count > 0)
        {
            moves.SortAndCopy(_forwardMoves, Moves);
            _forwardMoves.Clear();
        }

        if (PromisingCount > 2)
        {
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
        }
        else
        {
            if (_nonCaptures.Count > 0)
            {
                moves.SortAndCopy(_nonCaptures, Moves);
                _nonCaptures.Clear();
            }
            if (LooseCaptures.Count > 0)
            {
                LooseCaptures.SortBySee();
                moves.Add(LooseCaptures);
                LooseCaptures.Clear();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void ProcessOtherMoves(MoveList moves)
    {
    }
}