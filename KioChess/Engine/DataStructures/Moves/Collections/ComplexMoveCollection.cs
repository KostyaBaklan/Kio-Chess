using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections;

public class ComplexMoveCollection : SimpleMoveCollection
{
    protected MoveHistoryList _looseMinorPieces;
    protected MoveHistoryList _looseMajorPieces;
    protected MoveHistoryList _forward;
    protected MoveHistoryList _suggested;
    protected MoveHistoryList _bad;
    protected MoveHistoryList _mates;
    protected MoveHistoryList _looseCheck;
    protected MoveHistoryList _looseCheckAttack;
    protected MoveHistoryList _mobility;
    protected MoveHistoryList _missedEnemyPromotions;

    public ComplexMoveCollection() : base()
    {
        _looseMinorPieces = new();
        _looseMajorPieces = new();
        _forward = new();
        _suggested = new();
        _bad = new();
        _looseCheck = new();
        _looseCheckAttack = new();
        _mates = new();
        _mobility = new();
        _missedEnemyPromotions = new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMobility(MoveBase move) => _mobility.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMissedEnemyPromotions(MoveBase move) => _missedEnemyPromotions.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseCheck(MoveBase move) => _looseCheck.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseCheckAttack(AttackBase move) => _looseCheckAttack.Add(move.ToCaptureHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMateMove(MoveBase move) => _mates.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddForwardMove(MoveBase move) => _forward.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSuggested(MoveBase move) => _suggested.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBad(MoveBase move) => _bad.Insert(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseMinorPiece(MoveBase move) => _looseMinorPieces.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseMajorPiece(MoveBase move) => _looseMajorPieces.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void BuildBook(ref MoveHistoryList moves) => BuildBookOpening(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Build(ref MoveHistoryList moves) => BuildOpening(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void BuildBookEnd(ref MoveHistoryList moves)
    {
        if (_mates.Count > 0)
        {
            moves.Add(ref _mates);

            ClearBookAll();
        }
        else
        {
            moves.CopyClear(ref HashMoves);
            moves.SortCopyClear(ref SuggestedBookMoves);

            BuildInternal(ref moves);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void BuildEnd(ref MoveHistoryList moves)
    {
        if (_mates.Count > 0)
        {
            moves.Add(ref _mates);

            ClearAll();
        }
        else
        {
            moves.CopyClear(ref HashMoves);

            BuildInternal(ref moves);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void BuildMiddle(ref MoveHistoryList moves)
    {
        if (_mates.Count > 0)
        {
            moves.Add(ref _mates);

            ClearAll();
        }
        else
        {
            moves.CopyClear(ref HashMoves);

            BuildInternal(ref moves);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void BuildBookMiddle(ref MoveHistoryList moves)
    {
        if (_mates.Count > 0)
        {
            moves.Add(ref _mates);

            ClearBookAll();
        }
        else
        {
            moves.CopyClear(ref HashMoves);
            moves.SortCopyClear(ref SuggestedBookMoves);

            BuildInternal(ref moves);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void BuildBookOpening(ref MoveHistoryList moves)
    {
        if (_mates.Count > 0)
        {
            moves.Add(ref _mates);

            ClearBookAll();
        }
        else
        {
            moves.CopyClear(ref HashMoves);
            moves.SortCopyClear(ref SuggestedBookMoves);

            BuildInternal(ref moves);

            moves.CopyClear(ref _bad);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void BuildOpening(ref MoveHistoryList moves)
    {
        if (_mates.Count > 0)
        {
            moves.Add(ref _mates);
            ClearAll();
        }
        else
        {
            moves.CopyClear(ref HashMoves);

            BuildInternal(ref moves);

            moves.CopyClear(ref _bad);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BuildInternal(ref MoveHistoryList moves)
    {
        moves.SortCopyClear(ref WinCaptures);
        moves.CopyClear(ref Trades);
        moves.CopyClear(ref _killers);
        moves.CopyClear(ref _counters);

        moves.LmrIndex = moves.Count;

        moves.CopyClear(ref _countermoveHistory);
        moves.SortCopyClear(ref _suggested);
        moves.SortCopyClear(ref _forward);
        moves.SortCopyClear(ref _mobility);
        moves.SortCopyClear(ref _looseCheckAttack);
        moves.SortCopyClear(ref _looseCheck);
        moves.SortCopyClear(ref _nonCaptures);
        moves.SortCopyClear(ref LooseCaptures);
        moves.SortCopyClear(ref _notSuggested);
        moves.SortCopyClear(ref _looseMinorPieces);
        moves.SortCopyClear(ref _looseMajorPieces);
        moves.SortCopyClear(ref _missedEnemyPromotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearAll()
    {
        _mates.Clear();
        HashMoves.Clear();
        WinCaptures.Clear();
        Trades.Clear();
        _killers.Clear();
        _counters.Clear();
        _countermoveHistory.Clear();
        _suggested.Clear();
        _forward.Clear();
        _looseCheckAttack.Clear();
        _looseCheck.Clear();
        LooseCaptures.Clear();
        _nonCaptures.Clear();
        _notSuggested.Clear();
        _looseMinorPieces.Clear();
        _looseMajorPieces.Clear();
        _bad.Clear();
        _mobility.Clear();
        _missedEnemyPromotions.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearBookAll()
    {
        SuggestedBookMoves.Clear();
        ClearAll();
    }
}