using Engine.Interfaces.Config;
using Engine.Models.Boards.Buffers;
using Engine.Models.Config;
using Engine.Models.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Services.Evaluation;


public abstract class EvaluationServiceBase
{
    protected byte _doubleBishopValue;
    protected byte _blockedPawnValue;
    protected byte _doubledPawnValue;
    protected byte _isolatedPawnValue;
    protected byte _backwardPawnValue;
    protected byte _rookOnOpenFileValue;
    protected byte _rookOnHalfOpenFileValue;
    protected byte _rentgenValue;
    protected byte _rookBlockedByKingValue;
    protected short _noPawnsValue;
    private byte _knightMobilityValue;
    private byte _bishopMobilityValue;
    private byte _rookMobilityValue;
    private byte _queenMobilityValue;

    private readonly byte _pawnAttackValue;
    private readonly byte _knightAttackValue;
    private readonly byte _bishopAttackValue;
    private readonly byte _rookAttackValue;
    private readonly byte _queenAttackValue;
    private readonly byte _kingAttackValue;
    private readonly int[] _pieceAttackWeight;
    protected short[] _values;
    private DistanceBuffer _distances;

    private byte _rookOnOpenFileNextToKingValue;
    private byte _doubleRookOnOpenFileValue;
    private byte _rookOnHalfOpenFileNextToKingValue;
    private byte _doubleRookOnHalfOpenFileValue;
    private byte _connectedRooksOnFirstRankValue;

    private readonly byte _pawnShield2Value;
    private readonly byte _pawnShield3Value;
    private readonly byte _pawnShield4Value;
    private readonly byte _pawnKingShield2Value;
    private readonly byte _pawnKingShield3Value;
    private readonly byte _pawnKingShield4Value;

    private byte _discoveredCheckValue;
    private byte _discoveredAttackValue;
    private byte _absolutePinValue;
    private byte _partialPinValue;
    protected byte _bishopBattaryValue;
    protected byte _rookBattaryValue;
    protected byte _queenBattaryValue;

    private byte _rookBehindPassedPawnValue;
    private short _unstoppablePassedPawnValue;
    private byte _outsidePassedPawnValue;

    protected CellBuffer<byte> _whitePassedPawnValues;
    protected CellBuffer<byte> _blackPassedPawnValues;
    protected CellBuffer<byte> _whiteProtectedPassedPawnValues;
    protected CellBuffer<byte> _blackProtectedPassedPawnValues;
    protected CellBuffer<byte> _whiteConnectedPassedPawnValues;
    protected CellBuffer<byte> _blackConnectedPassedPawnValues;
    protected CellBuffer<CellBuffer<byte>> _kingDistances;
    protected PieceBuffer<byte> _blockadePenalties;
    protected byte[] _kingDistanceBonuses;
    protected byte[] _kingDistancePenalties;

    private CellBuffer<short> _fullWhitePawnValues;
    private CellBuffer<short> _fullWhiteKnightValues;
    private CellBuffer<short> _fullWhiteBishopValues;
    private CellBuffer<short> _fullWhiteRookValues;
    private CellBuffer<short> _fullWhiteQueenValues;
    private CellBuffer<short> _fullWhiteKingValues;
    private CellBuffer<short> _fullBlackPawnValues;
    private CellBuffer<short> _fullBlackKnightValues;
    private CellBuffer<short> _fullBlackBishopValues;
    private CellBuffer<short> _fullBlackRookValues;
    private CellBuffer<short> _fullBlackQueenValues;
    private CellBuffer<short> _fullBlackKingValues;

    protected EvaluationServiceBase(IConfigurationProvider configuration)
    {
        var evaluationProvider = configuration.Evaluation;

        _distances = new();
        for (int i = 0; i < 64; i++)
        {
            _distances[i] = new();
        }

        CalculateDistances();

        var pieceAttackValue = evaluationProvider.Static.KingSafety.PieceAttackValue;
        _pawnAttackValue = pieceAttackValue[Pieces.WhitePawn];
        _knightAttackValue = pieceAttackValue[Pieces.WhiteKnight];
        _bishopAttackValue = pieceAttackValue[Pieces.WhiteBishop];
        _rookAttackValue = pieceAttackValue[Pieces.WhiteRook];
        _queenAttackValue = pieceAttackValue[Pieces.WhiteQueen];
        _kingAttackValue = pieceAttackValue[Pieces.WhiteKing];

        _pawnShield2Value = evaluationProvider.Static.KingSafety.PawnShield2Value;
        _pawnShield3Value = evaluationProvider.Static.KingSafety.PawnShield3Value;
        _pawnShield4Value = evaluationProvider.Static.KingSafety.PawnShield4Value;
        _pawnKingShield2Value = evaluationProvider.Static.KingSafety.PawnKingShield2Value;
        _pawnKingShield3Value = evaluationProvider.Static.KingSafety.PawnKingShield3Value;
        _pawnKingShield4Value = evaluationProvider.Static.KingSafety.PawnKingShield4Value;

        _pieceAttackWeight = evaluationProvider.Static.KingSafety.AttackWeight;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CellBuffer<byte> Distance(byte kingPosition) => _distances[kingPosition];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPawnAttackValue() => _pawnAttackValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetKnightAttackValue() => _knightAttackValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBishopAttackValue() => _bishopAttackValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookAttackValue() => _rookAttackValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetQueenAttackValue() => _queenAttackValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetKingAttackValue() => _kingAttackValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAttackWeight(byte attackCount) => _pieceAttackWeight[attackCount];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBackwardPawnValue() => _backwardPawnValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBlockedPawnValue() => _blockedPawnValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetDoubleBishopValue() => _doubleBishopValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetDoubledPawnValue() => _doubledPawnValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetIsolatedPawnValue() => _isolatedPawnValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetNoPawnsValue() => _noPawnsValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetWhitePassedPawnValue(byte coordinate) => _whitePassedPawnValues[coordinate];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBlackPassedPawnValue(byte coordinate) => _blackPassedPawnValues[coordinate];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetWhiteProtectedPassedPawnValue(byte coordinate) => _whiteProtectedPassedPawnValues[coordinate];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBlackProtectedPassedPawnValue(byte coordinate) => _blackProtectedPassedPawnValues[coordinate];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetWhiteConnectedPassedPawnValue(byte coordinate) => _whiteConnectedPassedPawnValues[coordinate];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBlackConnectedPassedPawnValue(byte coordinate) => _blackConnectedPassedPawnValues[coordinate];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRentgenValue() => _rentgenValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookBlockedByKingValue() => _rookBlockedByKingValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookOnHalfOpenFileValue() => _rookOnHalfOpenFileValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookOnOpenFileValue() => _rookOnOpenFileValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetPieceValue(byte piece) => _values[piece];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetWhitePawnFullValue(byte square) => _fullWhitePawnValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetWhiteKnightFullValue(byte square) => _fullWhiteKnightValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetWhiteBishopFullValue(byte square) => _fullWhiteBishopValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetWhiteRookFullValue(byte square) => _fullWhiteRookValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetWhiteQueenFullValue(byte square) => _fullWhiteQueenValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetWhiteKingFullValue(byte square) => _fullWhiteKingValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetBlackPawnFullValue(byte square) => _fullBlackPawnValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetBlackKnightFullValue(byte square) => _fullBlackKnightValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetBlackBishopFullValue(byte square) => _fullBlackBishopValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetBlackRookFullValue(byte square) => _fullBlackRookValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetBlackQueenFullValue(byte square) => _fullBlackQueenValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetBlackKingFullValue(byte square) => _fullBlackKingValues[square];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetKnightMobilityValue() => _knightMobilityValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBishopMobilityValue() => _bishopMobilityValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookMobilityValue() => _rookMobilityValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetQueenMobilityValue() => _queenMobilityValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookOnOpenFileNextToKingValue() => _rookOnOpenFileNextToKingValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetDoubleRookOnOpenFileValue() => _doubleRookOnOpenFileValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookOnHalfOpenFileNextToKingValue() => _rookOnHalfOpenFileNextToKingValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetDoubleRookOnHalfOpenFileValue() => _doubleRookOnHalfOpenFileValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetConnectedRooksOnFirstRankValue() => _connectedRooksOnFirstRankValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookBehindPassedPawnValue() => _rookBehindPassedPawnValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetUnstoppablePassedPawnValue() => _unstoppablePassedPawnValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetOutsidePassedPawnValue() => _outsidePassedPawnValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetDiscoveredCheckValue() => _discoveredCheckValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetDiscoveredAttackValue() => _discoveredAttackValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetAbsolutePinValue() => _absolutePinValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPartialPinValue() => _partialPinValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBishopBattaryValue() => _bishopBattaryValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRookBattaryValue() => _rookBattaryValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetQueenBattaryValue() => _queenBattaryValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPawnShield2Value() => _pawnShield2Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPawnShield3Value() => _pawnShield3Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPawnShield4Value() => _pawnShield4Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetKingPawnShield2Value() => _pawnKingShield2Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetKingPawnShield3Value() => _pawnKingShield3Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetKingPawnShield4Value() => _pawnKingShield4Value;

    /// <summary>
    /// Calculates dynamic bonus/penalty for passed pawn based on king distance.
    /// Uses pre-computed lookup tables for O(1) evaluation.
    /// Closer friendly king = bonus, closer enemy king = penalty.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetKingDistanceFactor(byte pawnCoordinate, byte friendlyKingPosition, byte enemyKingPosition)
    {
        // Get pre-computed distances from pawn position to all squares
        var distancesFromPawn = _kingDistances[pawnCoordinate];

        // Look up distances to both kings        
        return _kingDistanceBonuses[distancesFromPawn[friendlyKingPosition]] - _kingDistancePenalties[distancesFromPawn[enemyKingPosition]];
    }

    /// <summary>
    /// Calculates blockade penalty when pawn is blockaded by enemy piece.
    /// Uses pre-computed buffer indexed by piece type for O(1) lookup.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetBlockadePenalty(byte blockaderType) => _blockadePenalties[blockaderType];

    protected void Initialize(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider, byte phase)
    {
        var evaluationProvider = configuration.Evaluation;

        var evaluationStatic = evaluationProvider.Static.GetBoard(phase);
        _doubleBishopValue = (byte)evaluationStatic.DoubleBishopValue;
        _blockedPawnValue = (byte)evaluationStatic.BlockedPawnValue;
        _doubledPawnValue = (byte)evaluationStatic.DoubledPawnValue;
        _isolatedPawnValue = (byte)evaluationStatic.IsolatedPawnValue;
        _backwardPawnValue = (byte)evaluationStatic.BackwardPawnValue;
        _rookOnOpenFileValue = (byte)evaluationStatic.RookOnOpenFileValue;
        _rentgenValue = (byte)evaluationStatic.RentgenValue;
        _rookOnHalfOpenFileValue = (byte)evaluationStatic.RookOnHalfOpenFileValue;
        _rookBlockedByKingValue = (byte)evaluationStatic.RookBlockedByKingValue;
        _noPawnsValue = (short)-evaluationStatic.NoPawnsValue;

        _rookOnOpenFileNextToKingValue = evaluationStatic.RookOnOpenFileNextToKingValue;
        _doubleRookOnOpenFileValue = evaluationStatic.DoubleRookOnOpenFileValue;
        _rookOnHalfOpenFileNextToKingValue = evaluationStatic.RookOnHalfOpenFileNextToKingValue;
        _doubleRookOnHalfOpenFileValue = evaluationStatic.DoubleRookOnHalfOpenFileValue;
        _connectedRooksOnFirstRankValue = evaluationStatic.ConnectedRooksOnFirstRankValue;
        _rookBehindPassedPawnValue = evaluationStatic.RookBehindPassedPawnValue;
        _unstoppablePassedPawnValue = evaluationStatic.UnstoppablePassedPawnValue;
        _outsidePassedPawnValue = evaluationStatic.OutsidePassedPawnValue;

        _discoveredCheckValue = evaluationStatic.DiscoveredCheckValue;
        _discoveredAttackValue = evaluationStatic.DiscoveredAttackValue;
        _absolutePinValue = evaluationStatic.AbsolutePinValue;
        _partialPinValue = evaluationStatic.PartialPinValue;
        _bishopBattaryValue = evaluationStatic.BishopBattaryValue;
        _rookBattaryValue = evaluationStatic.RookBattaryValue;
        _queenBattaryValue = evaluationStatic.QueenBattaryValue;

        _knightMobilityValue = evaluationStatic.MobilityValues[0];
        _bishopMobilityValue = evaluationStatic.MobilityValues[1];
        _rookMobilityValue = evaluationStatic.MobilityValues[2];
        _queenMobilityValue = evaluationStatic.MobilityValues[3];

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

        var _staticValues = new short[12][];
        for (byte i = 0; i < 12; i++)
        {
            _staticValues[i] = new short[64];
            for (byte k = 0; k < 64; k++)
            {
                _staticValues[i][k] = (short)staticValueProvider.GetValue(i, phase, k);
            }
        }

        _fullWhitePawnValues = new();
        _fullWhiteKnightValues = new();
        _fullWhiteBishopValues = new();
        _fullWhiteRookValues = new();
        _fullWhiteQueenValues = new();
        _fullWhiteKingValues = new();
        _fullBlackPawnValues = new();
        _fullBlackKnightValues = new();
        _fullBlackBishopValues = new();
        _fullBlackRookValues = new();
        _fullBlackQueenValues = new();
        _fullBlackKingValues = new();

        for (int k = 0; k < 64; k++)
        {
            _fullWhitePawnValues[k] = (short)(_staticValues[0][k] + _values[0]);
            _fullWhiteKnightValues[k] = (short)(_staticValues[1][k] + _values[1]);
            _fullWhiteBishopValues[k] = (short)(_staticValues[2][k] + _values[2]);
            _fullWhiteRookValues[k] = (short)(_staticValues[3][k] + _values[3]);
            _fullWhiteQueenValues[k] = (short)(_staticValues[4][k] + _values[4]);
            _fullWhiteKingValues[k] = (short)(_staticValues[5][k] + _values[5]);
            _fullBlackPawnValues[k] = (short)(_staticValues[6][k] + _values[6]);
            _fullBlackKnightValues[k] = (short)(_staticValues[7][k] + _values[7]);
            _fullBlackBishopValues[k] = (short)(_staticValues[8][k] + _values[8]);
            _fullBlackRookValues[k] = (short)(_staticValues[9][k] + _values[9]);
            _fullBlackQueenValues[k] = (short)(_staticValues[10][k] + _values[10]);
            _fullBlackKingValues[k] = (short)(_staticValues[11][k] + _values[11]);
        }


        SetPassedPawns(phase, evaluationProvider.Static.PassedPawnConfiguration);
        SetProtectedAndConnectedPassedPawns(evaluationProvider.Static, phase);
        SetKingDistanceFactors();
        SetBlockadePenalties(evaluationProvider.Static.KingSafety.BlockadePenalties);
        SetKingDistanceFactorLookup(evaluationProvider.Static.KingSafety.KingDistanceFactor);
    }

    private void SetPassedPawns(byte phase, PassedPawnConfiguration passedPawnConfiguration)
    {
        _whitePassedPawnValues = new();
        _blackPassedPawnValues = new();

        for (byte i = 0; i < 64; i++)
        {
            if (phase == 0)
            {
                _whitePassedPawnValues[i] = passedPawnConfiguration.WhiteOpening[i / 8];
                _blackPassedPawnValues[i] = passedPawnConfiguration.BlackOpening[i / 8];
            }
            else if (phase == 1)
            {
                _whitePassedPawnValues[i] = passedPawnConfiguration.WhiteMiddle[i / 8];
                _blackPassedPawnValues[i] = passedPawnConfiguration.BlackMiddle[i / 8];
            }
            else
            {
                _whitePassedPawnValues[i] = passedPawnConfiguration.WhiteEnd[i / 8];
                _blackPassedPawnValues[i] = passedPawnConfiguration.BlackEnd[i / 8];
            }
        }
    }

    private void SetProtectedAndConnectedPassedPawns(IStaticEvaluation staticEvaluation, byte phase)
    {
        _whiteProtectedPassedPawnValues = new();
        _blackProtectedPassedPawnValues = new();
        _whiteConnectedPassedPawnValues = new();
        _blackConnectedPassedPawnValues = new();

        var protectedConfig = staticEvaluation.ProtectedPassedPawnConfiguration;
        var connectedConfig = staticEvaluation.ConnectedPassedPawnConfiguration;

        for (byte i = 0; i < 64; i++)
        {
            byte rank = (byte)(i / 8);

            if (phase == 0)
            {
                _whiteProtectedPassedPawnValues[i] = protectedConfig.WhiteOpening[rank];
                _blackProtectedPassedPawnValues[i] = protectedConfig.BlackOpening[rank];
                _whiteConnectedPassedPawnValues[i] = connectedConfig.WhiteOpening[rank];
                _blackConnectedPassedPawnValues[i] = connectedConfig.BlackOpening[rank];
            }
            else if (phase == 1)
            {
                _whiteProtectedPassedPawnValues[i] = protectedConfig.WhiteMiddle[rank];
                _blackProtectedPassedPawnValues[i] = protectedConfig.BlackMiddle[rank];
                _whiteConnectedPassedPawnValues[i] = connectedConfig.WhiteMiddle[rank];
                _blackConnectedPassedPawnValues[i] = connectedConfig.BlackMiddle[rank];
            }
            else
            {
                _whiteProtectedPassedPawnValues[i] = protectedConfig.WhiteEnd[rank];
                _blackProtectedPassedPawnValues[i] = protectedConfig.BlackEnd[rank];
                _whiteConnectedPassedPawnValues[i] = connectedConfig.WhiteEnd[rank];
                _blackConnectedPassedPawnValues[i] = connectedConfig.BlackEnd[rank];
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

    private void SetKingDistanceFactors()
    {
        _kingDistances = new();

        // Pre-compute distance buffers: for each pawn position, store distances to all squares
        for (byte pawnSquare = 0; pawnSquare < 64; pawnSquare++)
        {
            _kingDistances[pawnSquare] = new();

            for (byte kingSquare = 0; kingSquare < 64; kingSquare++)
            {
                // Copy pre-computed distances
                _kingDistances[pawnSquare][kingSquare] = Distance(pawnSquare)[kingSquare];
            }
        }
    }

    private void SetBlockadePenalties(BlockadeConfiguration blockadeConfig)
    {
        _blockadePenalties = new PieceBuffer<byte>();
        _blockadePenalties[Pieces.WhitePawn] = blockadeConfig.WhitePawnPenalty;
        _blockadePenalties[Pieces.WhiteKnight] = blockadeConfig.WhiteKnightPenalty;
        _blockadePenalties[Pieces.WhiteBishop] = blockadeConfig.WhiteBishopPenalty;
        _blockadePenalties[Pieces.WhiteRook] = blockadeConfig.WhiteRookPenalty;
        _blockadePenalties[Pieces.WhiteQueen] = blockadeConfig.WhiteQueenPenalty;
        _blockadePenalties[Pieces.WhiteKing] = blockadeConfig.WhiteKingPenalty;
        _blockadePenalties[Pieces.BlackPawn] = blockadeConfig.BlackPawnPenalty;
        _blockadePenalties[Pieces.BlackKnight] = blockadeConfig.BlackKnightPenalty;
        _blockadePenalties[Pieces.BlackBishop] = blockadeConfig.BlackBishopPenalty;
        _blockadePenalties[Pieces.BlackRook] = blockadeConfig.BlackRookPenalty;
        _blockadePenalties[Pieces.BlackQueen] = blockadeConfig.BlackQueenPenalty;
        _blockadePenalties[Pieces.BlackKing] = blockadeConfig.BlackKingPenalty;
    }

    private void SetKingDistanceFactorLookup(KingDistanceFactorConfiguration kingDistanceFactorConfig)
    {
        _kingDistanceBonuses = new byte[15];
        _kingDistancePenalties = new byte[15];

        // Pre-compute bonuses for each distance 0-14 (max Manhattan distance on board is 14)
        // Bonus = max(0, (maxDistance - distance) * coefficient)
        for (byte distance = 0; distance < 15; distance++)
        {
            int bonus = Math.Max(0, (kingDistanceFactorConfig.FriendlyKingMaxDistance - distance) * kingDistanceFactorConfig.FriendlyKingBonusCoefficient);
            _kingDistanceBonuses[distance] = (byte)Math.Min(255, bonus);

            int penalty = Math.Max(0, (kingDistanceFactorConfig.EnemyKingMaxDistance - distance) * kingDistanceFactorConfig.EnemyKingPenaltyCoefficient);
            _kingDistancePenalties[distance] = (byte)Math.Min(255, penalty);
        }
    }
}
