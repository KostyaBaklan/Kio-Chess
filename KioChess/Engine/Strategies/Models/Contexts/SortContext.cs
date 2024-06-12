using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Sorting.Sorters;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts;

public abstract class SortContext
{
    public bool HasPv;
    public bool IsPvCapture;
    public short Pv;
    public short CounterMove;
    protected MoveSorterBase MoveSorter;
    public byte[] Pieces;
    public SquareList[] Squares;
    public SquareList PromotionSquares;
    public int Ply;
    public KillerMoves CurrentKillers;
    public byte Phase;

    public static Position Position;
    public static MoveHistoryService MoveHistory;
    public static MoveProvider MoveProvider;
    public static IDataPoolService DataPoolService;

    public abstract bool IsRegular { get; }

    protected SortContext()
    {
        Squares = new SquareList[6];
        for (int i = 0; i < Squares.Length; i++)
        {
            Squares[i] = new SquareList();
        }
        PromotionSquares = new SquareList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetInternal(MoveSorterBase sorter, short pv)
    {
        MoveSorter = sorter;
        CounterMove = sorter.GetCounterMove();
        MoveSorter.SetValues();

        HasPv = true;
        Pv = pv;
        IsPvCapture = MoveProvider.IsAttack(pv);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetInternal(MoveSorterBase sorter)
    {
        MoveSorter = sorter;
        CounterMove = sorter.GetCounterMove();
        MoveSorter.SetValues();

        HasPv = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void Set(MoveSorterBase sorter);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void Set(MoveSorterBase sorter, short pv);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetForEvaluation(MoveSorterBase sorter, int alpha, int standPat)
    {
        MoveSorter = sorter;
        MoveSorter.SetValues(alpha, standPat);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessHashMove(MoveBase move) => MoveSorter.ProcessHashMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessKillerMove(MoveBase move) => MoveSorter.ProcessKillerMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessCounterMove(MoveBase move) => MoveSorter.ProcessCounterMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessCaptureMove(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsKiller(short key) => CurrentKillers.Contains(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual MoveList GetMoves() => GetMovesInternal();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract MoveList GetMovesInternal();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract MoveList GetBookMovesInternal();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetAttacks() => MoveSorter.GetMoves();

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
    public abstract MoveList GetAllMoves(Position position);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract MoveList GetAllAttacks(Position position);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract MoveList GetAllForEvaluation(Position position);
}
