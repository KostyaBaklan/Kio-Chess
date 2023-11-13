using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public class SimpleSorter : MoveSorter<SimpleMoveCollection>
{
    public SimpleSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
    {
       
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMove(MoveBase move)
    {
        AttackCollection.AddHashMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessKillerMove(MoveBase move)
    {
        AttackCollection.AddKillerMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessCounterMove(MoveBase move)
    {
        AttackCollection.AddCounterMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionMoves(PromotionList promotions)
    {
        ProcessBlackPromotion(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhitePromotionMoves(PromotionList promotions)
    {
        ProcessWhitePromotion(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMoves(PromotionList promotions)
    {
        AttackCollection.AddHashMoves(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMoves(PromotionAttackList promotions)
    {
        AttackCollection.AddHashMoves(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddNonCapture(MoveBase move)
    {
        if (EvaluationService.IsForward(move))
            AttackCollection.AddForwardMove(move);
        else
            AttackCollection.AddNonCapture(move);
    }

    protected override void InitializeMoveCollection()
    {
       AttackCollection = new SimpleMoveCollection(Comparer);
    }
}
