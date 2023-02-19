using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{

    public abstract class MoveSorter 
    {
        protected readonly IKillerMoveCollection[] Moves;
        protected readonly AttackList attackList;
        protected readonly IMoveHistoryService MoveHistoryService;
        protected IMoveComparer Comparer;
        protected IKillerMoveCollection CurrentKillers;
        protected readonly IPosition Position;
        protected readonly MoveList EmptyList;

        protected AttackCollection AttackCollection;
        protected MoveCollection MoveCollection;
        protected readonly IBoard Board;
        protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
        protected readonly IDataPoolService DataPoolService = ServiceLocator.Current.GetInstance<IDataPoolService>();

        protected MoveSorter(IPosition position, IMoveComparer comparer)
        {
            EmptyList = new MoveList(0);
            attackList = new AttackList();
            Board = position.GetBoard();
            Comparer = comparer;
            Moves = ServiceLocator.Current.GetInstance<IKillerMoveCollectionFactory>().CreateMoves();
            Position = position;

            AttackCollection = new AttackCollection(comparer);
            MoveCollection = new MoveCollection(comparer);

            MoveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Add(short move)
        {
            Moves[MoveHistoryService.GetPly()].Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessCapture(AttackCollection collection, AttackBase attack)
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
                collection.AddLooseCapture(attack);
            }
            else
            {
                collection.AddTrade(attack);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessWinCaptures(AttackCollection collection)
        {
            if (attackList.Count <= 0) return;

            if (attackList.Count > 1)
                attackList.SortBySee();

            collection.AddWinCapture(attackList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessPromotionCaptures(PromotionAttackList promotions, AttackCollection collection)
        {
            var attack = promotions[0];
            attack.Captured = Board.GetPiece(attack.To);

            int attackValue = Board.StaticExchange(attack);
            if (attackValue > 0)
            {
                attackList.Add(promotions,attackValue);
            }
            else if (attackValue < 0)
            {
                collection.AddLooseCapture(promotions);
            }
            else
            {
                collection.AddTrade(promotions);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessBlackPromotion(PromotionList moves, AttackCollection ac)
        {
            Position.Make(moves[0]);
            MoveProvider.GetWhiteAttacksTo(moves[0].To.AsByte(), attackList);
            StaticBlackExchange(moves, ac);
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessWhitePromotion(PromotionList moves, AttackCollection ac)
        {
            Position.Make(moves[0]);
            MoveProvider.GetBlackAttacksTo(moves[0].To.AsByte(), attackList);
            StaticWhiteExchange(moves, ac);
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void StaticWhiteExchange(PromotionList moves, AttackCollection ac)
        {
            if (attackList.Count == 0)
            {
                ac.AddWinCapture(moves);
            }
            else
            {
                ProcessPromotion(moves, ac, Piece.WhitePawn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void StaticBlackExchange(PromotionList moves, AttackCollection ac)
        {
            if (attackList.Count == 0)
            {
                ac.AddWinCapture(moves);
            }
            else
            {
                ProcessPromotion(moves, ac, Piece.BlackPawn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessPromotion(PromotionList moves, AttackCollection ac, Piece pawn)
        {
            int max = short.MinValue;
            for (int i = 0; i < attackList.Count; i++)
            {
                var attack = attackList[i];
                attack.Captured = pawn;
                var see = Board.StaticExchange(attack);
                if (see > max)
                {
                    max = see;
                }
            }

            if (max < 0)
            {
                ac.AddWinCapture(moves);
            }
            else if (max > 0)
            {
                ac.AddLooseCapture(moves);
            }
            else
            {
                ac.AddTrade(moves);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessHashMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessKillerMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessCaptureMove(AttackBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract MoveList GetMoves();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessWhiteOpeningMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessWhiteMiddleMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessWhiteEndMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackOpeningMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackMiddleMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackEndMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsKiller(short key)
        {
            return CurrentKillers.Contains(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetKillers()
        {
            CurrentKillers = Moves[MoveHistoryService.GetPly()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void FinalizeSort();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void InitializeSort()
        {
            attackList.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessWhitePromotionMoves(PromotionList promotions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackPromotionMoves(PromotionList promotions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessWhitePromotionCaptures(PromotionAttackList promotionAttackList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackPromotionCaptures(PromotionAttackList promotionAttackList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        internal abstract void ProcessHashMoves(PromotionList promotions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessHashMoves(PromotionAttackList promotions);
    }
}