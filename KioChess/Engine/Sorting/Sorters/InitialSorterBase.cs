using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public abstract class InitialSorterBase<T> : MoveSorter<T> where T : InitialMoveCollection
    {
        protected readonly BitBoard _minorStartRanks;
        protected readonly BitBoard _perimeter;
        protected readonly BitBoard _whitePawnRank;
        protected readonly BitBoard _blackPawnRank;
        protected readonly BitBoard _minorStartPositions;
        protected readonly PositionsList PositionsList;
        protected readonly AttackList Attacks;

        public InitialSorterBase(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            PositionsList = new PositionsList();
            Attacks = new AttackList();
            Comparer = comparer;
            _minorStartPositions = B1.AsBitBoard() | C1.AsBitBoard() | F1.AsBitBoard() |
                                   G1.AsBitBoard() | B8.AsBitBoard() | C8.AsBitBoard() |
                                   F8.AsBitBoard() | G8.AsBitBoard();
            _minorStartRanks = Board.GetRank(0) | Board.GetRank(7);
            _whitePawnRank = Board.GetRank(2);
            _blackPawnRank = Board.GetRank(5);
            _perimeter = Board.GetPerimeter();
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
        internal override void ProcessWhiteOpeningCapture(AttackBase attack)
        {
            ProcessWhiteCapture(attack);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleCapture(AttackBase attack)
        {
            ProcessWhiteCapture(attack);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndCapture(AttackBase attack)
        {
            ProcessWhiteCapture(attack);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningCapture(AttackBase attack)
        {
            ProcessBlackCapture(attack);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleCapture(AttackBase attack)
        {
            ProcessBlackCapture(attack);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndCapture(AttackBase attack)
        {
            ProcessBlackCapture(attack);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteCapture(AttackBase attack)
        {
            Position.Make(attack);
            if (attack.IsCheck && !Position.AnyBlackMoves())
            {
                Position.UnMake();
                AttackCollection.AddMateMove(attack);
            }
            else
            {
                Position.UnMake();
                ProcessCaptureMove(attack);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackCapture(AttackBase attack)
        {
            Position.Make(attack);
            if (attack.IsCheck && !Position.AnyWhiteMoves())
            {
                Position.UnMake();
                AttackCollection.AddMateMove(attack);
            }
            else
            {
                Position.UnMake();
                ProcessCaptureMove(attack);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsGoodAttackForBlack()
        {
            GetBlackAttacks();
            return Attacks.Count > 0 && IsWinCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadAttackToBlack()
        {
            GetWhiteAttacks();
            return Attacks.Count > 0 && IsOpponentWinCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsGoodAttackForWhite()
        {
            GetWhiteAttacks();
            return Attacks.Count > 0 && IsWinCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadAttackToWhite()
        {
            GetBlackAttacks();
            return Attacks.Count > 0 && IsOpponentWinCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsWinCapture()
        {
            for (byte i = 0; i < Attacks.Count; i++)
            {
                var attack = Attacks[i];
                attack.Captured = Board.GetPiece(attack.To);

                if (Board.StaticExchange(attack) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsOpponentWinCapture()
        {
            for (byte i = 0; i < Attacks.Count; i++)
            {
                var attack = Attacks[i];
                attack.Captured = Board.GetPiece(attack.To);

                if (Board.StaticExchange(attack) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBlackAttacks()
        {
            Attacks.Clear();
            if (Position.CanBlackPromote())
            {
                Position.GetBlackPromotionAttacks(Attacks);
            }
            Position.GetBlackAttacks(Attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetWhiteAttacks()
        {
            Attacks.Clear();
            if (Position.CanWhitePromote())
            {
                Position.GetWhitePromotionAttacks(Attacks);
            }

            Position.GetWhiteAttacks(Attacks);
        }
    }
}