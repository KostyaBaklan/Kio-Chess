using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {
        // Pawn hash: one table per phase (each phase has its own evaluation tables).
        // Entries are keyed by the exact white/black pawn bitboards (no collisions possible).
        // Cached values contain every term that depends on pawns only.
        // Terms that depend on other pieces (blocked pawns, king distance, blockade,
        // rook behind, unstoppable) are computed live, so the result is identical to a full evaluation.
        private int _pawnHashShift;
        private const ulong PawnHashMultiplierWhite = 0x9E3779B97F4A7C15UL;
        private const ulong PawnHashMultiplierBlack = 0xC2B2AE3D27D4EB4FUL;

        [StructLayout(LayoutKind.Sequential)]
        private struct PawnHashEntry
        {
            public ulong White;
            public ulong Black;
            public ulong WhitePassed;
            public ulong BlackPassed;
            public int WhiteValue;
            public int BlackValue;
        }

        private PawnHashEntry[] _openingPawnHash;
        private PawnHashEntry[] _middlePawnHash;
        private PawnHashEntry[] _endPawnHash;

        private void InitializePawnHash(int bits)
        {
            bits = Math.Clamp(bits, 1, 24);
            _pawnHashShift = 64 - bits;
            _openingPawnHash = CreatePawnHashTable(bits);
            _middlePawnHash = CreatePawnHashTable(bits);
            _endPawnHash = CreatePawnHashTable(bits);
        }

        private static PawnHashEntry[] CreatePawnHashTable(int bits)
        {
            var table = new PawnHashEntry[1 << bits];
            // All-ones can never be a real pawn set, so empty slots never match.
            Array.Fill(table, new PawnHashEntry { White = ulong.MaxValue, Black = ulong.MaxValue });
            return table;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref PawnHashEntry ProbePawnHash(PawnHashEntry[] table, ulong whites, ulong blacks, byte phase)
        {
            int index = (int)(((whites * PawnHashMultiplierWhite) ^ (blacks * PawnHashMultiplierBlack)) >> _pawnHashShift);
            ref var entry = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(table), index);

            if (entry.White != whites || entry.Black != blacks)
            {
                FillPawnHashEntry(ref entry, whites, blacks, phase);
            }

            return ref entry;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void FillPawnHashEntry(ref PawnHashEntry entry, ulong whites, ulong blacks, byte phase)
        {
            BitBoard white = new(whites);
            BitBoard black = new(blacks);

            if (phase == Phase.Opening)
            {
                entry.WhiteValue = WhitePawnCoreOpening(white, black);
                entry.BlackValue = BlackPawnCoreOpening(white, black);
                entry.WhitePassed = 0;
                entry.BlackPassed = 0;
            }
            else
            {
                entry.WhiteValue = WhitePawnCore(white, black, out entry.WhitePassed);
                entry.BlackValue = BlackPawnCore(white, black, out entry.BlackPassed);
            }

            entry.White = whites;
            entry.Black = blacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteBlockedPawnsPenalty(ulong whites) =>
            BitOperations.PopCount((whites << 8) & (ulong)Blacks) * _evaluationService.GetBlockedPawnValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackBlockedPawnsPenalty(ulong blacks) =>
            BitOperations.PopCount((blacks >> 8) & (ulong)Whites) * _evaluationService.GetBlockedPawnValue();

        #region Phase entry points

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhitePawnOpening()
        {
            ref var boardBase = ref _boards[0];
            ulong whites = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            ulong blacks = Unsafe.Add(ref boardBase, Pieces.BlackPawn);

            ref var entry = ref ProbePawnHash(_openingPawnHash, whites, blacks, Phase.Opening);
            return entry.WhiteValue - WhiteBlockedPawnsPenalty(whites);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhitePawnMiddle()
        {
            ref var boardBase = ref _boards[0];
            ulong whites = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            ulong blacks = Unsafe.Add(ref boardBase, Pieces.BlackPawn);

            ref var entry = ref ProbePawnHash(_middlePawnHash, whites, blacks, Phase.Middle);
            return entry.WhiteValue - WhiteBlockedPawnsPenalty(whites);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhitePawnEnd()
        {
            ref var boardBase = ref _boards[0];
            ulong whites = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            ulong blacks = Unsafe.Add(ref boardBase, Pieces.BlackPawn);

            ref var entry = ref ProbePawnHash(_endPawnHash, whites, blacks, Phase.End);
            int value = entry.WhiteValue - WhiteBlockedPawnsPenalty(whites);

            if (entry.WhitePassed != 0)
            {
                value += WhitePassedPawnEndValue(entry.WhitePassed);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackPawnOpening()
        {
            ref var boardBase = ref _boards[0];
            ulong whites = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            ulong blacks = Unsafe.Add(ref boardBase, Pieces.BlackPawn);

            ref var entry = ref ProbePawnHash(_openingPawnHash, whites, blacks, Phase.Opening);
            return entry.BlackValue - BlackBlockedPawnsPenalty(blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackPawnMiddle()
        {
            ref var boardBase = ref _boards[0];
            ulong whites = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            ulong blacks = Unsafe.Add(ref boardBase, Pieces.BlackPawn);

            ref var entry = ref ProbePawnHash(_middlePawnHash, whites, blacks, Phase.Middle);
            return entry.BlackValue - BlackBlockedPawnsPenalty(blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackPawnEnd()
        {
            ref var boardBase = ref _boards[0];
            ulong whites = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            ulong blacks = Unsafe.Add(ref boardBase, Pieces.BlackPawn);

            ref var entry = ref ProbePawnHash(_endPawnHash, whites, blacks, Phase.End);
            int value = entry.BlackValue - BlackBlockedPawnsPenalty(blacks);

            if (entry.BlackPassed != 0)
            {
                value += BlackPassedPawnEndValue(entry.BlackPassed);
            }

            return value;
        }

        #endregion

        #region Cached (pawn-only) terms

        private int WhitePawnCoreOpening(BitBoard whites, BitBoard blacks)
        {
            int value = 0;
            var bits = whites;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhitePawnFullValue(coordinate);

                if ((_whiteDoublePawns[coordinate] & whites).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_whiteIsolatedPawns[coordinate] & whites).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else if (((_whiteBackwardSupportPawns[coordinate] & whites).IsZero() &&
                            (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < Squares.A3 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                            (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        private int BlackPawnCoreOpening(BitBoard whites, BitBoard blacks)
        {
            int value = 0;
            var bits = blacks;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackPawnFullValue(coordinate);

                if ((_blackDoublePawns[coordinate] & blacks).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_blackIsolatedPawns[coordinate] & blacks).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else if (((_blackBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                            (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > Squares.H6 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                            (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        // Shared by middle game and end game. 'passed' receives the passed pawns that
        // carry a bonus, so the end game can add its piece/king dependent terms for them.
        private int WhitePawnCore(BitBoard whites, BitBoard blacks, out ulong passed)
        {
            passed = 0;

            if (whites.IsZero())
                return _evaluationService.GetNoPawnsValue();

            int value = 0;
            BitBoard allPawns = whites | blacks;
            var bits = whites;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhitePawnFullValue(coordinate);

                if ((_whiteDoublePawns[coordinate] & whites).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_whiteFacing[coordinate] & allPawns).IsZero()
                    && (_whitePassedPawns[coordinate] & blacks).IsZero())
                {
                    var pp = _evaluationService.GetWhitePassedPawnValue(coordinate);
                    if (pp > 0)
                    {
                        value += pp;

                        if ((_whiteProtectedPassedPawns[coordinate] & whites).Any())
                        {
                            value += _evaluationService.GetWhiteProtectedPassedPawnValue(coordinate);
                        }

                        if ((_whiteConnectedPassedPawns[coordinate] & whites).Any())
                        {
                            value += _evaluationService.GetWhiteConnectedPassedPawnValue(coordinate);
                        }

                        passed |= 1UL << coordinate;
                    }
                }

                if ((_whiteIsolatedPawns[coordinate] & whites).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else if (((_whiteBackwardSupportPawns[coordinate] & whites).IsZero() &&
                            (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < Squares.A3 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                            (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        private int BlackPawnCore(BitBoard whites, BitBoard blacks, out ulong passed)
        {
            passed = 0;

            if (blacks.IsZero())
                return _evaluationService.GetNoPawnsValue();

            int value = 0;
            BitBoard allPawns = whites | blacks;
            var bits = blacks;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackPawnFullValue(coordinate);

                if ((_blackDoublePawns[coordinate] & blacks).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_blackFacing[coordinate] & allPawns).IsZero()
                    && (_blackPassedPawns[coordinate] & whites).IsZero())
                {
                    var pp = _evaluationService.GetBlackPassedPawnValue(coordinate);
                    if (pp > 0)
                    {
                        value += pp;

                        if ((_blackProtectedPassedPawns[coordinate] & blacks).Any())
                        {
                            value += _evaluationService.GetBlackProtectedPassedPawnValue(coordinate);
                        }

                        if ((_blackConnectedPassedPawns[coordinate] & blacks).Any())
                        {
                            value += _evaluationService.GetBlackConnectedPassedPawnValue(coordinate);
                        }

                        passed |= 1UL << coordinate;
                    }
                }

                if ((_blackIsolatedPawns[coordinate] & blacks).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else if (((_blackBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                            (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > Squares.H6 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                            (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        #endregion

        #region Live end game passed pawn terms (depend on kings, pieces and occupancy)

        private int WhitePassedPawnEndValue(ulong passedPawns)
        {
            int value = 0;
            ref var boardBase = ref _boards[0];
            var bits = new BitBoard(passedPawns);

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();

                // Full king distance factor in endgame
                value += _evaluationService.GetKingDistanceFactor(coordinate, _whiteKingPosition, _blackKingPosition);

                // Check for blockade on the next square
                byte nextSquare = (byte)(coordinate + 8);
                if (Blacks.IsSet(nextSquare))
                {
                    value -= _evaluationService.GetBlockadePenalty(_pieces[nextSquare]);
                }

                // Tarrasch Rule: Check if any friendly rook is behind this passed pawn
                BitBoard friendlyRooksOnFile = _rookFiles[coordinate] & Unsafe.Add(ref boardBase, Pieces.WhiteRook);
                if (friendlyRooksOnFile.Any())
                {
                    // Check if any rook is behind (lower square for white)
                    var rookSquare = friendlyRooksOnFile.BitScanForward();
                    if (rookSquare < coordinate && (_fileBetween[rookSquare][coordinate] & Occupied).IsZero())
                    {
                        value += _evaluationService.GetRookBehindPassedPawnValue();
                    }
                }

                // Unstoppable passed pawn: Check if enemy king is outside the "square of the pawn"
                // Only check if path ahead is clear (no blockade)
                if ((_whiteFacing[coordinate] & Occupied).IsZero() && !_whitePassedPawnSquare[coordinate].IsSet(_blackKingPosition))
                {
                    value += _evaluationService.GetUnstoppablePassedPawnValue();
                }

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        private int BlackPassedPawnEndValue(ulong passedPawns)
        {
            int value = 0;
            ref var boardBase = ref _boards[0];
            var bits = new BitBoard(passedPawns);

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();

                // Full king distance factor in endgame
                value += _evaluationService.GetKingDistanceFactor(coordinate, _blackKingPosition, _whiteKingPosition);

                // Check for blockade on the next square
                byte nextSquare = (byte)(coordinate - 8);
                if (Whites.IsSet(nextSquare))
                {
                    value -= _evaluationService.GetBlockadePenalty(_pieces[nextSquare]);
                }

                // Tarrasch Rule: Check if any friendly rook is behind this passed pawn
                BitBoard friendlyRooksOnFile = _rookFiles[coordinate] & Unsafe.Add(ref boardBase, Pieces.BlackRook);
                if (friendlyRooksOnFile.Any())
                {
                    // Check if any rook is behind (higher square for black)
                    var rookSquare = friendlyRooksOnFile.BitScanReverse();
                    if (rookSquare > coordinate && (_fileBetween[coordinate][rookSquare] & Occupied).IsZero())
                    {
                        value += _evaluationService.GetRookBehindPassedPawnValue();
                    }
                }

                // Unstoppable passed pawn: Check if enemy king is outside the "square of the pawn"
                // Only check if path ahead is clear (no blockade)
                if ((_blackFacing[coordinate] & Occupied).IsZero() && !_blackPassedPawnSquare[coordinate].IsSet(_whiteKingPosition))
                {
                    value += _evaluationService.GetUnstoppablePassedPawnValue();
                }

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        #endregion
    }
}
