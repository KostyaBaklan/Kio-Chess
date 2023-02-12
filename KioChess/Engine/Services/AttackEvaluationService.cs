using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Services
{
    public class AttackEvaluationService : IAttackEvaluationService
    {
        private BitBoard[] _boards;
        private Phase _phase;
        private BitBoard _occupied;
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

            new Span<BitBoard>(boards, 0, 12).CopyTo(new Span<BitBoard>(_boards, 0, 12));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StaticExchange(AttackBase attack)
        {
            BitBoard mayXRay = _boards[Piece.BlackPawn.AsByte()] |
                               _boards[Piece.BlackRook.AsByte()] |
                               _boards[Piece.BlackBishop.AsByte()] |
                               _boards[Piece.BlackQueen.AsByte()] |
                               _boards[Piece.WhitePawn.AsByte()] |
                               _boards[Piece.WhiteBishop.AsByte()] |
                               _boards[Piece.WhiteRook.AsByte()] |
                               _boards[Piece.WhiteQueen.AsByte()];

            _to = attack.To.AsBitBoard();
            _position = attack.To.AsByte();
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
                var value = _evaluationService.GetValue(target.AsByte(), _phase);
                if (first)
                {
                    var x = v + value;
                    if (x < 0) return x;

                    v = x;
                }
                else
                {
                    var x = v - value;
                    if (x > 0) return x;
                    v = x;
                }

                first = !first;

                _attackers ^= board.Board; // reset bit in set to traverse
                _occupied ^= board.Board; // reset bit in temporary occupancy (for x-Rays)

                _boards[board.Piece.AsByte()] ^= board.Board | _to;

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

            return v;
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
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.WhitePawn };
            }

            bit = _attackers & _boards[1];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.WhiteKnight };
            }

            bit = _attackers & _boards[2];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.WhiteBishop };
            }

            bit = _attackers & _boards[3];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.WhiteRook };
            }

            bit = _attackers & _boards[4];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.WhiteQueen };
            }

            bit = _attackers & _boards[5];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.WhiteKing };
            }

            return new AttackerBoard { Board = new BitBoard(0) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackerBoard GetNextAttackerToWhite()
        {
            var bit = _attackers & _boards[6];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackPawn };
            }

            bit = _attackers & _boards[7];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackKnight };
            }

            bit = _attackers & _boards[8];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackBishop };
            }

            bit = _attackers & _boards[9];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackRook };
            }

            bit = _attackers & _boards[10];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackQueen };
            }

            bit = _attackers & _boards[11];
            if (bit.Any())
            {
                return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackKing };
            }

            return new AttackerBoard { Board = new BitBoard(0) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard ConsiderBlackXrays()
        {
            return _position.BishopAttacks(_occupied) & (_boards[Piece.BlackBishop.AsByte()] | _boards[Piece.BlackQueen.AsByte()]) |
                _position.RookAttacks(_occupied) & (_boards[Piece.BlackRook.AsByte()] | _boards[Piece.BlackQueen.AsByte()]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard ConsiderWhiteXrays()
        {
            return _position.BishopAttacks(_occupied) & (_boards[Piece.WhiteBishop.AsByte()] | _boards[Piece.WhiteQueen.AsByte()]) |
                _position.RookAttacks(_occupied) & (_boards[Piece.WhiteRook.AsByte()] | _boards[Piece.WhiteQueen.AsByte()]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetAttackers()
        {
            return GetWhiteAttackers() | GetBlackAttackers();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetBlackAttackers()
        {
            return _moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), _position) & _boards[Piece.BlackPawn.AsByte()] |
                _moveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), _position) & _boards[Piece.BlackKnight.AsByte()] |
                _position.BishopAttacks(_occupied) & (_boards[Piece.BlackBishop.AsByte()] | _boards[Piece.BlackQueen.AsByte()]) |
                _position.RookAttacks(_occupied) & (_boards[Piece.BlackRook.AsByte()] | _boards[Piece.BlackQueen.AsByte()]) |
                _moveProvider.GetAttackPattern(Piece.WhiteKing.AsByte(), _position) & _boards[Piece.BlackKing.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetWhiteAttackers()
        {
            return _moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), _position) & _boards[Piece.WhitePawn.AsByte()] |
                _moveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), _position) & _boards[Piece.WhiteKnight.AsByte()] |
                _position.BishopAttacks(_occupied) & (_boards[Piece.WhiteBishop.AsByte()] | _boards[Piece.WhiteQueen.AsByte()]) |
                _position.RookAttacks(_occupied) & (_boards[Piece.WhiteRook.AsByte()] | _boards[Piece.WhiteQueen.AsByte()]) |
                _moveProvider.GetAttackPattern(Piece.BlackKing.AsByte(), _position) & _boards[Piece.WhiteKing.AsByte()];
        }
    }
}
