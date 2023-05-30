using Engine.Interfaces;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Collections;

namespace Engine.Sorting.Sorters
{
    public class ComplexSorter : InitialSorterBase<ComplexMoveCollection>
    {
        public ComplexSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteOpeningMove(MoveBase move)
        {
            Position.Make(move);
            bool hasResult = false;
            if (IsBadAttackToWhite())
            {
                AttackCollection.AddLooseNonCapture(move);
                hasResult = true;
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
                hasResult = true;
            }
            //else if (IsGoodAttackForWhite())
            //{
            //    AttackCollection.AddSuggested(move);
            //    hasResult = true;
            //}
            Position.UnMake();

            if (hasResult)
                return;

            switch (move.Piece)
            {
                case WhitePawn:
                    if ((move.From.AsBitBoard() & _whitePawnRank).Any())
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case WhiteKnight:
                case WhiteBishop:
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
                case WhiteRook:
                    if (move.From == A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        AttackCollection.AddBad(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }

                    break;
                case WhiteQueen:
                    if (MoveHistoryService.GetPly() < 7 || move.To == D1)
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case WhiteKing:
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningMove(MoveBase move)
        {
            Position.Make(move);

            bool hasResult = false;
            if (IsBadAttackToBlack())
            {
                AttackCollection.AddLooseNonCapture(move);
                hasResult = true;
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
                hasResult = true;
            }
            //else if (IsGoodAttackForBlack())
            //{
            //    AttackCollection.AddSuggested(move);
            //    hasResult = true;
            //}
            Position.UnMake();

            if (hasResult)
                return;

            switch (move.Piece)
            {
                case BlackPawn:
                    if ((move.From.AsBitBoard() & _blackPawnRank).Any())
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case BlackKnight:
                case BlackBishop:
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
                case BlackQueen:
                    if (MoveHistoryService.GetPly() < 8 || move.To == D8)
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case BlackRook:
                    if (move.From == A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        AttackCollection.AddBad(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }

                    break;
                case BlackKing:
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleMove(MoveBase move)
        {
            Position.Make(move); 
            
            bool hasResult = false;
            if (IsBadAttackToWhite())
            {
                AttackCollection.AddLooseNonCapture(move);
                hasResult = true;
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
                hasResult = true;
            }
            //else if (IsGoodAttackForWhite())
            //{
            //    AttackCollection.AddSuggested(move);
            //    hasResult = true;
            //}
            Position.UnMake();

            if (hasResult)
                return;

            switch (move.Piece)
            {
                case WhitePawn:
                    if (move.Piece == WhitePawn && move.To > H4 && Board.IsWhitePass(move.To))
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }

                    break;
                case WhiteKnight:
                case WhiteBishop:
                    if ((move.To.AsBitBoard() & _minorStartRanks).Any())
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }

                    break;
                case WhiteRook:
                    if (move.From == A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }

                    break;
                case WhiteKing:
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleMove(MoveBase move)
        {
            Position.Make(move);

            bool hasResult = false;
            if (IsBadAttackToBlack())
            {
                AttackCollection.AddLooseNonCapture(move);
                hasResult = true;
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
                hasResult = true;
            }
            //else if (IsGoodAttackForBlack())
            //{
            //    AttackCollection.AddSuggested(move);
            //    hasResult = true;
            //}
            Position.UnMake();

            if (hasResult) return;

            switch (move.Piece)
            {
                case BlackPawn:
                    if (move.Piece == BlackPawn && move.To < A5 && Board.IsBlackPass(move.To))
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case BlackKnight:
                case BlackBishop:
                    if ((move.To.AsBitBoard() & _minorStartRanks).Any())
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case BlackRook:
                    if (move.From == A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case BlackKing:
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
            else if (move.Piece == WhitePawn && Board.IsWhitePass(move.To)) // || IsGoodAttackForWhite())
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
            else if (move.Piece == BlackPawn && Board.IsBlackPass(move.To)) //|| IsGoodAttackForBlack())
            {
                AttackCollection.AddSuggested(move);
            }
            else
            {
                AttackCollection.AddNonCapture(move);
            }
            Position.UnMake();
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetValue()
        {
            StaticValue = Position.GetStaticValue();
        }

        protected override void InitializeMoveCollection()
        {
            AttackCollection = new ComplexMoveCollection(Comparer);
        }
    }
}
