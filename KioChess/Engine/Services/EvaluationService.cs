using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Services
{
    public class EvaluationService : IEvaluationService
    {
        private bool _useCache;
        private int _nextDepth;
        private int _threshold;

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
        private readonly int[] _doubleRookValue;
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
        private Dictionary<ulong, short> _table;
        private DynamicCollection<ulong>[] _depthTable;
        private readonly IMoveHistoryService _moveHistory;

        public EvaluationService(IMoveHistoryService moveHistory, IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
        {
            _moveHistory = moveHistory;

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
            _doubleRookValue = new int[3];
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
                _doubleRookValue[i] = evaluationStatic.DoubleRookValue * _unitValue;
                _openPawnValue[i] = evaluationStatic.OpenPawnValue * _unitValue;
            }

            _values = new int[3][];
            for (byte i = 0; i < 3; i++)
            {
                _values[i] = new int[12];
                _values[i][Piece.WhitePawn.AsByte()] = evaluationProvider.GetPiece(i).Pawn;
                _values[i][Piece.BlackPawn.AsByte()] = evaluationProvider.GetPiece(i).Pawn;
                _values[i][Piece.WhiteKnight.AsByte()] = evaluationProvider.GetPiece(i).Knight;
                _values[i][Piece.BlackKnight.AsByte()] = evaluationProvider.GetPiece(i).Knight;
                _values[i][Piece.WhiteBishop.AsByte()] = evaluationProvider.GetPiece(i).Bishop;
                _values[i][Piece.BlackBishop.AsByte()] = evaluationProvider.GetPiece(i).Bishop;
                _values[i][Piece.WhiteKing.AsByte()] = evaluationProvider.GetPiece(i).King;
                _values[i][Piece.BlackKing.AsByte()] = evaluationProvider.GetPiece(i).King;
                _values[i][Piece.WhiteRook.AsByte()] = evaluationProvider.GetPiece(i).Rook;
                _values[i][Piece.BlackRook.AsByte()] = evaluationProvider.GetPiece(i).Rook;
                _values[i][Piece.WhiteQueen.AsByte()] = evaluationProvider.GetPiece(i).Queen;
                _values[i][Piece.BlackQueen.AsByte()] = evaluationProvider.GetPiece(i).Queen;
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

        public int Size => _table.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _table.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(byte piece, Phase phase)
        {
            return _values[(byte)phase][piece];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(byte piece, byte square, Phase phase)
        {
            return _staticValues[piece][(int)phase][square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFullValue(byte piece, byte square, Phase phase)
        {
            return _fullValues[piece][(byte)phase][square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Evaluate(IPosition position)
        {
            short value;
            if (_useCache)
            {
                var key = position.GetKey();
                if (_table.TryGetValue(key, out value))
                {
                    return value;
                }

                if (_table.Count > _threshold)
                {
                    ClearOnThreshold();
                }

                value = position.GetValue();
                _table.Add(key, value);
                _depthTable[_moveHistory.GetPly()].Add(key);
            }
            else
            {
                value = position.GetValue();
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearOnThreshold()
        {
            var dynamicCollection = _depthTable[_nextDepth % _depthTable.Length];
            foreach (var k in dynamicCollection)
            {
                _table.Remove(k);
            }

            dynamicCollection.Clear();
            _nextDepth++;
        }

        public void Initialize(short level)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            var useEvaluationCache = configurationProvider
                .GeneralConfiguration.UseEvaluationCache;
            var depth = configurationProvider
                .GeneralConfiguration.GameDepth;
            if (useEvaluationCache && level > 6)
            {
                _useCache = true;
                _depthTable = new DynamicCollection<ulong>[depth];
                for (var i = 0; i < _depthTable.Length; i++)
                {
                    _depthTable[i] = new DynamicCollection<ulong>();
                }

                int capacity;
                if (level == 7)
                {
                    capacity = 30000781;
                }
                else if (level == 8)
                {
                    capacity = 40000651;
                }
                else
                {
                    capacity = 49979687;
                }

                _threshold = 2 * capacity / 3;
                _table = new Dictionary<ulong, short>(capacity);
            }
            else
            {
                _useCache = false;
                _table = new Dictionary<ulong, short>(0);
            }
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
        public int GetMinorDefendedByPawnValue(Phase phase)
        {
            return _minorDefendedByPawnValue[(byte) phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKnightAttackedByPawnValue(Phase phase)
        {
            return _knightAttackedByPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlockedPawnValue(Phase phase)
        {
            return _blockedPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPassedPawnValue(Phase phase)
        {
            return _passedPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubledPawnValue(Phase phase)
        {
            return _doubledPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIsolatedPawnValue(Phase phase)
        {
            return _isolatedPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBackwardPawnValue(Phase phase)
        {
            return _backwardPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNotAbleCastleValue(Phase phase)
        {
            return _notAbleCastleValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEarlyQueenValue(Phase phase)
        {
            return _earlyQueenValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubleBishopValue(Phase phase)
        {
            return _doubleBishopValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookOnOpenFileValue(Phase phase)
        {
            return _rookOnOpenFileValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRentgenValue(Phase phase)
        {
            return _rentgenValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookConnectionValue(Phase phase)
        {
            return _rookConnectionValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookOnHalfOpenFileValue(Phase phase)
        {
            return _rookOnHalfOpenFileValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBishopBlockedByPawnValue(Phase phase)
        {
            return _bishopBlockedByPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookBlockedByKingValue(Phase phase)
        {
            return _rookBlockedByKingValue[(byte)phase];
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
        public int GetOpenPawnValue(Phase phase)
        {
            return _openPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubleRookValue(Phase phase)
        {
            return _doubleRookValue[(byte)phase];
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
