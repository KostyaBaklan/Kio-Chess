using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;

namespace Engine.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly short _unitValue;
        private readonly short _mateValue;

        private readonly short[] _notAbleCastleValue;
        private readonly short[] _earlyQueenValue;
        private readonly short[] _doubleBishopValue;
        private readonly short[] _minorDefendedByPawnValue;
        private readonly short[] _blockedPawnValue;
        private readonly short[] _passedPawnValue;
        private readonly short[] _doubledPawnValue;
        private readonly short[] _isolatedPawnValue;
        private readonly short[] _backwardPawnValue;
        private readonly short[] _rookOnOpenFileValue;
        private readonly short[] _rookOnHalfOpenFileValue;
        private readonly short[] _rentgenValue;
        private readonly short[] _rookConnectionValue;
        private readonly short[] _knightAttackedByPawnValue;
        private readonly short[] _bishopBlockedByPawnValue;
        private readonly short[] _rookBlockedByKingValue;
        private readonly short[] _doubleRookVerticalValue;
        private readonly short[] _doubleRookHorizontalValue;
        private readonly short[] _battaryValue;
        private readonly short[] _openPawnValue;

        private readonly short _kingShieldPreFaceValue;
        private readonly short _kingShieldFaceValue;
        private readonly short _kingZoneOpenFileValue;
        private readonly double _pieceAttackFactor;
        private readonly short[] _pieceAttackValue;
        private readonly double[] _pieceAttackWeight;

        private readonly short[][] _values;
        private readonly short[][][] _staticValues;
        private readonly short[][][] _fullValues;
        private readonly byte[,] _distances;

        public EvaluationService(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
        {
            var evaluationProvider = configuration.Evaluation;
            _unitValue = evaluationProvider.Static.Unit;
            _mateValue = evaluationProvider.Static.Mate;

            _notAbleCastleValue = new short[3];
            _earlyQueenValue = new short[3];
            _doubleBishopValue = new short[3];
            _minorDefendedByPawnValue = new short[3];
            _blockedPawnValue = new short[3];
            _passedPawnValue = new short[3];
            _doubledPawnValue = new short[3];
            _isolatedPawnValue = new short[3];
            _backwardPawnValue = new short[3];
            _rookOnOpenFileValue = new short[3];
            _rookOnHalfOpenFileValue = new short[3];
            _rentgenValue = new short[3];
            _rookConnectionValue = new short[3];
            _knightAttackedByPawnValue = new short[3];
            _bishopBlockedByPawnValue = new short[3];
            _rookBlockedByKingValue = new short[3];
            _doubleRookVerticalValue = new short[3];
            _doubleRookHorizontalValue = new short[3];
            _battaryValue = new short[3];
            _openPawnValue = new short[3];
            for (byte i = 0; i < 3; i++)
            {
                var evaluationStatic = evaluationProvider.Static.GetBoard(i);
                _notAbleCastleValue[i] = (short)(evaluationStatic.NotAbleCastleValue * _unitValue);
                _earlyQueenValue[i] = (short)(evaluationStatic.EarlyQueenValue * _unitValue);
                _doubleBishopValue[i] = (short)(evaluationStatic.DoubleBishopValue * _unitValue);
                _minorDefendedByPawnValue[i] = (short)(evaluationStatic.MinorDefendedByPawnValue * _unitValue);
                _blockedPawnValue[i] = (short)(evaluationStatic.BlockedPawnValue * _unitValue);
                _passedPawnValue[i] = (short)(evaluationStatic.PassedPawnValue * _unitValue);
                _doubledPawnValue[i] = (short)(evaluationStatic.DoubledPawnValue * _unitValue);
                _isolatedPawnValue[i] = (short)(evaluationStatic.IsolatedPawnValue * _unitValue);
                _backwardPawnValue[i] = (short)(evaluationStatic.BackwardPawnValue * _unitValue);
                _rookOnOpenFileValue[i] = (short)(evaluationStatic.RookOnOpenFileValue * _unitValue);
                _rentgenValue[i] = (short)(evaluationStatic.RentgenValue * _unitValue);
                _rookConnectionValue[i] = (short)(evaluationStatic.RookConnectionValue * _unitValue);
                _rookOnHalfOpenFileValue[i] = (short)(evaluationStatic.RookOnHalfOpenFileValue * _unitValue);
                _knightAttackedByPawnValue[i] = (short)(evaluationStatic.KnightAttackedByPawnValue * _unitValue);
                _bishopBlockedByPawnValue[i] = (short)(evaluationStatic.BishopBlockedByPawnValue * _unitValue);
                _rookBlockedByKingValue[i] = (short)(evaluationStatic.RookBlockedByKingValue * _unitValue);
                _doubleRookVerticalValue[i] = (short)(evaluationStatic.DoubleRookVerticalValue * _unitValue);
                _doubleRookHorizontalValue[i] = (short)(evaluationStatic.DoubleRookHorizontalValue * _unitValue);
                _battaryValue[i] = (short)(evaluationStatic.BattaryValue * _unitValue);
                _openPawnValue[i] = (short)(evaluationStatic.OpenPawnValue * _unitValue);
            }

            _values = new short[3][];
            for (byte i = 0; i < 3; i++)
            {
                _values[i] = new short[12];
                _values[i][Pieces.WhitePawn] = evaluationProvider.GetPiece(i).Pawn;
                _values[i][Pieces.BlackPawn] = evaluationProvider.GetPiece(i).Pawn;
                _values[i][Pieces.WhiteKnight] = evaluationProvider.GetPiece(i).Knight;
                _values[i][Pieces.BlackKnight] = evaluationProvider.GetPiece(i).Knight;
                _values[i][Pieces.WhiteBishop] = evaluationProvider.GetPiece(i).Bishop;
                _values[i][Pieces.BlackBishop] = evaluationProvider.GetPiece(i).Bishop;
                _values[i][Pieces.WhiteKing] = evaluationProvider.GetPiece(i).King;
                _values[i][Pieces.BlackKing] = evaluationProvider.GetPiece(i).King;
                _values[i][Pieces.WhiteRook] = evaluationProvider.GetPiece(i).Rook;
                _values[i][Pieces.BlackRook] = evaluationProvider.GetPiece(i).Rook;
                _values[i][Pieces.WhiteQueen] = evaluationProvider.GetPiece(i).Queen;
                _values[i][Pieces.BlackQueen] = evaluationProvider.GetPiece(i).Queen;
            }

            _staticValues = new short[12][][];
            _fullValues = new short[12][][];

            short factor = evaluationProvider.Static.Factor;
            for (byte i = 0; i < 12; i++)
            {
                _staticValues[i] = new short[3][];
                for (byte j = 0; j < 3; j++)
                {
                    _staticValues[i][j] = new short[64];
                    for (byte k = 0; k < 64; k++)
                    {
                        _staticValues[i][j][k] = (short)(staticValueProvider.GetValue(i, j, k) * factor);
                    }
                }
            }
            for (byte i = 0; i < 12; i++)
            {
                _fullValues[i] = new short[3][];
                for (byte j = 0; j < 3; j++)
                {
                    _fullValues[i][j] = new short[64];
                    for (byte k = 0; k < 64; k++)
                    {
                        _fullValues[i][j][k] = (short)(_staticValues[i][j][k] + _values[j][i]);
                    }
                }
            }

            _distances = new byte[64, 64];

            CalculateDistances();

            _kingShieldFaceValue = (short)evaluationProvider.Static.KingSafety.KingShieldFaceValue;
            _kingShieldPreFaceValue = (short)evaluationProvider.Static.KingSafety.KingShieldPreFaceValue;
            _kingZoneOpenFileValue = (short)evaluationProvider.Static.KingSafety.KingZoneOpenFileValue;

            _pieceAttackFactor = evaluationProvider.Static.KingSafety.AttackValueFactor;
            _pieceAttackValue = evaluationProvider.Static.KingSafety.PieceAttackValue;
            _pieceAttackWeight = evaluationProvider.Static.KingSafety.AttackWeight;
        }

        private void CalculateDistances()
        {
            int manhattanDistance(int sq1, int sq2)
            {
                int file1, file2, rank1, rank2;
                int rankDistance, fileDistance;
                file1 = sq1 & 7;
                file2 = sq2 & 7;
                rank1 = sq1 >> 3;
                rank2 = sq2 >> 3;
                rankDistance = Math.Abs(rank2 - rank1);
                fileDistance = Math.Abs(file2 - file1);
                return 5 * (rankDistance + fileDistance);
            }

            for (int i = 0; i < _distances.GetLength(0); i++)
            {
                for (int j = 0; j < _distances.GetLength(1); j++)
                {
                    _distances[i, j] = (byte)manhattanDistance(i, j);
                }
            }
        }

        #region Implementation of ICacheService

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetValue(byte piece, byte phase)
        {
            return _values[phase][piece];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetValue(byte piece, byte square, byte phase)
        {
            return _staticValues[piece][phase][square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetFullValue(byte piece, byte square, byte phase)
        {
            return _fullValues[piece][phase][square];
        }

        #region Evaluations

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetMateValue()
        {
            return _mateValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetUnitValue()
        {
            return _unitValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetMinorDefendedByPawnValue(byte phase)
        {
            return _minorDefendedByPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetKnightAttackedByPawnValue(byte phase)
        {
            return _knightAttackedByPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetBlockedPawnValue(byte phase)
        {
            return _blockedPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetPassedPawnValue(byte phase)
        {
            return _passedPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetDoubledPawnValue(byte phase)
        {
            return _doubledPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetIsolatedPawnValue(byte phase)
        {
            return _isolatedPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetBackwardPawnValue(byte phase)
        {
            return _backwardPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetNotAbleCastleValue(byte phase)
        {
            return _notAbleCastleValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetEarlyQueenValue(byte phase)
        {
            return _earlyQueenValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetDoubleBishopValue(byte phase)
        {
            return _doubleBishopValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetRookOnOpenFileValue(byte phase)
        {
            return _rookOnOpenFileValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetRentgenValue(byte phase)
        {
            return _rentgenValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetRookConnectionValue(byte phase)
        {
            return _rookConnectionValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetRookOnHalfOpenFileValue(byte phase)
        {
            return _rookOnHalfOpenFileValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetBishopBlockedByPawnValue(byte phase)
        {
            return _bishopBlockedByPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetRookBlockedByKingValue(byte phase)
        {
            return _rookBlockedByKingValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetPawnAttackValue()
        {
            return _pieceAttackValue[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetKnightAttackValue()
        {
            return _pieceAttackValue[1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetBishopAttackValue()
        {
            return _pieceAttackValue[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetRookAttackValue()
        {
            return _pieceAttackValue[3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetQueenAttackValue()
        {
            return _pieceAttackValue[4];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetKingAttackValue()
        {
            return _pieceAttackValue[5];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetAttackWeight(int attackCount)
        {
            return _pieceAttackWeight[attackCount] / _pieceAttackFactor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetKingZoneOpenFileValue()
        {
            return _kingZoneOpenFileValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetKingShieldFaceValue()
        {
            return _kingShieldFaceValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetKingShieldPreFaceValue()
        {
            return _kingShieldPreFaceValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetOpenPawnValue(byte phase)
        {
            return _openPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetDoubleRookVerticalValue(byte phase)
        {
            return _doubleRookVerticalValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetDoubleRookHorizontalValue(byte phase)
        {
            return _doubleRookHorizontalValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetBattaryValue(byte phase)
        {
            return _battaryValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Distance(byte from, byte to)
        {
            return _distances[from, to];
        }

        #endregion

        #endregion
    }
}
