using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public class NullMoveSorter : MoveSorter<NullMoveCollection>
{
    public NullMoveSorter(Position position) : base(position) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndMove(MoveBase move) => AttackCollection.AddNonCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleMove(MoveBase move) => AttackCollection.AddNonCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningMove(MoveBase move) => AttackCollection.AddNonCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMove(MoveBase move) => AttackCollection.AddHashMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessKillerMove(MoveBase move) => AttackCollection.AddNonCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessCounterMove(MoveBase move) => AttackCollection.AddNonCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndMove(MoveBase move) => AttackCollection.AddNonCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleMove(MoveBase move) => AttackCollection.AddNonCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningMove(MoveBase move) => AttackCollection.AddNonCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionMoves(PromotionList promotions) => ProcessBlackPromotion(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhitePromotionMoves(PromotionList promotions) => ProcessWhitePromotion(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMoves(PromotionList promotions) => AttackCollection.AddHashMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMoves(PromotionAttackList promotions) => AttackCollection.AddHashMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningCapture(AttackBase move) => ProcessCaptureMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleCapture(AttackBase move) => ProcessCaptureMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndCapture(AttackBase move) => ProcessCaptureMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningCapture(AttackBase move) => ProcessCaptureMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleCapture(AttackBase move) => ProcessCaptureMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndCapture(AttackBase move) => ProcessCaptureMove(move);

    protected override void InitializeMoveCollection()
    {
        AttackCollection = new NullMoveCollection();
    }
}
