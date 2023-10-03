using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Interfaces.Evaluation;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Services;

public class AttackEvaluationService : IAttackEvaluationService
{
    const byte WhitePawn = 0;
    const byte WhiteKnight = 1;
    const byte WhiteBishop = 2;
    const byte WhiteRook = 3;
    const byte WhiteQueen = 4;
    const byte WhiteKing = 5;
    const byte BlackPawn = 6;
    const byte BlackKnight = 7;
    const byte BlackBishop = 8;
    const byte BlackRook = 9;
    const byte BlackQueen = 10;
    const byte BlackKing = 11;

    private BitBoard[] _boards;
    private BitBoard _occupied;
    private BitBoard _to;
    private byte _position;
    private BitBoard _attackers;

    private readonly IEvaluationServiceFactory _evaluationServiceFactory;
    private readonly IMoveProvider _moveProvider;
    private IBoard _board;

    public AttackEvaluationService(IEvaluationServiceFactory evaluationServiceFactory, IMoveProvider moveProvider)
    {
        _boards = new BitBoard[12];
        _evaluationServiceFactory = evaluationServiceFactory;
        _moveProvider = moveProvider;
    }

    #region Implementation of IAttackEvaluationService

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Initialize(BitBoard[] boards)
    {
        _occupied = _board.GetOccupied();

        new Span<BitBoard>(boards, 0, 12).CopyTo(new Span<BitBoard>(_boards, 0, 12));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short StaticExchange(AttackBase attack)
    {
        var _evaluationService = _evaluationServiceFactory.GetEvaluationService(_board.GetPhase());

        BitBoard mayXRay = _boards[BlackPawn] |
                           _boards[BlackRook] |
                           _boards[BlackBishop] |
                           _boards[BlackQueen] |
                           _boards[WhitePawn] |
                           _boards[WhiteBishop] |
                           _boards[WhiteRook] |
                           _boards[WhiteQueen];

        _to = attack.To.AsBitBoard();
        _position = attack.To;
        _attackers = GetAttackers();

        AttackerBoard board = new AttackerBoard
        {
            Board = attack.From.AsBitBoard(),
            Piece = attack.Piece
        };

        var target = attack.Captured;
        var v = 0;
        bool first = true;
        while (board.Board.Any())
        {
            var value = _evaluationService.GetPieceValue(target);
            if (first)
            {
                var x = v + value;
                if (x < 0) return (short)x;

                v = x;
            }
            else
            {
                var x = v - value;
                if (x > 0) return (short)x;
                v = x;
            }

            first = !first;

            _attackers ^= board.Board; // reset bit in set to traverse
            _occupied ^= board.Board; // reset bit in temporary occupancy (for x-Rays)

            _boards[board.Piece] ^= board.Board | _to;

            if (board.Piece.IsWhite())
            {
                if ((board.Board & mayXRay).Any())
                {
                    _attackers |= ConsiderWhiteXrays();
                }

                if (_attackers.IsZero()) break;

                target = board.Piece;
                board = GetNextAttackerToWhite();
            }
            else
            {
                if ((board.Board & mayXRay).Any())
                {
                    _attackers |= ConsiderBlackXrays();
                }

                if (_attackers.IsZero()) break;

                target = board.Piece;
                board = GetNextAttackerToBlack();
            }
        }

        return (short)v;
    }

    public void SetBoard(IBoard board)
    {
        _board = board;
    }

    #endregion


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AttackerBoard GetNextAttackerToBlack()
    {
        var bit = _attackers & _boards[0];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = WhitePawn };
        }

        bit = _attackers & _boards[1];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = WhiteKnight };
        }

        bit = _attackers & _boards[2];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = WhiteBishop };
        }

        bit = _attackers & _boards[3];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = WhiteRook };
        }

        bit = _attackers & _boards[4];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = WhiteQueen };
        }

        bit = _attackers & _boards[5];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = WhiteKing };
        }

        return new AttackerBoard { Board = new BitBoard(0) };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AttackerBoard GetNextAttackerToWhite()
    {
        var bit = _attackers & _boards[6];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = BlackPawn };
        }

        bit = _attackers & _boards[7];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = BlackKnight };
        }

        bit = _attackers & _boards[8];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = BlackBishop };
        }

        bit = _attackers & _boards[9];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = BlackRook };
        }

        bit = _attackers & _boards[10];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = BlackQueen };
        }

        bit = _attackers & _boards[11];
        if (bit.Any())
        {
            return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = BlackKing };
        }

        return new AttackerBoard { Board = new BitBoard(0) };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard ConsiderBlackXrays()
    {
        return _position.BishopAttacks(_occupied) & (_boards[BlackBishop] | _boards[BlackQueen]) |
            _position.RookAttacks(_occupied) & (_boards[BlackRook] | _boards[BlackQueen]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard ConsiderWhiteXrays()
    {
        return _position.BishopAttacks(_occupied) & (_boards[WhiteBishop] | _boards[WhiteQueen]) |
            _position.RookAttacks(_occupied) & (_boards[WhiteRook] | _boards[WhiteQueen]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetAttackers()
    {
        return GetWhiteAttackers() | GetBlackAttackers();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetBlackAttackers()
    {
        return _moveProvider.GetAttackPattern(WhitePawn, _position) & _boards[BlackPawn] |
            _moveProvider.GetAttackPattern(WhiteKnight, _position) & _boards[BlackKnight] |
            _position.BishopAttacks(_occupied) & (_boards[BlackBishop] | _boards[BlackQueen]) |
            _position.RookAttacks(_occupied) & (_boards[BlackRook] | _boards[BlackQueen]) |
            _moveProvider.GetAttackPattern(WhiteKing, _position) & _boards[BlackKing];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetWhiteAttackers()
    {
        return _moveProvider.GetAttackPattern(BlackPawn, _position) & _boards[WhitePawn] |
            _moveProvider.GetAttackPattern(BlackKnight, _position) & _boards[WhiteKnight] |
            _position.BishopAttacks(_occupied) & (_boards[WhiteBishop] | _boards[WhiteQueen]) |
            _position.RookAttacks(_occupied) & (_boards[WhiteRook] | _boards[WhiteQueen]) |
            _moveProvider.GetAttackPattern(BlackKing, _position) & _boards[WhiteKing];
    }
}
