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
        protected AdvancedMoveCollection AdvancedMoveCollection;

        public AdvancedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            AttackList = new AttackList();
            AdvancedMoveCollection = new AdvancedMoveCollection(comparer);
        }
        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
            OrderAttacks(AdvancedMoveCollection, attacks);

            if (Position.GetTurn() == Turn.White)
            {
                ProcessWhiteMoves(moves);
            }
            else
            {
                ProcessBlackMoves(moves);
            }

            return AdvancedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            if (pvNode is AttackBase attack)
            {
                OrderAttacks(AdvancedMoveCollection, attacks, attack);

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
                OrderAttacks(AdvancedMoveCollection, attacks);

                if (Position.GetTurn() == Turn.White)
                {
                    ProcessWhiteMoves(moves, pvNode.Key);
                }
                else
                {
                    ProcessBlackMoves(moves, pvNode.Key);
                }
            }

            return AdvancedMoveCollection.Build();
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
                    AdvancedMoveCollection.AddHashMove(move);
                }
                else
                {
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
                    AdvancedMoveCollection.AddHashMove(move);
                }
                else
                {
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
                    ProcessWhitePromotion(move, AdvancedMoveCollection);
                }
                else if (CurrentKillers.Contains(move.Key))
                {
                    AdvancedMoveCollection.AddKillerMove(move);
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
                    ProcessBlackPromotion(move, AdvancedMoveCollection);
                }
                else if (CurrentKillers.Contains(move.Key))
                {
                    AdvancedMoveCollection.AddKillerMove(move);
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
                AdvancedMoveCollection.AddNonSuggested(move);
            }
            else
            {
                AdvancedMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMove(MoveBase move)
        {
            Position.Make(move);
            if (IsBadAttackToWhite())
            {
                AdvancedMoveCollection.AddNonSuggested(move);
            }
            else
            {
                AdvancedMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();
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
