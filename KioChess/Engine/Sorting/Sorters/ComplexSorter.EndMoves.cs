using Engine.DataStructures.Moves.Lists;
using Engine.Models.Enums;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public partial class ComplexSorter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndMove(MoveBase move)
    {
        Position.MakeWhite(move);

        bool hasResult = CheckWhiteResult(move);

        Position.UnMakeWhite();

        if (hasResult)
            return;

        switch (move.Piece)
        {
            case Pieces.WhitePawn:
                if (Board.IsWhitePass(move.To))
                {
                    AttackCollection.AddSuggested(move);
                }
                else if (Board.IsWhiteCandidate(move.From, move.To))
                {
                    AttackCollection.AddForwardMove(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            case Pieces.WhiteRook:
                if (Board.IsBehindWhitePassed(move.From, move.To) || Board.IsWhiteRookAttacksKingZone(move.From, move.To))
                {
                    AttackCollection.AddForwardMove(move);
                }
                else
                    AttackCollection.AddNonCapture(move);
                break;
            case Pieces.WhiteQueen:
                if (Board.IsWhiteQueenAttacksKingZone(move.From, move.To))
                {
                    AttackCollection.AddForwardMove(move);
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
    internal override void ProcessBlackEndMove(MoveBase move)
    {
        Position.MakeBlack(move);
        bool hasResult = CheckBlackResult(move);

        Position.UnMakeBlack();

        if (hasResult)
            return;

        switch (move.Piece)
        {
            case Pieces.BlackPawn:
                if (Board.IsBlackPass(move.To))
                {
                    AttackCollection.AddSuggested(move);
                }
                else if (Board.IsBlackCandidate(move.From, move.To))
                {
                    AttackCollection.AddForwardMove(move);
                }
                else
                {
                    AttackCollection.AddNonCapture(move);
                }
                break;
            case Pieces.BlackRook:
                if (Board.IsBehindBlackPassed(move.From, move.To) || Board.IsBlackRookAttacksKingZone(move.From, move.To))
                {
                    AttackCollection.AddForwardMove(move);
                }
                else
                    AttackCollection.AddNonCapture(move);
                break;
            case Pieces.BlackQueen:
                if (Board.IsBlackQueenAttacksKingZone(move.From, move.To))
                {
                    AttackCollection.AddForwardMove(move);
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
    internal override MoveList GetOpeningMoves() => AttackCollection.BuildOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetBookOpeningMoves() => AttackCollection.BuildBookOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetBookMiddleMoves() => AttackCollection.BuildBookMiddle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetMiddleMoves() => AttackCollection.BuildMiddle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetEndMoves() => AttackCollection.BuildEnd();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetBookEndMoves() => AttackCollection.BuildBookEnd();
}