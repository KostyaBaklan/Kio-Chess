using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Collections;
using Engine.Interfaces;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class InitialSorter : InitialSorterBase<InitialMoveCollection>
    {
        public InitialSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
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
            //else if (IsGoodAttackForWhite())
            //{
            //    AttackCollection.AddSuggested(move);
            //}
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
            //else if (IsGoodAttackForBlack())
            //{
            //    AttackCollection.AddSuggested(move);
            //}
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
            //else if (IsGoodAttackForWhite())
            //{
            //    AttackCollection.AddSuggested(move);
            //}
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
            //else if (IsGoodAttackForBlack())
            //{
            //    AttackCollection.AddSuggested(move);
            //}
            else
            {
                AttackCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        #endregion


        protected override void InitializeMoveCollection()
        {
            AttackCollection = new InitialMoveCollection(Comparer);
        }
    }
}