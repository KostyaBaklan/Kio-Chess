using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards.Buffers;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Services.Evaluation;

namespace Engine.Models.Boards;

public partial class Board
{
    #region Fields

    private ulong _hash;
    private ulong[][] _hashTable;

    private BitBoard _empty;
    private BitBoard _occupied;
    private BitBoard _whites;
    private BitBoard _blacks;

    private BitBoard _whiteSmallCastleCondition;
    private BitBoard _whiteSmallCastleKing;
    private BitBoard _whiteSmallCastleRook;

    private BitBoard _whiteBigCastleCondition;
    private BitBoard _whiteBigCastleKing;
    private BitBoard _whiteBigCastleRook;

    private BitBoard _blackSmallCastleCondition;
    private BitBoard _blackSmallCastleKing;
    private BitBoard _blackSmallCastleRook;

    private BitBoard _blackBigCastleCondition;
    private BitBoard _blackBigCastleKing;
    private BitBoard _blackBigCastleRook;

    private BitBoard[] _ranks;
    private BitBoard[] _files;
    private PieceBuffer<BitBoard> _boards;
    private CellBuffer<BitBoard> _whiteKingShield;
    private CellBuffer<BitBoard> _blackKingShield;
    private CellBuffer<BitBoard> _whiteKingFaceShield;
    private CellBuffer<BitBoard> _blackKingFaceShield;
    private CellBuffer<BitBoard> _whiteKingFace;
    private CellBuffer<BitBoard> _blackKingFace;
    private CellBuffer<BitBoard> _rookFiles;
    private CellBuffer<BitBoard> _rookRanks;

    private CellBuffer<BitBoard> _whiteMinorDefense;
    private CellBuffer<BitBoard> _blackMinorDefense;
    private CellBuffer<BitBoard> _whiteProtectedPassedPawns;
    private CellBuffer<BitBoard> _blackProtectedPassedPawns;
    private CellBuffer<BitBoard> _whiteConnectedPassedPawns;
    private CellBuffer<BitBoard> _blackConnectedPassedPawns;
    private CellBuffer<BitBoard> _whiteFacing;
    private CellBuffer<BitBoard> _blackFacing;

    private CellBuffer<BitBoard> _whiteBlockedPawns;
    private CellBuffer<BitBoard> _whiteDoublePawns;
    private CellBuffer<BitBoard> _whitePassedPawns;
    private CellBuffer<BitBoard> _whiteIsolatedPawns;
    private CellBuffer<BitBoard> _whiteCandidatePawnsFront;
    private CellBuffer<BitBoard> _whiteCandidatePawnsBack;
    private CellBuffer<BitBoard> _whiteCandidatePawnsAttackFront;
    private CellBuffer<BitBoard> _whiteCandidatePawnsAttackBack;

    private CellBuffer<BitBoard> _whiteBackwardSupportPawns;
    private CellBuffer<BitBoard> _whiteBackwardAttackPawns;
    private CellBuffer<BitBoard> _whiteStartBackwardSupportPawns;
    private CellBuffer<BitBoard> _whiteStartBackwardAttackPawns;

    private CellBuffer<BitBoard> _blackBlockedPawns;
    private CellBuffer<BitBoard> _blackDoublePawns;
    private CellBuffer<BitBoard> _blackPassedPawns;
    private CellBuffer<BitBoard> _blackIsolatedPawns;
    private CellBuffer<BitBoard> _blackCandidatePawnsFront;
    private CellBuffer<BitBoard> _blackCandidatePawnsBack;
    private CellBuffer<BitBoard> _blackCandidatePawnsAttackFront;
    private CellBuffer<BitBoard> _blackCandidatePawnsAttackBack;

    private CellBuffer<BitBoard> _blackBackwardSupportPawns;
    private CellBuffer<BitBoard> _blackBackwardAttackPawns;
    private CellBuffer<BitBoard> _blackStartBackwardSupportPawns;
    private CellBuffer<BitBoard> _blackStartBackwardAttackPawns;

    private CellBuffer<byte> _pieces;
    private readonly BitBoard _whiteQueenOpening;
    private readonly BitBoard _blackQueenOpening;
    private BitBoard _notFileA;
    private BitBoard _notFileH;
    private BitBoard _outsideFiles; // Files A, B, G, H - for outside passed pawn bonus
    private BitBoard _rank1;
    private BitBoard _rank6;
    private BitBoard _notRank1;
    private BitBoard _notRank6;
    private CellBuffer<BitBoard> _whiteRookKingPattern;
    private CellBuffer<BitBoard> _whiteRookPawnPattern;
    private CellBuffer<BitBoard> _blackRookKingPattern;
    private CellBuffer<BitBoard> _blackRookPawnPattern;

    private CellBuffer<BitBoard> _whitePawnShield2;
    private CellBuffer<BitBoard> _whitePawnShield3;
    private CellBuffer<BitBoard> _whitePawnShield4;
    private CellBuffer<BitBoard> _whitePawnKingShield2;
    private CellBuffer<BitBoard> _whitePawnKingShield3;
    private CellBuffer<BitBoard> _whitePawnKingShield4;
    private CellBuffer<BitBoard> _blackPawnShield7;
    private CellBuffer<BitBoard> _blackPawnShield6;
    private CellBuffer<BitBoard> _blackPawnShield5;
    private CellBuffer<BitBoard> _blackPawnKingShield7;
    private CellBuffer<BitBoard> _blackPawnKingShield6;
    private CellBuffer<BitBoard> _blackPawnKingShield5;

    private CellBuffer<BitBoard> _whiteRookFileBlocking;
    private CellBuffer<BitBoard> _whiteRookRankBlocking;
    private CellBuffer<BitBoard> _blackRookFileBlocking;
    private CellBuffer<BitBoard> _blackRookRankBlocking;

    // Lookup table for squares between two squares on the same file (for Tarrasch Rule)
    private CellBuffer<CellBuffer<BitBoard>> _fileBetween;

    // Lookup tables for "rule of the square" - unstoppable passed pawn detection
    // _whitePassedPawnSquare[pawnSquare] = bitboard of squares enemy king must occupy to catch the pawn
    // _blackPassedPawnSquare[pawnSquare] = bitboard of squares enemy king must occupy to catch the pawn
    private CellBuffer<BitBoard> _whitePassedPawnSquare;
    private CellBuffer<BitBoard> _blackPassedPawnSquare;

    // Pre-computed opposition lookup table
    // _oppositionTable[kingPos1][kingPos2] = true if kings are in opposition (same file/rank/diagonal, 2 squares apart)
    private CellBuffer<CellBuffer<bool>> _oppositionTable;

    // Pre-computed rook cut-off tables for endgame evaluation
    // _whiteRookCutoffRanks[rookSquare] = bitboard of ranks above the rook (ranks black king is cut off from)
    // _blackRookCutoffRanks[rookSquare] = bitboard of ranks below the rook (ranks white king is cut off from)
    private CellBuffer<BitBoard> _whiteRookCutoffRanks;
    private CellBuffer<BitBoard> _blackRookCutoffRanks;

    // 7th rank bitboards for rook on 7th rank detection
    // _rank7 = White's 7th rank (rank index 6, squares 48-55)
    // _rank2 = Black's 7th rank (rank index 1, squares 8-15)
    private BitBoard _whiteRook7thRank;
    private BitBoard _blackRook7thRank;

    // Pre-computed king centralization table for endgame evaluation
    // Values: center squares (d4,d5,e4,e5) = 8, corners = 0
    // Indexed by square (0-63)
    private static readonly byte[] _kingCentralization = [
        0, 0, 1, 2, 2, 1, 0, 0,   // Rank 1
        0, 2, 3, 4, 4, 3, 2, 0,   // Rank 2
        1, 3, 5, 6, 6, 5, 3, 1,   // Rank 3
        2, 4, 6, 8, 8, 6, 4, 2,   // Rank 4
        2, 4, 6, 8, 8, 6, 4, 2,   // Rank 5
        1, 3, 5, 6, 6, 5, 3, 1,   // Rank 6
        0, 2, 3, 4, 4, 3, 2, 0,   // Rank 7
        0, 0, 1, 2, 2, 1, 0, 0    // Rank 8
    ];

    // Pre-computed key squares lookup tables for passed pawn evaluation
    // Key squares are squares that, if occupied by the friendly king, guarantee pawn promotion
    // _whiteKeySquares[pawnSquare] = bitboard of key squares for white passed pawn
    // _blackKeySquares[pawnSquare] = bitboard of key squares for black passed pawn
    private CellBuffer<BitBoard> _whiteKeySquares;
    private CellBuffer<BitBoard> _blackKeySquares;

    // Pre-computed outpost attackers lookup tables for knight evaluation
    // _whiteOutpostAttackers[sq] = bitboard of black pawn squares that could attack this square
    // _blackOutpostAttackers[sq] = bitboard of white pawn squares that could attack this square
    private CellBuffer<BitBoard> _whiteOutpostAttackers;
    private CellBuffer<BitBoard> _blackOutpostAttackers;
    private BitBoard _whiteOutpost;
    private BitBoard _blackOutpost;

    // Long diagonals for bishop evaluation (a1-h8 and a8-h1)
    private BitBoard _longDiagonalA1H8;
    private BitBoard _longDiagonalA8H1;

    // Light and dark squares for bishop color detection
    private BitBoard _lightSquares;
    private BitBoard _darkSquares;

    // Promotion rank bitboards
    private BitBoard _whitePromotionRank;
    private BitBoard _blackPromotionRank;

    private BitBoard _whiteKingZone;
    private BitBoard _blackKingZone;
    private BitBoard _whitePawnAttacks;
    private BitBoard _blackPawnAttacks;

    private CellBuffer<BitBoard> _whitePawnPatterns;
    private CellBuffer<BitBoard> _whiteKnightPatterns;
    private CellBuffer<BitBoard> _whiteBishopPatterns;
    private CellBuffer<BitBoard> _whiteRookPatterns;
    private CellBuffer<BitBoard> _whiteQueenPatterns;
    private CellBuffer<BitBoard> _whiteKingPatterns;
    private CellBuffer<BitBoard> _blackPawnPatterns;
    private CellBuffer<BitBoard> _blackKnightPatterns;
    private CellBuffer<BitBoard> _blackBishopPatterns;
    private CellBuffer<BitBoard> _blackRookPatterns;
    private CellBuffer<BitBoard> _blackQueenPatterns;
    private CellBuffer<BitBoard> _blackKingPatterns;

    private readonly int _trofismCoefficient;
    private readonly int[] _round;

    private readonly PositionsList _positionList;
    private readonly MoveProvider _moveProvider;
    private readonly MoveHistoryService _moveHistory;
    private EvaluationServiceBase _evaluationService;
    private readonly IEvaluationServiceFactory _evaluationServiceFactory;

    #endregion

    #region CTOR

    public Board()
    {
        _pieces = new();
        _positionList = new PositionsList();

        _round = new int[] { 0, -1, -2, 2, 1, 0, -1, -2, 2, 1 };

        MoveBase.Board = this;

        SetBoards();

        SetFilesAndRanks();

        SetCastles();

        _moveProvider = ContainerLocator.Current.Resolve<MoveProvider>();
        _moveHistory = ContainerLocator.Current.Resolve<MoveHistoryService>();
        _evaluationServiceFactory = ContainerLocator.Current.Resolve<IEvaluationServiceFactory>();
        _moveHistory.SetBoard(this);

        _pieceValues = new();
        var service = _evaluationServiceFactory.GetEvaluationService(0);
        for (byte j = 0; j < 12; j++)
        {
            _pieceValues[j] = service.GetPieceValue(j);
        }

        _trofismCoefficient = ContainerLocator.Current.Resolve<IConfigurationProvider>()
            .Evaluation.Static.KingSafety.TrofismCoefficientValue;

        HashSet<ulong> set = [];

        InitializeZoobrist(set);

        _moveProvider.SetBoard(this);


        _whiteQueenOpening = Squares.D1.AsBitBoard() | Squares.E1.AsBitBoard() | Squares.C1.AsBitBoard() |
                             Squares.D2.AsBitBoard() | Squares.E2.AsBitBoard() | Squares.C2.AsBitBoard();

        _blackQueenOpening = Squares.D8.AsBitBoard() | Squares.E8.AsBitBoard() | Squares.C8.AsBitBoard() |
                             Squares.D7.AsBitBoard() | Squares.E7.AsBitBoard() | Squares.C7.AsBitBoard();

        SetKingSafety();

        SetPawnProperties();

        SetKingRookPatterns();

        SetRookBlocking();

        SetAttackPatterns();

        TranspositionTable.SetBoard(this);
    }

    private void SetAttackPatterns()
    {
        _whitePawnPatterns = new();
        _whiteKnightPatterns = new();
        _whiteBishopPatterns = new();
        _whiteRookPatterns = new();
        _whiteQueenPatterns = new();
        _whiteKingPatterns = new();

        _blackPawnPatterns = new();
        _blackKnightPatterns = new();
        _blackBishopPatterns = new();
        _blackRookPatterns = new();
        _blackQueenPatterns = new();
        _blackKingPatterns = new();

        for (byte i = 0; i < 64; i++)
        {
            _whitePawnPatterns[i] = _moveProvider.GetAttackPattern(Pieces.WhitePawn, i);
            _whiteKnightPatterns[i] = _moveProvider.GetAttackPattern(Pieces.WhiteKnight, i);
            _whiteBishopPatterns[i] = _moveProvider.GetAttackPattern(Pieces.WhiteBishop, i);
            _whiteRookPatterns[i] = _moveProvider.GetAttackPattern(Pieces.WhiteRook, i);
            _whiteQueenPatterns[i] = _moveProvider.GetAttackPattern(Pieces.WhiteQueen, i);
            _whiteKingPatterns[i] = _moveProvider.GetAttackPattern(Pieces.WhiteKing, i);

            _blackPawnPatterns[i] = _moveProvider.GetAttackPattern(Pieces.BlackPawn, i);
            _blackKnightPatterns[i] = _moveProvider.GetAttackPattern(Pieces.BlackKnight, i);
            _blackBishopPatterns[i] = _moveProvider.GetAttackPattern(Pieces.BlackBishop, i);
            _blackRookPatterns[i] = _moveProvider.GetAttackPattern(Pieces.BlackRook, i);
            _blackQueenPatterns[i] = _moveProvider.GetAttackPattern(Pieces.BlackQueen, i);
            _blackKingPatterns[i] = _moveProvider.GetAttackPattern(Pieces.BlackKing, i);
        }
    }

    private void InitializeZoobrist(HashSet<ulong> set)
    {
        _hashTable = new ulong[64][];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                _hashTable[i * 8 + j] = new ulong[12];
                for (int k = 0; k < 12; k++)
                {
                    var x = RandomHelpers.NextLong();
                    while (!set.Add(x))
                    {
                        x = RandomHelpers.NextLong();
                    }

                    _hashTable[i * 8 + j][k] = x;
                }
            }
        }

        _hash = 0L;
        for (byte index = 0; index < 12; index++)
        {
            foreach (var b in _boards[index].BitScan())
            {
                _hash = _hash ^ _hashTable[b][index];
            }
        }
    }

    #endregion

    #region Initialization

    private void SetRookBlocking()
    {
        _whiteRookFileBlocking = new();
        _whiteRookRankBlocking = new();
        _blackRookFileBlocking = new();
        _blackRookRankBlocking = new();

        for (int i = 0; i < 48; i++)
        {
            _whiteRookFileBlocking[i] = i.AsBitBoard() << 8;
        }
        for (int i = 16; i < 64; i++)
        {
            _blackRookFileBlocking[i] = i.AsBitBoard() >> 8;
        }

        for (int i = 8; i < 56; i++)
        {
            _whiteRookRankBlocking[i] = _moveProvider.GetAttackPattern(Pieces.BlackPawn, (byte)(i + 8));
        }
        for (int i = 8; i < 56; i++)
        {
            _blackRookRankBlocking[i] = _moveProvider.GetAttackPattern(Pieces.WhitePawn, (byte)(i - 8));
        }
    }

    private void SetKingRookPatterns()
    {
        _whiteRookKingPattern = new();
        _whiteRookPawnPattern = new();
        _blackRookKingPattern = new();
        _blackRookPawnPattern = new();
        for (int i = 0; i < 64; i++)
        {
            _whiteRookKingPattern[i] = new BitBoard();
            _whiteRookPawnPattern[i] = new BitBoard();
            _blackRookKingPattern[i] = new BitBoard();
            _blackRookPawnPattern[i] = new BitBoard();
        }

        SetWhiteRookKingPattern();

        SetWhiteRookPawnPattern();

        SetBlackRookKingPattern();

        SetBlackRookPawnPattern();
    }

    private void SetBlackRookPawnPattern()
    {
        var bitBoard = _blackRookPawnPattern[Squares.A8];
        bitBoard = bitBoard | Squares.A7.AsBitBoard() | Squares.A6.AsBitBoard();
        _blackRookPawnPattern[Squares.A8] = bitBoard;

        bitBoard = _blackRookPawnPattern[Squares.B8];
        bitBoard = bitBoard | Squares.B7.AsBitBoard() | Squares.B6.AsBitBoard();
        _blackRookPawnPattern[Squares.B8] = bitBoard;

        bitBoard = _blackRookPawnPattern[Squares.C8];
        bitBoard = bitBoard | Squares.C7.AsBitBoard() | Squares.C6.AsBitBoard();
        _blackRookPawnPattern[Squares.C8] = bitBoard;

        bitBoard = _blackRookPawnPattern[Squares.H8];
        bitBoard = bitBoard | Squares.H7.AsBitBoard() | Squares.H6.AsBitBoard();
        _blackRookPawnPattern[Squares.H8] = bitBoard;

        bitBoard = _blackRookPawnPattern[Squares.G8];
        bitBoard = bitBoard | Squares.G7.AsBitBoard() | Squares.G6.AsBitBoard();
        _blackRookPawnPattern[Squares.G8] = bitBoard;
    }

    private void SetWhiteRookPawnPattern()
    {
        var bitBoard = _whiteRookPawnPattern[Squares.A1];
        bitBoard = bitBoard | Squares.A2.AsBitBoard() | Squares.A3.AsBitBoard();
        _whiteRookPawnPattern[Squares.A1] = bitBoard;

        bitBoard = _whiteRookPawnPattern[Squares.B1];
        bitBoard = bitBoard | Squares.B2.AsBitBoard() | Squares.B3.AsBitBoard();
        _whiteRookPawnPattern[Squares.B1] = bitBoard;

        bitBoard = _whiteRookPawnPattern[Squares.C1];
        bitBoard = bitBoard | Squares.C2.AsBitBoard() | Squares.C3.AsBitBoard();
        _whiteRookPawnPattern[Squares.C1] = bitBoard;

        bitBoard = _whiteRookPawnPattern[Squares.H1];
        bitBoard = bitBoard | Squares.H2.AsBitBoard() | Squares.H3.AsBitBoard();
        _whiteRookPawnPattern[Squares.H1] = bitBoard;

        bitBoard = _whiteRookPawnPattern[Squares.G1];
        bitBoard = bitBoard | Squares.G2.AsBitBoard() | Squares.G3.AsBitBoard();
        _whiteRookPawnPattern[Squares.G1] = bitBoard;
    }

    private void SetBlackRookKingPattern()
    {
        var bitBoard = _blackRookKingPattern[Squares.A8];
        bitBoard = bitBoard | Squares.B8.AsBitBoard() | Squares.C8.AsBitBoard() | Squares.D8.AsBitBoard();
        _blackRookKingPattern[Squares.A8] = bitBoard;

        bitBoard = _blackRookKingPattern[Squares.B8];
        bitBoard = bitBoard | Squares.C8.AsBitBoard() | Squares.D8.AsBitBoard();
        _blackRookKingPattern[Squares.B8] = bitBoard;

        bitBoard = _blackRookKingPattern[Squares.C8];
        bitBoard = bitBoard | Squares.D8.AsBitBoard();
        _blackRookKingPattern[Squares.C8] = bitBoard;

        bitBoard = _blackRookKingPattern[Squares.H8];
        bitBoard = bitBoard | Squares.G8.AsBitBoard() | Squares.F8.AsBitBoard();
        _blackRookKingPattern[Squares.H8] = bitBoard;

        bitBoard = _blackRookKingPattern[Squares.G8];
        bitBoard = bitBoard | Squares.F8.AsBitBoard();
        _blackRookKingPattern[Squares.G8] = bitBoard;
    }

    private void SetWhiteRookKingPattern()
    {
        var bitBoard = _whiteRookKingPattern[Squares.A1];
        bitBoard = bitBoard | Squares.B1.AsBitBoard() | Squares.C1.AsBitBoard() | Squares.D1.AsBitBoard();
        _whiteRookKingPattern[Squares.A1] = bitBoard;

        bitBoard = _whiteRookKingPattern[Squares.B1];
        bitBoard = bitBoard | Squares.C1.AsBitBoard() | Squares.D1.AsBitBoard();
        _whiteRookKingPattern[Squares.B1] = bitBoard;

        bitBoard = _whiteRookKingPattern[Squares.C1];
        bitBoard = bitBoard | Squares.D1.AsBitBoard();
        _whiteRookKingPattern[Squares.C1] = bitBoard;

        bitBoard = _whiteRookKingPattern[Squares.H1];
        bitBoard = bitBoard | Squares.G1.AsBitBoard() | Squares.F1.AsBitBoard();
        _whiteRookKingPattern[Squares.H1] = bitBoard;

        bitBoard = _whiteRookKingPattern[Squares.G1];
        bitBoard = bitBoard | Squares.F1.AsBitBoard();
        _whiteRookKingPattern[Squares.G1] = bitBoard;
    }

    private void SetPawnProperties()
    {
        _whiteBlockedPawns = new();
        _whiteDoublePawns = new();
        _whitePassedPawns = new();
        _whiteIsolatedPawns = new();
        _whiteCandidatePawnsFront = new();
        _whiteCandidatePawnsBack = new();
        _whiteCandidatePawnsAttackFront = new();
        _whiteCandidatePawnsAttackBack = new();

        _whiteBackwardAttackPawns = new();
        _whiteBackwardSupportPawns = new();
        _whiteStartBackwardAttackPawns = new();
        _whiteStartBackwardSupportPawns = new();

        _blackBlockedPawns = new();
        _blackDoublePawns = new();
        _blackPassedPawns = new();
        _blackIsolatedPawns = new();
        _blackCandidatePawnsFront = new();
        _blackCandidatePawnsBack = new();
        _blackCandidatePawnsAttackFront = new();
        _blackCandidatePawnsAttackBack = new();

        _blackBackwardAttackPawns = new();
        _blackBackwardSupportPawns = new();
        _blackStartBackwardAttackPawns = new();
        _blackStartBackwardSupportPawns = new();

        _whiteMinorDefense = new();
        _blackMinorDefense = new();

        _whiteProtectedPassedPawns = new();
        _blackProtectedPassedPawns = new();
        _whiteConnectedPassedPawns = new();
        _blackConnectedPassedPawns = new();

        _whiteFacing = new();
        _blackFacing = new();

        for (byte i = 0; i < 48; i++)
        {
            BitBoard b = new();
            for (byte j = (byte)(i + 8); j < 56; j += 8)
            {
                b |= j.AsBitBoard();
            }
            _whiteFacing[i] = b;
        }
        for (byte i = 16; i < 64; i++)
        {
            BitBoard b = new();
            for (byte j = (byte)(i - 8); j >= 8; j -= 8)
            {
                b |= j.AsBitBoard();
            }
            _blackFacing[i] = b;
        }

        for (byte i = 16; i < 64; i++)
        {
            _whiteMinorDefense[i] = _moveProvider.GetAttackPattern(Pieces.BlackPawn, i);
        }
        for (byte i = 0; i < 48; i++)
        {
            _blackMinorDefense[i] = _moveProvider.GetAttackPattern(Pieces.WhitePawn, i);
        }

        BitBoard ones = new();
        ones = ~ones;

        for (byte i = 8; i < 56; i++)
        {
            var f = i % 8;
            var r = i / 8;

            _whiteBlockedPawns[i] = ((byte)(i + 8)).AsBitBoard();
            _blackBlockedPawns[i] = ((byte)(i - 8)).AsBitBoard();

            _whiteDoublePawns[i] = _files[f] ^ i.AsBitBoard();
            _blackDoublePawns[i] = _files[f] ^ i.AsBitBoard();

            _whiteCandidatePawnsAttackFront[i] = _moveProvider.GetAttackPattern(Pieces.WhitePawn, i);
            _whiteCandidatePawnsAttackBack[i] = _moveProvider.GetAttackPattern(Pieces.BlackPawn, i);
            _blackCandidatePawnsAttackBack[i] = _moveProvider.GetAttackPattern(Pieces.WhitePawn, i);
            _blackCandidatePawnsAttackFront[i] = _moveProvider.GetAttackPattern(Pieces.BlackPawn, i);

            if (f == 0)
            {
                _whitePassedPawns[i] = (ones << i) & (_files[0] | _files[1]) & ~_ranks[r];
                _blackPassedPawns[i] = ~(ones << i) & (_files[0] | _files[1]) & ~_ranks[r];

                _whiteIsolatedPawns[i] = _files[1];
                _blackIsolatedPawns[i] = _files[1];
            }
            else if (f == 7)
            {
                _whitePassedPawns[i] = (ones << i) & (_files[6] | _files[7]) & ~_ranks[r];
                _blackPassedPawns[i] = ~(ones << i) & (_files[6] | _files[7]) & ~_ranks[r];

                _whiteIsolatedPawns[i] = _files[7];
                _blackIsolatedPawns[i] = _files[7];
            }
            else
            {
                _whitePassedPawns[i] = (ones << i) & (_files[f - 1] | _files[f] | _files[f + 1]) & ~_ranks[r];
                _blackPassedPawns[i] = ~(ones << i) & (_files[f - 1] | _files[f] | _files[f + 1]) & ~_ranks[r];

                _whiteIsolatedPawns[i] = _files[f - 1] | _files[f + 1];
                _blackIsolatedPawns[i] = _files[f - 1] | _files[f + 1];
            }
        }

        for (int i = 8; i < 48; i++)
        {
            _whiteCandidatePawnsFront[i] = _whitePassedPawns[i];
            _whiteCandidatePawnsBack[i] = _blackPassedPawns[i + 8];
        }

        for (int i = 16; i < 56; i++)
        {
            _blackCandidatePawnsFront[i] = _blackPassedPawns[i];
            _blackCandidatePawnsBack[i] = _whitePassedPawns[i - 8];
        }

        SetProtectedAndConnectedPassedPawns();
        SetBackwordPawns();
    }

    private void SetProtectedAndConnectedPassedPawns()
    {
        for (byte i = 8; i < 56; i++)
        {
            var f = i % 8;
            var rank = i / 8;

            BitBoard whitePawnMask = new();
            BitBoard blackPawnMask = new();
            BitBoard whiteConnectedMask = new();
            BitBoard blackConnectedMask = new();

            if (f > 0)
            {
                whitePawnMask |= ((byte)(i - 1)).AsBitBoard();
                blackPawnMask |= ((byte)(i + 1)).AsBitBoard();
                whiteConnectedMask |= ((byte)(i + 7)).AsBitBoard();
                blackConnectedMask |= ((byte)(i - 9)).AsBitBoard();
            }
            if (f < 7)
            {
                whitePawnMask |= ((byte)(i + 1)).AsBitBoard();
                blackPawnMask |= ((byte)(i - 1)).AsBitBoard();
                whiteConnectedMask |= ((byte)(i + 9)).AsBitBoard();
                blackConnectedMask |= ((byte)(i - 7)).AsBitBoard();
            }

            _whiteProtectedPassedPawns[i] = whitePawnMask;
            _blackProtectedPassedPawns[i] = blackPawnMask;
            _whiteConnectedPassedPawns[i] = whiteConnectedMask;
            _blackConnectedPassedPawns[i] = blackConnectedMask;
        }
    }

    private void SetBackwordPawns()
    {
        var _whiteBackwardPawns = new List<KeyValuePair<BitBoard, BitBoard>>[64];
        var _blackBackwardPawns = new List<KeyValuePair<BitBoard, BitBoard>>[64];
        for (int i = 0; i < 64; i++)
        {
            _whiteBackwardPawns[i] = [];
            _blackBackwardPawns[i] = [];
        }

        for (byte i = 8; i < 16; i++)
        {
            byte w = (byte)(i + 8);
            var bb = _moveProvider.GetAttackPattern(Pieces.WhitePawn, w);
            var wb = _blackPassedPawns[w];
            for (int j = i; j >= 0; j -= 8)
            {
                wb ^= j.AsBitBoard();
            }

            _whiteBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));

            w = (byte)(i + 16);
            bb = _moveProvider.GetAttackPattern(Pieces.WhitePawn, w);
            wb = _blackPassedPawns[w];
            for (int j = i; j >= 0; j -= 8)
            {
                wb ^= j.AsBitBoard();
            }
            _whiteBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
        }
        for (byte i = 16; i < 48; i++)
        {
            byte w = (byte)(i + 8);
            var bb = _moveProvider.GetAttackPattern(Pieces.WhitePawn, w);
            var wb = _blackPassedPawns[w];
            for (int j = i; j >= 0; j -= 8)
            {
                wb ^= j.AsBitBoard();
            }

            _whiteBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
        }
        for (byte i = 48; i < 56; i++)
        {
            byte w = (byte)(i - 8);
            var bb = _moveProvider.GetAttackPattern(Pieces.BlackPawn, w);
            var wb = _whitePassedPawns[w];
            for (int j = i; j < 56; j += 8)
            {
                wb ^= j.AsBitBoard();
            }

            _blackBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));

            w = (byte)(i - 16);
            bb = _moveProvider.GetAttackPattern(Pieces.BlackPawn, w);
            wb = _whitePassedPawns[w];
            for (int j = i; j < 56; j += 8)
            {
                wb ^= j.AsBitBoard();
            }

            _blackBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
        }
        for (byte i = 16; i < 48; i++)
        {
            byte w = (byte)(i - 8);
            var bb = _moveProvider.GetAttackPattern(Pieces.BlackPawn, w);
            var wb = _whitePassedPawns[w];
            for (int j = i; j < 56; j += 8)
            {
                wb ^= j.AsBitBoard();
            }

            _blackBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
        }

        var not0Rank = ~_ranks[0];
        var not7Rank = ~_ranks[7];

        for (int i = 0; i < 64; i++)
        {
            List<KeyValuePair<BitBoard, BitBoard>> wbp = _whiteBackwardPawns[i];
            List<KeyValuePair<BitBoard, BitBoard>> bbp = _blackBackwardPawns[i];

            if (wbp.Count > 0)
            {
                _whiteBackwardSupportPawns[i] = wbp[0].Key & not0Rank;
                _whiteBackwardAttackPawns[i] = wbp[0].Value;
                if (wbp.Count > 1)
                {
                    _whiteStartBackwardSupportPawns[i] = wbp[1].Key & not0Rank;
                    _whiteStartBackwardAttackPawns[i] = wbp[1].Value;
                }
            }
            else
            {
                _whiteBackwardSupportPawns[i] = new BitBoard();
                _whiteBackwardAttackPawns[i] = new BitBoard();
            }

            if (bbp.Count > 0)
            {
                _blackBackwardSupportPawns[i] = bbp[0].Key & not7Rank;
                _blackBackwardAttackPawns[i] = bbp[0].Value;

                if (bbp.Count > 1)
                {
                    _blackStartBackwardSupportPawns[i] = bbp[1].Key & not7Rank;
                    _blackStartBackwardAttackPawns[i] = bbp[1].Value;
                }
            }
            else
            {
                _blackBackwardSupportPawns[i] = new BitBoard();
                _blackBackwardAttackPawns[i] = new BitBoard();
            }
        }
    }

    private void SetKingSafety()
    {
        _whiteKingShield = new();
        _blackKingShield = new();

        for (byte i = 0; i < 64; i++)
        {
            _whiteKingShield[i] = _moveProvider.GetAttackPattern(Pieces.WhiteKing, i);
            _blackKingShield[i] = _moveProvider.GetAttackPattern(Pieces.BlackKing, i);
        }

        _whiteKingFace = new();
        for (byte i = 0; i < 32; i++)
        {
            _whiteKingFace[i] = _moveProvider.GetAttackPattern(Pieces.WhiteKing, i) &
                                _ranks[i / 8 + 1];
        }

        _blackKingFace = new();
        for (byte i = 32; i < 64; i++)
        {
            _blackKingFace[i] = _moveProvider.GetAttackPattern(Pieces.BlackKing, i) &
                                _ranks[i / 8 - 1];
        }

        _whiteKingFaceShield = new();
        for (byte i = 0; i < 32; i++)
        {
            _whiteKingFaceShield[i] = _moveProvider.GetAttackPattern(Pieces.WhiteKing, (byte)(i + 8)) &
                                      _ranks[i / 8 + 2];
        }

        _blackKingFaceShield = new();
        for (byte i = 32; i < 64; i++)
        {
            _blackKingFaceShield[i] = _moveProvider.GetAttackPattern(Pieces.BlackKing, (byte)(i - 8)) &
                                      _ranks[i / 8 - 2];
        }

        SetWhitePawnShield();

        SetBlackPawnShield();
    }

    private void SetBlackPawnShield()
    {
        _blackPawnShield7 = new();
        _blackPawnShield6 = new();
        _blackPawnShield5 = new();
        _blackPawnKingShield7 = new();
        _blackPawnKingShield6 = new();
        _blackPawnKingShield5 = new();

        for (int i = 0; i < 64; i++)
        {
            if (i > 23)
            {
                var file = i % 8;
                var rank = i / 8;
                if (file == 0)
                {
                    _blackPawnShield7[i] = _files[1] & _ranks[rank - 1];
                    _blackPawnShield6[i] = _files[1] & _ranks[rank - 2];
                    _blackPawnShield5[i] = _files[1] & _ranks[rank - 3];
                    _blackPawnKingShield7[i] = _files[0] & _ranks[rank - 1];
                    _blackPawnKingShield6[i] = _files[0] & _ranks[rank - 2];
                    _blackPawnKingShield5[i] = _files[0] & _ranks[rank - 3];
                }
                else if (file == 7)
                {
                    _blackPawnShield7[i] = _files[6] & _ranks[rank - 1];
                    _blackPawnShield6[i] = _files[6] & _ranks[rank - 2];
                    _blackPawnShield5[i] = _files[6] & _ranks[rank - 3];
                    _blackPawnKingShield7[i] = _files[7] & _ranks[rank - 1];
                    _blackPawnKingShield6[i] = _files[7] & _ranks[rank - 2];
                    _blackPawnKingShield5[i] = _files[7] & _ranks[rank - 3];
                }
                else
                {
                    _blackPawnShield7[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank - 1];
                    _blackPawnShield6[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank - 2];
                    _blackPawnShield5[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank - 3];
                    _blackPawnKingShield7[i] = _files[file] & _ranks[rank - 1];
                    _blackPawnKingShield6[i] = _files[file] & _ranks[rank - 2];
                    _blackPawnKingShield5[i] = _files[file] & _ranks[rank - 3];
                }
            }
            else
            {
                _blackPawnShield7[i] = new BitBoard();
                _blackPawnShield6[i] = new BitBoard();
                _blackPawnShield5[i] = new BitBoard();
                _blackPawnKingShield7[i] = new BitBoard();
                _blackPawnKingShield6[i] = new BitBoard();
                _blackPawnKingShield5[i] = new BitBoard();
            }
        }
    }

    private void SetWhitePawnShield()
    {
        _whitePawnShield2 = new();
        _whitePawnShield3 = new();
        _whitePawnShield4 = new();
        _whitePawnKingShield2 = new();
        _whitePawnKingShield3 = new();
        _whitePawnKingShield4 = new();

        for (int i = 0; i < 64; i++)
        {
            if (i < 40)
            {
                var file = i % 8;
                var rank = i / 8;
                if (file == 0)
                {
                    _whitePawnShield2[i] = _files[1] & _ranks[rank + 1];
                    _whitePawnShield3[i] = _files[1] & _ranks[rank + 2];
                    _whitePawnShield4[i] = _files[1] & _ranks[rank + 3];
                    _whitePawnKingShield2[i] = _files[0] & _ranks[rank + 1];
                    _whitePawnKingShield3[i] = _files[0] & _ranks[rank + 2];
                    _whitePawnKingShield4[i] = _files[0] & _ranks[rank + 3];
                }
                else if (file == 7)
                {
                    _whitePawnShield2[i] = _files[6] & _ranks[rank + 1];
                    _whitePawnShield3[i] = _files[6] & _ranks[rank + 2];
                    _whitePawnShield4[i] = _files[6] & _ranks[rank + 3];
                    _whitePawnKingShield2[i] = _files[7] & _ranks[rank + 1];
                    _whitePawnKingShield3[i] = _files[7] & _ranks[rank + 2];
                    _whitePawnKingShield4[i] = _files[7] & _ranks[rank + 3];
                }
                else
                {
                    _whitePawnShield2[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank + 1];
                    _whitePawnShield3[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank + 2];
                    _whitePawnShield4[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank + 3];
                    _whitePawnKingShield2[i] = _files[file] & _ranks[rank + 1];
                    _whitePawnKingShield3[i] = _files[file] & _ranks[rank + 2];
                    _whitePawnKingShield4[i] = _files[file] & _ranks[rank + 3];
                }
            }
            else
            {
                _whitePawnShield2[i] = new BitBoard();
                _whitePawnShield3[i] = new BitBoard();
                _whitePawnShield4[i] = new BitBoard();
                _whitePawnKingShield2[i] = new BitBoard();
                _whitePawnKingShield3[i] = new BitBoard();
                _whitePawnKingShield4[i] = new BitBoard();
            }
        }
    }

    private void SetBoards()
    {
        _boards = new();
        _boards[Pieces.WhitePawn] = _boards[Pieces.WhitePawn].Set(Enumerable.Range(8, 8).ToArray());
        _boards[Pieces.WhiteKnight] = _boards[Pieces.WhiteKnight].Set(1, 6);
        _boards[Pieces.WhiteBishop] = _boards[Pieces.WhiteBishop].Set(2, 5);
        _boards[Pieces.WhiteRook] = _boards[Pieces.WhiteRook].Set(0, 7);
        _boards[Pieces.WhiteQueen] = _boards[Pieces.WhiteQueen].Set(3);
        _boards[Pieces.WhiteKing] = _boards[Pieces.WhiteKing].Set(4);

        _whites = _boards[Pieces.WhitePawn] |
                  _boards[Pieces.WhiteKnight] |
                  _boards[Pieces.WhiteBishop] |
                  _boards[Pieces.WhiteRook] |
                  _boards[Pieces.WhiteQueen] |
                  _boards[Pieces.WhiteKing];

        _boards[Pieces.BlackPawn] =
            _boards[Pieces.BlackPawn].Set(Enumerable.Range(48, 8).ToArray());
        _boards[Pieces.BlackRook] = _boards[Pieces.BlackRook].Set(56, 63);
        _boards[Pieces.BlackKnight] = _boards[Pieces.BlackKnight].Set(57, 62);
        _boards[Pieces.BlackBishop] = _boards[Pieces.BlackBishop].Set(58, 61);
        _boards[Pieces.BlackQueen] = _boards[Pieces.BlackQueen].Set(59);
        _boards[Pieces.BlackKing] = _boards[Pieces.BlackKing].Set(60);

        _blacks = _boards[Pieces.BlackPawn] |
                  _boards[Pieces.BlackRook] |
                  _boards[Pieces.BlackKnight] |
                  _boards[Pieces.BlackBishop] |
                  _boards[Pieces.BlackQueen] |
                  _boards[Pieces.BlackKing];

        _occupied = _whites | _blacks;
        _empty = ~_occupied;

        foreach (var piece in Enumerable.Range(0, 12))
        {
            foreach (var b in _boards[piece].BitScan())
            {
                _pieces[b] = (byte)piece;
            }
        }
    }

    private void SetFilesAndRanks()
    {
        BitBoard rank = new(0);
        rank = rank.Set(Enumerable.Range(0, 8).ToArray());
        _ranks = new BitBoard[8];

        for (var i = 0; i < _ranks.Length; i++)
        {
            _ranks[i] = rank;
            rank = rank << 8;
        }

        _files = new BitBoard[8];
        BitBoard file = new(0);
        for (int i = 0; i < 60; i += 8)
        {
            file = file.Set(i);
        }

        for (var i = 0; i < _files.Length; i++)
        {
            _files[i] = file;
            file = file << 1;
        }

        _rookFiles = new();
        for (byte i = 0; i < 64; i++)
        {
            _rookFiles[i] = _files[i % 8] ^ i.AsBitBoard();
        }

        _rookRanks = new();
        for (byte i = 0; i < 64; i++)
        {
            _rookRanks[i] = _ranks[i / 8] ^ i.AsBitBoard();
        }

        _notFileA = ~_files[0];
        _notFileH = ~_files[7];

        // Outside files (A, B, G, H) for outside passed pawn bonus
        _outsideFiles = _files[0] | _files[1] | _files[6] | _files[7];

        _rank1 = _ranks[1];
        _rank6 = _ranks[6];
        _notRank1 = ~_ranks[1];
        _notRank6 = ~_ranks[6];

        // Initialize 7th rank bitboards for rook evaluation
        _whiteRook7thRank = _ranks[6]; // White's 7th rank (rank index 6)
        _blackRook7thRank = _ranks[1]; // Black's 7th rank (rank index 1)

        // Initialize rook cut-off tables for endgame evaluation
        // A rook on a rank "cuts off" the enemy king from ranks on the other side
        _whiteRookCutoffRanks = new();
        _blackRookCutoffRanks = new();
        for (byte sq = 0; sq < 64; sq++)
        {
            int rookRank = sq / 8;

            // White rook cuts off black king from ranks above the rook
            BitBoard whiteCutoff = new(0);
            for (int r = rookRank + 1; r <= 7; r++)
            {
                whiteCutoff = whiteCutoff | _ranks[r];
            }
            _whiteRookCutoffRanks[sq] = whiteCutoff;

            // Black rook cuts off white king from ranks below the rook
            BitBoard blackCutoff = new(0);
            for (int r = rookRank - 1; r >= 0; r--)
            {
                blackCutoff = blackCutoff | _ranks[r];
            }
            _blackRookCutoffRanks[sq] = blackCutoff;
        }

        // Initialize file-between lookup table for Tarrasch Rule
        // _fileBetween[sq1][sq2] contains bitboard of squares strictly between sq1 and sq2 on the same file
        _fileBetween = new();
        for (byte sq1 = 0; sq1 < 64; sq1++)
        {
            _fileBetween[sq1] = new CellBuffer<BitBoard>();
            int file1 = sq1 % 8;

            for (byte sq2 = 0; sq2 < 64; sq2++)
            {
                int file2 = sq2 % 8;

                // Only compute for squares on the same file
                if (file1 == file2 && sq1 != sq2)
                {
                    byte low = sq1 < sq2 ? sq1 : sq2;
                    byte high = sq1 < sq2 ? sq2 : sq1;

                    BitBoard between = new(0);
                    // Add all squares strictly between low and high on the same file
                    for (byte sq = (byte)(low + 8); sq < high; sq = (byte)(sq + 8))
                    {
                        between = between.Set(sq);
                    }
                    _fileBetween[sq1][sq2] = between;
                }
                else
                {
                    _fileBetween[sq1][sq2] = new BitBoard(0);
                }
            }
        }

        _whitePassedPawnSquare = new();
        _blackPassedPawnSquare = new();

        // Initialize "rule of the square" lookup tables for unstoppable passed pawns
        // The "square of the pawn" is a square region from the pawn to the promotion rank
        // If the enemy king is outside this square, the pawn cannot be caught
        for (byte sq = 0; sq < 64; sq++)
        {
            int pawnFile = sq % 8;
            int pawnRank = sq / 8;

            // White pawn: needs to reach rank 7 (index 7)
            // Distance to promotion = 7 - rank
            int whiteDistanceToPromotion = 7 - pawnRank;
            BitBoard whiteSquare = new(0);

            if (whiteDistanceToPromotion > 0 && whiteDistanceToPromotion <= 6) // Valid pawn ranks 1-6
            {
                // The square extends from current rank to rank 7, 
                // and horizontally by the same distance from the pawn file
                int leftFile = Math.Max(0, pawnFile - whiteDistanceToPromotion);
                int rightFile = Math.Min(7, pawnFile + whiteDistanceToPromotion);

                for (int r = pawnRank; r <= 7; r++)
                {
                    for (int f = leftFile; f <= rightFile; f++)
                    {
                        whiteSquare = whiteSquare.Set(r * 8 + f);
                    }
                }
            }
            _whitePassedPawnSquare[sq] = whiteSquare;

            // Black pawn: needs to reach rank 0 (index 0)
            // Distance to promotion = rank
            int blackDistanceToPromotion = pawnRank;
            BitBoard blackSquare = new(0);

            if (blackDistanceToPromotion > 0 && blackDistanceToPromotion <= 6) // Valid pawn ranks 1-6
            {
                // The square extends from current rank to rank 0,
                // and horizontally by the same distance from the pawn file
                int leftFile = Math.Max(0, pawnFile - blackDistanceToPromotion);
                int rightFile = Math.Min(7, pawnFile + blackDistanceToPromotion);

                for (int r = pawnRank; r >= 0; r--)
                {
                    for (int f = leftFile; f <= rightFile; f++)
                    {
                        blackSquare = blackSquare.Set(r * 8 + f);
                    }
                }
            }
            _blackPassedPawnSquare[sq] = blackSquare;
        }

        // Initialize opposition lookup table
        _oppositionTable = new();
        for (byte kingPos1 = 0; kingPos1 < 64; kingPos1++)
        {
            _oppositionTable[kingPos1] = new CellBuffer<bool>();
            for (byte kingPos2 = 0; kingPos2 < 64; kingPos2++)
            {
                // Kings are in opposition if they are on the same file, rank, or diagonal,
                // and exactly 2 squares apart
                int fileDiff = Math.Abs(kingPos1 % 8 - kingPos2 % 8);
                int rankDiff = Math.Abs(kingPos1 / 8 - kingPos2 / 8);
                bool inOpposition = (fileDiff == 2 && rankDiff == 0) || // Horizontal opposition
                                    (fileDiff == 0 && rankDiff == 2) || // Vertical opposition
                                    (fileDiff == 2 && rankDiff == 2);   // Diagonal opposition
                _oppositionTable[kingPos1][kingPos2] = inOpposition;
            }
        }

        // Initialize key squares lookup tables for passed pawn evaluation
        // Key squares are squares that, if occupied by the friendly king, guarantee pawn promotion
        _whiteKeySquares = new();
        _blackKeySquares = new();

        for (byte sq = 0; sq < 64; sq++)
        {
            int pawnFile = sq % 8;
            int pawnRank = sq / 8;

            BitBoard whiteKeys = new(0);
            BitBoard blackKeys = new(0);

            // White pawn key squares (pawn moves upward, ranks 1-6 are valid pawn positions)
            if (pawnRank >= 1 && pawnRank <= 6)
            {
                int leftFile = Math.Max(0, pawnFile - 1);
                int rightFile = Math.Min(7, pawnFile + 1);

                if (pawnRank <= 3) // Pawns on ranks 2-4 (indexes 1-3)
                {
                    // Key squares are 2 ranks ahead of the pawn
                    int keyRank = pawnRank + 2;
                    if (keyRank <= 7)
                    {
                        for (int f = leftFile; f <= rightFile; f++)
                        {
                            whiteKeys = whiteKeys.Set(keyRank * 8 + f);
                        }
                    }
                }
                else // Pawns on ranks 5-6 (indexes 4-5)
                {
                    // Key squares are ranks 6, 7, 8 (indexes 5, 6, 7)
                    for (int r = 5; r <= 7; r++)
                    {
                        for (int f = leftFile; f <= rightFile; f++)
                        {
                            whiteKeys = whiteKeys.Set(r * 8 + f);
                        }
                    }
                }
            }
            _whiteKeySquares[sq] = whiteKeys;

            // Black pawn key squares (pawn moves downward)
            if (pawnRank >= 1 && pawnRank <= 6)
            {
                int leftFile = Math.Max(0, pawnFile - 1);
                int rightFile = Math.Min(7, pawnFile + 1);

                if (pawnRank >= 4) // Pawns on ranks 5-7 (indexes 4-6)
                {
                    // Key squares are 2 ranks below the pawn
                    int keyRank = pawnRank - 2;
                    if (keyRank >= 0)
                    {
                        for (int f = leftFile; f <= rightFile; f++)
                        {
                            blackKeys = blackKeys.Set(keyRank * 8 + f);
                        }
                    }
                }
                else // Pawns on ranks 2-4 (indexes 1-3)
                {
                    // Key squares are ranks 1, 2, 3 (indexes 0, 1, 2)
                    for (int r = 0; r <= 2; r++)
                    {
                        for (int f = leftFile; f <= rightFile; f++)
                        {
                            blackKeys = blackKeys.Set(r * 8 + f);
                        }
                    }
                }
            }
            _blackKeySquares[sq] = blackKeys;
        }

        // Initialize outpost attackers lookup tables for knight/bishop evaluation
        // For a piece on a given square, these are the squares where enemy pawns would attack it
        // Outposts are only valid in enemy territory:
        // - White outposts: ranks 4-6 (squares 24-47)
        // - Black outposts: ranks 3-5 (squares 16-39)
        _whiteOutpostAttackers = new();
        _blackOutpostAttackers = new();

        _whiteOutpost = _ranks[3] | _ranks[4] | _ranks[5]; // Ranks 4-6
        _blackOutpost = _ranks[2] | _ranks[3] | _ranks[4]; // Ranks 3-5

        for (byte sq = 0; sq < 64; sq++)
        {
            int sqFile = sq % 8;
            int sqRank = sq / 8;

            BitBoard blackPawnAttackers = new();
            BitBoard whitePawnAttackers = new();

            // For white outpost: only valid on ranks 4-6 (sqRank 3-5, i.e., squares 24-47)
            // Find squares where black pawns could attack this square
            if (sqRank >= 3 && sqRank <= 5)
            {
                for (int attackRank = sqRank + 1; attackRank <= 6; attackRank++)
                {
                    if (sqFile > 0)
                    {
                        blackPawnAttackers = blackPawnAttackers.Set(attackRank * 8 + (sqFile - 1));
                    }
                    if (sqFile < 7)
                    {
                        blackPawnAttackers = blackPawnAttackers.Set(attackRank * 8 + (sqFile + 1));
                    }
                }
            }

            // For black outpost: only valid on ranks 3-5 (sqRank 2-4, i.e., squares 16-39)
            // Find squares where white pawns could attack this square
            if (sqRank >= 2 && sqRank <= 4)
            {
                for (int attackRank = sqRank - 1; attackRank >= 1; attackRank--)
                {
                    if (sqFile > 0)
                    {
                        whitePawnAttackers = whitePawnAttackers.Set(attackRank * 8 + (sqFile - 1));
                    }
                    if (sqFile < 7)
                    {
                        whitePawnAttackers = whitePawnAttackers.Set(attackRank * 8 + (sqFile + 1));
                    }
                }
            }

            _whiteOutpostAttackers[sq] = blackPawnAttackers;
            _blackOutpostAttackers[sq] = whitePawnAttackers;
        }

        // Initialize long diagonals for bishop evaluation
        _longDiagonalA1H8 = new();
        _longDiagonalA8H1 = new();
        for (byte sq = 0; sq < 64; sq++)
        {
            int sqFile = sq % 8;
            int sqRank = sq / 8;

            // A1-H8 diagonal: file == rank
            if (sqFile == sqRank)
            {
                _longDiagonalA1H8 = _longDiagonalA1H8.Set(sq);
            }
            // A8-H1 diagonal: file + rank == 7
            if (sqFile + sqRank == 7)
            {
                _longDiagonalA8H1 = _longDiagonalA8H1.Set(sq);
            }
        }

        // Initialize light and dark squares for bishop color detection
        _lightSquares = new();
        _darkSquares = new();
        for (byte sq = 0; sq < 64; sq++)
        {
            int sqFile = sq % 8;
            int sqRank = sq / 8;
            // Light squares: (file + rank) is odd
            if ((sqFile + sqRank) % 2 == 1)
            {
                _lightSquares = _lightSquares.Set(sq);
            }
            else
            {
                _darkSquares = _darkSquares.Set(sq);
            }
        }

        // Initialize promotion ranks
        _whitePromotionRank = _ranks[7]; // Rank 8 for white
        _blackPromotionRank = _ranks[0]; // Rank 1 for black
    }

    #endregion
}
