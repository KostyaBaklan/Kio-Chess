using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections.Advanced;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters.Advanced
{
    public class AdvancedSorter : MoveSorter
    {
        private AdvancedMoveCollection AdvancedMoveCollection;
        public AdvancedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            AdvancedMoveCollection = new AdvancedMoveCollection(comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveList OrderInternal(AttackList attacks, MoveList moves)
        {
            OrderAttacks(AdvancedMoveCollection, attacks);

            ProcessMoves(moves);

            return AdvancedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveList OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            if (pvNode is AttackBase attack)
            {
                OrderAttacks(AdvancedMoveCollection, attacks, attack);

                ProcessMoves(moves);
            }
            else
            {
                OrderAttacks(AdvancedMoveCollection, attacks);

                ProcessMoves(moves, pvNode.Key);
            }

            return AdvancedMoveCollection.Build();
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
        internal override void ProcessBlackPromotionMove(MoveBase move)
        {
            ProcessBlackPromotion(move, AdvancedMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCaptureMove(AttackBase move)
        {
            ProcessCapture(AdvancedMoveCollection,move);
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
        internal override void ProcessWhitePromotionMove(MoveBase move)
        {
            ProcessWhitePromotion(move, AdvancedMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMoves(MoveList moves, short pvNode)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (Position.CanWhitePromote())
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.Key == pvNode)
                        {
                            AdvancedMoveCollection.AddHashMove(move);
                        }
                        else if (move.IsPromotion)
                        {
                            ProcessWhitePromotion(move, AdvancedMoveCollection);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            AdvancedMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            AdvancedMoveCollection.AddNonCapture(move);
                        }
                    }
                }
                else
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.Key == pvNode)
                        {
                            AdvancedMoveCollection.AddHashMove(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            AdvancedMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            AdvancedMoveCollection.AddNonCapture(move);
                        }
                    }
                }
            }
            else
            {
                if (Position.CanBlackPromote())
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.Key == pvNode)
                        {
                            AdvancedMoveCollection.AddHashMove(move);
                        }
                        else if (move.IsPromotion)
                        {
                            ProcessBlackPromotion(move, AdvancedMoveCollection);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            AdvancedMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            AdvancedMoveCollection.AddNonCapture(move);
                        }
                    }
                }
                else
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.Key == pvNode)
                        {
                            AdvancedMoveCollection.AddHashMove(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            AdvancedMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            AdvancedMoveCollection.AddNonCapture(move);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMoves(MoveList moves)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (Position.CanWhitePromote())
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.IsPromotion)
                        {
                            ProcessWhitePromotion(move, AdvancedMoveCollection);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            AdvancedMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            AdvancedMoveCollection.AddNonCapture(move);
                        }
                    }
                }
                else
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (CurrentKillers.Contains(move.Key))
                        {
                            AdvancedMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            AdvancedMoveCollection.AddNonCapture(move);
                        }
                    }
                }
            }
            else
            {
                if (Position.CanBlackPromote())
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.IsPromotion)
                        {
                            ProcessBlackPromotion(move, AdvancedMoveCollection);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            AdvancedMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            AdvancedMoveCollection.AddNonCapture(move);
                        }
                    }
                }
                else
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (CurrentKillers.Contains(move.Key))
                        {
                            AdvancedMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            AdvancedMoveCollection.AddNonCapture(move);
                        }
                    }
                }
            }
        }
    }
}
