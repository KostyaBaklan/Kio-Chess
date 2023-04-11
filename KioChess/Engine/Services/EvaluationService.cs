using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;

namespace Engine.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly int _unitValue;
        private readonly int _mateValue;

        private readonly int[] _notAbleCastleValue;
        private readonly int[] _earlyQueenValue;
        private readonly int[] _doubleBishopValue;
        private readonly int[] _minorDefendedByPawnValue;
        private readonly int[] _blockedPawnValue;
        private readonly int[] _passedPawnValue;
        private readonly int[] _doubledPawnValue;
        private readonly int[] _isolatedPawnValue;
        private readonly int[] _backwardPawnValue;
        private readonly int[] _rookOnOpenFileValue;
        private readonly int[] _rookOnHalfOpenFileValue;
        private readonly int[] _rentgenValue;
        private readonly int[] _rookConnectionValue;
        private readonly int[] _knightAttackedByPawnValue;
        private readonly int[] _bishopBlockedByPawnValue;
        private readonly int[] _rookBlockedByKingValue;
        private readonly int[] _doubleRookVerticalValue;
        private readonly int[] _doubleRookHorizontalValue;
        private readonly int[] _battaryValue;
        private readonly int[] _openPawnValue;

        private readonly int _kingShieldPreFaceValue;
        private readonly int _kingShieldFaceValue;
        private readonly int _kingZoneOpenFileValue;
        private readonly double _pieceAttackFactor;
        private readonly int[] _pieceAttackValue;
        private readonly double[] _pieceAttackWeight;

        private readonly int[][] _values;
        private readonly int[][][] _staticValues;
        private readonly int[][][] _fullValues;
        private readonly byte[,] _distances;

        public EvaluationService(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
        {
            var evaluationProvider = configuration.Evaluation;
            _unitValue = evaluationProvider.Static.Unit;
            _mateValue = evaluationProvider.Static.Mate;

            _notAbleCastleValue = new int[3];
            _earlyQueenValue = new int[3];
            _doubleBishopValue = new int[3];
            _minorDefendedByPawnValue = new int[3];
            _blockedPawnValue = new int[3];
            _passedPawnValue = new int[3];
            _doubledPawnValue = new int[3];
            _isolatedPawnValue = new int[3];
            _backwardPawnValue = new int[3];
            _rookOnOpenFileValue = new int[3];
            _rookOnHalfOpenFileValue = new int[3];
            _rentgenValue = new int[3];
            _rookConnectionValue = new int[3];
            _knightAttackedByPawnValue = new int[3];
            _bishopBlockedByPawnValue = new int[3];
            _rookBlockedByKingValue = new int[3];
            _doubleRookVerticalValue = new int[3];
            _doubleRookHorizontalValue = new int[3];
            _battaryValue = new int[3];
            _openPawnValue = new int[3];
            for (byte i = 0; i < 3; i++)
            {
                var evaluationStatic = evaluationProvider.Static.GetBoard(i);
                _notAbleCastleValue[i] = evaluationStatic.NotAbleCastleValue * _unitValue;
                _earlyQueenValue[i] = evaluationStatic.EarlyQueenValue * _unitValue;
                _doubleBishopValue[i] = evaluationStatic.DoubleBishopValue * _unitValue;
                _minorDefendedByPawnValue[i] = evaluationStatic.MinorDefendedByPawnValue * _unitValue;
                _blockedPawnValue[i] = evaluationStatic.BlockedPawnValue * _unitValue;
                _passedPawnValue[i] = evaluationStatic.PassedPawnValue * _unitValue;
                _doubledPawnValue[i] = evaluationStatic.DoubledPawnValue * _unitValue;
                _isolatedPawnValue[i] = evaluationStatic.IsolatedPawnValue * _unitValue;
                _backwardPawnValue[i] = evaluationStatic.BackwardPawnValue * _unitValue;
                _rookOnOpenFileValue[i] = evaluationStatic.RookOnOpenFileValue * _unitValue;
                _rentgenValue[i] = evaluationStatic.RentgenValue * _unitValue;
                _rookConnectionValue[i] = evaluationStatic.RookConnectionValue * _unitValue;
                _rookOnHalfOpenFileValue[i] = evaluationStatic.RookOnHalfOpenFileValue * _unitValue;
                _knightAttackedByPawnValue[i] = evaluationStatic.KnightAttackedByPawnValue * _unitValue;
                _bishopBlockedByPawnValue[i] = evaluationStatic.BishopBlockedByPawnValue * _unitValue;
                _rookBlockedByKingValue[i] = evaluationStatic.RookBlockedByKingValue * _unitValue;
                _doubleRookVerticalValue[i] = evaluationStatic.DoubleRookVerticalValue * _unitValue;
                _doubleRookHorizontalValue[i] = evaluationStatic.DoubleRookHorizontalValue * _unitValue;
                _battaryValue[i] = evaluationStatic.BattaryValue * _unitValue;
                _openPawnValue[i] = evaluationStatic.OpenPawnValue * _unitValue;
            }

            _values = new int[3][];
            for (byte i = 0; i < 3; i++)
            {
                _values[i] = new int[12];
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

            _staticValues = new int[12][][];
            _fullValues = new int[12][][];

            short factor = evaluationProvider.Static.Factor;
            for (byte i = 0; i < 12; i++)
            {
                _staticValues[i] = new int[3][];
                for (byte j = 0; j < 3; j++)
                {
                    _staticValues[i][j] = new int[64];
                    for (byte k = 0; k < 64; k++)
                    {
                        _staticValues[i][j][k] = staticValueProvider.GetValue(i, j, k)* factor;
                    }
                }
            }
            for (byte i = 0; i < 12; i++)
            {
                _fullValues[i] = new int[3][];
                for (byte j = 0; j < 3; j++)
                {
                    _fullValues[i][j] = new int[64];
                    for (byte k = 0; k < 64; k++)
                    {
                        _fullValues[i][j][k] = _staticValues[i][j][k] + _values[j][i];
                    }
                }
            }

            _distances = new byte[64,64];

            CalculateDistances();

            _kingShieldFaceValue = evaluationProvider.Static.KingSafety.KingShieldFaceValue;
            _kingShieldPreFaceValue = evaluationProvider.Static.KingSafety.KingShieldPreFaceValue;
            _kingZoneOpenFileValue = evaluationProvider.Static.KingSafety.KingZoneOpenFileValue;

            _pieceAttackFactor = evaluationProvider.Static.KingSafety.AttackValueFactor;
            _pieceAttackValue = evaluationProvider.Static.KingSafety.PieceAttackValue;
            _pieceAttackWeight= evaluationProvider.Static.KingSafety.AttackWeight;
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
                return 5*(rankDistance + fileDistance);
            }

            for (int i = 0; i < _distances.GetLength(0); i++)
            {
                for(int j = 0; j < _distances.GetLength(1); j++)
                {
                    _distances[i, j] = (byte)manhattanDistance(i, j);
                }
            }
        }

        #region Implementation of ICacheService

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(byte piece,  byte phase)
        {
            return _values[phase][piece];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(byte piece, byte square,  byte phase)
        {
            return _staticValues[piece][phase][square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFullValue(byte piece, byte square,  byte phase)
        {
            return _fullValues[piece][phase][square];
        }

        #region Evaluations

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMateValue()
        {
            return _mateValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUnitValue()
        {
            return _unitValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMinorDefendedByPawnValue( byte phase)
        {
            return _minorDefendedByPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKnightAttackedByPawnValue( byte phase)
        {
            return _knightAttackedByPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlockedPawnValue( byte phase)
        {
            return _blockedPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPassedPawnValue( byte phase)
        {
            return _passedPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubledPawnValue( byte phase)
        {
            return _doubledPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIsolatedPawnValue( byte phase)
        {
            return _isolatedPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBackwardPawnValue( byte phase)
        {
            return _backwardPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNotAbleCastleValue( byte phase)
        {
            return _notAbleCastleValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEarlyQueenValue( byte phase)
        {
            return _earlyQueenValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubleBishopValue( byte phase)
        {
            return _doubleBishopValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookOnOpenFileValue( byte phase)
        {
            return _rookOnOpenFileValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRentgenValue( byte phase)
        {
            return _rentgenValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookConnectionValue( byte phase)
        {
            return _rookConnectionValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookOnHalfOpenFileValue( byte phase)
        {
            return _rookOnHalfOpenFileValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBishopBlockedByPawnValue( byte phase)
        {
            return _bishopBlockedByPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookBlockedByKingValue( byte phase)
        {
            return _rookBlockedByKingValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPawnAttackValue()
        {
            return _pieceAttackValue[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKnightAttackValue()
        {
            return _pieceAttackValue[1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBishopAttackValue()
        {
            return _pieceAttackValue[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookAttackValue()
        {
            return _pieceAttackValue[3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetQueenAttackValue()
        {
            return _pieceAttackValue[4];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKingAttackValue()
        {
            return _pieceAttackValue[5];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetAttackWeight(int attackCount)
        {
            return _pieceAttackWeight[attackCount] / _pieceAttackFactor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKingZoneOpenFileValue()
        {
            return _kingZoneOpenFileValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKingShieldFaceValue()
        {
            return _kingShieldFaceValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKingShieldPreFaceValue()
        {
            return _kingShieldPreFaceValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOpenPawnValue( byte phase)
        {
            return _openPawnValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubleRookVerticalValue( byte phase)
        {
            return _doubleRookVerticalValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubleRookHorizontalValue( byte phase)
        {
            return _doubleRookHorizontalValue[phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBattaryValue( byte phase)
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
