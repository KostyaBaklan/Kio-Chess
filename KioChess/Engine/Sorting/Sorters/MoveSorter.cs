using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public abstract class MoveSorter<T>:MoveSorterBase where T:AttackCollection
    {
        protected T AttackCollection;

        protected MoveSorter(IPosition position, IMoveComparer comparer):base(position, comparer)
        {
            InitializeMoveCollection();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override MoveList GetMoves()
        {
            return AttackCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCaptureMove(AttackBase attack)
        {
            attack.Captured = Board.GetPiece(attack.To);

            int attackValue = Board.StaticExchange(attack);
            if (attackValue > 0)
            {
                attack.See = attackValue;
                attackList.Add(attack);
            }
            else if (attackValue < 0)
            {
                AttackCollection.AddLooseCapture(attack);
            }
            else
            {
                AttackCollection.AddTrade(attack);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void FinalizeSort()
        {
            if (attackList.Count <= 0) return;

            if (attackList.Count > 1)
                attackList.SortBySee();

            AttackCollection.AddWinCapture(attackList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionCaptures(PromotionAttackList promotions)
        {
            ProcessPromotionCaptures(promotions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionCaptures(PromotionAttackList promotions)
        {
            ProcessPromotionCaptures(promotions);
        }

        protected abstract void InitializeMoveCollection();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessPromotionCaptures(PromotionAttackList promotions)
        {
            var attack = promotions[0];
            attack.Captured = Board.GetPiece(attack.To);

            int attackValue = Board.StaticExchange(attack);
            if (attackValue > 0)
            {
                attackList.Add(promotions,attackValue);
            }
            else
            {
                AttackCollection.AddLooseCapture(promotions);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessBlackPromotion(PromotionList moves)
        {
            Position.Make(moves[0]);
            Position.GetWhiteAttacksTo(moves[0].To, attackList);
            StaticBlackExchange(moves);
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessWhitePromotion(PromotionList moves)
        {
            Position.Make(moves[0]);
            Position.GetBlackAttacksTo(moves[0].To, attackList);
            StaticWhiteExchange(moves);
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void StaticWhiteExchange(PromotionList moves)
        {
            if (attackList.Count == 0)
            {
                AttackCollection.AddWinCapture(moves);
            }
            else
            {
                WhitePromotion(moves);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void StaticBlackExchange(PromotionList moves)
        {
            if (attackList.Count == 0)
            {
                AttackCollection.AddWinCapture(moves);
            }
            else
            {
                BlackPromotion(moves);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WhitePromotion(PromotionList moves)
        {
            int max = short.MinValue;
            for (int i = 0; i < attackList.Count; i++)
            {
                var attack = attackList[i];
                attack.Captured = Piece.WhitePawn;
                var see = Board.StaticExchange(attack);
                if (see > max)
                {
                    max = see;
                }
            }

            if (max < 0)
            {
                AttackCollection.AddWinCapture(moves);
            }
            else
            {
                AttackCollection.AddLooseCapture(moves);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BlackPromotion(PromotionList moves)
        {
            int max = short.MinValue;
            for (int i = 0; i < attackList.Count; i++)
            {
                var attack = attackList[i];
                attack.Captured = Piece.BlackPawn;
                var see = Board.StaticExchange(attack);
                if (see > max)
                {
                    max = see;
                }
            }

            if (max < 0)
            {
                AttackCollection.AddWinCapture(moves);
            }
            else
            {
                AttackCollection.AddLooseCapture(moves);
            }
        }
    }
}