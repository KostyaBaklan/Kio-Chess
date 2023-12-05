using Engine.Interfaces;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Collections;

namespace Engine.Sorting.Sorters;

public class ComplexSorter : ExtendedSorterBase<ComplexMoveCollection>
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

        Position.UnMake();

        if (hasResult) return;

        switch (move.Piece)
        {
            case WhitePawn:
                AddNonCapture(move);
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
                }

                break;
            case WhiteQueen:
                AttackCollection.AddNonSuggested(move);
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
                    AddNonCapture(move);
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

        Position.UnMake();

        if (hasResult) return;

        switch (move.Piece)
        {
            case BlackPawn:
                AddNonCapture(move);
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
                    AddNonCapture(move);
                }

                break;
            case BlackQueen:
                AttackCollection.AddNonSuggested(move);
                break;
            case BlackRook:
                if (move.From == A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                    move.From == H8 && MoveHistoryService.CanDoBlackSmallCastle())
                {
                    AttackCollection.AddBad(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            case BlackKing:
                if (move.IsCastle)
                {
                    AttackCollection.AddSuggested(move);
                }
                else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                {
                    AttackCollection.AddBad(move);
                }
                else
                {
                    AddNonCapture(move);
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
                if (Board.IsWhitePass(move.To))
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
                }

                break;
            case WhiteKing:
                if (move.IsCastle)
                {
                    AttackCollection.AddSuggested(move);
                }
                else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            default:
                AddNonCapture(move);
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
                if (Board.IsBlackPass(move.To))
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
                }
                break;
            case BlackKing:
                if (move.IsCastle)
                {
                    AttackCollection.AddSuggested(move);
                }
                else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            default:
                AddNonCapture(move);
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
            AddNonCapture(move);
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
            AddNonCapture(move);
        }
    }

    #endregion

    protected override void InitializeMoveCollection()
    {
        AttackCollection = new ComplexMoveCollection(Comparer);
    }
}
