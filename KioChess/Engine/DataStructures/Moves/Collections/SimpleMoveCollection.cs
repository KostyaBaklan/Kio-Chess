using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections;

public class SimpleMoveCollection : AttackCollection
{
    protected MoveHistoryList _killers;
    protected MoveHistoryList _nonCaptures;
    protected MoveHistoryList _counters;
    protected MoveHistoryList _notSuggested;

    public SimpleMoveCollection() : base()
    {
        _killers = new();
        _nonCaptures = new();
        _counters = new();
        _notSuggested = new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNonSuggested(MoveBase move) => _notSuggested.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddKillerMove(MoveBase move) => _killers.Insert(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCounterMove(MoveBase move) => _counters.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNonCapture(MoveBase move) => _nonCaptures.Add(move.ToMoveHistory());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void BuildBook(ref MoveHistoryList moves)
    {
        moves.CopyClear(ref HashMoves);
        moves.SortCopyClear(ref SuggestedBookMoves);
        moves.SortCopyClear(ref WinCaptures);
        moves.CopyClear(ref Trades);
        moves.CopyClear(ref _killers);
        moves.CopyClear(ref _counters);
        moves.SortCopyClear(ref _nonCaptures);
        moves.SortCopyClear(ref LooseCaptures);
        moves.SortCopyClear(ref _notSuggested);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Build(ref MoveHistoryList moves)
    {
        moves.CopyClear(ref HashMoves);
        moves.SortCopyClear(ref WinCaptures);
        moves.CopyClear(ref Trades);
        moves.CopyClear(ref _killers);
        moves.CopyClear(ref _counters);
        moves.SortCopyClear(ref _nonCaptures);
        moves.SortCopyClear(ref LooseCaptures);
        moves.SortCopyClear(ref _notSuggested);
    }
}