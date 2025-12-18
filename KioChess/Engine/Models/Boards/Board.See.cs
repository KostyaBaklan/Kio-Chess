using Engine.Models.Boards.Buffers;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        private PieceBuffer<int> _pieceValues;

        // SEE state struct
        private ref struct SeeState
        {
            public Span<BitBoard> Boards;
            public BitBoard Occupied;
            public BitBoard Attackers;
            public byte Position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StaticExchange(AttackBase attack)
        {
            var state = new SeeState
            {
                Boards = stackalloc BitBoard[12],
                Occupied = _occupied,
                Position = attack.To
            };

            Span<BitBoard> boards = _boards;
            boards.CopyTo(state.Boards);

            state.Attackers = GetAttackers(ref state);

            BitBoard mayXRay = state.Boards[Pieces.BlackPawn] |
                               state.Boards[Pieces.BlackRook] |
                               state.Boards[Pieces.BlackBishop] |
                               state.Boards[Pieces.BlackQueen] |
                               state.Boards[Pieces.WhitePawn] |
                               state.Boards[Pieces.WhiteBishop] |
                               state.Boards[Pieces.WhiteRook] |
                               state.Boards[Pieces.WhiteQueen];

            var to = attack.To.AsBitBoard();
            var target = attack.Captured;

            state.Boards[target] ^= to;

            AttackerBoard board = new()
            {
                Board = attack.From.AsBitBoard(),
                Piece = attack.Piece
            };

            int v = 0, x;
            bool first = true;

            Span<int> values = _pieceValues;

            while (board.Board.Any())
            {
                if (first)
                {
                    x = v + values[target];
                    if (x < 0) return x;
                    first = false;
                }
                else
                {
                    x = v - values[target];
                    if (x > 0) return x;
                    first = true;
                }

                v = x;

                state.Attackers ^= board.Board;
                state.Occupied ^= board.Board;
                state.Boards[board.Piece] ^= board.Board | to;
                target = board.Piece;

                if (board.Piece.IsWhite())
                {
                    if ((board.Board & mayXRay).Any())
                    {
                        state.Attackers |= ConsiderWhiteXrays(ref state);
                    }

                    if (state.Attackers.IsZero()) break;

                    board = GetNextAttackerToWhite(ref state);
                }
                else
                {
                    if ((board.Board & mayXRay).Any())
                    {
                        state.Attackers |= ConsiderBlackXrays(ref state);
                    }

                    if (state.Attackers.IsZero()) break;

                    board = GetNextAttackerToBlack(ref state);
                }
            }

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StaticExchangeWithPins(AttackBase attack)
        {
            var state = new SeeState
            {
                Boards = stackalloc BitBoard[12],
                Occupied = _occupied,
                Position = attack.To
            };

            Span<BitBoard> boards = _boards;
            boards.CopyTo(state.Boards);

            state.Attackers = GetAttackers(ref state);

            BitBoard mayXRay = state.Boards[Pieces.BlackPawn] |
                               state.Boards[Pieces.BlackRook] |
                               state.Boards[Pieces.BlackBishop] |
                               state.Boards[Pieces.BlackQueen] |
                               state.Boards[Pieces.WhitePawn] |
                               state.Boards[Pieces.WhiteBishop] |
                               state.Boards[Pieces.WhiteRook] |
                               state.Boards[Pieces.WhiteQueen];

            var to = attack.To.AsBitBoard();
            var target = attack.Captured;

            state.Boards[target] ^= to;

            AttackerBoard board = new()
            {
                Board = attack.From.AsBitBoard(),
                Piece = attack.Piece
            };

            int v = 0, x;
            bool first = true;
            Span<int> values = _pieceValues;

            while (board.Board.Any())
            {
                if (first)
                {
                    x = v + values[target];
                    if (x < 0) return x;
                    first = false;
                }
                else
                {
                    x = v - values[target];
                    if (x > 0) return x;
                    first = true;
                }

                v = x;

                state.Attackers ^= board.Board;
                state.Occupied ^= board.Board;
                state.Boards[board.Piece] ^= board.Board | to;
                target = board.Piece;

                if (board.Piece.IsWhite())
                {
                    if ((board.Board & mayXRay).Any())
                    {
                        state.Attackers |= ConsiderWhiteXrays(ref state);
                    }

                    if (state.Attackers.IsZero()) break;

                    board = GetNextAttackerPinToWhite(ref state);
                }
                else
                {
                    if ((board.Board & mayXRay).Any())
                    {
                        state.Attackers |= ConsiderBlackXrays(ref state);
                    }

                    if (state.Attackers.IsZero()) break;

                    board = GetNextAttackerPinToBlack(ref state);
                }
            }

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StaticExchangeWithPinsWithoutTarget(AttackBase attack)
        {
            attack.SetCapturedPiece();

            return StaticExchangeWithPins(attack);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackerBoard GetNextAttackerToBlack(ref SeeState state)
        {
            Span<BitBoard> boards = state.Boards;
            var bit = state.Attackers & state.Boards[Pieces.WhitePawn];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhitePawn };
            bit = state.Attackers & state.Boards[Pieces.WhiteKnight];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteKnight };
            bit = state.Attackers & state.Boards[Pieces.WhiteBishop];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteBishop };
            bit = state.Attackers & state.Boards[Pieces.WhiteRook];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteRook };
            bit = state.Attackers & state.Boards[Pieces.WhiteQueen];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteQueen };
            bit = state.Attackers & state.Boards[Pieces.WhiteKing];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteKing };
            return new AttackerBoard { Board = new BitBoard(0) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackerBoard GetNextAttackerToWhite(ref SeeState state)
        {
            var bit = state.Attackers & state.Boards[Pieces.BlackPawn];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackPawn };
            bit = state.Attackers & state.Boards[Pieces.BlackKnight];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackKnight };
            bit = state.Attackers & state.Boards[Pieces.BlackBishop];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackBishop };
            bit = state.Attackers & state.Boards[Pieces.BlackRook];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackRook };
            bit = state.Attackers & state.Boards[Pieces.BlackQueen];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackQueen };
            bit = state.Attackers & state.Boards[Pieces.BlackKing];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackKing };
            return new AttackerBoard { Board = new BitBoard(0) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackerBoard GetNextAttackerPinToBlack(ref SeeState state)
        {
            var king = state.Boards[Pieces.WhiteKing].BitScanForward();
            var bit = state.Attackers & state.Boards[Pieces.WhitePawn];
            while (bit.Any())
            {
                var position = bit.BitScanForward();
                var pin = king.XrayRookAttacks(state.Occupied, position.AsBitBoard()) & (state.Boards[Pieces.BlackRook] | state.Boards[Pieces.BlackQueen]);
                if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.WhitePawn };
                bit = bit.Remove(position);
            }
            bit = state.Attackers & state.Boards[Pieces.WhiteKnight];
            while (bit.Any())
            {
                var position = bit.BitScanForward();
                var pin = (king.XrayRookAttacks(state.Occupied, position.AsBitBoard()) & (state.Boards[Pieces.BlackRook] | state.Boards[Pieces.BlackQueen])) |
                          (king.XrayBishopAttacks(state.Occupied, position.AsBitBoard()) & (state.Boards[Pieces.BlackBishop] | state.Boards[Pieces.BlackQueen]));
                if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.WhiteKnight };
                bit = bit.Remove(position);
            }
            bit = state.Attackers & state.Boards[Pieces.WhiteBishop];
            while (bit.Any())
            {
                var position = bit.BitScanForward();
                var pin = king.XrayRookAttacks(state.Occupied, position.AsBitBoard()) & (state.Boards[Pieces.BlackRook] | state.Boards[Pieces.BlackQueen]);
                if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.WhiteBishop };
                bit = bit.Remove(position);
            }
            bit = state.Attackers & state.Boards[Pieces.WhiteRook];
            while (bit.Any())
            {
                var position = bit.BitScanForward();
                var pin = king.XrayBishopAttacks(state.Occupied, position.AsBitBoard()) & (state.Boards[Pieces.BlackBishop] | state.Boards[Pieces.BlackQueen]);
                if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.WhiteRook };
                bit = bit.Remove(position);
            }
            bit = state.Attackers & state.Boards[Pieces.WhiteQueen];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteQueen };
            bit = state.Attackers & state.Boards[Pieces.WhiteKing];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteKing };
            return new AttackerBoard { Board = new BitBoard(0) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackerBoard GetNextAttackerPinToWhite(ref SeeState state)
        {
            var king = state.Boards[Pieces.BlackKing].BitScanForward();
            var bit = state.Attackers & state.Boards[Pieces.BlackPawn];
            while (bit.Any())
            {
                var position = bit.BitScanForward();
                var pin = king.XrayRookAttacks(state.Occupied, position.AsBitBoard()) & (state.Boards[Pieces.WhiteRook] | state.Boards[Pieces.WhiteQueen]);
                if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.BlackPawn };
                bit = bit.Remove(position);
            }
            bit = state.Attackers & state.Boards[Pieces.BlackKnight];
            while (bit.Any())
            {
                var position = bit.BitScanForward();
                var pin = (king.XrayRookAttacks(state.Occupied, position.AsBitBoard()) & (state.Boards[Pieces.WhiteRook] | state.Boards[Pieces.WhiteQueen])) |
                          (king.XrayBishopAttacks(state.Occupied, position.AsBitBoard()) & (state.Boards[Pieces.WhiteBishop] | state.Boards[Pieces.WhiteQueen]));
                if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.BlackKnight };
                bit = bit.Remove(position);
            }
            bit = state.Attackers & state.Boards[Pieces.BlackBishop];
            while (bit.Any())
            {
                var position = bit.BitScanForward();
                var pin = king.XrayRookAttacks(state.Occupied, position.AsBitBoard()) & (state.Boards[Pieces.WhiteRook] | state.Boards[Pieces.WhiteQueen]);
                if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.BlackBishop };
                bit = bit.Remove(position);
            }
            bit = state.Attackers & state.Boards[Pieces.BlackRook];
            while (bit.Any())
            {
                var position = bit.BitScanForward();
                var pin = king.XrayBishopAttacks(state.Occupied, position.AsBitBoard()) & (state.Boards[Pieces.WhiteBishop] | state.Boards[Pieces.WhiteQueen]);
                if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.BlackRook };
                bit = bit.Remove(position);
            }
            bit = state.Attackers & state.Boards[Pieces.BlackQueen];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackQueen };
            bit = state.Attackers & state.Boards[Pieces.BlackKing];
            if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackKing };
            return new AttackerBoard { Board = new BitBoard(0) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard ConsiderBlackXrays(ref SeeState state) =>
            (state.Position.BishopAttacks(state.Occupied) & (state.Boards[Pieces.BlackBishop] | state.Boards[Pieces.BlackQueen])) |
            (state.Position.RookAttacks(state.Occupied) & (state.Boards[Pieces.BlackRook] | state.Boards[Pieces.BlackQueen]));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard ConsiderWhiteXrays(ref SeeState state) =>
            (state.Position.BishopAttacks(state.Occupied) & (state.Boards[Pieces.WhiteBishop] | state.Boards[Pieces.WhiteQueen])) |
            (state.Position.RookAttacks(state.Occupied) & (state.Boards[Pieces.WhiteRook] | state.Boards[Pieces.WhiteQueen]));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetAttackers(ref SeeState state) => (_whitePawnPatterns[state.Position] & state.Boards[Pieces.BlackPawn]) | (_blackPawnPatterns[state.Position] & state.Boards[Pieces.WhitePawn]) |
            _whiteKnightPatterns[state.Position] & (state.Boards[Pieces.BlackKnight] | state.Boards[Pieces.WhiteKnight]) |
            state.Position.BishopAttacks(state.Occupied) & (state.Boards[Pieces.BlackBishop] | state.Boards[Pieces.BlackQueen] | state.Boards[Pieces.WhiteBishop] | state.Boards[Pieces.WhiteQueen]) |
            state.Position.RookAttacks(state.Occupied) & (state.Boards[Pieces.BlackRook] | state.Boards[Pieces.BlackQueen] | state.Boards[Pieces.WhiteRook] | state.Boards[Pieces.WhiteQueen]) |
            _whiteKingPatterns[state.Position] & (state.Boards[Pieces.BlackKing] | state.Boards[Pieces.WhiteKing]);
    }
}
