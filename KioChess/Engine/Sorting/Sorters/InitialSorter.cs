using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public abstract class InitialSorter : MoveSorter
    {
        private readonly BitBoard _minorStartRanks;
        private readonly BitBoard _perimeter;
        private readonly BitBoard _whitePawnRank;
        private readonly BitBoard _blackPawnRank;
        private readonly BitBoard _minorStartPositions;
        protected readonly PositionsList PositionsList;
        protected readonly AttackList Attacks;
        protected InitialMoveCollection InitialMoveCollection;

        protected InitialSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
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
            switch (move.Piece)
            {
                case Piece.WhitePawn:
                    if ((move.From.AsBitBoard() & _whitePawnRank).Any())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.WhiteKnight:
                case Piece.WhiteBishop:
                    if ((move.To.AsBitBoard() & _perimeter).Any())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteRook:
                    if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        InitialMoveCollection.AddBad(move);
                        return;
                    }

                    break;
                case Piece.WhiteQueen:
                    if (MoveHistoryService.GetPly() < 7 || move.To == Squares.D1)
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.WhiteKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck())
                    {
                        if (MoveHistoryService.CanDoWhiteCastle())
                        {
                            InitialMoveCollection.AddBad(move);
                        }
                        else
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                        }

                        return;
                    }

                    break;
            }

            Position.Make(move);
            if (IsBadAttackToWhite())
            {
                InitialMoveCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck)
            {
                InitialMoveCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForWhite())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                InitialMoveCollection.AddNonCapture(move);
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
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.BlackKnight:
                case Piece.BlackBishop:
                    if ((move.To.AsBitBoard() & _perimeter).Any())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.BlackQueen:
                    if (MoveHistoryService.GetPly() < 8 || move.To == Squares.D8)
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.BlackRook:
                    if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        InitialMoveCollection.AddBad(move);
                        return;
                    }

                    break;
                case Piece.BlackKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck())
                    {
                        if (MoveHistoryService.CanDoBlackCastle())
                        {
                            InitialMoveCollection.AddBad(move);
                        }
                        else
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                        }

                        return;
                    }

                    break;
            }


            Position.Make(move);
            if (IsBadAttackToBlack())
            {
                InitialMoveCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck)
            {
                InitialMoveCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForBlack())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                InitialMoveCollection.AddNonCapture(move);
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
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteRook:
                    if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
            }

            Position.Make(move);
            if (IsBadAttackToWhite())
            {
                InitialMoveCollection.AddNonSuggested(move);
            }

            //if (IsGoodAttackForWhite())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}
            else if (move.IsCheck || move.Piece == Piece.WhitePawn && move.To > Squares.H4 && Board.IsWhitePass(move.To.AsByte()))
            {
                InitialMoveCollection.AddSuggested(move);
            }

            else
            {
                InitialMoveCollection.AddNonCapture(move);
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
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.BlackRook:
                    if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        InitialMoveCollection.AddNonSuggested(move); return;
                    }

                    break;
                case Piece.BlackKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
            }


            Position.Make(move);

            if (IsBadAttackToBlack())
            {
                InitialMoveCollection.AddNonSuggested(move);
            }

            //if (IsGoodAttackForBlack())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}
            else if (move.IsCheck || move.Piece == Piece.BlackPawn && move.To < Squares.A5 && Board.IsBlackPass(move.To.AsByte()))
            {
                InitialMoveCollection.AddSuggested(move);
            }

            else
            {
                InitialMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndMove(MoveBase move)
        {
            Position.Make(move);

            if (IsBadAttackToWhite())
            {
                InitialMoveCollection.AddNonSuggested(move);
            }

            else if (move.IsCheck || move.Piece == Piece.WhitePawn && Board.IsWhitePass(move.To.AsByte()))
            {
                InitialMoveCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForWhite())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                InitialMoveCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndMove(MoveBase move)
        {
            Position.Make(move);

            if (IsBadAttackToBlack())
            {
                InitialMoveCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck || move.Piece == Piece.BlackPawn && Board.IsBlackPass(move.To.AsByte()))
            {
                InitialMoveCollection.AddSuggested(move);
            }

            //if (IsGoodAttackForBlack())
            //{
            //    InitialMoveCollection.AddSuggested(move);
            //    return;
            //}

            else
            {
                InitialMoveCollection.AddNonCapture(move);
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
            InitialMoveCollection.AddHashMoves(promotions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMoves(PromotionAttackList promotions)
        {
            InitialMoveCollection.AddHashMoves(promotions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionMoves(PromotionList promotions)
        {
            ProcessBlackPromotion(promotions, InitialMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionMoves(PromotionList promotions)
        {
            ProcessWhitePromotion(promotions, InitialMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMove(MoveBase move)
        {
            InitialMoveCollection.AddHashMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessKillerMove(MoveBase move)
        {
            InitialMoveCollection.AddKillerMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCaptureMove(AttackBase move)
        {
            ProcessCapture(InitialMoveCollection, move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionCaptures(PromotionAttackList promotions)
        {
            ProcessPromotionCaptures(promotions, InitialMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionCaptures(PromotionAttackList promotions)
        {
            ProcessPromotionCaptures(promotions, InitialMoveCollection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override MoveList GetMoves()
        {
            return InitialMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void FinalizeSort()
        {
            ProcessWinCaptures(InitialMoveCollection);
        }
    }
}