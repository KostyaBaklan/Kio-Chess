using Engine.Models.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKnightMiddle() => GetWhiteKnightValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKnightEnd() => GetWhiteKnightEndValue();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKnightMiddle() => GetBlackKnightValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKnightEnd() => GetBlackKnightEndValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKnightEndValue()
        {
            int value = 0;

            var bits = Unsafe.Add(ref _boards[0], Pieces.WhiteKnight);
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();

                value += _evaluationService.GetWhiteKnightFullValue(coordinate);
                value += GetEvaluationWhiteKnightMobility(coordinate);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKnightEndValue()
        {
            int value = 0;

            var bits = Unsafe.Add(ref _boards[0], Pieces.BlackKnight);
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();

                value += _evaluationService.GetBlackKnightFullValue(coordinate);
                value += GetEvaluationBlackKnightMobility(coordinate);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsWhiteOutpost(byte coordinate) =>
            (Unsafe.Add(ref _boards[0], Pieces.WhitePawn) & _whiteOutpostSupport[coordinate]).Any()
            && (Unsafe.Add(ref _boards[0], Pieces.BlackPawn) & _whiteOutpostSpan[coordinate]).IsZero();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBlackOutpost(byte coordinate) =>
            (Unsafe.Add(ref _boards[0], Pieces.BlackPawn) & _blackOutpostSupport[coordinate]).Any()
            && (Unsafe.Add(ref _boards[0], Pieces.WhitePawn) & _blackOutpostSpan[coordinate]).IsZero();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKnightValue()
        {
            int value = 0;

            var bits = Unsafe.Add(ref _boards[0], Pieces.WhiteKnight);
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();

                value += _evaluationService.GetWhiteKnightFullValue(coordinate);
                if (IsWhiteOutpost(coordinate))
                {
                    value += _evaluationService.GetKnightOutpostValue();
                }
                //if ((_whiteMinorDefense[coordinate] & _boards[Pieces.WhitePawn]).Any())
                //{
                //    value += _evaluationService.GetMinorDefendedByPawnValue();
                //}

                value += GetEvaluationWhiteKnightMobility(coordinate);

                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKnightValue()
        {
            int value = 0;
            var bits = Unsafe.Add(ref _boards[0], Pieces.BlackKnight);
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackKnightFullValue(coordinate);
                if (IsBlackOutpost(coordinate))
                {
                    value += _evaluationService.GetKnightOutpostValue();
                }

                //if ((_blackMinorDefense[coordinate] & _boards[Pieces.BlackPawn]).Any())
                //{
                //    value += _evaluationService.GetMinorDefendedByPawnValue();
                //}

                value += GetEvaluationBlackKnightMobility(coordinate);
                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
