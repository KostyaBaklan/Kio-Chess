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
    protected readonly MoveList _tactical;
    protected readonly MoveList _checks;
    protected readonly MoveList _development;

    public ComplexMoveCollection() : base()
    {
        _looseNonCapture = new MoveList();
        _suggested = new MoveList();
        _bad = new MoveList();
        _mates = new MoveList();
        _tactical = new MoveList();
        _checks = new MoveList();
        _development = new MoveList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMateMove(MoveBase move) => _mates.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSuggested(MoveBase move) => _suggested.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBad(MoveBase move) => _bad.Insert(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseNonCapture(MoveBase move) => _looseNonCapture.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTactical(MoveBase move) => _tactical.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCheck(MoveBase move) => _checks.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddDevelopment(MoveBase move) => _development.Add(move);

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

        AddPromised(moves);

        if (LooseCaptures.Count > 0)
        {
            LooseCaptures.SortBySee();
            moves.Add(LooseCaptures);
            LooseCaptures.Clear();
        }

        AddOpeningNonCaptures(moves);

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

        AddPromised(moves);

        AddLooseCaptures(moves);

        AddOpeningNonCaptures(moves);

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildOpening()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        AddHashMoves(moves);

        AddOpeningPromised(moves);

        AddLooseCaptures(moves);

        AddOpeningNonCaptures(moves);

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildBookOpening()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        AddHashMoves(moves);

        if (SuggestedBookMoves.Count > 0)
        {
            SuggestedBookMoves.FullSort();
            moves.Add(SuggestedBookMoves);
            SuggestedBookMoves.Clear();
        }

        AddOpeningPromised(moves);

        if (LooseCaptures.Count > 0)
        {
            LooseCaptures.SortBySee();
            moves.Add(LooseCaptures);
            LooseCaptures.Clear();
        }

        AddOpeningNonCaptures(moves);

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildMiddle()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        AddHashMoves(moves);

        AddMiddlePromised(moves);

        AddLooseCaptures(moves);

        AddMiddleNonCaptures(moves);

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildBookMiddle()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        AddHashMoves(moves);

        if (SuggestedBookMoves.Count > 0)
        {
            SuggestedBookMoves.FullSort();
            moves.Add(SuggestedBookMoves);
            SuggestedBookMoves.Clear();
        }

        AddMiddlePromised(moves);

        if (LooseCaptures.Count > 0)
        {
            LooseCaptures.SortBySee();
            moves.Add(LooseCaptures);
            LooseCaptures.Clear();
        }

        AddMiddleNonCaptures(moves);

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildEnd()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        AddHashMoves(moves);

        AddEndPromised(moves);

        AddLooseCaptures(moves);

        AddEndNonCaptures(moves);

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddLooseCaptures(MoveList moves)
    {
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList BuildBookEnd()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

        AddHashMoves(moves);

        if (SuggestedBookMoves.Count > 0)
        {
            SuggestedBookMoves.FullSort();
            moves.Add(SuggestedBookMoves);
            SuggestedBookMoves.Clear();
        }

        AddEndPromised(moves);

        if (LooseCaptures.Count > 0)
        {
            LooseCaptures.SortBySee();
            moves.Add(LooseCaptures);
            LooseCaptures.Clear();
        }

        AddEndNonCaptures(moves);

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddHashMoves(MoveList moves)
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
    private void AddPromised(MoveList moves)
    {
        AddBest(moves);

        if (_development.Count > 0)
        {
            moves.SortAndCopy(_development);
            _development.Clear();
        }
        if (_checks.Count > 0)
        {
            moves.SortAndCopy(_checks);
            _checks.Clear();
        }
        if (_tactical.Count > 0)
        {
            moves.SortAndCopy(_tactical);
            _tactical.Clear();
        }
        if (_suggested.Count > 0)
        {
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddOpeningPromised(MoveList moves)
    {
        AddBest(moves);

        if (_development.Count > 0)
        {
            moves.SortAndCopy(_development);
            _development.Clear();
        }
        if (_suggested.Count > 0)
        {
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
        if (_checks.Count > 0)
        {
            moves.SortAndCopy(_checks);
            _checks.Clear();
        }
        if (_tactical.Count > 0)
        {
            moves.SortAndCopy(_tactical);
            _tactical.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddMiddlePromised(MoveList moves)
    {
        AddBest(moves);

        if (_checks.Count > 0)
        {
            moves.SortAndCopy(_checks);
            _checks.Clear();
        }
        if (_tactical.Count > 0)
        {
            moves.SortAndCopy(_tactical);
            _tactical.Clear();
        }
        if (_development.Count > 0)
        {
            moves.SortAndCopy(_development);
            _development.Clear();
        }
        if (_suggested.Count > 0)
        {
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddEndPromised(MoveList moves)
    {
        AddBest(moves);

        if (_suggested.Count > 0)
        {
            moves.SortAndCopy(_suggested);
            _suggested.Clear();
        }
        if (_tactical.Count > 0)
        {
            moves.SortAndCopy(_tactical);
            _tactical.Clear();
        }
        if (_checks.Count > 0)
        {
            moves.SortAndCopy(_checks);
            _checks.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddOpeningNonCaptures(MoveList moves)
    {
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddMiddleNonCaptures(MoveList moves)
    {
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddEndNonCaptures(MoveList moves)
    {
        if (_nonCaptures.Count > 0)
        {
            moves.SortAndCopy(_nonCaptures);
            _nonCaptures.Clear();
        }
        if (_looseNonCapture.Count > 0)
        {
            moves.SortAndCopy(_looseNonCapture);
            _looseNonCapture.Clear();
        }
    }
}
