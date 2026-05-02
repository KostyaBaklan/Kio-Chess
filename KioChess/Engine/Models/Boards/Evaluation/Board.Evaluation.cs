using Engine.Models.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {
        private byte _whiteKingPosition;
        private byte _blackKingPosition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Evaluate()
        {
            _whitePawnAttacks = GetWhitePawnAttacks();
            _blackPawnAttacks = GetBlackPawnAttacks();
            _whiteKingPosition = _boards[Pieces.WhiteKing].BitScanForward();
            _blackKingPosition = _boards[Pieces.BlackKing].BitScanForward();
            _whiteKingZone = _whiteKingShield[_whiteKingPosition];
            _blackKingZone = _blackKingShield[_blackKingPosition];
            var phase = _moveHistory.GetPhase();

            _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);

            return phase == Phase.Middle ? EvaluateMiddle() : phase == Phase.End ? EvaluateEnd() : EvaluateOpening();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EvaluateOpposite()
        {
            _whitePawnAttacks = GetWhitePawnAttacks();
            _blackPawnAttacks = GetBlackPawnAttacks();
            _whiteKingPosition = _boards[Pieces.WhiteKing].BitScanForward();
            _blackKingPosition = _boards[Pieces.BlackKing].BitScanForward();
            _whiteKingZone = _whiteKingShield[_whiteKingPosition];
            _blackKingZone = _blackKingShield[_blackKingPosition];
            var phase = _moveHistory.GetPhase();

            _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);

            return phase == Phase.Middle ? EvaluateMiddleOpposite() : phase == Phase.End ? EvaluateEndOpposite() : EvaluateOpeningOpposite();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateEndOpposite() => EvaluateBlackEnd() - EvaluateWhiteEnd();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateMiddleOpposite() => EvaluateBlackMiddle() - EvaluateWhiteMiddle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateOpeningOpposite() => EvaluateBlackOpening() - EvaluateWhiteOpening();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateEnd() => EvaluateWhiteEnd() - EvaluateBlackEnd();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateMiddle() => EvaluateWhiteMiddle() - EvaluateBlackMiddle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateOpening() => EvaluateWhiteOpening() - EvaluateBlackOpening();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteOpening()
        {
            var value = EvaluateWhitePawnOpening() + EvaluateWhiteKingOpening();

            if (_boards[Pieces.WhiteKnight].Any())
                value += EvaluateWhiteKnightOpening();

            if (_boards[Pieces.WhiteBishop].Any())
                value += EvaluateWhiteBishopOpening();

            if (_boards[Pieces.WhiteRook].Any())
                value += EvaluateWhiteRookOpening();

            if (_boards[Pieces.WhiteQueen].Any())
                value += EvaluateWhiteQueenOpening();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteMiddle()
        {
            var value = EvaluateWhitePawnMiddle() + EvaluateWhiteKingMiddle();

            if (_boards[Pieces.WhiteKnight].Any())
                value += EvaluateWhiteKnightMiddle();

            if (_boards[Pieces.WhiteBishop].Any())
                value += EvaluateWhiteBishopMiddle();

            if (_boards[Pieces.WhiteRook].Any())
                value += EvaluateWhiteRookOpening();

            if (_boards[Pieces.WhiteQueen].Any())
                value += EvaluateWhiteQueenMiddle();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteEnd()
        {
            var value = EvaluateWhitePawnEnd() + EvaluateWhiteKingEnd();

            if (_boards[Pieces.WhiteKnight].Any())
                value += EvaluateWhiteKnightEnd();

            if (_boards[Pieces.WhiteBishop].Any())
                value += EvaluateWhiteBishopEnd();

            if (_boards[Pieces.WhiteRook].Any())
                value += EvaluateWhiteRookEnd();

            if (_boards[Pieces.WhiteQueen].Any())
                value += EvaluateWhiteQueenEnd();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackOpening()
        {
            var value = EvaluateBlackPawnOpening() + EvaluateBlackKingOpening();

            if (_boards[Pieces.BlackKnight].Any())
                value += EvaluateBlackKnightOpening();

            if (_boards[Pieces.BlackBishop].Any())
                value += EvaluateBlackBishopOpening();

            if (_boards[Pieces.BlackRook].Any())
                value += EvaluateBlackRookOpening();

            if (_boards[Pieces.BlackQueen].Any())
                value += EvaluateBlackQueenOpening();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackMiddle()
        {
            var value = EvaluateBlackPawnMiddle() + EvaluateBlackKingMiddle();

            if (_boards[Pieces.BlackKnight].Any())
                value += EvaluateBlackKnightMiddle();

            if (_boards[Pieces.BlackBishop].Any())
                value += EvaluateBlackBishopMiddle();

            if (_boards[Pieces.BlackRook].Any())
                value += EvaluateBlackRookOpening();

            if (_boards[Pieces.BlackQueen].Any())
                value += EvaluateBlackQueenMiddle();

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackEnd()
        {
            var value = EvaluateBlackPawnEnd() + EvaluateBlackKingEnd();

            if (_boards[Pieces.BlackKnight].Any())
                value += EvaluateBlackKnightEnd();

            if (_boards[Pieces.BlackBishop].Any())
                value += EvaluateBlackBishopEnd();

            if (_boards[Pieces.BlackRook].Any())
                value += EvaluateBlackRookEnd();

            if (_boards[Pieces.BlackQueen].Any())
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
            for (byte i = 6; i < 11; i++)
            {
                value += _evaluationService.GetPieceValue(i) * _boards[i].Count();
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteStaticValue()
        {
            int value = 0;
            for (byte i = 0; i < 5; i++)
            {
                value += _evaluationService.GetPieceValue(i) * _boards[i].Count();
            }

            return value;
        }
    }
}
