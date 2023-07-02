using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Services
{
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
        private byte _phase;
        private BitBoard _occupied;
        //private BitBoard _illigal;
        private BitBoard _to;
        private byte _position;
        private BitBoard _attackers;

        private readonly IEvaluationService _evaluationService;
        private readonly IMoveProvider _moveProvider;
        private IBoard _board;

        public AttackEvaluationService(IEvaluationService evaluationService, IMoveProvider moveProvider)
        {
            _boards = new BitBoard[12];
            _evaluationService = evaluationService;
            _moveProvider = moveProvider;
        }

        #region Implementation of IAttackEvaluationService

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(BitBoard[] boards)
        {
            _phase = _board.GetPhase();
            _occupied = _board.GetOccupied();
           // _illigal = new BitBoard(ulong.MaxValue);

            new Span<BitBoard>(boards, 0, 12).CopyTo(new Span<BitBoard>(_boards, 0, 12));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short StaticExchange(AttackBase attack)
        {
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
                var value = _evaluationService.GetValue(target, _phase);
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
                _boards[target] ^= _to;

                if (board.Piece.IsWhite())
                {
                    if ((board.Board & mayXRay).Any())
                    {
                        _attackers |= ConsiderWhiteXrays();
                    }

                    if (_attackers.IsZero()) break;

                    target = board.Piece;
                    board = GetNextAttackerToWhite(target);
                }
                else
                {
                    if ((board.Board & mayXRay).Any())
                    {
                        _attackers |= ConsiderBlackXrays();
                    }

                    if (_attackers.IsZero()) break;

                    target = board.Piece;
                    board = GetNextAttackerToBlack(target);
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
        private AttackerBoard GetNextAttackerToBlack(byte target)
        {
            for (byte piece = 0; piece < 6; piece++)
            {
                BitBoard p = GetWhitePosition(target, piece);
                if (p.Any())
                {
                    return new AttackerBoard { Board = p, Piece = piece };
                }
                if (_attackers.IsZero()) break;
            }

            return new AttackerBoard { Board = new BitBoard(0) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackerBoard GetNextAttackerToWhite(byte target)
        {
            for (byte piece = 6; piece < 12; piece++)
            {
                BitBoard p = GetBlackPosition(target, piece);
                if (p.Any())
                {
                    return new AttackerBoard { Board = p, Piece = piece };
                }
                if (_attackers.IsZero()) break;
            }

            return new AttackerBoard { Board = new BitBoard(0) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetWhitePosition(byte target, byte piece)
        {
            var bit = _attackers & _boards[piece];
            while (bit.Any())
            {
                byte position = bit.BitScanForward();
                bit = bit.Remove(position);

                BitBoard pos = position.AsBitBoard();

                _boards[target] ^= _to;
                _boards[piece] ^= pos | _to;
                _occupied ^= pos;

                bool isCheck = IsCheckToWhite(_boards[WhiteKing].BitScanForward());

                _boards[target] ^= _to;
                _boards[piece] ^= pos | _to;
                _occupied ^= pos;

                if (isCheck)
                {
                    _attackers = _attackers.Remove(pos);
                    //_illigal = _illigal.Remove(pos);
                }
                else
                {
                    return pos;
                }
            }
            return new BitBoard(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetBlackPosition(byte target, byte piece)
        {
            var bit = _attackers & _boards[piece];
            while (bit.Any())
            {
                byte position = bit.BitScanForward();
                bit = bit.Remove(position);

                BitBoard pos = position.AsBitBoard();

                _boards[target] ^= _to;
                _boards[piece] ^= pos | _to;
                _occupied ^= pos;

                bool isCheck = IsCheckToBlack(_boards[BlackKing].BitScanForward());

                _boards[target] ^= _to;
                _boards[piece] ^= pos | _to;
                _occupied ^= pos;

                if (isCheck)
                {
                    _attackers = _attackers.Remove(pos);
                    //_illigal = _illigal.Remove(pos);
                }
                else
                {
                    return  pos;
                }
            }
            return new BitBoard(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsCheckToWhite(byte to)
        {
            return (_moveProvider.GetAttackPattern(WhiteKnight, to) & _boards[BlackKnight]).Any()
                || (to.BishopAttacks(_occupied) & (_boards[BlackBishop] | _boards[BlackQueen])).Any()
                || (to.RookAttacks(_occupied) & (_boards[BlackRook] | _boards[BlackQueen])).Any()
                || (_moveProvider.GetAttackPattern(WhitePawn, to) & _boards[BlackPawn]).Any()
                || (_moveProvider.GetAttackPattern(WhiteKing, to) & _boards[BlackKing]).Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsCheckToBlack(byte to)
        {
            return (_moveProvider.GetAttackPattern(BlackKnight, to) & _boards[WhiteKnight]).Any()
            || (to.BishopAttacks(_occupied) & (_boards[WhiteBishop] | _boards[WhiteQueen])).Any()
            || (to.RookAttacks(_occupied) & (_boards[WhiteRook] | _boards[WhiteQueen])).Any()
            || (_moveProvider.GetAttackPattern(BlackPawn, to) & _boards[WhitePawn]).Any()
            || (_moveProvider.GetAttackPattern(BlackKing, to) & _boards[WhiteKing]).Any();
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
}
