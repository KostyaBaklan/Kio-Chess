using Engine.Models.Bits;
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
            public BitBoard PositionBit;
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
            private readonly BitBoard GetWhiteAttackers()
            {
                ref var boardBase = ref Boards[0];
                return (_blackPawnPatterns[Position] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)) |
                       (_whiteKnightPatterns[Position] & Unsafe.Add(ref boardBase, Pieces.WhiteKnight)) |
                       (Position.BishopAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen))) |
                       (Position.RookAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen))) |
                       (_whiteKingPatterns[Position] & Unsafe.Add(ref boardBase, Pieces.WhiteKing));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private readonly BitBoard GetBlackAttackers()
            {
                ref var boardBase = ref Boards[0];
                return (_whitePawnPatterns[Position] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)) |
                       (_whiteKnightPatterns[Position] & Unsafe.Add(ref boardBase, Pieces.BlackKnight)) |
                       (Position.BishopAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.BlackBishop) | Unsafe.Add(ref boardBase, Pieces.BlackQueen))) |
                       (Position.RookAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen))) |
                       (_whiteKingPatterns[Position] & Unsafe.Add(ref boardBase, Pieces.BlackKing));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly AttackerBoard GetNextAttackerPinToBlack()
            {
                ref var boardBase = ref Boards[0];
                var king = Unsafe.Add(ref boardBase, Pieces.WhiteKing).BitScanForward();
                var bishops = Unsafe.Add(ref boardBase, Pieces.BlackBishop) | Unsafe.Add(ref boardBase, Pieces.BlackQueen);
                var rooks = Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen);

                // Pawns: can be pinned on ranks/files (rooks/queens) OR diagonals (bishops/queens)
                var bit = Attackers & Unsafe.Add(ref boardBase, Pieces.WhitePawn);
                while (bit.Any())
                {
                    var position = bit.Lsb();
                    var pin = king.XrayRookAttacks(Occupied, position) & rooks;
                    if (pin.IsZero())
                    {
                        pin = king.XrayBishopAttacks(Occupied, position) & bishops;
                        if (pin.IsZero() || pin == PositionBit)
                        {
                            return new AttackerBoard { Board = position, Piece = Pieces.WhitePawn };
                        }
                    }
                    bit = bit.Remove(position);
                }

                // Knights: can be pinned on ranks/files OR diagonals
                bit = Attackers & Unsafe.Add(ref boardBase, Pieces.WhiteKnight);
                while (bit.Any())
                {
                    var position = bit.Lsb();
                    var pin = (king.XrayRookAttacks(Occupied, position) & rooks) | (king.XrayBishopAttacks(Occupied, position) & bishops);
                    if (pin.IsZero()) return new AttackerBoard { Board = position, Piece = Pieces.WhiteKnight };
                    bit = bit.Remove(position);
                }

                // Bishops: check rook pins first (found cases show this order matters)
                bit = Attackers & Unsafe.Add(ref boardBase, Pieces.WhiteBishop);
                while (bit.Any())
                {
                    var position = bit.Lsb();
                    var pin = king.XrayRookAttacks(Occupied, position) & rooks;
                    if (pin.IsZero())
                    {
                        pin = king.XrayBishopAttacks(Occupied, position) & bishops;
                        if (pin.IsZero() || pin == PositionBit)
                        {
                            return new AttackerBoard { Board = position, Piece = Pieces.WhiteBishop };
                        }
                    }
                    bit = bit.Remove(position);
                }

                // Rooks: check bishop pins first, then rook pins
                bit = Attackers & Unsafe.Add(ref boardBase, Pieces.WhiteRook);
                while (bit.Any())
                {
                    var position = bit.Lsb();
                    var pin = king.XrayBishopAttacks(Occupied, position) & bishops;
                    if (pin.IsZero())
                    {
                        pin = king.XrayRookAttacks(Occupied, position) & rooks;
                        if (pin.IsZero() || pin == PositionBit)
                        {
                            return new AttackerBoard { Board = position, Piece = Pieces.WhiteRook };
                        }
                    }
                    bit = bit.Remove(position);
                }

                // Queens: must use while loop (not if) - can have multiple queens, all must be checked
                bit = Attackers & Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
                while (bit.Any())
                {
                    var position = bit.Lsb();
                    var pin = (king.XrayRookAttacks(Occupied, position) & rooks) | (king.XrayBishopAttacks(Occupied, position) & bishops);
                    if (pin.IsZero() || pin == PositionBit)
                    {
                        return new AttackerBoard { Board = position, Piece = Pieces.WhiteQueen };
                    }
                    bit = bit.Remove(position);
                }

                // King: cannot be pinned, always available if attacking
                bit = Attackers & Unsafe.Add(ref boardBase, Pieces.WhiteKing);
                if (bit.Any() && (Attackers & GetBlackAttackers()).IsZero())
                {
                    return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.WhiteKing };
                }

                return new AttackerBoard { Board = new BitBoard(0) };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly AttackerBoard GetNextAttackerPinToWhite()
            {
                ref var boardBase = ref Boards[0];
                var king = Unsafe.Add(ref boardBase, Pieces.BlackKing).BitScanForward();

                var bishops = Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
                var rooks = Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen);

                // Pawns: can be pinned on ranks/files (rooks/queens) OR diagonals (bishops/queens)
                var bit = Attackers & Unsafe.Add(ref boardBase, Pieces.BlackPawn);
                while (bit.Any())
                {
                    var position = bit.Lsb();
                    var pin = king.XrayRookAttacks(Occupied, position) & rooks;
                    if (pin.IsZero())
                    {
                        pin = king.XrayBishopAttacks(Occupied, position) & bishops;
                        if (pin.IsZero() || pin == PositionBit)
                        {
                            return new AttackerBoard { Board = position, Piece = Pieces.BlackPawn };
                        }
                    }
                    bit = bit.Remove(position);
                }

                // Knights: can be pinned on ranks/files OR diagonals
                bit = Attackers & Unsafe.Add(ref boardBase, Pieces.BlackKnight);
                while (bit.Any())
                {
                    var position = bit.Lsb();
                    var pin = (king.XrayRookAttacks(Occupied, position) & rooks) | (king.XrayBishopAttacks(Occupied, position) & bishops);
                    if (pin.IsZero()) return new AttackerBoard { Board = position, Piece = Pieces.BlackKnight };
                    bit = bit.Remove(position);
                }

                // Bishops: check rook pins first (found cases show this order matters)
                bit = Attackers & Unsafe.Add(ref boardBase, Pieces.BlackBishop);
                while (bit.Any())
                {
                    var position = bit.Lsb();
                    var pin = king.XrayRookAttacks(Occupied, position) & rooks;
                    if (pin.IsZero())
                    {
                        pin = king.XrayBishopAttacks(Occupied, position) & bishops;
                        if (pin.IsZero() || pin == PositionBit)
                        {
                            return new AttackerBoard { Board = position, Piece = Pieces.BlackBishop };
                        }
                    }
                    bit = bit.Remove(position);
                }

                // Rooks: check bishop pins first, then rook pins
                bit = Attackers & Unsafe.Add(ref boardBase, Pieces.BlackRook);
                while (bit.Any())
                {
                    var position = bit.Lsb();
                    var pin = king.XrayBishopAttacks(Occupied, position) & bishops;
                    if (pin.IsZero())
                    {
                        pin = king.XrayRookAttacks(Occupied, position) & rooks;
                        if (pin.IsZero() || pin == PositionBit)
                        {
                            return new AttackerBoard { Board = position, Piece = Pieces.BlackRook };
                        }
                    }
                    bit = bit.Remove(position);
                }

                // Queens: must use while loop (not if) - can have multiple queens, all must be checked
                bit = Attackers & Unsafe.Add(ref boardBase, Pieces.BlackQueen);
                while (bit.Any())
                {
                    var position = bit.Lsb();
                    var pin = (king.XrayRookAttacks(Occupied, position) & rooks) | (king.XrayBishopAttacks(Occupied, position) & bishops);
                    if (pin.IsZero() || pin == PositionBit)
                    {
                        return new AttackerBoard { Board = position, Piece = Pieces.BlackQueen };
                    }
                    bit = bit.Remove(position);
                }

                // King: cannot be pinned, always available if attacking
                bit = Attackers & Unsafe.Add(ref boardBase, Pieces.BlackKing);
                if (bit.Any() && (Attackers & GetWhiteAttackers()).IsZero())
                {
                    return new AttackerBoard { Board = bit.Lsb(), Piece = Pieces.BlackKing };
                }

                return new AttackerBoard { Board = new BitBoard(0) };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StaticExchangeWithPins(AttackBase attack)
        {
            var state = new SeeState
            {
                Boards = stackalloc BitBoard[12],
                Occupied = Occupied,
                Position = attack.To,
                PositionBit = attack.To.AsBitBoard()
            };

            Span<BitBoard> boards = _boards;
            boards.CopyTo(state.Boards);

            state.Attackers = GetAttackers(ref state);

            BitBoard mayXRay = ~(state.Boards[Pieces.BlackKing] |
                            state.Boards[Pieces.BlackKnight] |
                            state.Boards[Pieces.WhiteKnight] |
                            state.Boards[Pieces.WhiteKing] |
                            Empty);

            var to = state.PositionBit;
            var target = attack.Captured;

            state.Boards[target] ^= to;

            AttackerBoard board = new()
            {
                Board = attack.From.AsBitBoard(),
                Piece = attack.Piece
            };

            int v = 0, x;
            bool first = true;
            ref int values = ref _pieceValues[0];

            while (board.Board.Any())
            {
                if (first)
                {
                    x = v + Unsafe.Add(ref values, target);
                    if (x < 0) return x;
                    first = false;
                }
                else
                {
                    x = v - Unsafe.Add(ref values, target);
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
