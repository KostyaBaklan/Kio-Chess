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
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteMopUpValue()
        {
            ref var boardBase = ref _boards[0];

            // Defender (black) must have no pawns - a lone pawn gives real drawing
            // chances (promotion race, stalemate tricks) that this term should not
            // override.
            if (Unsafe.Add(ref boardBase, Pieces.BlackPawn).Any()) return 0;

            // Opposite-colored-bishops-only fortress: mathematically already yields
            // advantage == 0 below (one bishop of equal value per side), but guarded
            // explicitly so this stays correct if piece values or the mop-up gating
            // above ever change.
            if (IsOppositeColoredBishopsFortress()) return 0;

            if (GetNonPawnMaterialDifference() < _mopUpMinAdvantageThreshold) return 0;

            return _evaluationService.GetMopUpValue(_whiteKingPosition, _blackKingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackMopUpValue()
        {
            ref var boardBase = ref _boards[0];

            if (Unsafe.Add(ref boardBase, Pieces.WhitePawn).Any()) return 0;

            if (IsOppositeColoredBishopsFortress()) return 0;

            if (-GetNonPawnMaterialDifference() < _mopUpMinAdvantageThreshold) return 0;

            return _evaluationService.GetMopUpValue(_blackKingPosition, _whiteKingPosition);
        }

        /// <summary>
        /// White minus black non-pawn, non-king material value (knights, bishops,
        /// rooks, queens), using pre-cached piece values and hardware popcount.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNonPawnMaterialDifference()
        {
            ref var boardBase = ref _boards[0];

            int white = _pieceValues[Pieces.WhiteKnight] * Unsafe.Add(ref boardBase, Pieces.WhiteKnight).Count()
                      + _pieceValues[Pieces.WhiteBishop] * Unsafe.Add(ref boardBase, Pieces.WhiteBishop).Count()
                      + _pieceValues[Pieces.WhiteRook] * Unsafe.Add(ref boardBase, Pieces.WhiteRook).Count()
                      + _pieceValues[Pieces.WhiteQueen] * Unsafe.Add(ref boardBase, Pieces.WhiteQueen).Count();

            int black = _pieceValues[Pieces.BlackKnight] * Unsafe.Add(ref boardBase, Pieces.BlackKnight).Count()
                      + _pieceValues[Pieces.BlackBishop] * Unsafe.Add(ref boardBase, Pieces.BlackBishop).Count()
                      + _pieceValues[Pieces.BlackRook] * Unsafe.Add(ref boardBase, Pieces.BlackRook).Count()
                      + _pieceValues[Pieces.BlackQueen] * Unsafe.Add(ref boardBase, Pieces.BlackQueen).Count();

            return white - black;
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
