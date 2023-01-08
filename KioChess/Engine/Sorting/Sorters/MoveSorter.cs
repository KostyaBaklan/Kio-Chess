using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections;
using Engine.Interfaces;
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

        protected AttackCollection AttackCollection;
        protected MoveCollection MoveCollection;
        protected readonly IBoard Board;

        protected MoveSorter(IPosition position, IMoveComparer comparer)
        {
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

        public MoveBase[] Order(AttackList attacks, MoveList moves, MoveBase pvNode)
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

            var m = new MoveBase[moveList.Count];
            moveList.CopyTo(m, 0);
            return m;
        }

        public MoveBase[] Order(AttackList attacks)
        {
            if (attacks.Count == 0) return new MoveBase[0];
            if (attacks.Count == 1) return new MoveBase[]{attacks[0]};

            OrderAttacks(AttackCollection, attacks);

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

        protected abstract MoveBase[] OrderInternal(AttackList attacks, MoveList moves);
        protected abstract MoveBase[] OrderInternal(AttackList attacks, MoveList moves,  MoveBase pvNode);
    }
}