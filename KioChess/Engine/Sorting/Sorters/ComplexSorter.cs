using Engine.Interfaces;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public class ComplexSorter : ExtendedSorterBase
{
    public ComplexSorter(IPosition position) : base(position)
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
            MoveValueList.AddLooseNonCapture(move);
            hasResult = true;
        }
        else if (move.IsCheck)
        {
            if (Position.AnyBlackMoves())
            {
                MoveValueList.AddSuggested(move);
            }
            else
            {
                MoveValueList.AddMateMove(move);
            }
            hasResult = true;
        }

        Position.UnMake();

        if (hasResult) return;

        switch (move.Piece)
        {
            case WhitePawn:
                if (MoveHistoryService.GetPly() < 12 && (move.From.AsBitBoard() & _whitePawnRank).Any() || (move.From == H2 && move.To == H4) || (move.From == G2 && move.To == G4) || (move.From == A2 && move.To == A4) || (move.From == B2 && move.To == B4))
                {
                    MoveValueList.AddNonSuggested(move);
                }
                else if (move.From == D2|| move.From == E2 )
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }
                break;
            case WhiteKnight:
            case WhiteBishop:
                if (MoveHistoryService.GetPly() < 12 && ((move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero()))
                {
                    MoveValueList.AddNonSuggested(move);
                }
                else if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                {
                    MoveValueList.AddSuggested(move);
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
                    MoveValueList.AddBad(move);
                }
                else if (Board.IsWhiteRookOnOpenFile(move.From, move.To))
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            case WhiteQueen:
                if (move.From == D1)
                {
                    MoveValueList.AddNonSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }
                break;
            case WhiteKing:
                if (move.IsCastle)
                {
                    MoveValueList.AddSuggested(move);
                }
                else if (!MoveHistoryService.IsLastMoveWasCheck())
                {
                    MoveValueList.AddBad(move);
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
            MoveValueList.AddLooseNonCapture(move);
            hasResult = true;
        }
        else if (move.IsCheck)
        {
            if (Position.AnyWhiteMoves())
            {
                MoveValueList.AddSuggested(move);
            }
            else
            {
                MoveValueList.AddMateMove(move);
            }
            hasResult = true;
        }

        Position.UnMake();

        if (hasResult) return;

        switch (move.Piece)
        {
            case BlackPawn:
                if (MoveHistoryService.GetPly() < 12 && (move.From.AsBitBoard() & _blackPawnRank).Any() || (move.From == H7 && move.To == H5) || (move.From == G7 && move.To == G5) || (move.From == A7 && move.To == A5) || (move.From == B7 && move.To == B5))
                {
                    MoveValueList.AddNonSuggested(move);
                }
                else if (move.From == D7 || move.From == E7)
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }
                break;
            case BlackKnight:
            case BlackBishop:
                if (MoveHistoryService.GetPly() < 12 && ((move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero()))
                {
                    MoveValueList.AddNonSuggested(move);
                }
                else if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            case BlackQueen:
                if (move.From == D8)
                {
                    MoveValueList.AddNonSuggested(move);
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
                    MoveValueList.AddBad(move);
                }
                else if (Board.IsBlackRookOnOpenFile(move.From, move.To))
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            case BlackKing:
                if (move.IsCastle)
                {
                    MoveValueList.AddSuggested(move);
                }
                else if (!MoveHistoryService.IsLastMoveWasCheck())
                {
                    MoveValueList.AddBad(move);
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
            MoveValueList.AddLooseNonCapture(move);
            hasResult = true;
        }
        else if (move.IsCheck)
        {
            if (Position.AnyBlackMoves())
            {
                MoveValueList.AddSuggested(move);
            }
            else
            {
                MoveValueList.AddMateMove(move);
            }
            hasResult = true;
        }

        Position.UnMake();

        if (hasResult) return;

        switch (move.Piece)
        {
            case WhitePawn:
                if (Board.IsWhitePass(move.To) || Board.IsWhiteCandidate(move.From, move.To) || Board.IsWhitePawnStorm(move.From))
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            case WhiteKnight:
            case WhiteBishop:
                if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            case WhiteRook:
                if (Board.IsWhiteRookOnOpenFile(move.From, move.To) || Board.IsDoubleWhiteRook(move.From, move.To) || Board.IsWhiteRookOnSeven(move.From, move.To))
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            case WhiteKing:
                if (move.IsCastle)
                {
                    MoveValueList.AddSuggested(move);
                }
                else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                {
                    MoveValueList.AddNonSuggested(move);
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
            MoveValueList.AddLooseNonCapture(move);
            hasResult = true;
        }
        else if (move.IsCheck)
        {
            if (Position.AnyWhiteMoves())
            {
                MoveValueList.AddSuggested(move);
            }
            else
            {
                MoveValueList.AddMateMove(move);
            }
            hasResult = true;
        }

        Position.UnMake();

        if (hasResult) return;

        switch (move.Piece)
        {
            case BlackPawn:
                if (Board.IsBlackPass(move.To) || Board.IsBlackCandidate(move.From, move.To) || Board.IsBlackPawnStorm(move.From))
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }
                break;
            case BlackKnight:
            case BlackBishop:
                if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }
                break;
            case BlackRook:
                if (Board.IsBlackRookOnOpenFile(move.From, move.To) || Board.IsDoubleBlackRook(move.From, move.To) || Board.IsBlackRookOnSeven(move.From, move.To))
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }
                break;
            case BlackKing:
                if (move.IsCastle)
                {
                    MoveValueList.AddSuggested(move);
                }
                else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                {
                    MoveValueList.AddNonSuggested(move);
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
            MoveValueList.AddLooseNonCapture(move);
            hasResult = true;
        }
        else if (move.IsCheck)
        {
            if (Position.AnyBlackMoves())
            {
                MoveValueList.AddSuggested(move);
            }
            else
            {
                MoveValueList.AddMateMove(move);
            }
            hasResult = true;
        }

        Position.UnMake();

        if (hasResult)
            return;

        switch (move.Piece)
        {
            case WhitePawn:
                if (Board.IsWhitePass(move.To) || Board.IsWhiteCandidate(move.From, move.To))
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                    AddNonCapture(move);
                break;
            default:
                AddNonCapture(move); 
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndMove(MoveBase move)
    {
        Position.Make(move);

        bool hasResult = false;
        if (IsBadAttackToBlack())
        {
            MoveValueList.AddLooseNonCapture(move);
            hasResult = true;
        }
        else if (move.IsCheck)
        {
            if (Position.AnyWhiteMoves())
            {
                MoveValueList.AddSuggested(move);
            }
            else
            {
                MoveValueList.AddMateMove(move);
            }
            hasResult = true;
        }

        Position.UnMake();

        if (hasResult)
            return; 
        
        switch (move.Piece)
        {
            case BlackPawn:
                if (Board.IsBlackPass(move.To) || Board.IsBlackCandidate(move.From, move.To))
                {
                    MoveValueList.AddSuggested(move);
                }
                else
                    AddNonCapture(move);
                break;
            default:
                AddNonCapture(move);
                break;
        }
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void SetValues()
    {
        base.SetValues();
        StaticValue = Position.GetStaticValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessCaptureMove(AttackBase attack)
    {
        attack.Captured = Board.GetPiece(attack.To);
        short attackValue = Board.StaticExchange(attack);
        if (attackValue > 0)
        {
            attack.See = attackValue;
            MoveValueList.AddWinCapture(attack);
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            MoveValueList.AddLooseCapture(attack);
        }
        else
        {
            if (StaticValue <= -100 && attack.Piece % 6 > 0)
            {
                attack.See = attackValue;
                MoveValueList.AddLooseCapture(attack);
            }
            else if(StaticValue > 150 && attack.Piece % 6 > 0)
            {
                attack.See = attackValue;
                MoveValueList.AddWinCapture(attack);
            }
            else
            {
                MoveValueList.AddTrade(attack);
            }
        }
    }
}
