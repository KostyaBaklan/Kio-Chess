using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections;

public class ExtendedMoveCollection : SimpleMoveCollection
{
    protected readonly int _sortThreshold;

    protected readonly MoveList _suggested;
    protected readonly MoveList _bad;
    protected readonly MoveList _mates;

    public ExtendedMoveCollection(IMoveComparer comparer) : this(comparer, 6)
    {
    }

    protected ExtendedMoveCollection(IMoveComparer comparer, int sortThreshold) : base(comparer)
    {
        _sortThreshold = sortThreshold;
        _suggested = new MoveList();
        _bad = new MoveList();
        _mates = new MoveList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMateMove(MoveBase move)
    {
        _mates.Add(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSuggested(MoveBase move)
    {
        _suggested.Insert(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBad(MoveBase move)
    {
        _bad.Insert(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ProcessNonCaptures(MoveList moves)
    {
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
    protected override void ProcessOtherMoves(MoveList moves)
    {
        if (_notSuggested.Count > 0)
        {
            moves.SortAndCopy(_notSuggested, Moves);
            _notSuggested.Clear();
        }

        if (_bad.Count > 0)
        {
            moves.Add(_bad);
            _bad.Clear();
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SetBestMoves(MoveList moves)
    {
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SetBestBookMoves(MoveList moves)
    {
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
    }
}