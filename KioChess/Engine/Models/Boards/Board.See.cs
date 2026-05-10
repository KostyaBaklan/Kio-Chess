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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly BitBoard ConsiderBlackXrays() =>
                (Position.BishopAttacks(Occupied) & (Boards[Pieces.BlackBishop] | Boards[Pieces.BlackQueen])) |
                (Position.RookAttacks(Occupied) & (Boards[Pieces.BlackRook] | Boards[Pieces.BlackQueen]));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly BitBoard ConsiderWhiteXrays() =>
                (Position.BishopAttacks(Occupied) & (Boards[Pieces.WhiteBishop] | Boards[Pieces.WhiteQueen])) |
                (Position.RookAttacks(Occupied) & (Boards[Pieces.WhiteRook] | Boards[Pieces.WhiteQueen]));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly AttackerBoard GetNextAttackerToBlack()
            {
                var bit = Attackers & Boards[Pieces.WhitePawn];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhitePawn };
                bit = Attackers & Boards[Pieces.WhiteKnight];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteKnight };
                bit = Attackers & Boards[Pieces.WhiteBishop];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteBishop };
                bit = Attackers & Boards[Pieces.WhiteRook];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteRook };
                bit = Attackers & Boards[Pieces.WhiteQueen];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteQueen };
                bit = Attackers & Boards[Pieces.WhiteKing];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteKing };
                return new AttackerBoard { Board = new BitBoard(0) };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly AttackerBoard GetNextAttackerToWhite()
            {
                var bit = Attackers & Boards[Pieces.BlackPawn];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackPawn };
                bit = Attackers & Boards[Pieces.BlackKnight];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackKnight };
                bit = Attackers & Boards[Pieces.BlackBishop];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackBishop };
                bit = Attackers & Boards[Pieces.BlackRook];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackRook };
                bit = Attackers & Boards[Pieces.BlackQueen];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackQueen };
                bit = Attackers & Boards[Pieces.BlackKing];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackKing };
                return new AttackerBoard { Board = new BitBoard(0) };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly AttackerBoard GetNextAttackerPinToBlack()
            {
                var king = Boards[Pieces.WhiteKing].BitScanForward();
                var bit = Attackers & Boards[Pieces.WhitePawn];
                while (bit.Any())
                {
                    var position = bit.BitScanForward();
                    var pin = king.XrayRookAttacks(Occupied, position.AsBitBoard()) & (Boards[Pieces.BlackRook] | Boards[Pieces.BlackQueen]);
                    if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.WhitePawn };
                    bit = bit.Remove(position);
                }
                bit = Attackers & Boards[Pieces.WhiteKnight];
                while (bit.Any())
                {
                    var position = bit.BitScanForward();
                    var pin = (king.XrayRookAttacks(Occupied, position.AsBitBoard()) & (Boards[Pieces.BlackRook] | Boards[Pieces.BlackQueen])) |
                              (king.XrayBishopAttacks(Occupied, position.AsBitBoard()) & (Boards[Pieces.BlackBishop] | Boards[Pieces.BlackQueen]));
                    if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.WhiteKnight };
                    bit = bit.Remove(position);
                }
                bit = Attackers & Boards[Pieces.WhiteBishop];
                while (bit.Any())
                {
                    var position = bit.BitScanForward();
                    var pin = king.XrayRookAttacks(Occupied, position.AsBitBoard()) & (Boards[Pieces.BlackRook] | Boards[Pieces.BlackQueen]);
                    if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.WhiteBishop };
                    bit = bit.Remove(position);
                }
                bit = Attackers & Boards[Pieces.WhiteRook];
                while (bit.Any())
                {
                    var position = bit.BitScanForward();
                    var pin = king.XrayBishopAttacks(Occupied, position.AsBitBoard()) & (Boards[Pieces.BlackBishop] | Boards[Pieces.BlackQueen]);
                    if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.WhiteRook };
                    bit = bit.Remove(position);
                }
                bit = Attackers & Boards[Pieces.WhiteQueen];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteQueen };
                bit = Attackers & Boards[Pieces.WhiteKing];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteKing };
                return new AttackerBoard { Board = new BitBoard(0) };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly AttackerBoard GetNextAttackerPinToWhite()
            {
                var king = Boards[Pieces.BlackKing].BitScanForward();
                var bit = Attackers & Boards[Pieces.BlackPawn];
                while (bit.Any())
                {
                    var position = bit.BitScanForward();
                    var pin = king.XrayRookAttacks(Occupied, position.AsBitBoard()) & (Boards[Pieces.WhiteRook] | Boards[Pieces.WhiteQueen]);
                    if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.BlackPawn };
                    bit = bit.Remove(position);
                }
                bit = Attackers & Boards[Pieces.BlackKnight];
                while (bit.Any())
                {
                    var position = bit.BitScanForward();
                    var pin = (king.XrayRookAttacks(Occupied, position.AsBitBoard()) & (Boards[Pieces.WhiteRook] | Boards[Pieces.WhiteQueen])) |
                              (king.XrayBishopAttacks(Occupied, position.AsBitBoard()) & (Boards[Pieces.WhiteBishop] | Boards[Pieces.WhiteQueen]));
                    if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.BlackKnight };
                    bit = bit.Remove(position);
                }
                bit = Attackers & Boards[Pieces.BlackBishop];
                while (bit.Any())
                {
                    var position = bit.BitScanForward();
                    var pin = king.XrayRookAttacks(Occupied, position.AsBitBoard()) & (Boards[Pieces.WhiteRook] | Boards[Pieces.WhiteQueen]);
                    if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.BlackBishop };
                    bit = bit.Remove(position);
                }
                bit = Attackers & Boards[Pieces.BlackRook];
                while (bit.Any())
                {
                    var position = bit.BitScanForward();
                    var pin = king.XrayBishopAttacks(Occupied, position.AsBitBoard()) & (Boards[Pieces.WhiteBishop] | Boards[Pieces.WhiteQueen]);
                    if (pin.IsZero()) return new AttackerBoard { Board = position.AsBitBoard(), Piece = Pieces.BlackRook };
                    bit = bit.Remove(position);
                }
                bit = Attackers & Boards[Pieces.BlackQueen];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackQueen };
                bit = Attackers & Boards[Pieces.BlackKing];
                if (bit.Any()) return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackKing };
                return new AttackerBoard { Board = new BitBoard(0) };
            }
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

            BitBoard mayXRay = ~(state.Boards[Pieces.BlackKing] |
                            state.Boards[Pieces.BlackKnight] |
                            state.Boards[Pieces.WhiteKnight] |
                            state.Boards[Pieces.WhiteKing] |
                            _empty);

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
                        state.Attackers |= state.ConsiderWhiteXrays();
                    }

                    if (state.Attackers.IsZero()) break;

                    board = state.GetNextAttackerToWhite();
                }
                else
                {
                    if ((board.Board & mayXRay).Any())
                    {
                        state.Attackers |= state.ConsiderBlackXrays();
                    }

                    if (state.Attackers.IsZero()) break;

                    board = state.GetNextAttackerToBlack();
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

            BitBoard mayXRay = ~(state.Boards[Pieces.BlackKing] |
                            state.Boards[Pieces.BlackKnight] |
                            state.Boards[Pieces.WhiteKnight] |
                            state.Boards[Pieces.WhiteKing] |
                            _empty);

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
                        state.Attackers |= state.ConsiderWhiteXrays();
                    }

                    if (state.Attackers.IsZero()) break;

                    board = state.GetNextAttackerPinToWhite();
                }
                else
                {
                    if ((board.Board & mayXRay).Any())
                    {
                        state.Attackers |= state.ConsiderBlackXrays();
                    }

                    if (state.Attackers.IsZero()) break;

                    board = state.GetNextAttackerPinToBlack();
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
        private BitBoard GetAttackers(ref SeeState state)
        {
            ref var boardBase = ref state.Boards[0];
            return (_whitePawnPatterns[state.Position] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)) | 
                   (_blackPawnPatterns[state.Position] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)) |
                   _whiteKnightPatterns[state.Position] & (Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteKnight)) |
                   state.Position.BishopAttacks(state.Occupied) & (Unsafe.Add(ref boardBase, Pieces.BlackBishop) | Unsafe.Add(ref boardBase, Pieces.BlackQueen) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen)) |
                   state.Position.RookAttacks(state.Occupied) & (Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen) | Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen)) |
                   _whiteKingPatterns[state.Position] & (Unsafe.Add(ref boardBase, Pieces.BlackKing) | Unsafe.Add(ref boardBase, Pieces.WhiteKing));
        }
    }
}
