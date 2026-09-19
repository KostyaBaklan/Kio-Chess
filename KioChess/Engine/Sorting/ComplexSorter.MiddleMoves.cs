using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public partial class ComplexSorter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleMove(MoveBase move)
    {
        Position.MakeWhite(move);

        bool hasResult = CheckWhiteResult(move);

        if (!hasResult)
        {
            switch (move.Piece)
            {
                case Pieces.WhitePawn:
                    if (Board.IsWhitePass(move.To) || move.From == Squares.D2 || move.From == Squares.E2)
                    {
                        MoveCollection.AddSuggested(move);
                    }
                    else if (move.From == Squares.C2 || Board.IsWhiteCandidate(move.From, move.To) || Board.IsWhitePawnStorm(move.From) || Board.IsWhitePawnFork(move.To))
                    {
                        MoveCollection.AddForwardMove(move);
                    }
                    else if (Board.CountTotalWhiteMobility() > _mobilityValue)
                    {
                        MoveCollection.AddMobility(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }

                    break;
                case Pieces.WhiteKnight:
                    if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        MoveCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsWhiteKnightAttacksKingZone(move.From, move.To)
                        || _whiteForpost.IsSet(move.To) || Board.IsWhiteKnightFork(move.To))
                    {
                        MoveCollection.AddForwardMove(move);
                    }
                    else if (Board.CountTotalWhiteMobility() > _mobilityValue)
                    {
                        MoveCollection.AddMobility(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }

                    break;
                case Pieces.WhiteBishop:
                    if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        MoveCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsWhiteBishopAttacksKingZone(move.From, move.To)
                        || Board.IsWhiteBishopPin(move.To) || _whiteForpost.IsSet(move.To) || Board.IsWhiteBishopFork(move.To) || Board.IsWhiteBishopBattary(move.To))
                    {
                        MoveCollection.AddForwardMove(move);
                    }
                    else if (Board.CountTotalWhiteMobility() > _mobilityValue)
                    {
                        MoveCollection.AddMobility(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }

                    break;
                case Pieces.WhiteRook:
                    if (Board.IsWhiteRookOnOpenFile(move.From, move.To) || Board.IsDoubleWhiteRook(move.From, move.To)
                        || Board.IsWhiteRookOnSeven(move.From, move.To) || Board.IsWhiteRookAttacksKingZone(move.From, move.To)
                        || Board.IsWhiteRookPin(move.To) || Board.IsWhiteRookBattary(move.To))
                    {
                        MoveCollection.AddForwardMove(move);
                    }
                    else if (Board.CountTotalWhiteMobility() > _mobilityValue)
                    {
                        MoveCollection.AddMobility(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }

                    break;
                case Pieces.WhiteQueen:
                    if (Board.IsWhiteQueenAttacksKingZone(move.From, move.To) || Board.IsWhiteQueenPin(move.To) || Board.IsWhiteQueenBattary(move.To))
                    {
                        MoveCollection.AddForwardMove(move);
                    }
                    //else if (Board.CountTotalWhiteMobility() > _mobilityValue)
                    //{
                    //    AttackCollection.AddMobility(move);
                    //}
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }

                    break;
                case Pieces.WhiteKing:
                    if (move.IsCastle)
                    {
                        MoveCollection.AddSuggested(move);
                    }
                    else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                    {
                        MoveCollection.AddNonSuggested(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }

                    break;
                default:
                    MoveCollection.AddNonCapture(move);
                    break;
            }
        }

        Position.UnMakeWhite();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleMove(MoveBase move)
    {
        Position.MakeBlack(move);
        bool hasResult = CheckBlackResult(move);

        if (!hasResult)
        {
            switch (move.Piece)
            {
                case Pieces.BlackPawn:
                    if (Board.IsBlackPass(move.To) || move.From == Squares.D7 || move.From == Squares.E7)
                    {
                        MoveCollection.AddSuggested(move);
                    }
                    else if (move.From == Squares.C7 || Board.IsBlackCandidate(move.From, move.To) || Board.IsBlackPawnStorm(move.From) || Board.IsBlackPawnFork(move.To))
                    {
                        MoveCollection.AddForwardMove(move);
                    }
                    else if (Board.CountTotalBlackMobility() > _mobilityValue)
                    {
                        MoveCollection.AddMobility(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackKnight:
                    if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        MoveCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsBlackKnightAttacksKingZone(move.From, move.To)
                        || _blackForpost.IsSet(move.To) || Board.IsBlackKnightFork(move.To))
                    {
                        MoveCollection.AddForwardMove(move);
                    }
                    else if (Board.CountTotalBlackMobility() > _mobilityValue)
                    {
                        MoveCollection.AddMobility(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackBishop:
                    if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        MoveCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsBlackBishopAttacksKingZone(move.From, move.To)
                        || Board.IsBlackBishopPin(move.To) || _blackForpost.IsSet(move.To) || Board.IsBlackBishopFork(move.To) || Board.IsBlackBishopBattary(move.To))
                    {
                        MoveCollection.AddForwardMove(move);
                    }
                    else if (Board.CountTotalBlackMobility() > _mobilityValue)
                    {
                        MoveCollection.AddMobility(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackRook:
                    if (Board.IsBlackRookOnOpenFile(move.From, move.To) || Board.IsDoubleBlackRook(move.From, move.To)
                        || Board.IsBlackRookOnSeven(move.From, move.To) || Board.IsBlackRookAttacksKingZone(move.From, move.To)
                         || Board.IsBlackRookPin(move.To) || Board.IsBlackRookBattary(move.To))
                    {
                        MoveCollection.AddForwardMove(move);
                    }
                    else if (Board.CountTotalBlackMobility() > _mobilityValue)
                    {
                        MoveCollection.AddMobility(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackQueen:
                    if (Board.IsBlackQueenAttacksKingZone(move.From, move.To) || Board.IsBlackQueenPin(move.To) || Board.IsBlackQueenBattary(move.To))
                    {
                        MoveCollection.AddForwardMove(move);
                    }
                    //else if (Board.CountTotalBlackMobility() > _mobilityValue)
                    //{
                    //    AttackCollection.AddMobility(move);
                    //}
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }

                    break;
                case Pieces.BlackKing:
                    if (move.IsCastle)
                    {
                        MoveCollection.AddSuggested(move);
                    }
                    else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                    {
                        MoveCollection.AddNonSuggested(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }

                    break;
                default:
                    MoveCollection.AddNonCapture(move);
                    break;
            }
        }

        Position.UnMakeBlack();
    }
}