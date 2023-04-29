using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;

namespace Engine.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly byte _unitValue;
        private readonly short _mateValue;

        private readonly byte[] _notAbleCastleValue;
        private readonly byte[] _earlyQueenValue;
        private readonly byte[] _doubleBishopValue;
        private readonly byte[] _minorDefendedByPawnValue;
        private readonly byte[] _blockedPawnValue;
        private readonly byte[] _passedPawnValue;
        private readonly byte[] _doubledPawnValue;
        private readonly byte[] _isolatedPawnValue;
        private readonly byte[] _backwardPawnValue;
        private readonly byte[] _rookOnOpenFileValue;
        private readonly byte[] _rookOnHalfOpenFileValue;
        private readonly byte[] _rentgenValue;
        private readonly byte[] _rookConnectionValue;
        private readonly byte[] _knightAttackedByPawnValue;
        private readonly byte[] _bishopBlockedByPawnValue;
        private readonly byte[] _rookBlockedByKingValue;
        private readonly byte[] _doubleRookVerticalValue;
        private readonly byte[] _doubleRookHorizontalValue;
        private readonly byte[] _battaryValue;
        private readonly byte[] _openPawnValue;

        private readonly byte _kingShieldPreFaceValue;
        private readonly byte _kingShieldFaceValue;
        private readonly byte _kingZoneOpenFileValue;
        private readonly double _pieceAttackFactor;
        private readonly byte[] _pieceAttackValue;
        private readonly double[] _pieceAttackWeight;

        private readonly short[][] _values;
        private readonly short[][][] _staticValues;
        private readonly short[][][] _fullValues;
        private readonly byte[][] _distances;

        public EvaluationService(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
        {
            var evaluationProvider = configuration.Evaluation;
            _unitValue = (byte)evaluationProvider.Static.Unit;
            _mateValue = evaluationProvider.Static.Mate;

            _notAbleCastleValue = new byte[3];
            _earlyQueenValue = new byte[3];
            _doubleBishopValue = new byte[3];
            _minorDefendedByPawnValue = new byte[3];
            _blockedPawnValue = new byte[3];
            _passedPawnValue = new byte[3];
            _doubledPawnValue = new byte[3];
            _isolatedPawnValue = new byte[3];
            _backwardPawnValue = new byte[3];
            _rookOnOpenFileValue = new byte[3];
            _rookOnHalfOpenFileValue = new byte[3];
            _rentgenValue = new byte[3];
            _rookConnectionValue = new byte[3];
            _knightAttackedByPawnValue = new byte[3];
            _bishopBlockedByPawnValue = new byte[3];
            _rookBlockedByKingValue = new byte[3];
            _doubleRookVerticalValue = new byte[3];
            _doubleRookHorizontalValue = new byte[3];
            _battaryValue = new byte[3];
            _openPawnValue = new byte[3];
            for (byte i = 0; i < 3; i++)
            {
                var evaluationStatic = evaluationProvider.Static.GetBoard(i);
                _notAbleCastleValue[i] = (byte)(evaluationStatic.NotAbleCastleValue * _unitValue);
                _earlyQueenValue[i] = (byte)(evaluationStatic.EarlyQueenValue * _unitValue);
                _doubleBishopValue[i] = (byte)(evaluationStatic.DoubleBishopValue * _unitValue);
                _minorDefendedByPawnValue[i] = (byte)(evaluationStatic.MinorDefendedByPawnValue * _unitValue);
                _blockedPawnValue[i] = (byte)(evaluationStatic.BlockedPawnValue * _unitValue);
                _passedPawnValue[i] = (byte)(evaluationStatic.PassedPawnValue * _unitValue);
                _doubledPawnValue[i] = (byte)(evaluationStatic.DoubledPawnValue * _unitValue);
                _isolatedPawnValue[i] = (byte)(evaluationStatic.IsolatedPawnValue * _unitValue);
                _backwardPawnValue[i] = (byte)(evaluationStatic.BackwardPawnValue * _unitValue);
                _rookOnOpenFileValue[i] = (byte)(evaluationStatic.RookOnOpenFileValue * _unitValue);
                _rentgenValue[i] = (byte)(evaluationStatic.RentgenValue * _unitValue);
                _rookConnectionValue[i] = (byte)(evaluationStatic.RookConnectionValue * _unitValue);
                _rookOnHalfOpenFileValue[i] = (byte)(evaluationStatic.RookOnHalfOpenFileValue * _unitValue);
                _knightAttackedByPawnValue[i] = (byte)(evaluationStatic.KnightAttackedByPawnValue * _unitValue);
                _bishopBlockedByPawnValue[i] = (byte)(evaluationStatic.BishopBlockedByPawnValue * _unitValue);
                _rookBlockedByKingValue[i] = (byte)(evaluationStatic.RookBlockedByKingValue * _unitValue);
                _doubleRookVerticalValue[i] = (byte)(evaluationStatic.DoubleRookVerticalValue * _unitValue);
                _doubleRookHorizontalValue[i] = (byte)(evaluationStatic.DoubleRookHorizontalValue * _unitValue);
                _battaryValue[i] = (byte)(evaluationStatic.BattaryValue * _unitValue);
                _openPawnValue[i] = (byte)(evaluationStatic.OpenPawnValue * _unitValue);
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

            _distances = new byte[64][];
            for (int i = 0; i < 64; i++)
            {
                _distances[i] = new byte[64];
            }

            CalculateDistances();

            _kingShieldFaceValue = evaluationProvider.Static.KingSafety.KingShieldFaceValue;
            _kingShieldPreFaceValue = evaluationProvider.Static.KingSafety.KingShieldPreFaceValue;
            _kingZoneOpenFileValue = evaluationProvider.Static.KingSafety.KingZoneOpenFileValue;

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

            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    _distances[i][j] = (byte)manhattanDistance(i, j);
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
        public byte GetUnitValue()
        {
            return _unitValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetMinorDefendedByPawnValue(byte phase)
        {
            return _minorDefendedByPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetKnightAttackedByPawnValue(byte phase)
        {
            return _knightAttackedByPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetBlockedPawnValue(byte phase)
        {
            return _blockedPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetPassedPawnValue(byte phase)
        {
            return _passedPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetDoubledPawnValue(byte phase)
        {
            return _doubledPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetIsolatedPawnValue(byte phase)
        {
            return _isolatedPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetBackwardPawnValue(byte phase)
        {
            return _backwardPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetNotAbleCastleValue(byte phase)
        {
            return _notAbleCastleValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetEarlyQueenValue(byte phase)
        {
            return _earlyQueenValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetDoubleBishopValue(byte phase)
        {
            return _doubleBishopValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetRookOnOpenFileValue(byte phase)
        {
            return _rookOnOpenFileValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetRentgenValue(byte phase)
        {
            return _rentgenValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetRookConnectionValue(byte phase)
        {
            return _rookConnectionValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetRookOnHalfOpenFileValue(byte phase)
        {
            return _rookOnHalfOpenFileValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetBishopBlockedByPawnValue(byte phase)
        {
            return _bishopBlockedByPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetRookBlockedByKingValue(byte phase)
        {
            return _rookBlockedByKingValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetPawnAttackValue()
        {
            return _pieceAttackValue[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetKnightAttackValue()
        {
            return _pieceAttackValue[1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetBishopAttackValue()
        {
            return _pieceAttackValue[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetRookAttackValue()
        {
            return _pieceAttackValue[3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetQueenAttackValue()
        {
            return _pieceAttackValue[4];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetKingAttackValue()
        {
            return _pieceAttackValue[5];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetAttackWeight(int attackCount)
        {
            return _pieceAttackWeight[attackCount] / _pieceAttackFactor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetKingZoneOpenFileValue()
        {
            return _kingZoneOpenFileValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetKingShieldFaceValue()
        {
            return _kingShieldFaceValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetKingShieldPreFaceValue()
        {
            return _kingShieldPreFaceValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetOpenPawnValue(byte phase)
        {
            return _openPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetDoubleRookVerticalValue(byte phase)
        {
            return _doubleRookVerticalValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetDoubleRookHorizontalValue(byte phase)
        {
            return _doubleRookHorizontalValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetBattaryValue(byte phase)
        {
            return _battaryValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Distance(byte from, byte to)
        {
            return _distances[from][to];
        }

        #endregion

        #endregion
    }
}
