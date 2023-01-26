using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public abstract class MoveSorter : IMoveSorter
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

        public MoveList Order(AttackList attacks, MoveList moves, MoveBase pvNode)
        {
            int depth = MoveHistoryService.GetPly();

            if (depth > 0 || pvNode!=null)
            {
                CurrentKillers = Moves[depth];
                return pvNode != null
                    ? OrderInternal(attacks, moves, pvNode)
                    : OrderInternal(attacks, moves);
            }

            MoveList moveList = new MoveList();
            foreach (var move in moves)
            {
                moveList.Add(move);
            }

            moveList.FullSort();
            return moveList;
        }

        public MoveList Order(AttackList attacks)
        {
            if (attacks.Count == 0)
            {
                return EmptyList;
            }

            attackList.Clear();

            for (int i = 0; i < attacks.Count; i++)
            {
                var attack = attacks[i];
                attack.Captured = Board.GetPiece(attack.To);

                var see = Board.StaticExchange(attack);

                if (see > 0)
                {
                    attack.See = see;
                    attackList.Add(attack);
                }
                else if (see < 0)
                {
                    AttackCollection.AddLooseCapture(attack);
                }
                else
                {
                    AttackCollection.AddTrade(attack);
                }
            }

            if (attackList.Count == 0)
                return AttackCollection.Build();

            if (attackList.Count > 1)
            {
                attackList.SortBySee();
            }

            AttackCollection.AddWinCapture(attackList);

            return AttackCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OrderAttacks(AttackCollection collection, AttackList sortedAttacks, AttackBase pv)
        {
            attackList.Clear();
            for (int i = 0; i < sortedAttacks.Count; i++)
            {
                var attack = sortedAttacks[i];
                if (attack.Key == pv.Key)
                {
                    collection.AddHashMove(attack);
                    continue;
                }

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

            if (attackList.Count <= 0) return;
            if (attackList.Count > 1)
            {
                attackList.SortBySee();
            }
            collection.AddWinCapture(attackList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OrderAttacks(AttackCollection collection,
            AttackList sortedAttacks)
        {
            attackList.Clear();
            for (int i = 0; i < sortedAttacks.Count; i++)
            {
                var attack = sortedAttacks[i]; 
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

            if (attackList.Count <= 0) return;
            if (attackList.Count > 1)
            {
                attackList.SortBySee();
            }
            collection.AddWinCapture(attackList);
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
            {
                attackList.SortBySee();
            }
            collection.AddWinCapture(attackList);
        }

        protected abstract MoveList OrderInternal(AttackList attacks, MoveList moves);
        protected abstract MoveList OrderInternal(AttackList attacks, MoveList moves,  MoveBase pvNode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessBlackPromotion(MoveBase move,AttackCollection ac)
        {
            Position.Make(move);
            MoveProvider.GetWhiteAttacksTo(move.To.AsByte(), attackList);
            StaticBlackExchange(move, ac);
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessWhitePromotion(MoveBase move, AttackCollection ac)
        {
            Position.Make(move);
            MoveProvider.GetBlackAttacksTo(move.To.AsByte(), attackList);
            StaticWhiteExchange(move, ac);
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void StaticWhiteExchange(MoveBase move, AttackCollection ac)
        {
            if (attackList.Count == 0)
            {
                ac.AddWinCapture(move);
            }
            else
            {

                ProcessPromotion(move, ac, Piece.WhitePawn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void StaticBlackExchange(MoveBase move, AttackCollection ac)
        {
            if (attackList.Count == 0)
            {
                ac.AddWinCapture(move);
            }
            else
            {
                ProcessPromotion(move, ac, Piece.BlackPawn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessPromotion(MoveBase move, AttackCollection ac, Piece pawn)
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
                ac.AddWinCapture(move);
            }
            else if (max > 0)
            {
                ac.AddLooseCapture(move);
            }
            else
            {
                ac.AddTrade(move);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessHashMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessKillerMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessCastleMove(MoveBase move);

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
        internal abstract void ProcessWhitePromotionMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackPromotionMove(MoveBase move);

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
    }
}