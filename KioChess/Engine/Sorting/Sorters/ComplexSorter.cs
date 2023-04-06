using Engine.DataStructures.Moves.Lists;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Collections;

namespace Engine.Sorting.Sorters
{
    public class ComplexSorter : MoveSorter<ComplexMoveCollection>
    {
        private readonly BitBoard _minorStartRanks;
        private readonly BitBoard _perimeter;
        private readonly BitBoard _whitePawnRank;
        private readonly BitBoard _blackPawnRank;
        private readonly BitBoard _minorStartPositions;
        protected readonly PositionsList PositionsList;
        protected readonly AttackList Attacks;

        public ComplexSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            PositionsList = new PositionsList();
            Attacks = new AttackList();
            Comparer = comparer;
            _minorStartPositions = Squares.B1.AsBitBoard() | Squares.C1.AsBitBoard() | Squares.F1.AsBitBoard() |
                                   Squares.G1.AsBitBoard() | Squares.B8.AsBitBoard() | Squares.C8.AsBitBoard() |
                                   Squares.F8.AsBitBoard() | Squares.G8.AsBitBoard();
            _minorStartRanks = Board.GetRank(0) | Board.GetRank(7);
            _whitePawnRank = Board.GetRank(2);
            _blackPawnRank = Board.GetRank(5);
            _perimeter = Board.GetPerimeter();
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteOpeningMove(MoveBase move)
        {
            Position.Make(move);
            var isBad = IsBadAttackToWhite();
            Position.UnMake();

            if (isBad)
            {
                AttackCollection.AddLooseNonCapture(move);
            }
            else if (move.IsCheck)
            {
                AttackCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForWhite())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                switch (move.Piece)
                {
                    case Piece.WhitePawn:
                        if ((move.From.AsBitBoard() & _whitePawnRank).Any())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }
                        break;
                    case Piece.WhiteKnight:
                    case Piece.WhiteBishop:
                        if ((move.To.AsBitBoard() & _perimeter).Any())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                    case Piece.WhiteRook:
                        if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                            move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                        {
                            AttackCollection.AddBad(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                    case Piece.WhiteQueen:
                        if (MoveHistoryService.GetPly() < 7 || move.To == Squares.D1)
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }
                        break;
                    case Piece.WhiteKing:
                        if (!MoveHistoryService.IsLastMoveWasCheck() && !move.IsCastle && MoveHistoryService.CanDoWhiteCastle())
                        {
                            AttackCollection.AddBad(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningMove(MoveBase move)
        {
            Position.Make(move);
            var isBad = IsBadAttackToBlack();
            Position.UnMake();

            if (isBad)
            {
                AttackCollection.AddLooseNonCapture(move);
            }
            else if (move.IsCheck)
            {
                AttackCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForBlack())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}
            else
            {
                switch (move.Piece)
                {
                    case Piece.BlackPawn:
                        if ((move.From.AsBitBoard() & _blackPawnRank).Any())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }
                        break;
                    case Piece.BlackKnight:
                    case Piece.BlackBishop:
                        if ((move.To.AsBitBoard() & _perimeter).Any())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }

                        else if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                    case Piece.BlackQueen:
                        if (MoveHistoryService.GetPly() < 8 || move.To == Squares.D8)
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }
                        break;
                    case Piece.BlackRook:
                        if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                            move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                        {
                            AttackCollection.AddBad(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                    case Piece.BlackKing:
                        if (!MoveHistoryService.IsLastMoveWasCheck() && !move.IsCastle && MoveHistoryService.CanDoBlackCastle())
                        {
                            AttackCollection.AddBad(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleMove(MoveBase move)
        {
            Position.Make(move);
            var isBad = IsBadAttackToWhite();
            Position.UnMake();

            if (isBad)
            {
                AttackCollection.AddLooseNonCapture(move);
            }

            //if (IsGoodAttackForWhite())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}
            else if (move.IsCheck)
            {
                AttackCollection.AddSuggested(move);
            }
            else
            {
                switch (move.Piece)
                {
                    case Piece.WhitePawn:
                        if (move.Piece == Piece.WhitePawn && move.To > Squares.H4 && Board.IsWhitePass(move.To.AsByte()))
                        {
                            AttackCollection.AddSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                    case Piece.WhiteKnight:
                    case Piece.WhiteBishop:
                        if ((move.To.AsBitBoard() & _minorStartRanks).Any())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                    case Piece.WhiteRook:
                        if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                            move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                    case Piece.WhiteKing:
                        if (!MoveHistoryService.IsLastMoveWasCheck() && !move.IsCastle && MoveHistoryService.CanDoWhiteCastle())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                    default:
                        AttackCollection.AddNonCapture(move);
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleMove(MoveBase move)
        {
            Position.Make(move);
            var isBad = IsBadAttackToBlack();
            Position.UnMake();

            if (isBad)
            {
                AttackCollection.AddLooseNonCapture(move);
            }

            //if (IsGoodAttackForBlack())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}
            else if (move.IsCheck)
            {
                AttackCollection.AddSuggested(move);
            }
            else
            {
                switch (move.Piece)
                {
                    case Piece.BlackPawn:
                        if (move.Piece == Piece.BlackPawn && move.To < Squares.A5 && Board.IsBlackPass(move.To.AsByte()))
                        {
                            AttackCollection.AddSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }
                        break;
                    case Piece.BlackKnight:
                    case Piece.BlackBishop:
                        if ((move.To.AsBitBoard() & _minorStartRanks).Any())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }
                        break;
                    case Piece.BlackRook:
                        if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                            move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }
                        break;
                    case Piece.BlackKing:
                        if (!MoveHistoryService.IsLastMoveWasCheck() && !move.IsCastle && MoveHistoryService.CanDoBlackCastle())
                        {
                            AttackCollection.AddNonSuggested(move);
                        }
                        else
                        {
                            AttackCollection.AddNonCapture(move);
                        }

                        break;
                    default:
                        AttackCollection.AddNonCapture(move);
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndMove(MoveBase move)
        {
            Position.Make(move);
            var isBad = IsBadAttackToWhite();
            Position.UnMake();

            if (isBad)
            {
                AttackCollection.AddLooseNonCapture(move);
            }

            else if (move.IsCheck || move.Piece == Piece.WhitePawn && Board.IsWhitePass(move.To.AsByte()))
            {
                AttackCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForWhite())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                AttackCollection.AddNonCapture(move);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndMove(MoveBase move)
        {
            Position.Make(move);
            var isBad = IsBadAttackToBlack();
            Position.UnMake();

            if (isBad)
            {
                AttackCollection.AddLooseNonCapture(move);
            }
            else if (move.IsCheck || move.Piece == Piece.BlackPawn && Board.IsBlackPass(move.To.AsByte()))
            {
                AttackCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForBlack())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                AttackCollection.AddNonCapture(move);
            }
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
            for (int i = 0; i < Attacks.Count; i++)
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
            for (int i = 0; i < Attacks.Count; i++)
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
            AttackCollection = new ComplexMoveCollection(Comparer);
        }
    }
}
