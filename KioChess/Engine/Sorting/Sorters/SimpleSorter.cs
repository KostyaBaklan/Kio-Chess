using Engine.DataStructures.Moves.Collections;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public class SimpleSorter : MoveSorter<SimpleMoveCollection>
{
    public SimpleSorter(Position position) : base(position)
    {

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessKillerMove(MoveBase move) => AttackCollection.AddKillerMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessCounterMove(MoveBase move) => AttackCollection.AddCounterMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningCapture(AttackBase attack) => ProcessWhiteCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleCapture(AttackBase attack) => ProcessWhiteCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndCapture(AttackBase attack) => ProcessWhiteCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningCapture(AttackBase attack) => ProcessBlackCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleCapture(AttackBase attack) => ProcessBlackCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndCapture(AttackBase attack) => ProcessBlackCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteCapture(AttackBase attack)
    {
        Position.MakeWhite(attack);
        if (attack.IsCheck)
        {
            if (!Position.AnyBlackMoves())
            {
                Position.UnMakeWhite();
                AttackCollection.AddMateMove(attack);
            }
            else if (!Board.AnyBlackAttackTo(attack.To))
            {
                Position.UnMakeWhite();
                attack.SetCapturedValue();
                AttackCollection.AddWinCapture(attack);
            }
            else
            {
                Position.UnMakeWhite();
                ProcessCaptureMove(attack);
            }
        }
        else
        {
            Position.UnMakeWhite();
            ProcessCaptureMove(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCapture(AttackBase attack)
    {
        Position.MakeBlack(attack);
        if (attack.IsCheck)
        {
            if (!Position.AnyWhiteMoves())
            {
                Position.UnMakeBlack();
                AttackCollection.AddMateMove(attack);
            }
            else if (!Board.AnyWhiteAttackTo(attack.To))
            {
                Position.UnMakeBlack();
                attack.SetCapturedValue();
                AttackCollection.AddWinCapture(attack);
            }
            else
            {
                Position.UnMakeBlack();
                ProcessCaptureMove(attack);
            }
        }
        else
        {
            Position.UnMakeBlack();
            ProcessCaptureMove(attack);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningMove(MoveBase move)
    {
        if (IsWhiteCheck(move)) return;

        switch (move.Piece)
        {
            case WhiteKnight:
            case WhiteBishop:
                if (Board.IsAttackedByBlackPawn(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else if ((move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero())
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
                else if (Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case WhiteQueen:
                if (move.From == D1)
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else if (Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            case WhiteKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            default: AttackCollection.AddNonCapture(move); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningMove(MoveBase move)
    {
        if (IsBlackCheck(move)) return;

        switch (move.Piece)
        {
            case BlackKnight:
            case BlackBishop:
                if (Board.IsAttackedByWhitePawn(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else if ((move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case BlackRook:
                if (move.From == A1 && MoveHistoryService.CanDoBlackBigCastle() ||
                    move.From == H1 && MoveHistoryService.CanDoBlackSmallCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else if (Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case BlackQueen:
                if (move.From == D8)
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else if (Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            case BlackKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            default: AttackCollection.AddNonCapture(move); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleMove(MoveBase move)
    {
        if (IsWhiteCheck(move)) return;

        switch (move.Piece)
        {
            case WhiteKnight:
            case WhiteBishop:
                if (Board.IsAttackedByBlackPawn(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case WhiteRook:
            case WhiteQueen:
                if (Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            case WhiteKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            default: AttackCollection.AddNonCapture(move); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleMove(MoveBase move)
    {
        if (IsBlackCheck(move)) return;

        switch (move.Piece)
        {
            case BlackKnight:
            case BlackBishop:
                if (Board.IsAttackedByWhitePawn(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case BlackRook:
            case BlackQueen:
                if (Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            case BlackKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            default: AttackCollection.AddNonCapture(move); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndMove(MoveBase move)
    {
        if (IsWhiteCheck(move)) return;

        switch (move.Piece)
        {
            case WhiteKnight:
            case WhiteBishop:
                if (Board.IsAttackedByBlackPawn(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case WhiteRook:
            case WhiteQueen:
                if (Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            default: AttackCollection.AddNonCapture(move); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndMove(MoveBase move)
    {
        if (IsBlackCheck(move)) return;

        switch (move.Piece)
        {
            case BlackKnight:
            case BlackBishop:
                if (Board.IsAttackedByWhitePawn(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case BlackRook:
            case BlackQueen:
                if (Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            default: AttackCollection.AddNonCapture(move); break;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackCheck(MoveBase move)
    {
        Position.MakeBlack(move);
        if (!move.IsCheck)
        {
            Position.UnMakeBlack();
            return false;
        }

        if (Position.AnyWhiteMoves())
        {
            AttackCollection.AddCheck(move);
        }
        else
        {
            AttackCollection.AddMateMove(move);
        }
        Position.UnMakeBlack();
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteCheck(MoveBase move)
    {
        Position.MakeWhite(move);
        if (!move.IsCheck)
        {
            Position.UnMakeWhite();
            return false;
        }

        if (Position.AnyBlackMoves())
        {
            AttackCollection.AddCheck(move);
        }
        else
        {
            AttackCollection.AddMateMove(move);
        }

        Position.UnMakeWhite();
        return true;
    }

    protected override void InitializeMoveCollection() => AttackCollection = new SimpleMoveCollection();
}
