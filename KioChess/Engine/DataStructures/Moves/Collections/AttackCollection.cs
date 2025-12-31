using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Services;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections;

public class AttackCollection
{
    protected MoveHistoryList WinCaptures;
    protected MoveHistoryList Trades;
    protected MoveHistoryList LooseCaptures;
    protected MoveHistoryList HashMoves;
    protected MoveHistoryList SuggestedBookMoves;
    protected readonly DataPoolService DataPoolService = ContainerLocator.Current.Resolve<DataPoolService>();

    public AttackCollection()
    {
        WinCaptures = new();
        Trades = new();
        LooseCaptures = new();
        HashMoves = new();
        SuggestedBookMoves = new();
    }

    #region Implementation of IMoveCollection

    #endregion

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
    public virtual void Build(ref MoveHistoryList moves)
    {
        moves.SortCopyClear(ref WinCaptures);
        moves.CopyClear(ref Trades);
        moves.SortCopyClear(ref LooseCaptures);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void BuildBook(ref MoveHistoryList moves) => Build(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHashMoves(PromotionAttackList moves) => HashMoves.Add(moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHashMoves(PromotionList moves) => HashMoves.Add(moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCaptures(PromotionList moves, int attackValue) => WinCaptures.Add(moves, attackValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCaptures(PromotionAttackList moves, int attackValue) => WinCaptures.Add(moves, attackValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCaptures(PromotionList moves, int attackValue) => LooseCaptures.Add(moves, attackValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCaptures(PromotionAttackList moves, int attackValue) => LooseCaptures.Add(moves, attackValue);
}