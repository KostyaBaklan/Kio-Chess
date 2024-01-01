﻿using Engine.DataStructures.Moves.Collections;
using Engine.Interfaces;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public class SimpleSorter : CommonMoveSorter<SimpleMoveCollection>
{
    public SimpleSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
    {
       
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningMove(MoveBase move)
    {
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
                }
                break;
            case WhiteKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            default:AddNonCapture(move);break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningMove(MoveBase move)
    {
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
                }
                break;
            case BlackKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            default: AddNonCapture(move); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleMove(MoveBase move)
    {
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
                }
                break;
            case WhiteKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            default: AddNonCapture(move); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleMove(MoveBase move)
    {
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
                }
                break;
            case BlackKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            default: AddNonCapture(move); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndMove(MoveBase move)
    {
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
                }
                break;
            default: AddNonCapture(move); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndMove(MoveBase move)
    {
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
                    AddNonCapture(move);
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
                    AddNonCapture(move);
                }
                break;
            default: AddNonCapture(move); break;
        }
    }

    protected override void InitializeMoveCollection()
    {
       AttackCollection = new SimpleMoveCollection(Comparer);
    }
}
