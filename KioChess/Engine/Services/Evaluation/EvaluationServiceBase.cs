using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces.Config;
using Engine.Interfaces.Evaluation;
using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Services.Evaluation;


public abstract class EvaluationServiceBase : IEvaluationService
{
    private readonly short _mateValue;

    protected byte _doubleBishopValue;
    protected byte _minorDefendedByPawnValue;
    protected byte _blockedPawnValue;
    protected byte _passedPawnValue;
    protected byte _doubledPawnValue;
    protected byte _isolatedPawnValue;
    protected byte _backwardPawnValue;
    protected byte _rookOnOpenFileValue;
    protected byte _rookOnHalfOpenFileValue;
    protected byte _rentgenValue;
    protected byte _knightAttackedByPawnValue;
    protected byte _bishopBlockedByPawnValue;
    protected byte _rookBlockedByKingValue;
    protected byte _doubleRookVerticalValue;
    protected byte _doubleRookHorizontalValue;
    protected byte _battaryValue;
    protected short _noPawnsValue;
    private readonly byte _kingShieldPreFaceValue;
    private readonly byte _kingShieldFaceValue;
    private readonly byte _kingZoneOpenFileValue;
    private readonly byte _pawnStormValue4;
    private readonly byte _pawnStormValue5;
    private readonly byte _pawnStormValue6;
    private readonly byte _pawnAttackValue;
    private readonly byte _knightAttackValue;
    private readonly byte _bishopAttackValue;
    private readonly byte _rookAttackValue;
    private readonly byte _queenAttackValue;
    private readonly byte _kingAttackValue;
    private readonly double[] _pieceAttackWeight;
    protected short[] _values;
    protected short[][] _staticValues;
    protected short[][] _fullValues;
    private readonly byte[][] _distances;
    private  byte _forwardMoveValue;
    private byte _queenDistanceToKingValue;

    protected EvaluationServiceBase(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
    {
        var evaluationProvider = configuration.Evaluation;
        _mateValue = evaluationProvider.Static.Mate;

        _distances = new byte[64][];
        for (int i = 0; i < 64; i++)
        {
            _distances[i] = new byte[64];
        }

        CalculateDistances();

        _kingShieldFaceValue = evaluationProvider.Static.KingSafety.KingShieldFaceValue;
        _kingShieldPreFaceValue = evaluationProvider.Static.KingSafety.KingShieldPreFaceValue;
        _kingZoneOpenFileValue = evaluationProvider.Static.KingSafety.KingZoneOpenFileValue;

        _pawnStormValue4 = evaluationProvider.Static.KingSafety.PawnStormValue4;
        _pawnStormValue5 = evaluationProvider.Static.KingSafety.PawnStormValue5;
        _pawnStormValue6 = evaluationProvider.Static.KingSafety.PawnStormValue6;

        var pieceAttackValue = evaluationProvider.Static.KingSafety.PieceAttackValue;
        _pawnAttackValue = pieceAttackValue[Pieces.WhitePawn];
        _knightAttackValue = pieceAttackValue[Pieces.WhiteKnight];
        _bishopAttackValue = pieceAttackValue[Pieces.WhiteBishop];
        _rookAttackValue = pieceAttackValue[Pieces.WhiteRook];
        _queenAttackValue = pieceAttackValue[Pieces.WhiteQueen];
        _kingAttackValue = pieceAttackValue[Pieces.WhiteKing];

        _pieceAttackWeight = evaluationProvider.Static.KingSafety.AttackWeight;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short Distance(byte kingPosition, BitList positions)
    {
        short value = 0;
        var distances = _distances[kingPosition];
        for (byte i = 0; i < positions.Count; i++)
        {
            value += distances[positions[i]];
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetDistance(byte king, byte queen)
    {
        return _distances[king][queen];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsForward(MoveBase move)
    {
        return _fullValues[move.Piece][move.To] - _fullValues[move.Piece][move.From] > _forwardMoveValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetDifference(MoveBase move)
    {
        return _fullValues[move.Piece][move.To] - _fullValues[move.Piece][move.From];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetMateValue()
    {
        return _mateValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetQueenDistanceToKingValue()
    {
        return _queenDistanceToKingValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPawnAttackValue()
    {
        return _pawnAttackValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetKnightAttackValue()
    {
        return _knightAttackValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBishopAttackValue()
    {
        return _bishopAttackValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookAttackValue()
    {
        return _rookAttackValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetQueenAttackValue()
    {
        return _queenAttackValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetKingAttackValue()
    {
        return _kingAttackValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetAttackWeight(byte attackCount)
    {
        return _pieceAttackWeight[attackCount];
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
    public byte GetPawnStormValue4()
    {
        return _pawnStormValue4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPawnStormValue5()
    {
        return _pawnStormValue5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPawnStormValue6()
    {
        return _pawnStormValue6;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBackwardPawnValue() { return _backwardPawnValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBattaryValue() { return _battaryValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBishopBlockedByPawnValue() { return _bishopBlockedByPawnValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBlockedPawnValue() { return _blockedPawnValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetDoubleBishopValue() { return _doubleBishopValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetDoubledPawnValue() { return _doubledPawnValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetDoubleRookHorizontalValue() { return _doubleRookHorizontalValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetDoubleRookVerticalValue() { return _doubleRookVerticalValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetIsolatedPawnValue() { return _isolatedPawnValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetKnightAttackedByPawnValue() { return _knightAttackedByPawnValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetMinorDefendedByPawnValue() { return _minorDefendedByPawnValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetNoPawnsValue() { return _noPawnsValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPassedPawnValue() { return _passedPawnValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRentgenValue() { return _rentgenValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookBlockedByKingValue() { return _rookBlockedByKingValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookOnHalfOpenFileValue() { return _rookOnHalfOpenFileValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookOnOpenFileValue() { return _rookOnOpenFileValue; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetPieceValue(byte piece) { return _values[piece]; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetFullValue(byte piece, byte square) { return _fullValues[piece][square]; }

    protected void Initialize(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider, byte phase)
    {
        var evaluationProvider = configuration.Evaluation;

        var evaluationStatic = evaluationProvider.Static.GetBoard(phase);
        _doubleBishopValue = (byte)evaluationStatic.DoubleBishopValue;
        _minorDefendedByPawnValue = (byte)evaluationStatic.MinorDefendedByPawnValue;
        _blockedPawnValue = (byte)evaluationStatic.BlockedPawnValue;
        _passedPawnValue = (byte)evaluationStatic.PassedPawnValue;
        _doubledPawnValue = (byte)evaluationStatic.DoubledPawnValue;
        _isolatedPawnValue = (byte)evaluationStatic.IsolatedPawnValue;
        _backwardPawnValue = (byte)evaluationStatic.BackwardPawnValue;
        _rookOnOpenFileValue = (byte)evaluationStatic.RookOnOpenFileValue;
        _rentgenValue = (byte)evaluationStatic.RentgenValue;
        _rookOnHalfOpenFileValue = (byte)evaluationStatic.RookOnHalfOpenFileValue;
        _knightAttackedByPawnValue = (byte)evaluationStatic.KnightAttackedByPawnValue;
        _bishopBlockedByPawnValue = (byte)evaluationStatic.BishopBlockedByPawnValue;
        _rookBlockedByKingValue = (byte)evaluationStatic.RookBlockedByKingValue;
        _doubleRookVerticalValue = (byte)evaluationStatic.DoubleRookVerticalValue;
        _doubleRookHorizontalValue = (byte)evaluationStatic.DoubleRookHorizontalValue;
        _battaryValue = (byte)evaluationStatic.BattaryValue;
        _noPawnsValue = (short)-evaluationStatic.NoPawnsValue;
        _forwardMoveValue = evaluationStatic.ForwardMoveValue;
        _queenDistanceToKingValue = evaluationStatic.QueenDistanceToKingValue;

        _values = new short[12];
        _values[Pieces.WhitePawn] = evaluationProvider.GetPiece(phase).Pawn;
        _values[Pieces.BlackPawn] = evaluationProvider.GetPiece(phase).Pawn;
        _values[Pieces.WhiteKnight] = evaluationProvider.GetPiece(phase).Knight;
        _values[Pieces.BlackKnight] = evaluationProvider.GetPiece(phase).Knight;
        _values[Pieces.WhiteBishop] = evaluationProvider.GetPiece(phase).Bishop;
        _values[Pieces.BlackBishop] = evaluationProvider.GetPiece(phase).Bishop;
        _values[Pieces.WhiteKing] = evaluationProvider.GetPiece(phase).King;
        _values[Pieces.BlackKing] = evaluationProvider.GetPiece(phase).King;
        _values[Pieces.WhiteRook] = evaluationProvider.GetPiece(phase).Rook;
        _values[Pieces.BlackRook] = evaluationProvider.GetPiece(phase).Rook;
        _values[Pieces.WhiteQueen] = evaluationProvider.GetPiece(phase).Queen;
        _values[Pieces.BlackQueen] = evaluationProvider.GetPiece(phase).Queen;

        _staticValues = new short[12][];
        _fullValues = new short[12][];
        for (byte i = 0; i < 12; i++)
        {
            _staticValues[i] = new short[64];
            for (byte k = 0; k < 64; k++)
            {
                _staticValues[i][k] = (short)staticValueProvider.GetValue(i, phase, k);
            }
        }
        for (byte i = 0; i < 12; i++)
        {
            _fullValues[i] = new short[64];
            for (byte k = 0; k < 64; k++)
            {
                _fullValues[i][k] = (short)(_staticValues[i][k] + _values[i]);
            }
        }
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
            return rankDistance + fileDistance;
        }

        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                _distances[i][j] = (byte)manhattanDistance(i, j);
            }
        }
    }
}
