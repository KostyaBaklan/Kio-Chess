using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves;

public class MoveCollection
{
    private MoveHistoryList WinCaptures;
    private MoveHistoryList Trades;
    private MoveHistoryList LooseCaptures;
    private MoveHistoryList HashMoves;
    private MoveHistoryList SuggestedBookMoves;
    private MoveHistoryList _killers;
    private MoveHistoryList _nonCaptures;
    private MoveHistoryList _counters;
    private MoveHistoryList _notSuggested;
    private MoveHistoryList _countermoveHistory;
    private MoveHistoryList _looseMinorPieces;
    private MoveHistoryList _looseMajorPieces;
    private MoveHistoryList _forward;
    private MoveHistoryList _suggested;
    private MoveHistoryList _bad;
    private MoveHistoryList _mates;
    private MoveHistoryList _looseCheck;
    private MoveHistoryList _looseCheckAttack;
    private MoveHistoryList _mobility;
    private MoveHistoryList _missedEnemyPromotions;

    public MoveCollection()
    {
        WinCaptures = new();
        Trades = new();
        LooseCaptures = new();
        HashMoves = new();
        SuggestedBookMoves = new();
        _killers = new();
        _nonCaptures = new();
        _counters = new();
        _notSuggested = new();
        _countermoveHistory = new();
        _looseMinorPieces = new();
        _looseMajorPieces = new();
        _forward = new();
        _suggested = new();
        _bad = new();
        _mates = new();
        _looseCheck = new();
        _looseCheckAttack = new();
        _mobility = new();
        _missedEnemyPromotions = new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSuggestedBookMove(MoveBase move) => SuggestedBookMoves.Add(move.ToBookHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWinCapture(AttackBase move) => WinCaptures.Add(move.ToCaptureHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTrade(AttackBase move) => Trades.Add(new MoveHistory(move.Key, 0));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseCapture(AttackBase move) => LooseCaptures.Add(move.ToCaptureHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddHashMove(MoveBase move) => HashMoves.Add(move.ToMoveHistory()); 
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHashMoves(PromotionAttackList moves) => HashMoves.Add(moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHashMoves(PromotionList moves) => HashMoves.Add(moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNonSuggested(MoveBase move) => _notSuggested.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddKillerMove(MoveBase move) => _killers.Insert(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCounterMove(MoveBase move) => _counters.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCountermoveHistory(MoveBase move) => _countermoveHistory.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNonCapture(MoveBase move) => _nonCaptures.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCaptures(PromotionList moves, int attackValue) => WinCaptures.Add(moves, attackValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCaptures(PromotionAttackList moves, int attackValue) => WinCaptures.Add(moves, attackValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCaptures(PromotionList moves, int attackValue) => LooseCaptures.Add(moves, attackValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCaptures(PromotionAttackList moves, int attackValue) => LooseCaptures.Add(moves, attackValue);

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
    public void Build(ref MoveHistoryList moves)
    {
        moves.SortCopyClear(ref WinCaptures);
        moves.CopyClear(ref Trades);
        moves.SortCopyClear(ref LooseCaptures);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BuildBook(ref MoveHistoryList moves) => Build(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BuildSimpleBook(ref MoveHistoryList moves)
    {
        moves.CopyClear(ref HashMoves);
        moves.SortCopyClear(ref SuggestedBookMoves);
        BuildSimpleInternal(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BuildSimple(ref MoveHistoryList moves)
    {
        moves.CopyClear(ref HashMoves);
        BuildSimpleInternal(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BuildSimpleInternal(ref MoveHistoryList moves)
    {
        moves.SortCopyClear(ref WinCaptures);
        moves.CopyClear(ref Trades);
        moves.CopyClear(ref _killers);
        moves.CopyClear(ref _counters);
        moves.CopyClear(ref _countermoveHistory);
        moves.SortCopyClear(ref _nonCaptures);
        moves.SortCopyClear(ref LooseCaptures);
        moves.SortCopyClear(ref _notSuggested);
    }

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

            BuildComplexInternal(ref moves);
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

            BuildComplexInternal(ref moves);
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

            BuildComplexInternal(ref moves);
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

            BuildComplexInternal(ref moves);
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

            BuildComplexInternal(ref moves);

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

            BuildComplexInternal(ref moves);

            moves.CopyClear(ref _bad);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BuildComplexInternal(ref MoveHistoryList moves)
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