using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public partial class ComplexSorter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningMove(MoveBase move)
    {
        Position.MakeWhite(move);

        bool hasResult = CheckWhiteResult(move);

        if (!hasResult)
        {
            switch (move.Piece)
            {
                case Pieces.WhitePawn:
                    if (MoveHistoryService.GetPly() < 12 && ((move.From == Squares.H2 && move.To == Squares.H4) || (move.From == Squares.G2 && move.To == Squares.G4) || (move.From == Squares.A2 && move.To == Squares.A4) || (move.From == Squares.B2 && move.To == Squares.B4)))
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else if (move.From == Squares.D2 || move.From == Squares.E2)
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if (move.From == Squares.C2 || Board.IsWhitePawnFork(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.WhiteKnight:
                    if (MoveHistoryService.GetPly() < 12 && ((move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero()))
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsWhiteKnightAttacksKingZone(move.From, move.To)
                        || _whiteForpost.IsSet(move.To) || Board.IsWhiteKnightFork(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.WhiteBishop:
                    if (MoveHistoryService.GetPly() < 12 && ((move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero()))
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsWhiteBishopAttacksKingZone(move.From, move.To)
                        || Board.IsWhiteBishopPin(move.To) || _whiteForpost.IsSet(move.To) || Board.IsWhiteBishopFork(move.To) || Board.IsWhiteBishopBattary(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.WhiteRook:
                    if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        AttackCollection.AddBad(move);
                    }
                    else if (Board.IsWhiteRookOnOpenFile(move.From, move.To) || Board.IsWhiteRookPin(move.To) || Board.IsWhiteRookBattary(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.WhiteQueen:
                    if (move.From == Squares.D1)
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else if (Board.IsWhiteQueenPin(move.To) || Board.IsWhiteQueenBattary(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.WhiteKing:
                    if (move.IsCastle)
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if (!MoveHistoryService.IsLastMoveWasCheck())
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

        Position.UnMakeWhite();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningMove(MoveBase move)
    {
        Position.MakeBlack(move);
        bool hasResult = CheckBlackResult(move);

        if (!hasResult)
        {
            switch (move.Piece)
            {
                case Pieces.BlackPawn:
                    if (MoveHistoryService.GetPly() < 12 && ((move.From == Squares.H7 && move.To == Squares.H5) || (move.From == Squares.G7 && move.To == Squares.G5) || (move.From == Squares.A7 && move.To == Squares.A5) || (move.From == Squares.B7 && move.To == Squares.B5)))
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else if (move.From == Squares.D7 || move.From == Squares.E7)
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if (move.From == Squares.C7 || Board.IsBlackPawnFork(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackKnight:
                    if (MoveHistoryService.GetPly() < 12 && ((move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero()))
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsBlackKnightAttacksKingZone(move.From, move.To)
                        || _blackForpost.IsSet(move.To) || Board.IsBlackKnightFork(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackBishop:
                    if (MoveHistoryService.GetPly() < 12 && ((move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero()))
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsBlackBishopAttacksKingZone(move.From, move.To)
                        || Board.IsBlackBishopPin(move.To) || _blackForpost.IsSet(move.To) || Board.IsBlackBishopFork(move.To) || Board.IsBlackBishopBattary(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackRook:
                    if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        AttackCollection.AddBad(move);
                    }
                    else if (Board.IsBlackRookOnOpenFile(move.From, move.To) || Board.IsBlackRookPin(move.To) || Board.IsBlackRookBattary(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackQueen:
                    if (move.From == Squares.D8)
                    {
                        AttackCollection.AddNonSuggested(move);
                    }
                    else if (Board.IsBlackQueenPin(move.To) || Board.IsBlackQueenBattary(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackKing:
                    if (move.IsCastle)
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if (!MoveHistoryService.IsLastMoveWasCheck())
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

        Position.UnMakeBlack();
    }
}