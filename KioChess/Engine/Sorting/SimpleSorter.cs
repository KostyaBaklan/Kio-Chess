using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting;

public class SimpleSorter : MoveSorterBase
{
    public SimpleSorter(Position position) : base(position)
    {

    }

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
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            case Pieces.WhiteRook:
                if ((MoveHistoryService.GetPly() < 12 && ((move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle()) ||
                        (move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle()))) ||
                        Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            case Pieces.WhiteQueen:
                if (move.From == Squares.D1 || Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }
                break;
            case Pieces.WhiteKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            default: MoveCollection.AddNonCapture(move); break;
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
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            case Pieces.BlackRook:
                if ((MoveHistoryService.GetPly() < 12 && ((move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle()) || (move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle()))) ||
                        Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            case Pieces.BlackQueen:
                if (move.From == Squares.D8 || Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }
                break;
            case Pieces.BlackKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            default: MoveCollection.AddNonCapture(move); break;
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
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            case Pieces.WhiteRook:
            case Pieces.WhiteQueen:
                if (Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }
                break;
            case Pieces.WhiteKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            default: MoveCollection.AddNonCapture(move); break;
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
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            case Pieces.BlackRook:
            case Pieces.BlackQueen:
                if (Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }
                break;
            case Pieces.BlackKing:
                if (!move.IsCastle && !MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            default: MoveCollection.AddNonCapture(move); break;
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
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            case Pieces.WhiteRook:
            case Pieces.WhiteQueen:
                if (Board.IsAttackedByBlackPawn(move.To) || Board.IsAttackedByBlackKnight(move.To) || Board.IsAttackedByBlackBishop(move.To))
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }
                break;
            default: MoveCollection.AddNonCapture(move); break;
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
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }

                break;
            case Pieces.BlackRook:
            case Pieces.BlackQueen:
                if (Board.IsAttackedByWhitePawn(move.To) || Board.IsAttackedByWhiteKnight(move.To) || Board.IsAttackedByWhiteBishop(move.To))
                {
                    MoveCollection.AddNonSuggested(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }
                break;
            default: MoveCollection.AddNonCapture(move); break;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void GetMoves(ref MoveHistoryList moves) => MoveCollection.BuildSimple(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void GetBookMoves(ref MoveHistoryList moves) => MoveCollection.BuildSimpleBook(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionMoves(PromotionList moves)
    {
        Position.MakeBlack(moves[0]);
        AttackBase attack = Board.GetWhiteAttackToForPromotion(moves[0].To);
        if (attack == null)
        {
            AddWinCapture(moves);
        }
        else
        {
            attack.Captured = Pieces.BlackPawn;

            PromotionStaticExchange(moves, attack);
        }
        Position.UnMakeBlack();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhitePromotionMoves(PromotionList moves)
    {
        Position.MakeWhite(moves[0]);

        AttackBase attack = Board.GetBlackAttackToForPromotion(moves[0].To);
        if (attack == null)
        {
            AddWinCapture(moves);
        }
        else
        {
            attack.Captured = Pieces.WhitePawn;

            PromotionStaticExchange(moves, attack);
        }
        Position.UnMakeWhite();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhitePromotionCaptures(PromotionAttackList moves)
    {
        Position.MakeWhite(moves[0]);

        AttackBase attack = Board.GetBlackAttackToForPromotion(moves[0].To);
        if (attack == null)
        {
            Position.UnMakeWhite();
            AddWinCapture(moves, Board.GetPiece(moves[0].To));
        }
        else
        {
            attack.Captured = Pieces.WhitePawn;

            PromotionStaticExchange(moves, attack);
            Position.UnMakeWhite();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionCaptures(PromotionAttackList moves)
    {
        Position.MakeBlack(moves[0]);
        AttackBase attack = Board.GetWhiteAttackToForPromotion(moves[0].To);
        if (attack == null)
        {
            Position.UnMakeBlack();
            AddWinCapture(moves, Board.GetPiece(moves[0].To));
        }
        else
        {
            attack.Captured = Pieces.BlackPawn;
            PromotionStaticExchange(moves, attack);
            Position.UnMakeBlack();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PromotionStaticExchange(PromotionList moves, AttackBase attack)
    {
        int see = -Board.StaticExchangeWithPins(attack);

        if (see > 0)
        {
            MoveCollection.AddWinCaptures(moves, see);
        }
        else
        {
            MoveCollection.AddLooseCaptures(moves, see);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PromotionStaticExchange(PromotionAttackList moves, AttackBase attack)
    {
        int see = -Board.StaticExchangeWithPins(attack);

        if (see > 0)
        {
            MoveCollection.AddWinCaptures(moves, see);
        }
        else
        {
            MoveCollection.AddLooseCaptures(moves, see);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddWinCapture(PromotionList moves)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            var move = moves[i];
            move.SetSee();
            MoveCollection.AddWinCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddWinCapture(PromotionAttackList moves, byte captured)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            var move = moves[i];
            move.SetSee(captured);
            MoveCollection.AddWinCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessCaptureMove(AttackBase attack)
    {
        attack.SetCapturedPiece();
        int attackValue = Board.StaticExchangeWithPins(attack);
        if (attackValue > 0)
        {
            attack.See = attackValue;
            MoveCollection.AddWinCapture(attack);
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            MoveCollection.AddLooseCapture(attack);
        }
        else
        {
            MoveCollection.AddTrade(attack);
        }
    }
}
