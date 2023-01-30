using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Basic
{
    public class BasicSorter : MoveSorter
    {
        public BasicSorter(IPosition position, IMoveComparer comparer) : base(position,comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override MoveList GetMoves()
        {
            return MoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndMove(MoveBase move)
        {
            MoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleMove(MoveBase move)
        {
            MoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningMove(MoveBase move)
        {
            MoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionMove(MoveBase move)
        {
            ProcessBlackPromotion(move, MoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCaptureMove(AttackBase move)
        {
            ProcessCapture(MoveCollection, move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCastleMove(MoveBase move)
        {
            MoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMove(MoveBase move)
        {
            MoveCollection.AddHashMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessKillerMove(MoveBase move)
        {
            MoveCollection.AddKillerMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndMove(MoveBase move)
        {
            MoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleMove(MoveBase move)
        {
            MoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteOpeningMove(MoveBase move)
        {
            MoveCollection.AddNonCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionMove(MoveBase move)
        {
            ProcessWhitePromotion(move, MoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void FinalizeSort()
        {
            ProcessWinCaptures(MoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionMoves(PromotionList promotions)
        {
            ProcessBlackPromotion(promotions, MoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionMoves(PromotionList promotions)
        {
            ProcessWhitePromotion(promotions, MoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionCaptures(PromotionAttackList promotions)
        {
            ProcessPromotionCaptures(promotions, MoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionCaptures(PromotionAttackList promotions)
        {
            ProcessPromotionCaptures(promotions, MoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMoves(PromotionList promotions)
        {
            MoveCollection.AddHashMoves(promotions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMoves(PromotionAttackList promotions)
        {
            MoveCollection.AddHashMoves(promotions);
        }
    }
}