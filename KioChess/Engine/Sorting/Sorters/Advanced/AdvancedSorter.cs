using CommonServiceLocator;
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
        protected readonly AttackList AttackList;
        protected AdvancedMoveCollection InitialMoveCollection;
        protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        public AdvancedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            AttackList = new AttackList();
            InitialMoveCollection = new AdvancedMoveCollection(comparer);
        }
        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
            OrderAttacks(InitialMoveCollection, attacks);

            if (Position.GetTurn() == Turn.White)
            {
                ProcessWhiteMoves(moves);
            }
            else
            {
                ProcessBlackMoves(moves);
            }

            return InitialMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            if (pvNode is AttackBase attack)
            {
                OrderAttacks(InitialMoveCollection, attacks, attack);

                if (Position.GetTurn() == Turn.White)
                {
                    ProcessWhiteMoves(moves);
                }
                else
                {
                    ProcessBlackMoves(moves);
                }
            }
            else
            {
                OrderAttacks(InitialMoveCollection, attacks);

                if (Position.GetTurn() == Turn.White)
                {
                    ProcessWhiteMoves(moves, pvNode.Key);
                }
                else
                {
                    ProcessBlackMoves(moves, pvNode.Key);
                }
            }

            return InitialMoveCollection.Build();
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMoves(MoveList moves, short key)
        {
            for (var index = 0; index < moves.Count; index++)
            {
                var move = moves[index];
                if (move.Key == key)
                {
                    InitialMoveCollection.AddHashMove(move);
                }
                else
                {
                    if (move.IsPromotion)
                    {
                        ProcessBlackPromotion(move);
                    }
                    else if (CurrentKillers.Contains(move.Key))
                    {
                        InitialMoveCollection.AddKillerMove(move);
                    }
                    else
                    {
                        ProcessBlackMove(move);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMoves(MoveList moves, short key)
        {
            for (var index = 0; index < moves.Count; index++)
            {
                var move = moves[index];
                if (move.Key == key)
                {
                    InitialMoveCollection.AddHashMove(move);
                }
                else
                {
                    if (move.IsPromotion)
                    {
                        ProcessWhitePromotion(move);
                    }
                    else if (CurrentKillers.Contains(move.Key))
                    {
                        InitialMoveCollection.AddKillerMove(move);
                    }
                    else
                    {
                        ProcessWhiteMove(move);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMoves(MoveList moves)
        {
            for (var index = 0; index < moves.Count; index++)
            {
                var move = moves[index];
                if (move.IsPromotion)
                {
                    ProcessWhitePromotion(move);
                }
                else if (CurrentKillers.Contains(move.Key))
                {
                    InitialMoveCollection.AddKillerMove(move);
                }
                else
                {
                    ProcessWhiteMove(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMoves(MoveList moves)
        {
            for (var index = 0; index < moves.Count; index++)
            {
                var move = moves[index];
                if (move.IsPromotion)
                {
                    ProcessBlackPromotion(move);
                }
                else if (CurrentKillers.Contains(move.Key))
                {
                    InitialMoveCollection.AddKillerMove(move);
                }
                else
                {
                    ProcessBlackMove(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMove(MoveBase move)
        {
            Position.Make(move);
            if (IsBadAttackToBlack())
            {
                InitialMoveCollection.AddNonSuggested(move);
            }
            else
            {
                InitialMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMove(MoveBase move)
        {
            Position.Make(move);
            if (IsBadAttackToWhite())
            {
                InitialMoveCollection.AddNonSuggested(move);
            }
            else
            {
                InitialMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotion(MoveBase move)
        {
            Position.Make(move);
            MoveProvider.GetWhiteAttacksTo(move.To.AsByte(), AttackList);
            StaticBlackExchange(move);
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotion(MoveBase move)
        {
            Position.Make(move);
            MoveProvider.GetBlackAttacksTo(move.To.AsByte(), AttackList);
            StaticWhiteExchange(move);
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StaticWhiteExchange(MoveBase move)
        {
            if (AttackList.Count == 0)
            {
                InitialMoveCollection.AddWinCapture(move);
            }
            else
            {
                int max = short.MinValue;
                for (int i = 0; i < AttackList.Count; i++)
                {
                    var attack = AttackList[i];
                    attack.Captured = Piece.WhitePawn;
                    var see = Board.StaticExchange(attack);
                    if (see > max)
                    {
                        max = see;
                    }
                }

                if (max < 0)
                {
                    InitialMoveCollection.AddWinCapture(move);
                }
                else if (max > 0)
                {
                    InitialMoveCollection.AddLooseCapture(move);
                }
                else
                {
                    InitialMoveCollection.AddTrade(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StaticBlackExchange(MoveBase move)
        {
            if (AttackList.Count == 0)
            {
                InitialMoveCollection.AddWinCapture(move);
            }
            else
            {
                int max = short.MinValue;
                for (int i = 0; i < AttackList.Count; i++)
                {
                    var attack = AttackList[i];
                    attack.Captured = Piece.BlackPawn;
                    var see = Board.StaticExchange(attack);
                    if (see > max)
                    {
                        max = see;
                    }
                }

                if (max < 0)
                {
                    InitialMoveCollection.AddWinCapture(move);
                }
                else if (max > 0)
                {
                    InitialMoveCollection.AddLooseCapture(move);
                }
                else
                {
                    InitialMoveCollection.AddTrade(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBadAttackToBlack()
        {
            AttackList attacks = Position.GetWhiteAttacks();
            return attacks.Count > 0 && IsOpponentWinCapture(attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBadAttackToWhite()
        {
            AttackList attacks = Position.GetBlackAttacks();
            return attacks.Count > 0 && IsOpponentWinCapture(attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsOpponentWinCapture(AttackList attacks)
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                var attack = attacks[i];
                attack.Captured = Board.GetPiece(attack.To);

                int attackValue = Board.StaticExchange(attack);
                if (attackValue > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
