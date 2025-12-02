using Engine.DataStructures.Moves.Collections;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting;
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
    internal override void ProcessCountermoveHistoryMove(MoveBase move) => AttackCollection.AddCountermoveHistory(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningCapture(AttackBase move) => ProcessCaptureMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleCapture(AttackBase move) => ProcessCaptureMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndCapture(AttackBase move) => ProcessCaptureMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningCapture(AttackBase move) => ProcessCaptureMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleCapture(AttackBase move) => ProcessCaptureMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndCapture(AttackBase move) => ProcessCaptureMove(move);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningMove(MoveBase move)
    {
        switch (move.Piece)
        {
            case Pieces.WhiteKnight:
            case Pieces.WhiteBishop:
                if (Board.IsAttackedByBlackPawn(move.To) || (move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case Pieces.WhiteRook:
                if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                    move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle() || Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case Pieces.WhiteQueen:
                if (move.From == Squares.D1 || Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            case Pieces.WhiteKing:
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
        switch (move.Piece)
        {
            case Pieces.BlackKnight:
            case Pieces.BlackBishop:
                if (Board.IsAttackedByWhitePawn(move.To) || (move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case Pieces.BlackRook:
                if (move.From == Squares.A1 && MoveHistoryService.CanDoBlackBigCastle() ||
                    move.From == Squares.H1 && MoveHistoryService.CanDoBlackSmallCastle() || Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case Pieces.BlackQueen:
                if (move.From == Squares.D8 || Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            case Pieces.BlackKing:
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
        switch (move.Piece)
        {
            case Pieces.WhiteKnight:
            case Pieces.WhiteBishop:
                if (Board.IsAttackedByBlackPawn(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case Pieces.WhiteRook:
            case Pieces.WhiteQueen:
                if (Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            case Pieces.WhiteKing:
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
        switch (move.Piece)
        {
            case Pieces.BlackKnight:
            case Pieces.BlackBishop:
                if (Board.IsAttackedByWhitePawn(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case Pieces.BlackRook:
            case Pieces.BlackQueen:
                if (Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            case Pieces.BlackKing:
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
        switch (move.Piece)
        {
            case Pieces.WhiteKnight:
            case Pieces.WhiteBishop:
                if (Board.IsAttackedByBlackPawn(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case Pieces.WhiteRook:
            case Pieces.WhiteQueen:
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
        switch (move.Piece)
        {
            case Pieces.BlackKnight:
            case Pieces.BlackBishop:
                if (Board.IsAttackedByWhitePawn(move.To))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }

                break;
            case Pieces.BlackRook:
            case Pieces.BlackQueen:
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

    protected override void InitializeMoveCollection() => AttackCollection = new SimpleMoveCollection();
}
