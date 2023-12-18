using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Evaluation;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts;

public abstract class SortContext
{
    public bool HasPv;
    public bool IsPvCapture;
    public short Pv;
    public short CounterMove;
    public MoveSorterBase MoveSorter;
    public byte[] Pieces;
    public SquareList[] Squares;
    public SquareList PromotionSquares;
    public int Ply;

    public static IPosition Position;
    public static IMoveHistoryService MoveHistory;
    public static IDataPoolService DataPoolService;
    public IEvaluationService EvaluationService;

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
    protected void SetInternal(MoveSorterBase sorter, MoveBase pv = null)
    {
        MoveSorter = sorter;
        MoveSorter.SetKillers();
        CounterMove = sorter.GetCounterMove();
        MoveSorter.EvaluationService = EvaluationService;
        MoveSorter.SetValues();

        if (pv != null)
        {
            HasPv = true;
            Pv = pv.Key;
            IsPvCapture = pv.IsAttack;
        }
        else
        {
            HasPv = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void Set(MoveSorterBase sorter, MoveBase pv = null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetForEvaluation(MoveSorterBase sorter)
    {
        MoveSorter = sorter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessHashMove(MoveBase move)
    {
        MoveSorter.ProcessHashMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessKillerMove(MoveBase move)
    {
        MoveSorter.ProcessKillerMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessCounterMove(MoveBase move)
    {
        MoveSorter.ProcessCounterMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessCaptureMove(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsKiller(short key)
    {
        return MoveSorter.IsKiller(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual MoveList GetMoves() { return MoveSorter.GetMoves(); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetAttacks() { return MoveSorter.GetMoves(); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessPromotionMoves(PromotionList promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessPromotionCaptures(PromotionAttackList promotionAttackList);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessHashMoves(PromotionList promotions)
    {
        MoveSorter.ProcessHashMoves(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ProcessHashMoves(PromotionAttackList promotions)
    {
        MoveSorter.ProcessHashMoves(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddSuggestedBookMove(MoveBase move)
    {
        MoveSorter.AddSuggestedBookMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract bool IsRegularMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract MoveList GetAllMoves(IPosition position);
}
