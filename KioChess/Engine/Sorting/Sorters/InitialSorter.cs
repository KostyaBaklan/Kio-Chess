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
    public class InitialSorter : MoveSorter<InitialMoveCollection>
    {
        private readonly BitBoard _minorStartRanks;
        private readonly BitBoard _perimeter;
        private readonly BitBoard _whitePawnRank;
        private readonly BitBoard _blackPawnRank;
        private readonly BitBoard _minorStartPositions;
        protected readonly PositionsList PositionsList;
        protected readonly AttackList Attacks;
        public InitialSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
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

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteOpeningMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case WhitePawn:
                    if ((move.From.AsBitBoard() & _whitePawnRank).Any())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case WhiteKnight:
                case WhiteBishop:
                    if ((move.To.AsBitBoard() & _perimeter).Any())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }

                    if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case WhiteRook:
                    if (move.From == A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        AttackCollection.AddBad(move);
                        return;
                    }

                    break;
                case WhiteQueen:
                    if (MoveHistoryService.GetPly() < 7 || move.To == D1)
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case WhiteKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck() && !move.IsCastle && MoveHistoryService.CanDoWhiteCastle())
                    {
                        AttackCollection.AddBad(move);
                        return;
                    }

                    break;
            }

            Position.Make(move);
            if (IsBadAttackToWhite())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck)
            {
                if (Position.AnyBlackMoves())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }
            else
            {
                AttackCollection.AddNonCapture(move);
            }
            Position.UnMake();

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case BlackPawn:
                    if ((move.From.AsBitBoard() & _blackPawnRank).Any())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case BlackKnight:
                case BlackBishop:
                    if ((move.To.AsBitBoard() & _perimeter).Any())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }

                    if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case BlackQueen:
                    if (MoveHistoryService.GetPly() < 8 || move.To == D8)
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case BlackRook:
                    if (move.From == A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        AttackCollection.AddBad(move);
                        return;
                    }

                    break;
                case BlackKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck() && !move.IsCastle && MoveHistoryService.CanDoBlackCastle())
                    {
                        AttackCollection.AddBad(move);
                        return;
                    }

                    break;
            }


            Position.Make(move);
            if (IsBadAttackToBlack())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck)
            {
                if (Position.AnyWhiteMoves())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }
            else
            {
                AttackCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case WhiteKnight:
                case WhiteBishop:
                    if ((move.To.AsBitBoard() & _minorStartRanks).Any())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case WhiteRook:
                    if (move.From == A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case WhiteKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck() && !move.IsCastle && MoveHistoryService.CanDoWhiteCastle())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
            }

            Position.Make(move);
            if (IsBadAttackToWhite())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck)
            {
                if (Position.AnyBlackMoves())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }
            else if (move.Piece == WhitePawn && move.To > H4 && Board.IsWhitePass(move.To))
            {
                AttackCollection.AddSuggested(move);
            }
            else
            {
                AttackCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case BlackKnight:
                case BlackBishop:
                    if ((move.To.AsBitBoard() & _minorStartRanks).Any())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case BlackRook:
                    if (move.From == A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        AttackCollection.AddNonSuggested(move); return;
                    }

                    break;
                case BlackKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck() && !move.IsCastle && MoveHistoryService.CanDoBlackCastle())
                    {
                        AttackCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
            }

            Position.Make(move);

            if (IsBadAttackToBlack())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck)
            {
                if (Position.AnyWhiteMoves())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }
            else if (move.Piece == BlackPawn && move.To < A5 && Board.IsBlackPass(move.To))
            {
                AttackCollection.AddSuggested(move);
            }

            else
            {
                AttackCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndMove(MoveBase move)
        {
            Position.Make(move);

            if (IsBadAttackToWhite())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck)
            {
                if (Position.AnyBlackMoves())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }
            else if (move.Piece == WhitePawn && Board.IsWhitePass(move.To))
            {
                AttackCollection.AddSuggested(move);
            }
            else
            {
                AttackCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndMove(MoveBase move)
        {
            Position.Make(move);

            if (IsBadAttackToBlack())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck)
            {
                if (Position.AnyWhiteMoves())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }
            else if (move.Piece == BlackPawn && Board.IsBlackPass(move.To))
            {
                AttackCollection.AddSuggested(move);
            }
            else
            {
                AttackCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsGoodAttackForBlack()
        {
            Attacks.Clear();
            if (Position.CanBlackPromote())
            {
                Position.GetBlackPromotionAttacks(Attacks);
            }
            Position.GetBlackAttacks(Attacks);
            return Attacks.Count > 0 && IsWinCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBadAttackToBlack()
        {
            Attacks.Clear();
            if (Position.CanWhitePromote())
            {
                Position.GetWhitePromotionAttacks(Attacks);
            }

            Position.GetWhiteAttacks(Attacks);
            return Attacks.Count > 0 && IsOpponentWinCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsGoodAttackForWhite()
        {
            Attacks.Clear();
            if (Position.CanWhitePromote())
            {
                Position.GetWhitePromotionAttacks(Attacks);
            }

            Position.GetWhiteAttacks(Attacks);
            return Attacks.Count > 0 && IsWinCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBadAttackToWhite()
        {
            Attacks.Clear();
            if (Position.CanBlackPromote())
            {
                Position.GetBlackPromotionAttacks(Attacks);
            }
            Position.GetBlackAttacks(Attacks);
            return Attacks.Count > 0 && IsOpponentWinCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsWinCapture()
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
        private bool IsOpponentWinCapture()
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

        #endregion

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

        protected override void InitializeMoveCollection()
        {
            AttackCollection = new InitialMoveCollection(Comparer);
        }
    }
}