using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Services;

namespace Engine.DataStructures.Moves.Collections;

public class AttackCollection 
{
    protected readonly MoveHistoryList WinCaptures;
    protected readonly MoveHistoryList Trades;
    protected readonly MoveHistoryList LooseCaptures;
    protected readonly MoveHistoryList HashMoves;
    protected readonly MoveHistoryList SuggestedBookMoves;
    protected readonly DataPoolService DataPoolService = ContainerLocator.Current.Resolve<DataPoolService>();

    public AttackCollection() 
    {
        WinCaptures = new MoveHistoryList();
        Trades = new MoveHistoryList();
        LooseCaptures = new MoveHistoryList();
        HashMoves = new MoveHistoryList();
        SuggestedBookMoves= new MoveHistoryList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSuggestedBookMove(MoveBase move) => SuggestedBookMoves.Add(new MoveHistory { Key = move.Key, History = move.BookValue});

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWinCapture(AttackBase move) => WinCaptures.Add(new MoveHistory { Key = move.Key, History = move.See });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTrade(AttackBase move) => Trades.Add(new MoveHistory { Key = move.Key });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseCapture(AttackBase move) => LooseCaptures.Add(new MoveHistory { Key = move.Key, History = move.See });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddHashMove(MoveBase move) => HashMoves.Add(new MoveHistory { Key = move.Key });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual MoveHistoryList Build()
    {
        var moves = DataPoolService.GetCurrentMoveHistoryList();
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
    public virtual MoveHistoryList BuildBook() => Build();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHashMoves(PromotionAttackList moves)
    {
        for (byte i = 0; i < moves.Count; i++)
        {
            WinCaptures.Add(new MoveHistory { Key = moves[i].Key});
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHashMoves(PromotionList moves)
    {
        for (byte i = 0; i < moves.Count; i++)
        {
            WinCaptures.Add(new MoveHistory { Key = moves[i].Key});
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCaptures(PromotionList moves, int attackValue)
    {
        for (byte i = 0; i < moves.Count; i++)
        {
            WinCaptures.Add(new MoveHistory { Key = moves[i].Key, History = attackValue });
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCaptures(PromotionAttackList moves, int attackValue)
    {
        for (byte i = 0; i < moves.Count; i++)
        {
            WinCaptures.Add(new MoveHistory { Key = moves[i].Key, History = attackValue });
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCaptures(PromotionList moves, int attackValue)
    {
        for (byte i = 0; i < moves.Count; i++)
        {
            LooseCaptures.Add(new MoveHistory { Key = moves[i].Key, History = attackValue });
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCaptures(PromotionAttackList moves, int attackValue)
    {
        for (byte i = 0; i < moves.Count; i++)
        {
            LooseCaptures.Add(new MoveHistory { Key = moves[i].Key, History = attackValue });
        }
    }
}