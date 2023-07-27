using Engine.Interfaces;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Collections;

namespace Engine.Sorting.Sorters
{
    public class ComplexSuggestedSorter : InitialSorterBase<ComplexMoveCollection>
    {
        private readonly HashSet<short> _openingSuggested = new HashSet<short>
        {
            7686,7687,7688,7689,7730,7732,7749,7750,8078,8079,8080,8081,8083,8099,8101,8102,8103,8104,11436,11437,11438,11439,11759,11760,11777,11778,12300,12301,12302,12303,12305,12321,12323,12324,12325,12326
        };

        public ComplexSuggestedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
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
                    AttackCollection.AddNonCapture(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
                hasResult = true;
            }

            Position.UnMake();

            if (hasResult) return;

            if (_openingSuggested.Contains(move.Key))
            {
                AttackCollection.AddSuggested(move);
                return;
            }

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
                    if (move.From != D1)
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    else
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    break;
                case WhiteKing:
                    if (move.IsCastle)
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
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
                    AttackCollection.AddNonCapture(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
                hasResult = true;
            }

            Position.UnMake();

            if (hasResult) return;

            if (_openingSuggested.Contains(move.Key))
            {
                AttackCollection.AddSuggested(move);
                return;
            }

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
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }

                    break;
                case BlackQueen:
                    if (move.From != D1)
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    else
                    {
                        AttackCollection.AddNonSuggested(move);
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
                    if (move.IsCastle && MoveHistoryService.CanDoBlackCastle())
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
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

            Position.UnMake();

            if (hasResult) return;

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

            Position.UnMake();

            if (hasResult)
                return;

            if (move.Piece == WhitePawn && Board.IsWhitePass(move.To))
            {
                AttackCollection.AddSuggested(move);
            }
            else
            {
                AttackCollection.AddNonCapture(move);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndMove(MoveBase move)
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

            Position.UnMake();

            if (hasResult)
                return;

            if (move.Piece == BlackPawn && Board.IsBlackPass(move.To))
            {
                AttackCollection.AddSuggested(move);
            }
            else
            {
                AttackCollection.AddNonCapture(move);
            }
        }

        #endregion

        protected override void InitializeMoveCollection()
        {
            AttackCollection = new ComplexMoveCollection(Comparer);
        }
    }
}
