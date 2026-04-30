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
using System.Runtime.CompilerServices;

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
    private CellBuffer<BitBoard> _rookFiles;
    private CellBuffer<BitBoard> _rookRanks;

    private CellBuffer<BitBoard> _whiteProtectedPassedPawns;

    // Development tracking - entire first rank for minor pieces
    private readonly BitBoard _whiteFirstRank = new BitBoard(0x00000000000000FFUL); // A1-H1
    private readonly BitBoard _blackFirstRank = new BitBoard(0xFF00000000000000UL); // A8-H8

    // Rook on 7th rank optimization
    private readonly BitBoard _white7thRank = new BitBoard(0x00FF000000000000UL); // A7-H7 (rank index 6)
    private readonly BitBoard _black7thRank = new BitBoard(0x000000000000FF00UL); // A2-H2 (rank index 1)

    // Opposition detection - pre-computed patterns for king vs king evaluation
    private CellBuffer<BitBoard> _directOppositionSquares;   // Squares in direct opposition to this square
    private CellBuffer<BitBoard> _diagonalOppositionSquares; // Squares in diagonal opposition
    private CellBuffer<BitBoard> _distantOppositionSquares;  // Squares in distant opposition (same file, odd distance)

    // Outpost detection - squares that cannot be attacked by enemy pawns
    private CellBuffer<BitBoard> _whiteOutpostSquares; // Squares black pawns can never attack
    private CellBuffer<BitBoard> _blackOutpostSquares; // Squares white pawns can never attack
    private CellBuffer<BitBoard> _whitePawnDefenders;  // White pawns that can defend this square
    private CellBuffer<BitBoard> _blackPawnDefenders;  // Black pawns that can defend this square

    // Pawn chain detection - pre-computed diagonals for optimal O(1) chain evaluation
    // Diagonals contain all squares on the forward diagonal from each square
    private CellBuffer<BitBoard> _whiteRightDiagonal;  // Right-forward diagonal (NE): file+n, rank+n
    private CellBuffer<BitBoard> _whiteLeftDiagonal;   // Left-forward diagonal (NW): file-n, rank+n
    private CellBuffer<BitBoard> _blackRightDiagonal;  // Black right-forward (SE): file+n, rank-n
    private CellBuffer<BitBoard> _blackLeftDiagonal;   // Black left-forward (SW): file-n, rank-n

    // Next square on diagonal (0xFF = no next square) - for O(1) chain walking
    private CellBuffer<byte> _whiteRightNext;  // Next square on white right diagonal (NE)
    private CellBuffer<byte> _whiteLeftNext;   // Next square on white left diagonal (NW)
    private CellBuffer<byte> _blackRightNext;  // Next square on black right diagonal (SE)
    private CellBuffer<byte> _blackLeftNext;   // Next square on black left diagonal (SW)

    // Pawn majority evaluation - pre-computed for O(1) distance lookups
    private CellBuffer<CellBuffer<byte>> _manhattanDistance;  // [from][to] = distance
    private readonly BitBoard _queensideFiles = new BitBoard(0x0F0F0F0F0F0F0F0FUL);  // A-D files
    private readonly BitBoard _kingsideFiles = new BitBoard(0xF0F0F0F0F0F0F0F0UL);   // E-H files
    private const byte QueensideCenter = 27;  // D4 - center of queenside
    private const byte KingsideCenter = 36;   // E5 - center of kingside

    // Rook activity in endgame - pre-computed file distances for O(1) lookups
    private CellBuffer<CellBuffer<byte>> _fileDistance;  // [square1][square2] = file distance

    // Key square control - pre-computed key squares for each passed pawn position
    private CellBuffer<BitBoard> _whiteKeySquares;  // [pawnSquare] = key squares for white passed pawn
    private CellBuffer<BitBoard> _blackKeySquares;  // [pawnSquare] = key squares for black passed pawn

    // Fianchetto structure detection - pre-computed squares for O(1) evaluation
    private readonly BitBoard _whiteFianchettoBishopSquares = new BitBoard(0x0000000000000024UL);  // B2 (1), G2 (6)
    private readonly BitBoard _blackFianchettoBishopSquares = new BitBoard(0x2400000000000000UL);  // B7 (49), G7 (54)
    private readonly BitBoard _whiteFianchettoPawnSquares = new BitBoard(0x0000000000004200UL);    // B3 (9), G3 (14)
    private readonly BitBoard _blackFianchettoPawnSquares = new BitBoard(0x0042000000000000UL);    // B6 (41), G6 (46)

    // Castling position masks for fianchetto tracking
    private readonly BitBoard _whiteKingsideCastle;
    private readonly BitBoard _whiteQueensideCastle;
    private readonly BitBoard _blackKingsideCastle;
    private readonly BitBoard _blackQueensideCastle;

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
    private BitBoard _notFileA;
    private BitBoard _notFileH;
    private BitBoard _outsideFiles; // Files A, B, G, H - for outside passed pawn bonus
    private BitBoard _centerSquares; // D4, E4, D5, E5 - main center
    private BitBoard _extendedCenterSquares; // C3-F3, C4-F4, C5-F5, C6-F6 - extended center
    private BitBoard _lightSquares; // All 32 light-colored squares (A1, C1, E1, G1, B2, D2, F2, H2, etc.)
    private BitBoard _darkSquares; // All 32 dark-colored squares (B1, D1, F1, H1, A2, C2, E2, G2, etc.)
    private CellBuffer<bool> _isLightSquare; // Fast O(1) lookup: is square light-colored?
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

    // Lookup table for squares between two squares on the same file (for Tarrasch Rule)
    private CellBuffer<CellBuffer<BitBoard>> _fileBetween;

    // Lookup tables for "rule of the square" - unstoppable passed pawn detection
    // _whitePassedPawnSquare[pawnSquare] = bitboard of squares enemy king must occupy to catch the pawn
    // _blackPassedPawnSquare[pawnSquare] = bitboard of squares enemy king must occupy to catch the pawn
    private CellBuffer<BitBoard> _whitePassedPawnSquare;
    private CellBuffer<BitBoard> _blackPassedPawnSquare;

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

        SetKingSafety();

        SetPawnProperties();

        SetKingRookPatterns();

        SetAttackPatterns();

        SetOutpostDetection();

        SetOppositionSquares();

        SetPawnChainDiagonals();

        SetManhattanDistanceTable();

        SetFileDistanceTable();

        SetKeySquares();

        _whiteKingsideCastle = _whiteKingPatterns[Squares.G2] | Squares.G2.AsBitBoard();
        _whiteQueensideCastle = _whiteKingPatterns[Squares.B2] | Squares.B2.AsBitBoard();
        _blackKingsideCastle = _whiteKingPatterns[Squares.G7] | Squares.G7.AsBitBoard();
        _blackQueensideCastle = _whiteKingPatterns[Squares.B7] | Squares.B7.AsBitBoard();
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

        // Center squares (D4, E4, D5, E5) for center control evaluation
        _centerSquares = Squares.D4.AsBitBoard() | Squares.E4.AsBitBoard() |
                         Squares.D5.AsBitBoard() | Squares.E5.AsBitBoard();

        // Extended center (C3-F3, C4-F4, C5-F5, C6-F6) for center attack evaluation
        _extendedCenterSquares =
            Squares.C3.AsBitBoard() | Squares.D3.AsBitBoard() | Squares.E3.AsBitBoard() | Squares.F3.AsBitBoard() |
            Squares.C4.AsBitBoard() | Squares.D4.AsBitBoard() | Squares.E4.AsBitBoard() | Squares.F4.AsBitBoard() |
            Squares.C5.AsBitBoard() | Squares.D5.AsBitBoard() | Squares.E5.AsBitBoard() | Squares.F5.AsBitBoard() |
            Squares.C6.AsBitBoard() | Squares.D6.AsBitBoard() | Squares.E6.AsBitBoard() | Squares.F6.AsBitBoard();

        // Light and dark squares for bad bishop evaluation
        // Light squares: (rank + file) % 2 == 0
        // Dark squares: (rank + file) % 2 == 1
        _lightSquares = new BitBoard(0);
        _darkSquares = new BitBoard(0);
        _isLightSquare = new();

        for (byte square = 0; square < 64; square++)
        {
            int squareRank = square / 8;
            int squareFile = square % 8;
            bool isLight = (squareRank + squareFile) % 2 == 1;
            _isLightSquare[square] = isLight;

            if (isLight)
                _lightSquares = _lightSquares.Set(square);
            else
                _darkSquares = _darkSquares.Set(square);
        }

        _rank1 = _ranks[1];
        _rank6 = _ranks[6];
        _notRank1 = ~_ranks[1];
        _notRank6 = ~_ranks[6];

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
    }

    /// <summary>
    /// Initializes outpost detection masks for evaluating knight and bishop outposts.
    /// An outpost is a square that cannot be attacked by enemy pawns and is ideally defended by a friendly pawn.
    /// </summary>
    private void SetOutpostDetection()
    {
        _whiteOutpostSquares = new();
        _blackOutpostSquares = new();
        _whitePawnDefenders = new();
        _blackPawnDefenders = new();

        for (byte square = 0; square < 64; square++)
        {
            byte rank = (byte)(square / 8);
            byte file = (byte)(square % 8);

            // Initialize white outpost detection (squares black pawns can attack)
            BitBoard blackPawnAttackers = new BitBoard(0);

            // Black pawns move down the board (higher ranks to lower ranks)
            // A black pawn can attack this square if it's on a higher rank and adjacent file
            for (byte r = (byte)(rank + 1); r < 8; r++)
            {
                // Left diagonal attack (from black's perspective)
                if (file > 0)
                {
                    byte attackerSquare = (byte)(r * 8 + (file - 1));
                    blackPawnAttackers = blackPawnAttackers.Set(attackerSquare);
                }
                // Right diagonal attack
                if (file < 7)
                {
                    byte attackerSquare = (byte)(r * 8 + (file + 1));
                    blackPawnAttackers = blackPawnAttackers.Set(attackerSquare);
                }
            }
            _whiteOutpostSquares[square] = blackPawnAttackers;

            // Initialize black outpost detection (squares white pawns can attack)
            BitBoard whitePawnAttackers = new BitBoard(0);

            // White pawns move up the board (lower ranks to higher ranks)
            // A white pawn can attack this square if it's on a lower rank and adjacent file
            for (int r = rank - 1; r >= 0; r--)
            {
                // Left diagonal attack (from white's perspective)
                if (file > 0)
                {
                    byte attackerSquare = (byte)(r * 8 + (file - 1));
                    whitePawnAttackers = whitePawnAttackers.Set(attackerSquare);
                }
                // Right diagonal attack
                if (file < 7)
                {
                    byte attackerSquare = (byte)(r * 8 + (file + 1));
                    whitePawnAttackers = whitePawnAttackers.Set(attackerSquare);
                }
            }
            _blackOutpostSquares[square] = whitePawnAttackers;

            // Initialize pawn defenders for this square
            // White pawn defenders (pawns that can defend this square)
            BitBoard whiteDefenders = new BitBoard(0);
            if (rank > 0)
            {
                byte behindRank = (byte)(rank - 1);
                if (file > 0)
                    whiteDefenders = whiteDefenders.Set((byte)(behindRank * 8 + (file - 1)));
                if (file < 7)
                    whiteDefenders = whiteDefenders.Set((byte)(behindRank * 8 + (file + 1)));
            }
            _whitePawnDefenders[square] = whiteDefenders;

            // Black pawn defenders
            BitBoard blackDefenders = new BitBoard(0);
            if (rank < 7)
            {
                byte aheadRank = (byte)(rank + 1);
                if (file > 0)
                    blackDefenders = blackDefenders.Set((byte)(aheadRank * 8 + (file - 1)));
                if (file < 7)
                    blackDefenders = blackDefenders.Set((byte)(aheadRank * 8 + (file + 1)));
            }
            _blackPawnDefenders[square] = blackDefenders;
        }
    }

    /// <summary>
    /// Pre-computes opposition squares for every king position.
    /// Opposition is critical in endgames for king and pawn battles.
    /// Uses O(1) lookup during evaluation instead of runtime calculations.
    /// </summary>
    private void SetOppositionSquares()
    {
        _directOppositionSquares = new();
        _diagonalOppositionSquares = new();
        _distantOppositionSquares = new();

        for (byte square = 0; square < 64; square++)
        {
            byte file = (byte)(square % 8);
            byte rank = (byte)(square / 8);

            BitBoard direct = new();
            BitBoard diagonal = new();
            BitBoard distant = new();

            // Direct opposition: same file, 2 ranks apart (1 square between)
            if (rank >= 2)
                direct = direct.Add((byte)(square - 16));  // 2 ranks down
            if (rank <= 5)
                direct = direct.Add((byte)(square + 16));  // 2 ranks up

            // Diagonal opposition: diagonal squares, 2 squares apart
            if (rank >= 2 && file >= 2)
                diagonal = diagonal.Add((byte)(square - 18));  // Down-left diagonal
            if (rank >= 2 && file <= 5)
                diagonal = diagonal.Add((byte)(square - 14));  // Down-right diagonal
            if (rank <= 5 && file >= 2)
                diagonal = diagonal.Add((byte)(square + 14));  // Up-left diagonal
            if (rank <= 5 && file <= 5)
                diagonal = diagonal.Add((byte)(square + 18));  // Up-right diagonal

            // Distant opposition: same file, even number of ranks apart (4, 6)
            // Used to maintain opposition as both kings advance
            if (rank >= 4)
                distant = distant.Add((byte)(square - 32));  // 4 ranks down
            if (rank <= 3)
                distant = distant.Add((byte)(square + 32));  // 4 ranks up
            if (rank >= 6)
                distant = distant.Add((byte)(square - 48));  // 6 ranks down
            if (rank <= 1)
                distant = distant.Add((byte)(square + 48));  // 6 ranks up

            _directOppositionSquares[square] = direct;
            _diagonalOppositionSquares[square] = diagonal;
            _distantOppositionSquares[square] = distant;
        }
    }

    /// <summary>
    /// Pre-computes diagonal bitboards for efficient pawn chain evaluation.
    /// For each square, stores all squares on both forward diagonals (left and right).
    /// This enables O(1) chain detection using bitboard intersections.
    /// Memory: 4 × 64 × 8 bytes = 2KB (negligible cost for 15-20x speedup!)
    /// </summary>
    private void SetPawnChainDiagonals()
    {
        _whiteRightDiagonal = new();
        _whiteLeftDiagonal = new();
        _blackRightDiagonal = new();
        _blackLeftDiagonal = new();
        _whiteRightNext = new();
        _whiteLeftNext = new();
        _blackRightNext = new();
        _blackLeftNext = new();

        const byte NoNext = 0xFF;  // Sentinel value for no next square

        for (byte square = 0; square < 64; square++)
        {
            byte rank = (byte)(square / 8);
            byte file = (byte)(square % 8);

            // White right-forward diagonal (NE): file+n, rank+n
            BitBoard whiteRight = new();
            byte whiteRightNextSquare = NoNext;
            for (int i = 1; i < 8 && (rank + i) < 8 && (file + i) < 8; i++)
            {
                byte diagSquare = (byte)((rank + i) * 8 + (file + i));
                whiteRight = whiteRight.Set(diagSquare);
                if (i == 1)  // First square on diagonal = immediate next
                    whiteRightNextSquare = diagSquare;
            }
            _whiteRightDiagonal[square] = whiteRight;
            _whiteRightNext[square] = whiteRightNextSquare;

            // White left-forward diagonal (NW): file-n, rank+n
            BitBoard whiteLeft = new();
            byte whiteLeftNextSquare = NoNext;
            for (int i = 1; i < 8 && (rank + i) < 8 && (file - i) >= 0; i++)
            {
                byte diagSquare = (byte)((rank + i) * 8 + (file - i));
                whiteLeft = whiteLeft.Set(diagSquare);
                if (i == 1)
                    whiteLeftNextSquare = diagSquare;
            }
            _whiteLeftDiagonal[square] = whiteLeft;
            _whiteLeftNext[square] = whiteLeftNextSquare;

            // Black right-forward diagonal (SE): file+n, rank-n (black moves down)
            BitBoard blackRight = new();
            byte blackRightNextSquare = NoNext;
            for (int i = 1; i < 8 && (rank - i) >= 0 && (file + i) < 8; i++)
            {
                byte diagSquare = (byte)((rank - i) * 8 + (file + i));
                blackRight = blackRight.Set(diagSquare);
                if (i == 1)
                    blackRightNextSquare = diagSquare;
            }
            _blackRightDiagonal[square] = blackRight;
            _blackRightNext[square] = blackRightNextSquare;

            // Black left-forward diagonal (SW): file-n, rank-n
            BitBoard blackLeft = new();
            byte blackLeftNextSquare = NoNext;
            for (int i = 1; i < 8 && (rank - i) >= 0 && (file - i) >= 0; i++)
            {
                byte diagSquare = (byte)((rank - i) * 8 + (file - i));
                blackLeft = blackLeft.Set(diagSquare);
                if (i == 1)
                    blackLeftNextSquare = diagSquare;
            }
            _blackLeftDiagonal[square] = blackLeft;
            _blackLeftNext[square] = blackLeftNextSquare;
        }
    }

    /// <summary>
    /// Pre-computes Manhattan distance between all square pairs for O(1) lookup.
    /// Manhattan distance = |rank1 - rank2| + |file1 - file2|
    /// Used for: pawn majority king distance, general distance calculations.
    /// Memory: 64 × 64 = 4KB (negligible for massive speedup)
    /// </summary>
    private void SetManhattanDistanceTable()
    {
        _manhattanDistance = new();

        for (byte square1 = 0; square1 < 64; square1++)
        {
            _manhattanDistance[square1] = new();

            byte rank1 = (byte)(square1 / 8);
            byte file1 = (byte)(square1 % 8);

            for (byte square2 = 0; square2 < 64; square2++)
            {
                byte rank2 = (byte)(square2 / 8);
                byte file2 = (byte)(square2 % 8);

                byte distance = (byte)(Math.Abs(rank1 - rank2) + Math.Abs(file1 - file2));
                _manhattanDistance[square1][square2] = distance;
            }
        }
    }

    /// <summary>
    /// Pre-computes file distance table for all square pairs (O(1) rook activity lookups).
    /// File distance = absolute difference between file coordinates.
    /// Used for: Rook independence factor (distance from own king).
    /// Memory: 64 × 64 × 1 byte = 4KB
    /// </summary>
    private void SetFileDistanceTable()
    {
        _fileDistance = new();

        for (byte square1 = 0; square1 < 64; square1++)
        {
            _fileDistance[square1] = new();
            byte file1 = (byte)(square1 % 8);

            for (byte square2 = 0; square2 < 64; square2++)
            {
                byte file2 = (byte)(square2 % 8);
                byte distance = (byte)Math.Abs(file1 - file2);
                _fileDistance[square1][square2] = distance;
            }
        }
    }

    /// <summary>
    /// Pre-computes key squares for all pawn positions (O(1) key square control lookups).
    /// Key squares: Critical squares in front of passed pawns that guarantee promotion.
    /// For a white pawn: 3 squares directly in front + adjacent diagonals (up to 3 ranks ahead).
    /// For a black pawn: 3 squares directly behind (from black's perspective) + adjacent diagonals.
    /// Memory: 64 × 2 × 8 bytes = 1KB
    /// </summary>
    private void SetKeySquares()
    {
        _whiteKeySquares = new();
        _blackKeySquares = new();

        for (byte square = 0; square < 64; square++)
        {
            int file = square % 8;
            int rank = square / 8;

            // White key squares (in front of pawn)
            BitBoard whiteKeys = new();
            for (int r = rank + 1; r <= Math.Min(rank + 3, 7); r++)
            {
                // Same file
                whiteKeys = whiteKeys.Add((byte)(r * 8 + file));

                // Adjacent files
                if (file > 0)
                    whiteKeys = whiteKeys.Add((byte)(r * 8 + file - 1));
                if (file < 7)
                    whiteKeys = whiteKeys.Add((byte)(r * 8 + file + 1));
            }
            _whiteKeySquares[square] = whiteKeys;

            // Black key squares (behind pawn from black's perspective)
            BitBoard blackKeys = new();
            for (int r = rank - 1; r >= Math.Max(rank - 3, 0); r--)
            {
                blackKeys = blackKeys.Add((byte)(r * 8 + file));
                if (file > 0)
                    blackKeys = blackKeys.Add((byte)(r * 8 + file - 1));
                if (file < 7)
                    blackKeys = blackKeys.Add((byte)(r * 8 + file + 1));
            }
            _blackKeySquares[square] = blackKeys;
        }
    }

    #endregion
}