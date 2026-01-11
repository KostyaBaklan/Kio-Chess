using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Sorting;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts;

public abstract class SortContext
{
    public bool HasPv;
    public bool IsPvCapture;
    public short Pv;
    public short CounterMove;
    public short CountermoveHistoryMove;
    public short CountiniousMoveHistory;
    protected MoveSorterBase MoveSorter;
    public int Ply;
    public KillerMoves CurrentKillers;
    public byte Phase;

    public static MoveHistoryService MoveHistory;
    public static MoveProvider MoveProvider;
    public static DataPoolService DataPoolService;

    public abstract bool IsRegular { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetInternal(MoveSorterBase sorter, short pv)
    {
        MoveSorter = sorter;
        CounterMove = MoveHistory.GetCounterMove();
        CountermoveHistoryMove = MoveHistory.GetCountermoveHistory();
        CountiniousMoveHistory = MoveHistory.GetCountiniousMoveHistory();
        MoveSorter.SetValues();

        HasPv = true;
        Pv = pv;
        IsPvCapture = MoveProvider.IsAttack(pv);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetInternal(MoveSorterBase sorter)
    {
        MoveSorter = sorter;
        CounterMove = MoveHistory.GetCounterMove();
        CountermoveHistoryMove = MoveHistory.GetCountermoveHistory();
        CountiniousMoveHistory = MoveHistory.GetCountiniousMoveHistory();
        MoveSorter.SetValues();

        HasPv = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void Set(MoveSorterBase sorter);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void Set(MoveSorterBase sorter, short pv);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetForEvaluation(EvaluationSorter sorter, int alphaDifference)
    {
        MoveSorter = sorter;
        sorter.SetValues(alphaDifference);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessHashMove(MoveBase move) => MoveSorter.ProcessHashMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessKillerMove(MoveBase move) => MoveSorter.ProcessKillerMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessCounterMove(MoveBase move) => MoveSorter.ProcessCounterMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessCountermoveHistoryMove(MoveBase move) => MoveSorter.ProcessCountermoveHistoryMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessCountiniousMoveHistoryMove(MoveBase move) => MoveSorter.ProcessCountiniousMoveHistoryMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessCaptureMove(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsKiller(short key) => CurrentKillers.Contains(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void GetMoves(ref MoveHistoryList moves) => GetMovesInternal(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract void GetMovesInternal(ref MoveHistoryList moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract void GetBookMovesInternal(ref MoveHistoryList moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessPromotionMoves(PromotionList promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessPromotionCaptures(PromotionAttackList promotionAttackList);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessHashMoves(PromotionList promotions) => MoveSorter.ProcessHashMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ProcessHashMoves(PromotionAttackList promotions) => MoveSorter.ProcessHashMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddSuggestedBookMove(MoveBase move) => MoveSorter.AddSuggestedBookMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract bool IsRegularMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void GetAllMoves(Position position, ref MoveHistoryList moves);
}