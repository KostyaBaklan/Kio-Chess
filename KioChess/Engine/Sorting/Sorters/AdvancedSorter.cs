using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters
{
    public class AdvancedSorter : MoveSorter
    {
        private AdvancedMoveCollection AdvancedMoveCollection;
        public AdvancedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            AdvancedMoveCollection = new AdvancedMoveCollection(comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void FinalizeSort()
        {
            ProcessWinCaptures(AdvancedMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override MoveList GetMoves()
        {
            return AdvancedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndMove(MoveBase move)
        {
            AdvancedMoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleMove(MoveBase move)
        {
            AdvancedMoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningMove(MoveBase move)
        {
            AdvancedMoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCaptureMove(AttackBase move)
        {
            ProcessCapture(AdvancedMoveCollection, move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCastleMove(MoveBase move)
        {
            AdvancedMoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMove(MoveBase move)
        {
            AdvancedMoveCollection.AddHashMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessKillerMove(MoveBase move)
        {
            AdvancedMoveCollection.AddKillerMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndMove(MoveBase move)
        {
            AdvancedMoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleMove(MoveBase move)
        {
            AdvancedMoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteOpeningMove(MoveBase move)
        {
            AdvancedMoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionMoves(PromotionList promotions)
        {
            ProcessBlackPromotion(promotions, AdvancedMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionMoves(PromotionList promotions)
        {
            ProcessWhitePromotion(promotions, AdvancedMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionCaptures(PromotionAttackList promotions)
        {
            ProcessPromotionCaptures(promotions, AdvancedMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionCaptures(PromotionAttackList promotions)
        {
            ProcessPromotionCaptures(promotions, AdvancedMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMoves(PromotionList promotions)
        {
            AdvancedMoveCollection.AddHashMoves(promotions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMoves(PromotionAttackList promotions)
        {
            AdvancedMoveCollection.AddHashMoves(promotions);
        }
    }
}
