using Engine.Models.Enums;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {
        private byte _whiteKingPosition;
        private byte _blackKingPosition;

        // Evaluation cache: independent from the transposition table.
        // Stores the white-relative static evaluation (EvaluateOpposite() == -Evaluate() exactly).
        // The evaluation depends on piece placement (Hash), the phase (ply dependent) and the
        // castling rights (king shield in the opening); all of them are part of the entry check.
        private int _evaluationCacheShift;
        private EvaluationCacheEntry[] _evaluationCache;

        [StructLayout(LayoutKind.Sequential)]
        private struct EvaluationCacheEntry
        {
            public ulong Key;
            public int Value;
            public int Meta;
        }

        private void InitializeEvaluationCache(int bits)
        {
            bits = Math.Clamp(bits, 1, 24);
            _evaluationCacheShift = 64 - bits;
            _evaluationCache = new EvaluationCacheEntry[1 << bits];
            // Meta is always >= 0 for a real entry, so empty slots never match.
            Array.Fill(_evaluationCache, new EvaluationCacheEntry { Meta = -1 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Evaluate()
        {
            var phase = _moveHistory.GetPhase();

            _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);

            return EvaluateCached(phase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EvaluateOpposite()
        {
            var phase = _moveHistory.GetPhase();

            _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);

            return -EvaluateCached(phase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateCached(byte phase)
        {
            ulong hash = Hash;
            int meta = phase | _moveHistory.GetCastleRights();

            ref var entry = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_evaluationCache), (int)(hash >> _evaluationCacheShift));

            if (entry.Key == hash && entry.Meta == meta)
            {
                return entry.Value;
            }

            int value = ComputeEvaluation(phase);
            entry.Key = hash;
            entry.Value = value;
            entry.Meta = meta;
            return value;
        }

        // White-relative evaluation without the cache.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ComputeEvaluation(byte phase)
        {
            ComputeAttacks();

            return phase == Phase.Middle ? EvaluateMiddle() : phase == Phase.End ? EvaluateEnd() : EvaluateOpening();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateEndOpposite() => EvaluateBlackEnd() - EvaluateWhiteEnd() - GetMopUpValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateMiddleOpposite() => EvaluateBlackMiddle() - EvaluateWhiteMiddle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateOpeningOpposite() => EvaluateBlackOpening() - EvaluateWhiteOpening();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateEnd() => EvaluateWhiteEnd() - EvaluateBlackEnd() + GetMopUpValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateMiddle() => EvaluateWhiteMiddle() - EvaluateBlackMiddle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateOpening() => EvaluateWhiteOpening() - EvaluateBlackOpening();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteOpening()
        {
            var value = EvaluateWhitePawnOpening() + EvaluateWhiteKingOpening();
            ref var boardBase = ref _boards[0];

            if (Unsafe.Add(ref boardBase, Pieces.WhiteKnight).Any())
                value += EvaluateWhiteKnightOpening();

            if (Unsafe.Add(ref boardBase, Pieces.WhiteBishop).Any())
                value += EvaluateWhiteBishopOpening();

            if (Unsafe.Add(ref boardBase, Pieces.WhiteRook).Any())
                value += EvaluateWhiteRookOpening();

            if (Unsafe.Add(ref boardBase, Pieces.WhiteQueen).Any())
                value += EvaluateWhiteQueenOpening();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteMiddle()
        {
            var value = EvaluateWhitePawnMiddle() + EvaluateWhiteKingMiddle();
            ref var boardBase = ref _boards[0];

            if (Unsafe.Add(ref boardBase, Pieces.WhiteKnight).Any())
                value += EvaluateWhiteKnightMiddle();

            if (Unsafe.Add(ref boardBase, Pieces.WhiteBishop).Any())
                value += EvaluateWhiteBishopMiddle();

            if (Unsafe.Add(ref boardBase, Pieces.WhiteRook).Any())
                value += EvaluateWhiteRookOpening();

            if (Unsafe.Add(ref boardBase, Pieces.WhiteQueen).Any())
                value += EvaluateWhiteQueenMiddle();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteEnd()
        {
            var value = EvaluateWhitePawnEnd() + EvaluateWhiteKingEnd();
            ref var boardBase = ref _boards[0];

            if (Unsafe.Add(ref boardBase, Pieces.WhiteKnight).Any())
                value += EvaluateWhiteKnightEnd();

            if (Unsafe.Add(ref boardBase, Pieces.WhiteBishop).Any())
                value += EvaluateWhiteBishopEnd();

            if (Unsafe.Add(ref boardBase, Pieces.WhiteRook).Any())
                value += EvaluateWhiteRookEnd();

            if (Unsafe.Add(ref boardBase, Pieces.WhiteQueen).Any())
                value += EvaluateWhiteQueenEnd();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackOpening()
        {
            var value = EvaluateBlackPawnOpening() + EvaluateBlackKingOpening();
            ref var boardBase = ref _boards[0];

            if (Unsafe.Add(ref boardBase, Pieces.BlackKnight).Any())
                value += EvaluateBlackKnightOpening();

            if (Unsafe.Add(ref boardBase, Pieces.BlackBishop).Any())
                value += EvaluateBlackBishopOpening();

            if (Unsafe.Add(ref boardBase, Pieces.BlackRook).Any())
                value += EvaluateBlackRookOpening();

            if (Unsafe.Add(ref boardBase, Pieces.BlackQueen).Any())
                value += EvaluateBlackQueenOpening();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackMiddle()
        {
            var value = EvaluateBlackPawnMiddle() + EvaluateBlackKingMiddle();
            ref var boardBase = ref _boards[0];

            if (Unsafe.Add(ref boardBase, Pieces.BlackKnight).Any())
                value += EvaluateBlackKnightMiddle();

            if (Unsafe.Add(ref boardBase, Pieces.BlackBishop).Any())
                value += EvaluateBlackBishopMiddle();

            if (Unsafe.Add(ref boardBase, Pieces.BlackRook).Any())
                value += EvaluateBlackRookOpening();

            if (Unsafe.Add(ref boardBase, Pieces.BlackQueen).Any())
                value += EvaluateBlackQueenMiddle();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackEnd()
        {
            var value = EvaluateBlackPawnEnd() + EvaluateBlackKingEnd();
            ref var boardBase = ref _boards[0];

            if (Unsafe.Add(ref boardBase, Pieces.BlackKnight).Any())
                value += EvaluateBlackKnightEnd();

            if (Unsafe.Add(ref boardBase, Pieces.BlackBishop).Any())
                value += EvaluateBlackBishopEnd();

            if (Unsafe.Add(ref boardBase, Pieces.BlackRook).Any())
                value += EvaluateBlackRookEnd();

            if (Unsafe.Add(ref boardBase, Pieces.BlackQueen).Any())
                value += EvaluateBlackQueenEnd();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetStaticValue()
        {
            var _phase = _moveHistory.GetPhase();
            _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
            return GetWhiteStaticValue() - GetBlackStaticValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackStaticValue()
        {
            int value = 0;
            ref var boardBase = ref _boards[0];
            for (byte i = 6; i < 11; i++)
            {
                value += _evaluationService.GetPieceValue(i) * Unsafe.Add(ref boardBase, i).Count();
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteStaticValue()
        {
            int value = 0;
            ref var boardBase = ref _boards[0];
            for (byte i = 0; i < 5; i++)
            {
                value += _evaluationService.GetPieceValue(i) * Unsafe.Add(ref boardBase, i).Count();
            }

            return value;
        }
    }
}
