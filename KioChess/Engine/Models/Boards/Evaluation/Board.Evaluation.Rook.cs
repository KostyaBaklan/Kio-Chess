using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookOpening()
        {
            int value = 0;

            ref var boardBase = ref _boards[0];
            var bits = Unsafe.Add(ref boardBase, Pieces.WhiteRook);
            BitBoard whiteRooks = bits;
            BitBoard whitePawns = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            BitBoard allPawns = whitePawns | Unsafe.Add(ref boardBase, Pieces.BlackPawn);

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteRookFullValue(coordinate);

                BitBoard rookFile = _rookFiles[coordinate];

                if ((coordinate > Squares.H2 && (_whiteFacing[coordinate] & allPawns).IsZero()) || (rookFile & allPawns).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();

                    if ((_blackKingPatterns[_blackKingPosition] & rookFile).Any())
                    {
                        value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                    }

                    if (whiteRooks != bits && (_whiteRookAttacks[coordinate] & whiteRooks).Any()
                        && (rookFile & whiteRooks).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnOpenFileValue();
                    }
                }
                else if ((coordinate > Squares.H2 && (_whiteFacing[coordinate] & whitePawns).IsZero()) ||
                    (rookFile & whitePawns).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();

                    if ((_blackKingPatterns[_blackKingPosition] & rookFile).Any())
                    {
                        value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                    }

                    if (whiteRooks != bits && (_whiteRookAttacks[coordinate] & whiteRooks).Any()
                        && (rookFile & whiteRooks).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                    }
                }
                if (whiteRooks != bits && coordinate < Squares.A2 && (_whiteRookAttacks[coordinate] & whiteRooks).Any()
                        && (_rookRanks[coordinate] & whiteRooks).Any())
                {
                    value += _evaluationService.GetConnectedRooksOnFirstRankValue();
                }

                value += GetWhiteRookPinsOpening(coordinate);

                if (coordinate < Squares.A2 && (_whiteRookKingPattern[coordinate] & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any() &&
                    (_whiteRookPawnPattern[coordinate] & whitePawns).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }

                //value += GetWhiteRookMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookEnd()
        {
            int value = 0;
            ref var boardBase = ref _boards[0];
            var bits = Unsafe.Add(ref boardBase, Pieces.WhiteRook);
            BitBoard whitePawns = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            BitBoard blackPawns = Unsafe.Add(ref boardBase, Pieces.BlackPawn);

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhiteRookFullValue(coordinate);

                BitBoard rookFile = _rookFiles[coordinate];

                if ((rookFile & (whitePawns | blackPawns)).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((rookFile & whitePawns).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                value += GetWhiteRookPinsEnd(coordinate);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookOpening()
        {
            int value = 0;
            ref var boardBase = ref _boards[0];
            var bits = Unsafe.Add(ref boardBase, Pieces.BlackRook);
            BitBoard blackPawns = Unsafe.Add(ref boardBase, Pieces.BlackPawn);
            BitBoard blackRooks = bits;

            BitBoard allPawns = Unsafe.Add(ref boardBase, Pieces.WhitePawn) | blackPawns;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackRookFullValue(coordinate);
                BitBoard rookFile = _rookFiles[coordinate];

                if ((coordinate < Squares.A7 && (_blackFacing[coordinate] & allPawns).IsZero()) ||
                    (rookFile & allPawns).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();

                    if ((_whiteKingPatterns[_whiteKingPosition] & rookFile).Any())
                    {
                        value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                    }

                    if (blackRooks != bits && (_blackRookAttacks[coordinate] & blackRooks).Any()
                        && (rookFile & blackRooks).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnOpenFileValue();
                    }
                }
                else if ((coordinate < Squares.A7 && (_blackFacing[coordinate] & blackPawns).IsZero()) ||
                    (rookFile & blackPawns).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();

                    if ((_whiteKingPatterns[_whiteKingPosition] & rookFile).Any())
                    {
                        value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                    }

                    if (blackRooks != bits && (_blackRookAttacks[coordinate] & blackRooks).Any()
                        && (rookFile & blackRooks).Any())
                    {
                        value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                    }
                }
                if (blackRooks != bits && coordinate > Squares.H7 && (_blackRookAttacks[coordinate] & blackRooks).Any()
                        && (_rookRanks[coordinate] & blackRooks).Any())
                {
                    value += _evaluationService.GetConnectedRooksOnFirstRankValue();
                }

                value += GetBlackRookPinsOpening(coordinate);

                if (coordinate > Squares.H7 && (_blackRookKingPattern[coordinate] & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any() &&
                    (_blackRookPawnPattern[coordinate] & blackPawns).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }

                //value += GetBlackRookMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookEnd()
        {
            int value = 0;
            ref var boardBase = ref _boards[0];
            var bits = Unsafe.Add(ref boardBase, Pieces.BlackRook);
            BitBoard whitePawns = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            BitBoard blackPawns = Unsafe.Add(ref boardBase, Pieces.BlackPawn);

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackRookFullValue(coordinate);

                BitBoard rookFile = _rookFiles[coordinate];

                if ((rookFile & (whitePawns | blackPawns)).IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((rookFile & blackPawns).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                value += GetBlackRookPinsEnd(coordinate);

                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
