using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Collections;

public class AttackCollection : MoveCollectionBase
{
    protected readonly AttackList WinCaptures;
    protected readonly MoveList Trades;
    protected readonly AttackList LooseCaptures;
    protected readonly MoveList HashMoves;
    protected readonly BookMoveList SuggestedBookMoves;
    protected readonly IDataPoolService DataPoolService = ServiceLocator.Current.GetInstance<IDataPoolService>();

    public AttackCollection() : base()
    {
        WinCaptures = new AttackList();
        Trades = new MoveList();
        LooseCaptures = new AttackList();
        HashMoves = new MoveList();
        SuggestedBookMoves= new BookMoveList();
    }

    #region Implementation of IMoveCollection

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSuggestedBookMove(MoveBase move)
    {
        SuggestedBookMoves.Add(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWinCapture(AttackBase move)
    {
        WinCaptures.Add(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTrade(AttackBase move)
    {
        Trades.Add(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseCapture(AttackBase move)
    {
        LooseCaptures.Add(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddHashMove(MoveBase move)
    {
        HashMoves.Add(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList Build()
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
    public override MoveList BuildBook()
    {
        return Build();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCapture(PromotionList moves)
    {
        WinCaptures.Add(moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCapture(PromotionList moves)
    {
        LooseCaptures.Add(moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHashMoves(PromotionAttackList moves)
    {
        HashMoves.Add(moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHashMoves(PromotionList moves)
    {
        HashMoves.Add(moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCaptures(PromotionAttackList moves, short attackValue)
    {
        for (byte i = 0; i < moves.Count; i++)
        {
            moves[i].See = attackValue;
            WinCaptures.Add(moves[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCapture(PromotionAttackList moves, short attackValue)
    {
        for (byte i = 0; i < moves.Count; i++)
        {
            moves[i].See = attackValue;
            LooseCaptures.Add(moves[i]);
        }
    }
}