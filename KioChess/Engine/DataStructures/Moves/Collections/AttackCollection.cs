using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Services;

namespace Engine.DataStructures.Moves.Collections;

public class AttackCollection
{
    protected readonly AttackList WinCaptures;
    protected readonly MoveList Trades;
    protected readonly AttackList LooseCaptures;
    protected readonly MoveList HashMoves;
    protected readonly BookMoveList SuggestedBookMoves;
    protected readonly DataPoolService DataPoolService = ContainerLocator.Current.Resolve<DataPoolService>();

    public AttackCollection()
    {
        WinCaptures = new AttackList();
        Trades = new MoveList();
        LooseCaptures = new AttackList();
        HashMoves = new MoveList();
        SuggestedBookMoves = new BookMoveList();
    }

    #region Implementation of IMoveCollection

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSuggestedBookMove(MoveBase move) => SuggestedBookMoves.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWinCapture(AttackBase move) => WinCaptures.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTrade(AttackBase move) => Trades.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseCapture(AttackBase move) => LooseCaptures.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddHashMove(MoveBase move) => HashMoves.Add(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual MoveList Build()
    {
        var moves = DataPoolService.GetCurrentMoveList();
        moves.Clear();

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

        if (LooseCaptures.Count > 0)
        {
            LooseCaptures.SortBySee();
            moves.Add(LooseCaptures);
            LooseCaptures.Clear();
        }

        return moves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual MoveList BuildBook() => Build();

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