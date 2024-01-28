using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Collections;
using Engine.Models.Boards;
using Engine.DataStructures.Moves.Lists;
using Engine.DataStructures;

namespace Engine.Sorting.Sorters;

public class ComplexSorter : CommonMoveSorter<ComplexMoveCollection>
{
    protected readonly BitBoard _minorStartRanks;
    protected readonly BitBoard _whitePawnRank;
    protected readonly BitBoard _blackPawnRank;
    protected readonly PositionsList PositionsList;
    protected readonly AttackList Attacks;

    public ComplexSorter(Position position) : base(position)
    {
        PositionsList = new PositionsList();
        Attacks = new AttackList();
        _minorStartRanks = Board.GetRank(0) | Board.GetRank(7);
        _whitePawnRank = Board.GetRank(2);
        _blackPawnRank = Board.GetRank(5);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessCounterMove(MoveBase move)
    {
        Position.Make(move);

        if (move.IsWhite)
        {
            if (IsBadAttackToWhite())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck && !Position.AnyBlackMoves())
            {
                AttackCollection.AddMateMove(move);
            }
            else
            {
                AttackCollection.AddCounterMove(move);
            }
        }
        else
        {
            if (IsBadAttackToBlack())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck && !Position.AnyWhiteMoves())
            {
                AttackCollection.AddMateMove(move);
            }
            else
            {
                AttackCollection.AddCounterMove(move);
            }
        }

        Position.UnMake();
    }

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
        Position.Make(attack);
        if (attack.IsCheck && !Position.AnyBlackMoves())
        {
            Position.UnMake();
            AttackCollection.AddMateMove(attack);
        }
        else
        {
            Position.UnMake();
            ProcessWhiteCaptureMove(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCapture(AttackBase attack)
    {
        Position.Make(attack);
        if (attack.IsCheck && !Position.AnyWhiteMoves())
        {
            Position.UnMake();
            AttackCollection.AddMateMove(attack);
        }
        else
        {
            Position.UnMake();
            ProcessBlackCaptureMove(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsGoodAttackForBlack()
    {
        GetBlackAttacks();
        return Attacks.Count > 0 && IsWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsBadAttackToBlack()
    {
        GetWhiteAttacks();
        return Attacks.Count > 0 && IsOpponentWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsGoodAttackForWhite()
    {
        GetWhiteAttacks();
        return Attacks.Count > 0 && IsWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsBadAttackToWhite()
    {
        GetBlackAttacks();
        return Attacks.Count > 0 && IsOpponentWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsWinCapture()
    {
        for (byte i = 0; i < Attacks.Count; i++)
        {
            var attack = Attacks[i];
            attack.Captured = Board.GetPiece(attack.To);

            if (Board.StaticExchange(attack) > 0)
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsOpponentWinCapture()
    {
        for (byte i = 0; i < Attacks.Count; i++)
        {
            var attack = Attacks[i];
            attack.Captured = Board.GetPiece(attack.To);

            if (Board.StaticExchange(attack) > 0)
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetBlackAttacks()
    {
        Attacks.Clear();
        if (Position.CanBlackPromote())
        {
            Position.GetBlackPromotionAttacks(Attacks);
        }
        Position.GetBlackAttacks(Attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetWhiteAttacks()
    {
        Attacks.Clear();
        if (Position.CanWhitePromote())
        {
            Position.GetWhitePromotionAttacks(Attacks);
        }

        Position.GetWhiteAttacks(Attacks);
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
                if (MoveHistoryService.GetPly() < 12 && ((move.From == H2 && move.To == H4) || (move.From == G2 && move.To == G4) || (move.From == A2 && move.To == A4) || (move.From == B2 && move.To == B4)))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else if (move.From == D2|| move.From == E2 )
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
                if (MoveHistoryService.GetPly() < 12 && ((move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero()))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                {
                    AttackCollection.AddSuggested(move);
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
                else if (Board.IsWhiteRookOnOpenFile(move.From, move.To))
                {
                    AttackCollection.AddSuggested(move);
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
                else if (!MoveHistoryService.IsLastMoveWasCheck())
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
                if (MoveHistoryService.GetPly() < 12 && ((move.From == H7 && move.To == H5) || (move.From == G7 && move.To == G5) || (move.From == A7 && move.To == A5) || (move.From == B7 && move.To == B5)))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else if (move.From == D7 || move.From == E7)
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
                if (MoveHistoryService.GetPly() < 12 && ((move.To.AsBitBoard() & _perimeter).Any() || (_minorStartPositions & move.From.AsBitBoard()).IsZero()))
                {
                    AttackCollection.AddNonSuggested(move);
                }
                else if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                {
                    AttackCollection.AddSuggested(move);
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
                else
                {
                    AddNonCapture(move);
                }
                break;
            case BlackRook:
                if (move.From == A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                    move.From == H8 && MoveHistoryService.CanDoBlackSmallCastle())
                {
                    AttackCollection.AddBad(move);
                }
                else if (Board.IsBlackRookOnOpenFile(move.From, move.To))
                {
                    AttackCollection.AddSuggested(move);
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
                else if (!MoveHistoryService.IsLastMoveWasCheck())
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
                if (Board.IsWhitePass(move.To) || Board.IsWhiteCandidate(move.From, move.To) || Board.IsWhitePawnStorm(move.From))
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
                if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }

                break;
            case WhiteRook:
                if (Board.IsWhiteRookOnOpenFile(move.From, move.To) || Board.IsDoubleWhiteRook(move.From, move.To) || Board.IsWhiteRookOnSeven(move.From, move.To))
                {
                    AttackCollection.AddSuggested(move);
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
                if (Board.IsBlackPass(move.To) || Board.IsBlackCandidate(move.From, move.To) || Board.IsBlackPawnStorm(move.From))
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
                if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AddNonCapture(move);
                }
                break;
            case BlackRook:
                if (Board.IsBlackRookOnOpenFile(move.From, move.To) || Board.IsDoubleBlackRook(move.From, move.To) || Board.IsBlackRookOnSeven(move.From, move.To))
                {
                    AttackCollection.AddSuggested(move);
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

        switch (move.Piece)
        {
            case WhitePawn:
                if (Board.IsWhitePass(move.To) || Board.IsWhiteCandidate(move.From, move.To))
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                    AddNonCapture(move);
                break;
            case WhiteRook:
                if (Board.IsBehindWhitePassed(move.From, move.To))
                {
                    AttackCollection.AddSuggested(move);
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
        
        switch (move.Piece)
        {
            case BlackPawn:
                if (Board.IsBlackPass(move.To) || Board.IsBlackCandidate(move.From, move.To))
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                    AddNonCapture(move);
                break;
            case BlackRook:
                if (Board.IsBehindBlackPassed(move.From, move.To))
                {
                    AttackCollection.AddSuggested(move);
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
        StaticValue = Position.GetStaticValue();
        Phase = Board.GetPhase();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCaptureMove(AttackBase attack)
    {
        attack.Captured = Board.GetPiece(attack.To);
        int attackValue = Board.StaticExchange(attack);
        if (attackValue > 0)
        {
            attack.See = attackValue;
            AttackCollection.AddWinCapture(attack);
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            AttackCollection.AddLooseCapture(attack);
        }
        else
        {
            if (StaticValue < -99)
            {
                attack.See = attackValue;
                AttackCollection.AddLooseCapture(attack);
            }
            else if (StaticValue > 99)
            {
                attack.See = attackValue;
                AttackCollection.AddWinCapture(attack);
            }
            else
            {
                if(attack.Piece == BlackBishop && Board.GetPieceBits(BlackBishop).Count() > 1 && attack.Captured == WhiteKnight)
                {
                    attack.See = -50;
                    AttackCollection.AddLooseCapture(attack);
                }
                else if(attack.Piece == BlackKnight && attack.Captured == WhiteBishop && Board.GetPieceBits(WhiteBishop).Count() > 1)
                {
                    attack.See = 50;
                    AttackCollection.AddWinCapture(attack);
                }
                else
                {
                    AttackCollection.AddTrade(attack); 
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteCaptureMove(AttackBase attack)
    {
        attack.Captured = Board.GetPiece(attack.To);
        int attackValue = Board.StaticExchange(attack);
        if (attackValue > 0)
        {
            attack.See = attackValue;
            AttackCollection.AddWinCapture(attack);
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            AttackCollection.AddLooseCapture(attack);
        }
        else
        {
            if (StaticValue < -99)
            {
                attack.See = attackValue;
                AttackCollection.AddLooseCapture(attack);
            }
            else if (StaticValue > 99)
            {
                attack.See = attackValue;
                AttackCollection.AddWinCapture(attack);
            }
            else
            {
                if (attack.Piece == WhiteBishop && Board.GetPieceBits(WhiteBishop).Count() > 1 && attack.Captured == BlackKnight)
                {
                    attack.See = -50;
                    AttackCollection.AddLooseCapture(attack);
                }
                else if (attack.Piece == WhiteKnight && attack.Captured == BlackBishop && Board.GetPieceBits(BlackBishop).Count() > 1)
                {
                    attack.See = 50;
                    AttackCollection.AddWinCapture(attack);
                }
                else
                {
                    AttackCollection.AddTrade(attack); 
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessCaptureMove(AttackBase attack)
    {
        attack.Captured = Board.GetPiece(attack.To);
        int attackValue = Board.StaticExchange(attack);
        if (attackValue > 0)
        {
            attack.See = attackValue;
            AttackCollection.AddWinCapture(attack);
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            AttackCollection.AddLooseCapture(attack);
        }
        else
        {
            if (StaticValue < -99)
            {
                attack.See = attackValue;
                AttackCollection.AddLooseCapture(attack);
            }
            else if (StaticValue > 99)
            {
                attack.See = attackValue;
                AttackCollection.AddWinCapture(attack);
            }
            else
            {
                AttackCollection.AddTrade(attack);
            }
        }
    }

    protected override void InitializeMoveCollection() => AttackCollection = new ComplexMoveCollection();
}
