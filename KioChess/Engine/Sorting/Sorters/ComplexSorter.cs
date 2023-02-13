using CommonServiceLocator;
using Engine.DataStructures.Moves.Collections.Initial;
using Engine.DataStructures.Moves.Lists;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Engine.DataStructures.Moves.Collections;

namespace Engine.Sorting.Sorters
{
    public class ComplexSorter : MoveSorter
    {
        private readonly BitBoard _minorStartRanks;
        private readonly BitBoard _perimeter;
        private readonly BitBoard _whitePawnRank;
        private readonly BitBoard _blackPawnRank;
        private readonly BitBoard _minorStartPositions;
        protected readonly PositionsList PositionsList;
        protected readonly AttackList Attacks;

        protected ComplexMoveCollection ComplexMoveCollection;

        protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

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

            ComplexMoveCollection = new ComplexMoveCollection(comparer);
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteOpeningMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case Piece.WhitePawn:
                    if ((move.From.AsBitBoard() & _whitePawnRank).Any())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.WhiteKnight:
                case Piece.WhiteBishop:
                    if ((move.To.AsBitBoard() & _perimeter).Any())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteRook:
                    if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        ComplexMoveCollection.AddBad(move);
                        return;
                    }

                    break;
                case Piece.WhiteQueen:
                    if (MoveHistoryService.GetPly() < 7 || move.To == Squares.D1)
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.WhiteKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck())
                    {
                        if (MoveHistoryService.CanDoWhiteCastle())
                        {
                            ComplexMoveCollection.AddBad(move);
                        }
                        else
                        {
                            ComplexMoveCollection.AddNonSuggested(move);
                        }

                        return;
                    }

                    break;
            }

            Position.Make(move);
            if (IsBadAttackToWhite())
            {
                ComplexMoveCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck)
            {
                ComplexMoveCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForWhite())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                ComplexMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case Piece.BlackPawn:
                    if ((move.From.AsBitBoard() & _blackPawnRank).Any())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.BlackKnight:
                case Piece.BlackBishop:
                    if ((move.To.AsBitBoard() & _perimeter).Any())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.BlackQueen:
                    if (MoveHistoryService.GetPly() < 8 || move.To == Squares.D8)
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.BlackRook:
                    if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        ComplexMoveCollection.AddBad(move);
                        return;
                    }

                    break;
                case Piece.BlackKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck())
                    {
                        if (MoveHistoryService.CanDoBlackCastle())
                        {
                            ComplexMoveCollection.AddBad(move);
                        }
                        else
                        {
                            ComplexMoveCollection.AddNonSuggested(move);
                        }

                        return;
                    }

                    break;
            }


            Position.Make(move);
            if (IsBadAttackToBlack())
            {
                ComplexMoveCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck)
            {
                ComplexMoveCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForBlack())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                ComplexMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case Piece.WhiteKnight:
                case Piece.WhiteBishop:
                    if ((move.To.AsBitBoard() & _minorStartRanks).Any())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteRook:
                    if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
            }

            Position.Make(move);
            if (IsBadAttackToWhite())
            {
                ComplexMoveCollection.AddNonSuggested(move);
            }

            //if (IsGoodAttackForWhite())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}
            else if (move.IsCheck || (move.Piece == Piece.WhitePawn && move.To > Squares.H4 && Board.IsWhitePass(move.To.AsByte())))
            {
                ComplexMoveCollection.AddSuggested(move);
            }

            else
            {
                ComplexMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case Piece.BlackKnight:
                case Piece.BlackBishop:
                    if ((move.To.AsBitBoard() & _minorStartRanks).Any())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.BlackRook:
                    if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        ComplexMoveCollection.AddNonSuggested(move); return;
                    }

                    break;
                case Piece.BlackKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                    {
                        ComplexMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
            }


            Position.Make(move);

            if (IsBadAttackToBlack())
            {
                ComplexMoveCollection.AddNonSuggested(move);
            }

            //if (IsGoodAttackForBlack())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}
            else if (move.IsCheck || (move.Piece == Piece.BlackPawn && move.To < Squares.A5 && Board.IsBlackPass(move.To.AsByte())))
            {
                ComplexMoveCollection.AddSuggested(move);
            }

            else
            {
                ComplexMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndMove(MoveBase move)
        {
            Position.Make(move);

            if (IsBadAttackToWhite())
            {
                ComplexMoveCollection.AddNonSuggested(move);
            }

            else if (move.IsCheck || move.Piece == Piece.WhitePawn && Board.IsWhitePass(move.To.AsByte()))
            {
                ComplexMoveCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForWhite())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                ComplexMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndMove(MoveBase move)
        {
            Position.Make(move);

            if (IsBadAttackToBlack())
            {
                ComplexMoveCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck || move.Piece == Piece.BlackPawn && Board.IsBlackPass(move.To.AsByte()))
            {
                ComplexMoveCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForBlack())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                ComplexMoveCollection.AddNonCapture(move);
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
            ComplexMoveCollection.AddHashMoves(promotions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMoves(PromotionAttackList promotions)
        {
            ComplexMoveCollection.AddHashMoves(promotions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionMoves(PromotionList promotions)
        {
            ProcessBlackPromotion(promotions, ComplexMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionMoves(PromotionList promotions)
        {
            ProcessWhitePromotion(promotions, ComplexMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMove(MoveBase move)
        {
            ComplexMoveCollection.AddHashMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessKillerMove(MoveBase move)
        {
            ComplexMoveCollection.AddKillerMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCaptureMove(AttackBase move)
        {
            ProcessCapture(ComplexMoveCollection, move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionCaptures(PromotionAttackList promotions)
        {
            ProcessPromotionCaptures(promotions, ComplexMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionCaptures(PromotionAttackList promotions)
        {
            ProcessPromotionCaptures(promotions, ComplexMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCastleMove(MoveBase move)
        {
            ComplexMoveCollection.AddSuggested(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override MoveList GetMoves()
        {
            return ComplexMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void FinalizeSort()
        {
            ProcessWinCaptures(ComplexMoveCollection);
        }
    }
}
