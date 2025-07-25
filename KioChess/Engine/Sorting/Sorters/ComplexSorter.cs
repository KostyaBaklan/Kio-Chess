using Engine.DataStructures;
using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public class ComplexSorter : MoveSorter<ComplexMoveCollection>
{
    private readonly int _tradeMargin;
    private readonly int _minusTradeMargin;
    protected readonly BitBoard _minorStartRanks;
    protected readonly BitBoard _whitePawnRank;
    protected readonly BitBoard _blackPawnRank;
    protected readonly BitBoard _whiteForpost;
    protected readonly BitBoard _blackForpost;
    protected readonly PositionsList PositionsList;
    protected readonly AttackList Attacks;
    private bool[] LowSee;

    public ComplexSorter(Position position) : base(position)
    {
        PositionsList = new PositionsList();
        Attacks = [];
        _minorStartRanks = Board.GetRank(0) | Board.GetRank(7);
        _whitePawnRank = Board.GetRank(2);
        _blackPawnRank = Board.GetRank(5);
        _whiteForpost = (Board.GetRank(4) | Board.GetRank(5)).Remove(Board.GetFile(0) | Board.GetFile(7));
        _blackForpost = (Board.GetRank(2) | Board.GetRank(3)).Remove(Board.GetFile(0) | Board.GetFile(7));

        _tradeMargin = ConfigurationProvider.AlgorithmConfiguration.MarginConfiguration.TradeMargin;
        _minusTradeMargin = -_tradeMargin;
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
                ProcessWhiteCaptureMove(attack);
            }
        }
        else
        {
            Position.UnMakeWhite();
            ProcessWhiteCaptureMove(attack);
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
                ProcessBlackCaptureMove(attack);
            }
        }
        else
        {
            Position.UnMakeBlack();
            ProcessBlackCaptureMove(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsGoodAttackForBlack()
    {
        GetBlackAttacks();
        return IsWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsBadAttackToBlack()
    {
        GetWhiteAttacks();
        return IsOpponentWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsGoodAttackForWhite()
    {
        GetWhiteAttacks();
        return IsWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsBadAttackToWhite()
    {
        GetBlackAttacks();
        return IsOpponentWinCapture();
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
        if (Board.CanBlackPromote())
        {
            Position.GetBlackPromotionAttacks(Attacks);
        }
        Position.GetBlackAttacks(Attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetWhiteAttacks()
    {
        Attacks.Clear();
        if (Board.CanWhitePromote())
        {
            Position.GetWhitePromotionAttacks(Attacks);
        }

        Position.GetWhiteAttacks(Attacks);
    }

    #region Overrides of MoveSorter

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
                        AttackCollection.AddSuggested(move);
                    }
                    else if (move.From == Squares.C2 || Board.IsWhiteCandidate(move.From, move.To) || Board.IsWhitePawnStorm(move.From) || Board.IsWhitePawnFork(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }

                    break;
                case Pieces.WhiteKnight:
                    if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsWhiteKnightAttacksKingZone(move.From, move.To)
                        //|| Board.IsWhiteKnightAttacksHardPiece(move.To)
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
                    if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsWhiteBishopAttacksKingZone(move.From, move.To)
                        //|| Board.IsWhiteBishopAttacksHardPiece(move.To) 
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
                    if (Board.IsWhiteRookOnOpenFile(move.From, move.To) || Board.IsDoubleWhiteRook(move.From, move.To)
                        || Board.IsWhiteRookOnSeven(move.From, move.To) || Board.IsWhiteRookAttacksKingZone(move.From, move.To)
                        || Board.IsWhiteRookPin(move.To) || Board.IsWhiteRookBattary(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }

                    break;
                case Pieces.WhiteQueen:
                    if (Board.IsWhiteQueenAttacksKingZone(move.From, move.To) || Board.IsWhiteQueenPin(move.To) || Board.IsWhiteQueenBattary(move.To))
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
                    else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                    {
                        AttackCollection.AddNonSuggested(move);
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
                        AttackCollection.AddSuggested(move);
                    }
                    else if (move.From == Squares.C7 || Board.IsBlackCandidate(move.From, move.To) || Board.IsBlackPawnStorm(move.From) || Board.IsBlackPawnFork(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackKnight:
                    if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsBlackKnightAttacksKingZone(move.From, move.To)
                        //|| Board.IsBlackKnightAttacksHardPiece(move.To) 
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
                    if ((move.From.AsBitBoard() & _minorStartPositions).Any())
                    {
                        AttackCollection.AddSuggested(move);
                    }
                    else if ((move.From.AsBitBoard() & _perimeter).Any() || Board.IsBlackBishopAttacksKingZone(move.From, move.To)
                        //|| Board.IsBlackBishopAttacksHardPiece(move.To) 
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
                    if (Board.IsBlackRookOnOpenFile(move.From, move.To) || Board.IsDoubleBlackRook(move.From, move.To)
                        || Board.IsBlackRookOnSeven(move.From, move.To) || Board.IsBlackRookAttacksKingZone(move.From, move.To)
                         || Board.IsBlackRookPin(move.To) || Board.IsBlackRookBattary(move.To))
                    {
                        AttackCollection.AddForwardMove(move);
                    }
                    else
                    {
                        AttackCollection.AddNonCapture(move);
                    }
                    break;
                case Pieces.BlackQueen:
                    if (Board.IsBlackQueenAttacksKingZone(move.From, move.To) || Board.IsBlackQueenPin(move.To) || Board.IsBlackQueenBattary(move.To))
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
                    else if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                    {
                        AttackCollection.AddNonSuggested(move);
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

        Position.UnMakeBlack();
    }

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
    private bool CheckWhiteResult(MoveBase move)
    {
        if (move.IsCheck)
        {
            var attack = Board.GetBlackAttackToForCheck(move.To);
            if (attack != null && Board.StaticExchange(attack) > 0)
            {
                AttackCollection.AddLooseCheck(move);
            }
            else
            {
                if (Position.AnyBlackMoves())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }
            return true;
        }

        if (IsBadAttackToWhite())
        {
            AttackCollection.AddLooseNonCapture(move);
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckBlackResult(MoveBase move)
    {
        if (move.IsCheck)
        {
            var attack = Board.GetWhiteAttackToForCheck(move.To);
            if (attack != null && Board.StaticExchange(attack) > 0)
            {
                AttackCollection.AddLooseCheck(move);
            }
            else
            {
                if (Position.AnyWhiteMoves())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }
            return true;
        }

        if (IsBadAttackToBlack())
        {
            AttackCollection.AddLooseNonCapture(move);
            return true;
        }

        return false;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void SetValues()
    {
        StaticValue = Position.GetStaticValue();
        //Phase = MoveHistoryService.GetPhase();
        LowSee = DataPoolService.GetCurrentLowSee();
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
            LowSee[attack.Key] = false;
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            if (!attack.IsCheck)
            {
                AttackCollection.AddLooseCapture(attack);
                LowSee[attack.Key] = true;
            }
            else
            {
                AttackCollection.AddLooseCheckAttack(attack);
                LowSee[attack.Key] = false;
            }
        }
        else
        {
            if (StaticValue < _minusTradeMargin)
            {
                attack.See = attackValue;
                AttackCollection.AddLooseCapture(attack);
                LowSee[attack.Key] = false;
            }
            else if (StaticValue > _tradeMargin)
            {
                attack.See = attackValue;
                AttackCollection.AddWinCapture(attack);
                LowSee[attack.Key] = false;
            }
            else
            {
                if (attack.Piece == Pieces.BlackBishop && Board.GetPieceBits(Pieces.BlackBishop).Count() > 1 && attack.Captured == Pieces.WhiteKnight)
                {
                    attack.See = -50;
                    AttackCollection.AddLooseCapture(attack);
                    LowSee[attack.Key] = false;
                }
                else if (attack.Piece == Pieces.BlackKnight && attack.Captured == Pieces.WhiteBishop && Board.GetPieceBits(Pieces.WhiteBishop).Count() > 1)
                {
                    attack.See = 50;
                    AttackCollection.AddWinCapture(attack);
                    LowSee[attack.Key] = false;
                }
                else
                {
                    AttackCollection.AddTrade(attack);
                    LowSee[attack.Key] = false;
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
            LowSee[attack.Key] = false;
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            if (!attack.IsCheck)
            {
                AttackCollection.AddLooseCapture(attack);
                LowSee[attack.Key] = true;
            }
            else
            {
                AttackCollection.AddLooseCheckAttack(attack);
                LowSee[attack.Key] = false;
            }
        }
        else
        {
            if (StaticValue < _minusTradeMargin)
            {
                attack.See = attackValue;
                AttackCollection.AddLooseCapture(attack);
                LowSee[attack.Key] = false;
            }
            else if (StaticValue > _tradeMargin)
            {
                attack.See = attackValue;
                AttackCollection.AddWinCapture(attack);
                LowSee[attack.Key] = false;
            }
            else
            {
                if (attack.Piece == Pieces.WhiteBishop && Board.GetPieceBits(Pieces.WhiteBishop).Count() > 1 && attack.Captured == Pieces.BlackKnight)
                {
                    attack.See = -50;
                    AttackCollection.AddLooseCapture(attack);
                    LowSee[attack.Key] = false;
                }
                else if (attack.Piece == Pieces.WhiteKnight && attack.Captured == Pieces.BlackBishop && Board.GetPieceBits(Pieces.BlackBishop).Count() > 1)
                {
                    attack.See = 50;
                    AttackCollection.AddWinCapture(attack);
                    LowSee[attack.Key] = false;
                }
                else
                {
                    AttackCollection.AddTrade(attack);
                    LowSee[attack.Key] = false;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionMoves(PromotionList moves)
    {
        Position.MakeBlack(moves[0]);
        AttackBase attack = Board.GetWhiteAttackToForPromotion(moves[0].To);
        if (attack == null)
        {
            for (byte i = Zero; i < moves.Count; i++)
            {
                var move = moves[i];
                LowSee[move.Key] = false;
                move.SetSee();
                AttackCollection.AddWinCapture(move);
            }
        }
        else
        {
            attack.Captured = Pieces.BlackPawn;

            int see = -Board.StaticExchange(attack);

            if (see > 0)
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee[move.Key] = false;
                    AttackCollection.AddWinCapture(move);
                }
            }
            else
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee[move.Key] = true;
                    AttackCollection.AddLooseCapture(move);
                }
            }
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
            for (byte i = Zero; i < moves.Count; i++)
            {
                var move = moves[i];
                LowSee[move.Key] = false;
                move.SetSee();
                AttackCollection.AddWinCapture(move);
            }
        }
        else
        {
            attack.Captured = Pieces.WhitePawn;

            int see = -Board.StaticExchange(attack);

            if (see > 0)
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee[move.Key] = false;
                    AttackCollection.AddWinCapture(move);
                }
            }
            else
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee[move.Key] = true;
                    AttackCollection.AddLooseCapture(move);
                }
            }
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
            var captured = Board.GetPiece(moves[0].To);
            for (byte i = Zero; i < moves.Count; i++)
            {
                var move = moves[i];
                LowSee[move.Key] = false;
                move.SetSee(captured);
                AttackCollection.AddWinCapture(move);
            }
        }
        else
        {
            attack.Captured = Pieces.WhitePawn;

            int see = -Board.StaticExchange(attack);

            if (see > 0)
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee[move.Key] = false;
                    AttackCollection.AddWinCapture(move);
                }
            }
            else
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee[move.Key] = true;
                    AttackCollection.AddLooseCapture(move);
                }
            }
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
            var captured = Board.GetPiece(moves[0].To);
            for (byte i = Zero; i < moves.Count; i++)
            {
                var move = moves[i];
                LowSee[move.Key] = false;
                move.SetSee(captured);
                AttackCollection.AddWinCapture(move);
            }
        }
        else
        {
            attack.Captured = Pieces.BlackPawn;

            int see = -Board.StaticExchange(attack);

            if (see > 0)
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee[move.Key] = false;
                    AttackCollection.AddWinCapture(move);
                }
            }
            else
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee[move.Key] = true;
                    AttackCollection.AddLooseCapture(move);
                }
            }
            Position.UnMakeBlack();
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

    protected override void InitializeMoveCollection() => AttackCollection = new ComplexMoveCollection();
}
