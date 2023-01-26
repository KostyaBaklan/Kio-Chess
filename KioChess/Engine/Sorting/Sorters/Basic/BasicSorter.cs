using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Collections.Advanced;
using Engine.DataStructures.Moves.Collections.Extended;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Enums;
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
        protected override MoveList OrderInternal(AttackList attacks, MoveList moves)
        {
            OrderAttacks(MoveCollection, attacks);

            ProcessMoves(moves);

            return MoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveList OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            if (pvNode is AttackBase attack)
            {
                OrderAttacks(MoveCollection, attacks, attack);

                ProcessMoves(moves);
            }
            else
            {
                OrderAttacks(MoveCollection, attacks);

                ProcessMoves(moves, pvNode.Key);
            }

            return MoveCollection.Build();
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
                            MoveCollection.AddHashMove(move);
                        }
                        else if (move.IsPromotion)
                        {
                            ProcessWhitePromotion(move, MoveCollection);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            MoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            MoveCollection.AddNonCapture(move);
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
                            MoveCollection.AddHashMove(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            MoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            MoveCollection.AddNonCapture(move);
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
                            MoveCollection.AddHashMove(move);
                        }
                        else if ( move.IsPromotion)
                        {
                            ProcessBlackPromotion(move, MoveCollection);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            MoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            MoveCollection.AddNonCapture(move);
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
                            MoveCollection.AddHashMove(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            MoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            MoveCollection.AddNonCapture(move);
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
                            ProcessWhitePromotion(move, MoveCollection);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            MoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            MoveCollection.AddNonCapture(move);
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
                            MoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            MoveCollection.AddNonCapture(move);
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
                            ProcessBlackPromotion(move, MoveCollection);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            MoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            MoveCollection.AddNonCapture(move);
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
                            MoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            MoveCollection.AddNonCapture(move);
                        }
                    }
                }
            }
        }
    }
}