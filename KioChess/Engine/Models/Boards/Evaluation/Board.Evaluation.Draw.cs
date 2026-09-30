using Engine.Models.Enums;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {
        /// <summary>
        /// Mop-up (mating-drive) evaluation: in decisively-winning, pawnless-for-the-
        /// defender endgames (e.g. K+Q vs K, K+R vs K, K+BB vs K), rewards driving the
        /// losing king toward the edge/corner and closing the distance between kings.
        /// Without this term the search has little static incentive to make progress
        /// once material is already decisive, which can lead to repetition/50-move
        /// draws instead of converting the win.
        ///
        /// Cost: a handful of bitboard Count() (popcount) calls plus two O(1) lookup
        /// table reads, only reached from the already phase-gated End evaluation.
        /// <summary>
        /// White-relative mop-up value; negate for the black-relative score.
        /// Computes the non-pawn material difference exactly once.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetMopUpValue()
        {
            ref var boardBase = ref _boards[0];

            // Cheapest gate first: if both sides have pawns the term is always 0
            // and we skip the material popcounts entirely.
            bool blackPawnless = Unsafe.Add(ref boardBase, Pieces.BlackPawn).IsZero();
            bool whitePawnless = Unsafe.Add(ref boardBase, Pieces.WhitePawn).IsZero();
            if (!blackPawnless && !whitePawnless) return 0;

            int advantage = GetNonPawnMaterialDifference();   // <-- once

            if (blackPawnless && advantage > _mopUpMinAdvantageThreshold)
            {
                return _evaluationService.GetMopUpValue(
                    _evaluationService.GetCornerDistance(_blackKingPosition),
                    _evaluationService.Distance(_whiteKingPosition)[_blackKingPosition]);
            }

            if (whitePawnless && -advantage > _mopUpMinAdvantageThreshold)
            {
                return -_evaluationService.GetMopUpValue(
                    _evaluationService.GetCornerDistance(_whiteKingPosition),
                    _evaluationService.Distance(_blackKingPosition)[_whiteKingPosition]);
            }

            return 0;
        }

        /// <summary>
        /// White minus black non-pawn, non-king material value (knights, bishops,
        /// rooks, queens), using pre-cached piece values and hardware popcount.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNonPawnMaterialDifference()
        {
            ref var boardBase = ref _boards[0];
            ref var pieceValuesBase = ref _pieceValues[0];

            return      Unsafe.Add(ref pieceValuesBase, Pieces.WhiteKnight) * Unsafe.Add(ref boardBase, Pieces.WhiteKnight).Count()
                      + Unsafe.Add(ref pieceValuesBase, Pieces.WhiteBishop) * Unsafe.Add(ref boardBase, Pieces.WhiteBishop).Count()
                      + Unsafe.Add(ref pieceValuesBase, Pieces.WhiteRook) * Unsafe.Add(ref boardBase, Pieces.WhiteRook).Count()
                      + Unsafe.Add(ref pieceValuesBase, Pieces.WhiteQueen) * Unsafe.Add(ref boardBase, Pieces.WhiteQueen).Count()
                      - Unsafe.Add(ref pieceValuesBase, Pieces.BlackKnight) * Unsafe.Add(ref boardBase, Pieces.BlackKnight).Count()
                      - Unsafe.Add(ref pieceValuesBase, Pieces.BlackBishop) * Unsafe.Add(ref boardBase, Pieces.BlackBishop).Count()
                      - Unsafe.Add(ref pieceValuesBase, Pieces.BlackRook) * Unsafe.Add(ref boardBase, Pieces.BlackRook).Count()
                      - Unsafe.Add(ref pieceValuesBase, Pieces.BlackQueen) * Unsafe.Add(ref boardBase, Pieces.BlackQueen).Count();
        }

        /// <summary>
        /// Scales an End-phase evaluation value by the precomputed rule50-decay
        /// penalty (fixed-point, denominator 1000), based on the current reversible
        /// -move (halfmove clock) count. Below the configured StartPly this is a
        /// fast-path no-op (no lookup, no multiply/divide). Not applied to mate
        /// scores - callers must only invoke this on plain positional/material
        /// evaluation values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ScaleByRule50(int value)
        {
            int reversibleMoves = _moveHistory.GetReversibleMovesCount();
            if (reversibleMoves <= _rule50StartPly) return value;

            if (reversibleMoves > 99) reversibleMoves = 99;

            int penalty = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_rule50PenaltyNumerators), reversibleMoves);

            return value - value * penalty / 1000;
        }
    }
}
