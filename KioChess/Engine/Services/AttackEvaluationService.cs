using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Services;

public class AttackEvaluationService
{
    private readonly BitBoard[] _boards;
    private BitBoard _occupied;
    private BitBoard _to;
    private byte _position;
    private BitBoard _attackers;
    private readonly int[] _pieceValues;

    private readonly BitBoard[] _whitePawnPatterns;
    private readonly BitBoard[] _whiteKnightPatterns;
    private readonly BitBoard[] _whiteKingPatterns;
    private readonly BitBoard[] _blackPawnPatterns;
    private readonly BitBoard[] _blackKnightPatterns;
    private readonly BitBoard[] _blackKingPatterns;
    private Board _board;

    public AttackEvaluationService(IEvaluationServiceFactory evaluationServiceFactory, MoveProvider moveProvider)
    {
        _boards = new BitBoard[12];
        _pieceValues = new int[12];
        var service = evaluationServiceFactory.GetEvaluationService(0);
        for (byte j = 0; j < 12; j++)
        {
            _pieceValues[j] = service.GetPieceValue(j);
        }

        _whitePawnPatterns = new BitBoard[64];
        _whiteKnightPatterns = new BitBoard[64];
        _whiteKingPatterns = new BitBoard[64];
        _blackPawnPatterns = new BitBoard[64];
        _blackKnightPatterns = new BitBoard[64];
        _blackKingPatterns = new BitBoard[64];

        for (byte i = 0; i < 64; i++)
        {
            _whitePawnPatterns[i] = moveProvider.GetAttackPattern(Pieces.WhitePawn, i);
            _whiteKnightPatterns[i] = moveProvider.GetAttackPattern(Pieces.WhiteKnight, i);
            _whiteKingPatterns[i] = moveProvider.GetAttackPattern(Pieces.WhiteKing, i);
            _blackPawnPatterns[i] = moveProvider.GetAttackPattern(Pieces.BlackPawn, i);
            _blackKnightPatterns[i] = moveProvider.GetAttackPattern(Pieces.BlackKnight, i);
            _blackKingPatterns[i] = moveProvider.GetAttackPattern(Pieces.BlackKing, i);
        }
    }

    #region Implementation of IAttackEvaluationService

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Initialize(BitBoard[] boards)
    {
        _occupied = _board.GetOccupied();

        new Span<BitBoard>(boards, 0, 12).CopyTo(new Span<BitBoard>(_boards, 0, 12));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int StaticExchange(AttackBase attack)
    {
        BitBoard mayXRay = _boards[Pieces.BlackPawn] |
                           _boards[Pieces.BlackRook] |
                           _boards[Pieces.BlackBishop] |
                           _boards[Pieces.BlackQueen] |
                           _boards[Pieces.WhitePawn] |
                           _boards[Pieces.WhiteBishop] |
                           _boards[Pieces.WhiteRook] |
                           _boards[Pieces.WhiteQueen];

        _to = attack.To.AsBitBoard();
        _position = attack.To;
        _attackers = GetAttackers();

        AttackerBoard board = new()
        {
            Board = attack.From.AsBitBoard(),
            Piece = attack.Piece
        };

        var target = attack.Captured;
        int v = 0, x;
        bool first = true;
        var values = _pieceValues.AsSpan();

        while (board.Board.Any())
        {
            if (first)
            {
                x = v + values[target];
                if (x < 0) return x;
            }
            else
            {
                x = v - values[target];
                if (x > 0) return x;
            }

            v = x;
            first = !first;

            _attackers ^= board.Board; // reset bit in set to traverse
            _occupied ^= board.Board; // reset bit in temporary occupancy (for x-Rays)

            _boards[board.Piece] ^= board.Board | _to;
            target = board.Piece;

            if (board.Piece.IsWhite())
            {
                if ((board.Board & mayXRay).Any())
                {
                    _attackers |= ConsiderWhiteXrays();
                }

                if (_attackers.IsZero()) break;

                board = GetNextAttackerToWhite();
            }
            else
            {
                if ((board.Board & mayXRay).Any())
                {
                    _attackers |= ConsiderBlackXrays();
                }

                if (_attackers.IsZero()) break;

                board = GetNextAttackerToBlack();
            }
        }

        return v;
    }

    public void SetBoard(Board board) => _board = board;

    #endregion


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AttackerBoard GetNextAttackerToBlack()
    {
        // Use local references to reduce array access overhead
        Span<BitBoard> boards = _boards.AsSpan();

        // Check pieces in order of value (cheapest first)
        var bit = _attackers & boards[Pieces.WhitePawn];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.WhitePawn };
        }

        bit = _attackers & boards[Pieces.WhiteKnight];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.WhiteKnight };
        }

        bit = _attackers & boards[Pieces.WhiteBishop];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.WhiteBishop };
        }

        bit = _attackers & boards[Pieces.WhiteRook];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.WhiteRook };
        }

        bit = _attackers & boards[Pieces.WhiteQueen];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.WhiteQueen };
        }

        bit = _attackers & boards[Pieces.WhiteKing];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.WhiteKing };
        }

        return new AttackerBoard { Board = new BitBoard(0) };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AttackerBoard GetNextAttackerToWhite()
    {
        // Use local references to reduce array access overhead
        Span<BitBoard> boards = _boards.AsSpan();

        var bit = _attackers & boards[Pieces.BlackPawn];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.BlackPawn };
        }

        bit = _attackers & boards[Pieces.BlackKnight];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.BlackKnight };
        }

        bit = _attackers & boards[Pieces.BlackBishop];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.BlackBishop };
        }

        bit = _attackers & boards[Pieces.BlackRook];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.BlackRook };
        }

        bit = _attackers & boards[Pieces.BlackQueen];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.BlackQueen };
        }

        bit = _attackers & boards[Pieces.BlackKing];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Pieces.BlackKing };
        }

        return new AttackerBoard { Board = new BitBoard(0) };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard ConsiderBlackXrays() => (_position.BishopAttacks(_occupied) & (_boards[Pieces.BlackBishop] | _boards[Pieces.BlackQueen])) |
            (_position.RookAttacks(_occupied) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen]));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard ConsiderWhiteXrays() => (_position.BishopAttacks(_occupied) & (_boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteQueen])) |
            (_position.RookAttacks(_occupied) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen]));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetAttackers() => GetWhiteAttackers() | GetBlackAttackers();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetBlackAttackers() => _whitePawnPatterns[_position] & _boards[Pieces.BlackPawn] |
            _whiteKnightPatterns[_position] & _boards[Pieces.BlackKnight] |
            _position.BishopAttacks(_occupied) & (_boards[Pieces.BlackBishop] | _boards[Pieces.BlackQueen]) |
            _position.RookAttacks(_occupied) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen]) |
            _whiteKingPatterns[_position] & _boards[Pieces.BlackKing];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetWhiteAttackers() => _blackPawnPatterns[_position] & _boards[Pieces.WhitePawn] |
            _blackKnightPatterns[_position] & _boards[Pieces.WhiteKnight] |
            _position.BishopAttacks(_occupied) & (_boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteQueen]) |
            _position.RookAttacks(_occupied) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen]) |
            _blackKingPatterns[_position] & _boards[Pieces.WhiteKing];
}

