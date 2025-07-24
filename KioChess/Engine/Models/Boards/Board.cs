using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Services.Evaluation;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Models.Boards;

public class Board
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
    private BitBoard[] _boards;
    private BitBoard[] _whiteKingShield;
    private BitBoard[] _blackKingShield;
    private BitBoard[] _whiteKingFaceShield;
    private BitBoard[] _blackKingFaceShield;
    private BitBoard[] _whiteKingFace;
    private BitBoard[] _blackKingFace;
    private BitBoard[][] _whiteKingOpenFile;
    private BitBoard[][] _blackKingOpenFile;
    private BitBoard[] _rookFiles;
    private BitBoard[] _rookRanks;

    private BitBoard[] _whiteMinorDefense;
    private BitBoard[] _blackMinorDefense;
    private BitBoard[] _whiteFacing;
    private BitBoard[] _blackFacing;

    private BitBoard[] _whiteBlockedPawns;
    private BitBoard[] _whiteDoublePawns;
    private BitBoard[] _whitePassedPawns;
    private BitBoard[] _whiteIsolatedPawns;
    private BitBoard[] _whiteCandidatePawnsFront;
    private BitBoard[] _whiteCandidatePawnsBack;
    private BitBoard[] _whiteCandidatePawnsAttackFront;
    private BitBoard[] _whiteCandidatePawnsAttackBack;
    private List<KeyValuePair<BitBoard, BitBoard>>[] _whiteBackwardPawns;

    private BitBoard[] _blackBlockedPawns;
    private BitBoard[] _blackDoublePawns;
    private BitBoard[] _blackPassedPawns;
    private BitBoard[] _blackIsolatedPawns;
    private BitBoard[] _blackCandidatePawnsFront;
    private BitBoard[] _blackCandidatePawnsBack;
    private BitBoard[] _blackCandidatePawnsAttackFront;
    private BitBoard[] _blackCandidatePawnsAttackBack;
    private List<KeyValuePair<BitBoard, BitBoard>>[] _blackBackwardPawns;

    private readonly byte[] _pieces;
    private readonly BitBoard _whiteQueenOpening;
    private readonly BitBoard _blackQueenOpening;
    private BitBoard _notFileA;
    private BitBoard _notFileH;
    private BitBoard _rank1;
    private BitBoard _rank6;
    private BitBoard _notRank1;
    private BitBoard _notRank6;
    private BitBoard[] _whiteRookKingPattern;
    private BitBoard[] _whiteRookPawnPattern;
    private BitBoard[] _blackRookKingPattern;
    private BitBoard[] _blackRookPawnPattern;

    private BitBoard[] _whitePawnShield2;
    private BitBoard[] _whitePawnShield3;
    private BitBoard[] _whitePawnShield4;
    private BitBoard[] _whitePawnKingShield2;
    private BitBoard[] _whitePawnKingShield3;
    private BitBoard[] _whitePawnKingShield4;
    private BitBoard[] _blackPawnShield7;
    private BitBoard[] _blackPawnShield6;
    private BitBoard[] _blackPawnShield5;
    private BitBoard[] _blackPawnKingShield7;
    private BitBoard[] _blackPawnKingShield6;
    private BitBoard[] _blackPawnKingShield5;

    private BitBoard[] _whiteRookFileBlocking;
    private BitBoard[] _whiteRookRankBlocking;
    private BitBoard[] _blackRookFileBlocking;
    private BitBoard[] _blackRookRankBlocking;

    private BitBoard _whiteKingZone;
    private BitBoard _blackKingZone;
    private BitBoard _whitePawnAttacks;
    private BitBoard _blackPawnAttacks;

    private BitBoard[] _whitePawnPatterns;
    private BitBoard[] _whiteKnightPatterns;
    private BitBoard[] _whiteBishopPatterns;
    private BitBoard[] _whiteRookPatterns;
    private BitBoard[] _whiteQueenPatterns;
    private BitBoard[] _whiteKingPatterns;
    private BitBoard[] _blackPawnPatterns;
    private BitBoard[] _blackKnightPatterns;
    private BitBoard[] _blackBishopPatterns;
    private BitBoard[] _blackRookPatterns;
    private BitBoard[] _blackQueenPatterns;
    private BitBoard[] _blackKingPatterns;

    private readonly int _trofismCoefficient;
    private readonly int[] _round;

    private readonly PositionsList _positionList;
    private readonly MoveProvider _moveProvider;
    private readonly MoveHistoryService _moveHistory;
    private EvaluationServiceBase _evaluationService;
    private readonly IEvaluationServiceFactory _evaluationServiceFactory;
    private readonly AttackEvaluationService _attackEvaluationService;

    #endregion

    #region CTOR

    public Board()
    {
        _pieces = new byte[64];
        _positionList = new PositionsList();

        _round = new int[] { 0, -1, -2, 2, 1, 0, -1, -2, 2, 1 };
        //_round = Enumerable.Range(0, 2000).Select(i =>
        //{
        //    return i + round[i % 10];
        //}).ToArray();

        MoveBase.Board = this;

        SetBoards();

        SetFilesAndRanks();

        SetCastles();

        _moveProvider = ContainerLocator.Current.Resolve<MoveProvider>();
        _moveHistory = ContainerLocator.Current.Resolve<MoveHistoryService>();
        _evaluationServiceFactory = ContainerLocator.Current.Resolve<IEvaluationServiceFactory>();
        _attackEvaluationService = new AttackEvaluationService(_evaluationServiceFactory, _moveProvider);
        _attackEvaluationService.SetBoard(this);
        _moveHistory.SetBoard(this);

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
        _whitePawnPatterns = new BitBoard[64];
        _whiteKnightPatterns = new BitBoard[64];
        _whiteBishopPatterns = new BitBoard[64];
        _whiteRookPatterns = new BitBoard[64];
        _whiteQueenPatterns = new BitBoard[64];
        _whiteKingPatterns = new BitBoard[64];

        _blackPawnPatterns = new BitBoard[64];
        _blackKnightPatterns = new BitBoard[64];
        _blackBishopPatterns = new BitBoard[64];
        _blackRookPatterns = new BitBoard[64];
        _blackQueenPatterns = new BitBoard[64];
        _blackKingPatterns = new BitBoard[64];

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
        for (byte index = 0; index < _boards.Length; index++)
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
        _whiteRookFileBlocking = new BitBoard[64];
        _whiteRookRankBlocking = new BitBoard[64];
        _blackRookFileBlocking = new BitBoard[64];
        _blackRookRankBlocking = new BitBoard[64];

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
        _whiteRookKingPattern = new BitBoard[64];
        _whiteRookPawnPattern = new BitBoard[64];
        _blackRookKingPattern = new BitBoard[64];
        _blackRookPawnPattern = new BitBoard[64];
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
        _whiteBlockedPawns = new BitBoard[64];
        _whiteDoublePawns = new BitBoard[64];
        _whitePassedPawns = new BitBoard[64];
        _whiteIsolatedPawns = new BitBoard[64];
        _whiteBackwardPawns = new List<KeyValuePair<BitBoard, BitBoard>>[64];
        _whiteCandidatePawnsFront = new BitBoard[64];
        _whiteCandidatePawnsBack = new BitBoard[64];
        _whiteCandidatePawnsAttackFront = new BitBoard[64];
        _whiteCandidatePawnsAttackBack = new BitBoard[64];

        _blackBlockedPawns = new BitBoard[64];
        _blackDoublePawns = new BitBoard[64];
        _blackPassedPawns = new BitBoard[64];
        _blackIsolatedPawns = new BitBoard[64];
        _blackBackwardPawns = new List<KeyValuePair<BitBoard, BitBoard>>[64];
        _blackCandidatePawnsFront = new BitBoard[64];
        _blackCandidatePawnsBack = new BitBoard[64];
        _blackCandidatePawnsAttackFront = new BitBoard[64];
        _blackCandidatePawnsAttackBack = new BitBoard[64];

        _whiteMinorDefense = new BitBoard[64];
        _blackMinorDefense = new BitBoard[64];

        _whiteFacing = new BitBoard[64];
        _blackFacing = new BitBoard[64];

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
    }

    private void SetKingSafety()
    {
        _whiteKingShield = new BitBoard[64];
        _blackKingShield = new BitBoard[64];

        for (byte i = 0; i < 64; i++)
        {
            _whiteKingShield[i] = _moveProvider.GetAttackPattern(Pieces.WhiteKing, i);
            _blackKingShield[i] = _moveProvider.GetAttackPattern(Pieces.BlackKing, i);
        }

        _whiteKingFace = new BitBoard[64];
        for (byte i = 0; i < 32; i++)
        {
            _whiteKingFace[i] = _moveProvider.GetAttackPattern(Pieces.WhiteKing, i) &
                                _ranks[i / 8 + 1];
        }

        _blackKingFace = new BitBoard[64];
        for (byte i = 32; i < 64; i++)
        {
            _blackKingFace[i] = _moveProvider.GetAttackPattern(Pieces.BlackKing, i) &
                                _ranks[i / 8 - 1];
        }

        _whiteKingFaceShield = new BitBoard[64];
        for (byte i = 0; i < 32; i++)
        {
            _whiteKingFaceShield[i] = _moveProvider.GetAttackPattern(Pieces.WhiteKing, (byte)(i + 8)) &
                                      _ranks[i / 8 + 2];
        }

        _blackKingFaceShield = new BitBoard[64];
        for (byte i = 32; i < 64; i++)
        {
            _blackKingFaceShield[i] = _moveProvider.GetAttackPattern(Pieces.BlackKing, (byte)(i - 8)) &
                                      _ranks[i / 8 - 2];
        }

        _whiteKingOpenFile = new BitBoard[64][];
        for (byte i = 0; i < _whiteKingOpenFile.Length; i++)
        {
            var rank = i % 8;
            if (rank == 0)
            {
                _whiteKingOpenFile[i] = new BitBoard[2];

                BitBoard b = new();
                for (byte j = (byte)(i + 8); j < 56; j += 8)
                {
                    b |= j.AsBitBoard();
                }
                _whiteKingOpenFile[i][0] = b;
                b = new BitBoard();
                for (byte j = (byte)(i + 9); j < 56; j += 8)
                {
                    b |= j.AsBitBoard();
                }
                _whiteKingOpenFile[i][1] = b;
            }
            else if (rank == 7)
            {
                _whiteKingOpenFile[i] = new BitBoard[2];

                BitBoard b = new();
                for (byte j = (byte)(i + 7); j < 56; j += 8)
                {
                    b |= j.AsBitBoard();
                }
                _whiteKingOpenFile[i][0] = b;
                b = new BitBoard();
                for (byte j = (byte)(i + 8); j < 56; j += 8)
                {
                    b |= j.AsBitBoard();
                }
                _whiteKingOpenFile[i][1] = b;
            }
            else
            {
                _whiteKingOpenFile[i] = new BitBoard[3];

                BitBoard b = new();
                for (byte j = (byte)(i + 7); j < 56; j += 8)
                {
                    b |= j.AsBitBoard();
                }
                _whiteKingOpenFile[i][0] = b;
                b = new BitBoard();
                for (byte j = (byte)(i + 8); j < 56; j += 8)
                {
                    b |= j.AsBitBoard();
                }
                _whiteKingOpenFile[i][1] = b;
                b = new BitBoard();
                for (byte j = (byte)(i + 9); j < 56; j += 8)
                {
                    b |= j.AsBitBoard();
                }
                _whiteKingOpenFile[i][2] = b;
            }
        }

        _blackKingOpenFile = new BitBoard[64][];
        for (byte i = 0; i < 8; i++)
        {
            var rank = i % 8;
            if (rank == 0 || rank == 7)
            {
                _blackKingOpenFile[i] = new BitBoard[2];
            }
            else
            {
                _blackKingOpenFile[i] = new BitBoard[3];
            }
        }
        for (byte i = 8; i < _blackKingOpenFile.Length; i++)
        {
            var rank = i % 8;
            if (rank == 0)
            {
                _blackKingOpenFile[i] = new BitBoard[2];

                BitBoard b = new();
                for (byte j = (byte)(i - 7); j > 7; j -= 8)
                {
                    b |= j.AsBitBoard();
                }
                _blackKingOpenFile[i][0] = b;

                b = new BitBoard();
                for (byte j = (byte)(i - 8); j > 7; j -= 8)
                {
                    b |= j.AsBitBoard();
                }
                _blackKingOpenFile[i][1] = b;
            }
            else if (rank == 7)
            {
                _blackKingOpenFile[i] = new BitBoard[2];

                BitBoard b = new();
                for (byte j = (byte)(i - 8); j > 7; j -= 8)
                {
                    b |= j.AsBitBoard();
                }
                _blackKingOpenFile[i][0] = b;

                b = new BitBoard();
                for (byte j = (byte)(i - 9); j > 7; j -= 8)
                {
                    b |= j.AsBitBoard();
                }
                _blackKingOpenFile[i][1] = b;
            }
            else
            {
                _blackKingOpenFile[i] = new BitBoard[3];

                BitBoard b = new();
                for (byte j = (byte)(i - 7); j > 7; j -= 8)
                {
                    b |= j.AsBitBoard();
                }
                _blackKingOpenFile[i][0] = b;

                b = new BitBoard();
                for (byte j = (byte)(i - 8); j > 7; j -= 8)
                {
                    b |= j.AsBitBoard();
                }
                _blackKingOpenFile[i][1] = b;

                b = new BitBoard();
                for (byte j = (byte)(i - 9); j > 7; j -= 8)
                {
                    b |= j.AsBitBoard();
                }
                _blackKingOpenFile[i][2] = b;
            }
        }

        SetWhitePawnShield();

        SetBlackPawnShield();
    }

    private void SetBlackPawnShield()
    {
        _blackPawnShield7 = new BitBoard[64];
        _blackPawnShield6 = new BitBoard[64];
        _blackPawnShield5 = new BitBoard[64];
        _blackPawnKingShield7 = new BitBoard[64];
        _blackPawnKingShield6 = new BitBoard[64];
        _blackPawnKingShield5 = new BitBoard[64];

        for (int i = 0; i < 64; i++)
        {
            if (i > 23)
            {
                var file = i % 8;
                var rank = i / 8;
                if (file == 0)
                {
                    //if (rank == 7)
                    //{
                    _blackPawnShield7[i] = _files[1] & _ranks[rank - 1];
                    _blackPawnShield6[i] = _files[1] & _ranks[rank - 2];
                    _blackPawnShield5[i] = _files[1] & _ranks[rank - 3];
                    _blackPawnKingShield7[i] = _files[0] & _ranks[rank - 1];
                    _blackPawnKingShield6[i] = _files[0] & _ranks[rank - 2];
                    _blackPawnKingShield5[i] = _files[0] & _ranks[rank - 3];
                    //}
                    //else
                    //{
                    //    _blackPawnShield7[i] = _files[1] & _ranks[rank];
                    //    _blackPawnShield6[i] = _files[1] & _ranks[rank - 1];
                    //    _blackPawnShield5[i] = _files[1] & _ranks[rank - 2];
                    //    _blackPawnKingShield7[i] = _files[0] & _ranks[rank];
                    //    _blackPawnKingShield6[i] = _files[0] & _ranks[rank - 1];
                    //    _blackPawnKingShield5[i] = _files[0] & _ranks[rank - 2];
                    //}
                }
                else if (file == 7)
                {
                    //if (rank == 7)
                    //{
                    _blackPawnShield7[i] = _files[6] & _ranks[rank - 1];
                    _blackPawnShield6[i] = _files[6] & _ranks[rank - 2];
                    _blackPawnShield5[i] = _files[6] & _ranks[rank - 3];
                    _blackPawnKingShield7[i] = _files[7] & _ranks[rank - 1];
                    _blackPawnKingShield6[i] = _files[7] & _ranks[rank - 2];
                    _blackPawnKingShield5[i] = _files[7] & _ranks[rank - 3];
                    //}
                    //else
                    //{
                    //    _blackPawnShield7[i] = _files[6] & _ranks[rank];
                    //    _blackPawnShield6[i] = _files[6] & _ranks[rank - 1];
                    //    _blackPawnShield5[i] = _files[6] & _ranks[rank - 2];
                    //    _blackPawnKingShield7[i] = _files[7] & _ranks[rank];
                    //    _blackPawnKingShield6[i] = _files[7] & _ranks[rank - 1];
                    //    _blackPawnKingShield5[i] = _files[7] & _ranks[rank - 2];
                    //}
                }
                else
                {
                    //if (rank == 7)
                    //{
                    _blackPawnShield7[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank - 1];
                    _blackPawnShield6[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank - 2];
                    _blackPawnShield5[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank - 3];
                    _blackPawnKingShield7[i] = _files[file] & _ranks[rank - 1];
                    _blackPawnKingShield6[i] = _files[file] & _ranks[rank - 2];
                    _blackPawnKingShield5[i] = _files[file] & _ranks[rank - 3];
                    //}
                    //else
                    //{
                    //    _blackPawnShield7[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank];
                    //    _blackPawnShield6[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank - 1];
                    //    _blackPawnShield5[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank - 2];
                    //    _blackPawnKingShield7[i] = _files[file] & _ranks[rank];
                    //    _blackPawnKingShield6[i] = _files[file] & _ranks[rank - 1];
                    //    _blackPawnKingShield5[i] = _files[file] & _ranks[rank - 2];
                    //}
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
        _whitePawnShield2 = new BitBoard[64];
        _whitePawnShield3 = new BitBoard[64];
        _whitePawnShield4 = new BitBoard[64];
        _whitePawnKingShield2 = new BitBoard[64];
        _whitePawnKingShield3 = new BitBoard[64];
        _whitePawnKingShield4 = new BitBoard[64];

        for (int i = 0; i < 64; i++)
        {
            if (i < 40)
            {
                var file = i % 8;
                var rank = i / 8;
                if (file == 0)
                {
                    //if (rank == 0)
                    //{
                    _whitePawnShield2[i] = _files[1] & _ranks[rank + 1];
                    _whitePawnShield3[i] = _files[1] & _ranks[rank + 2];
                    _whitePawnShield4[i] = _files[1] & _ranks[rank + 3];
                    _whitePawnKingShield2[i] = _files[0] & _ranks[rank + 1];
                    _whitePawnKingShield3[i] = _files[0] & _ranks[rank + 2];
                    _whitePawnKingShield4[i] = _files[0] & _ranks[rank + 3];
                    //}
                    //else
                    //{
                    //    _whitePawnShield2[i] = _files[1] & _ranks[rank];
                    //    _whitePawnShield3[i] = _files[1] & _ranks[rank + 1];
                    //    _whitePawnShield4[i] = _files[1] & _ranks[rank + 2];
                    //    _whitePawnKingShield2[i] = _files[0] & _ranks[rank];
                    //    _whitePawnKingShield3[i] = _files[0] & _ranks[rank + 1];
                    //    _whitePawnKingShield4[i] = _files[0] & _ranks[rank + 2];
                    //}
                }
                else if (file == 7)
                {
                    //if (rank == 0)
                    //{
                    _whitePawnShield2[i] = _files[6] & _ranks[rank + 1];
                    _whitePawnShield3[i] = _files[6] & _ranks[rank + 2];
                    _whitePawnShield4[i] = _files[6] & _ranks[rank + 3];
                    _whitePawnKingShield2[i] = _files[7] & _ranks[rank + 1];
                    _whitePawnKingShield3[i] = _files[7] & _ranks[rank + 2];
                    _whitePawnKingShield4[i] = _files[7] & _ranks[rank + 3];
                    //}
                    //else
                    //{
                    //    _whitePawnShield2[i] = _files[6] & _ranks[rank];
                    //    _whitePawnShield3[i] = _files[6] & _ranks[rank + 1];
                    //    _whitePawnShield4[i] = _files[6] & _ranks[rank + 2];
                    //    _whitePawnKingShield2[i] = _files[7] & _ranks[rank];
                    //    _whitePawnKingShield3[i] = _files[7] & _ranks[rank + 1];
                    //    _whitePawnKingShield4[i] = _files[7] & _ranks[rank + 2];
                    //}
                }
                else
                {
                    //if (rank == 0)
                    //{
                    _whitePawnShield2[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank + 1];
                    _whitePawnShield3[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank + 2];
                    _whitePawnShield4[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank + 3];
                    _whitePawnKingShield2[i] = _files[file] & _ranks[rank + 1];
                    _whitePawnKingShield3[i] = _files[file] & _ranks[rank + 2];
                    _whitePawnKingShield4[i] = _files[file] & _ranks[rank + 3];
                    //}
                    //else
                    //{
                    //    _whitePawnShield2[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank];
                    //    _whitePawnShield3[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank + 1];
                    //    _whitePawnShield4[i] = (_files[file - 1] | _files[file + 1]) & _ranks[rank + 2];
                    //    _whitePawnKingShield2[i] = _files[file] & _ranks[rank];
                    //    _whitePawnKingShield3[i] = _files[file] & _ranks[rank + 1];
                    //    _whitePawnKingShield4[i] = _files[file] & _ranks[rank + 2];
                    //}
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

    #endregion

    #region Implementation of IBoard

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopBattary(byte to)
    {
        if (_boards[Pieces.WhiteQueen].IsZero()) return false;

        var pattern = _whiteBishopPatterns[to] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        return pattern.Any() && (to.XrayBishopAttacks(_occupied, _boards[Pieces.WhiteQueen]) & pattern).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteRookBattary(byte to)
    {
        var pattern = _whiteRookPatterns[to] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        return pattern.Any() && _boards[Pieces.WhiteQueen].Any() && (to.XrayRookAttacks(_occupied, _boards[Pieces.WhiteQueen]) & pattern).Any() || (_boards[Pieces.WhiteRook].Count() > 1 && (to.XrayRookAttacks(_occupied, _boards[Pieces.WhiteRook]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteQueenBattary(byte to)
    {
        var pattern = _whiteQueenPatterns[to] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        return pattern.Any() && _boards[Pieces.WhiteRook].Any() && (to.XrayRookAttacks(_occupied, _boards[Pieces.WhiteRook]) & pattern).Any() || (_boards[Pieces.WhiteBishop].Any() && (to.XrayBishopAttacks(_occupied, _boards[Pieces.WhiteBishop]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopBattary(byte to)
    {
        if (_boards[Pieces.BlackQueen].IsZero()) return false;

        var pattern = _blackBishopPatterns[to] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        return pattern.Any() && (to.XrayBishopAttacks(_occupied, _boards[Pieces.BlackQueen]) & pattern).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackQueenBattary(byte to)
    {
        var pattern = _blackQueenPatterns[to] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        return pattern.Any() && _boards[Pieces.BlackRook].Any() && (to.XrayRookAttacks(_occupied, _boards[Pieces.BlackRook]) & pattern).Any() || (_boards[Pieces.BlackBishop].Any() && (to.XrayBishopAttacks(_occupied, _boards[Pieces.BlackBishop]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackRookBattary(byte to)
    {
        var pattern = _blackRookPatterns[to] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        return pattern.Any() && _boards[Pieces.BlackQueen].Any() && (to.XrayRookAttacks(_occupied, _boards[Pieces.BlackQueen]) & pattern).Any() || (_boards[Pieces.BlackRook].Count() > 1 && (to.XrayRookAttacks(_occupied, _boards[Pieces.BlackRook]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackQueenPin(byte to) => (to.XrayRookAttacks(_occupied, _whites) & (_boards[Pieces.WhiteKing])).Any()
            || (to.XrayRookAttacks(_occupied, _blacks.Remove(_boards[Pieces.BlackPawn])) & _boards[Pieces.WhiteKing]).Any()
            || (to.XrayBishopAttacks(_occupied, _whites) & (_boards[Pieces.WhiteKing])).Any()
            || (to.XrayBishopAttacks(_occupied, _blacks.Remove(_boards[Pieces.BlackPawn])) & _boards[Pieces.WhiteKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteQueenPin(byte to) => (to.XrayRookAttacks(_occupied, _blacks) & (_boards[Pieces.BlackKing])).Any()
            || (to.XrayRookAttacks(_occupied, _whites.Remove(_boards[Pieces.WhitePawn])) & _boards[Pieces.BlackKing]).Any()
            || (to.XrayBishopAttacks(_occupied, _blacks) & (_boards[Pieces.BlackKing])).Any()
            || (to.XrayBishopAttacks(_occupied, _whites.Remove(_boards[Pieces.WhitePawn])) & _boards[Pieces.BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackRookPin(byte to) => (to.XrayRookAttacks(_occupied, _whites) & (_boards[Pieces.WhiteKing] | _boards[Pieces.WhiteQueen])).Any() || (to.XrayRookAttacks(_occupied, _blacks.Remove(_boards[Pieces.BlackPawn])) & _boards[Pieces.WhiteKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteRookPin(byte to) => (to.XrayRookAttacks(_occupied, _blacks) & (_boards[Pieces.BlackKing] | _boards[Pieces.BlackQueen])).Any() || (to.XrayRookAttacks(_occupied, _whites.Remove(_boards[Pieces.WhitePawn])) & _boards[Pieces.BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopPin(byte to) => (to.XrayBishopAttacks(_occupied, _whites) & (_boards[Pieces.WhiteKing] | _boards[Pieces.WhiteQueen] | _boards[Pieces.WhiteRook])).Any() || (to.XrayBishopAttacks(_occupied, _blacks.Remove(_boards[Pieces.BlackPawn])) & _boards[Pieces.WhiteKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopPin(byte to) => (to.XrayBishopAttacks(_occupied, _blacks) & (_boards[Pieces.BlackKing] | _boards[Pieces.BlackQueen] | _boards[Pieces.BlackRook])).Any() || (to.XrayBishopAttacks(_occupied, _whites.Remove(_boards[Pieces.WhitePawn])) & _boards[Pieces.BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackPawnFork(byte to) => (_blackPawnPatterns[to] & _whites.Remove(_boards[Pieces.WhitePawn])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhitePawnFork(byte to) => (_whitePawnPatterns[to] & _blacks.Remove(_boards[Pieces.BlackPawn])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackKnightFork(byte to) => (_blackKnightPatterns[to] & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopFork(byte to) => (to.BishopAttacks(_empty) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopFork(byte to) => (to.BishopAttacks(_empty) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteKnightFork(byte to) => (_whiteKnightPatterns[to] & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];

        return (from.BishopAttacks(_occupied) & shield).Count() < (to.BishopAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackKnightAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];

        return (_blackKnightPatterns[from] & shield).Count() < (_blackKnightPatterns[to] & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];

        return (from.BishopAttacks(_occupied) & shield).Count() < (to.BishopAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteKnightAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];

        return (_whiteKnightPatterns[from] & shield).Count() < (_whiteKnightPatterns[to] & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];

        return (from.RookAttacks(_occupied) & shield).Count() < (to.RookAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];

        return (from.RookAttacks(_occupied) & shield).Count() < (to.RookAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteQueenAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];

        return (from.QueenAttacks(_occupied) & shield).Count() < (to.QueenAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackQueenAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];

        return (from.QueenAttacks(_occupied) & shield).Count() < (to.QueenAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackPawn(byte to) => (_whitePawnPatterns[to] & _boards[Pieces.BlackPawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackKnight(byte to) => (_whiteKnightPatterns[to] & _boards[Pieces.BlackKnight]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackBishop(byte to) => (to.BishopAttacks(_occupied) & _boards[Pieces.BlackBishop]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhiteBishop(byte to) => (to.BishopAttacks(_occupied) & _boards[Pieces.WhiteBishop]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhitePawn(byte to) => (_blackPawnPatterns[to] & _boards[Pieces.WhitePawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhiteKnight(byte to) => (_blackKnightPatterns[to] & _boards[Pieces.WhiteKnight]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookOnSeven(byte from, byte to) => (_rank6 & from.AsBitBoard()).IsZero() && (_rank6 & to.AsBitBoard()).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookOnSeven(byte from, byte to) => (_rank1 & from.AsBitBoard()).IsZero() && (_rank1 & to.AsBitBoard()).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDoubleBlackRook(byte from, byte to) => (from.RookAttacks(_occupied) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).IsZero() &&
            (to.RookAttacks(_occupied) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDoubleWhiteRook(byte from, byte to) => (from.RookAttacks(_occupied) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).IsZero() &&
            (to.RookAttacks(_occupied) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookOnOpenFile(byte from, byte to) => (_rookFiles[from] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).Any() && (_rookFiles[to] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookOnOpenFile(byte from, byte to) => (_rookFiles[from] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).Any() && (_rookFiles[to] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackCandidate(byte from, byte to)
    {
        if ((_blackFacing[from] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).Any()) return false;

        return (_blackCandidatePawnsFront[from] & _boards[Pieces.WhitePawn]).Count() < (_blackCandidatePawnsBack[from] & _boards[Pieces.BlackPawn]).Count() &&
                (_blackCandidatePawnsAttackFront[from] & _boards[Pieces.WhitePawn]).Count() <= (_blackCandidatePawnsAttackBack[from] & _boards[Pieces.BlackPawn]).Count()
                &&
                (_blackCandidatePawnsFront[to] & _boards[Pieces.WhitePawn]).Count() < (_blackCandidatePawnsBack[to] & _boards[Pieces.BlackPawn]).Count() &&
                (_blackCandidatePawnsAttackFront[to] & _boards[Pieces.WhitePawn]).Count() <= (_blackCandidatePawnsAttackBack[to] & _boards[Pieces.BlackPawn]).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteCandidate(byte from, byte to)
    {
        if ((_whiteFacing[from] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).Any()) return false;

        return (_whiteCandidatePawnsFront[from] & _boards[Pieces.BlackPawn]).Count() < (_whiteCandidatePawnsBack[from] & _boards[Pieces.WhitePawn]).Count() &&
                (_whiteCandidatePawnsAttackFront[from] & _boards[Pieces.BlackPawn]).Count() <= (_whiteCandidatePawnsAttackBack[from] & _boards[Pieces.WhitePawn]).Count()
                &&
                (_whiteCandidatePawnsFront[to] & _boards[Pieces.BlackPawn]).Count() < (_whiteCandidatePawnsBack[to] & _boards[Pieces.WhitePawn]).Count() &&
                (_whiteCandidatePawnsAttackFront[to] & _boards[Pieces.BlackPawn]).Count() <= (_whiteCandidatePawnsAttackBack[to] & _boards[Pieces.WhitePawn]).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackPawnStorm(byte from) => (_blackPassedPawns[from] & _boards[Pieces.WhiteKing]).Any() && (_whitePassedPawns[from] & _boards[Pieces.BlackKing]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhitePawnStorm(byte from) => (_whitePassedPawns[from] & _boards[Pieces.BlackKing]).Any() && (_blackPassedPawns[from] & _boards[Pieces.WhiteKing]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmpty(BitBoard bitBoard) => _empty.IsSet(bitBoard);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteOpposite(byte square) => _blacks.IsSet(square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackOpposite(byte square) => _whites.IsSet(square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlockedByBlack(byte square) => _blacks.IsSet(square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlockedByWhite(byte square) => _whites.IsSet(square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPiece(byte cell) => _pieces[cell];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetPiece(byte cell, out byte? piece)
    {
        piece = null;

        foreach (var p in Enumerable.Range(0, 12))
        {
            if (!_boards[p].IsSet(cell)) continue;

            piece = (byte)p;
            break;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveWhite(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];

        var bit = ~square.AsBitBoard();

        _boards[piece] &= bit;
        _whites &= bit;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWhite(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];
        _pieces[square] = piece;

        BitBoard bitBoard = square.AsBitBoard();

        _boards[piece] |= bitBoard;
        _whites |= bitBoard;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveWhite(byte piece, byte from, byte to)
    {
        _hash = _hash ^ _hashTable[from][piece] ^ _hashTable[to][piece];
        _pieces[to] = piece;

        BitBoard bitBoard = from.AsBitBoard() | to.AsBitBoard();

        _boards[piece] ^= bitBoard;
        _whites ^= bitBoard;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveBlack(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];

        var bit = ~square.AsBitBoard();

        _boards[piece] &= bit;
        _blacks &= bit;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBlack(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];
        _pieces[square] = piece;

        BitBoard bitBoard = square.AsBitBoard();

        _boards[piece] |= bitBoard;
        _blacks |= bitBoard;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveBlack(byte piece, byte from, byte to)
    {
        _hash = _hash ^ _hashTable[from][piece] ^ _hashTable[to][piece];
        _pieces[to] = piece;

        BitBoard bitBoard = from.AsBitBoard() | to.AsBitBoard();

        _boards[piece] ^= bitBoard;
        _blacks ^= bitBoard;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetWhiteKingPosition() => _boards[Pieces.WhiteKing].BitScanForward();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBlackKingPosition() => _boards[Pieces.BlackKing].BitScanForward();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PositionsList GetPiecePositions(byte index)
    {
        _boards[index].GetPositions(_positionList);
        return _positionList;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhitePawnSquares() => _notRank6 & _boards[Pieces.WhitePawn];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPawnSquares() => _notRank1 & _boards[Pieces.BlackPawn];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhitePromotionSquares() => _rank6 & _boards[Pieces.WhitePawn];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPromotionSquares() => _rank1 & _boards[Pieces.BlackPawn];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetKey() => _hash;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetOccupied() => _occupied;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetEmpty() => _empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlacks() => _blacks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhites() => _whites;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetPieceBits(byte piece) => _boards[piece];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetPerimeter() => _ranks[0] | _ranks[7] | _files[0] | _files[7];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhitePawnAttacks() => ((_boards[Pieces.WhitePawn] & _notFileA) << 7) |
               ((_boards[Pieces.WhitePawn] & _notFileH) << 9);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPawnAttacks() => ((_boards[Pieces.BlackPawn] & _notFileA) >> 9) |
               ((_boards[Pieces.BlackPawn] & _notFileH) >> 7);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteCheck(MoveBase move)
    {
        try
        {
            move.Make();

            return IsWhiteAttacksTo(GetBlackKingPosition());
        }
        finally
        {
            move.UnMake();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackCheck(MoveBase move)
    {
        try
        {
            move.Make();

            return IsBlackAttacksTo(GetWhiteKingPosition());
        }
        finally
        {
            move.UnMake();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheck(MoveBase move)
    {
        try
        {
            move.Make();

            return move.IsBlack
                ? IsBlackAttacksTo(GetWhiteKingPosition())
                : IsWhiteAttacksTo(GetBlackKingPosition());
        }
        finally
        {
            move.UnMake();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLateEndGame() => IsLateEndGameForWhite() && IsLateEndGameForBlack();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateEndGameForBlack() => (_boards[Pieces.BlackQueen] | _boards[Pieces.BlackRook]).IsZero() && (_boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop]).Count() < 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateEndGameForWhite() => (_boards[Pieces.WhiteQueen] | _boards[Pieces.WhiteRook]).IsZero() && (_boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop]).Count() < 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLateMiddleGame() => IsLateMiddleGameForWhite() || IsLateMiddleGameForBlack();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateMiddleGameForBlack()
    {
        var wq = _boards[Pieces.BlackQueen].Count();

        if (wq > 1) return (_boards[Pieces.BlackRook] | _boards[Pieces.BlackBishop] | _boards[Pieces.BlackKnight]).IsZero();
        if (wq == 1) return (_boards[Pieces.BlackRook] | _boards[Pieces.BlackBishop] | _boards[Pieces.BlackKnight]).Count() < 2;
        return (_boards[Pieces.BlackRook] | _boards[Pieces.BlackBishop] | _boards[Pieces.BlackKnight]).Count() < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateMiddleGameForWhite()
    {
        var wq = _boards[Pieces.WhiteQueen].Count();

        if (wq > 1) return (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteKnight]).IsZero();
        if (wq == 1) return (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteKnight]).Count() < 2;
        return (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteKnight]).Count() < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEndGame() => IsEndGameForWhite() || IsEndGameForBlack();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEndGameForBlack()
    {
        var bqr = (_boards[Pieces.BlackQueen] | _boards[Pieces.BlackRook]).Count();

        return bqr <= 1 && (bqr == 1
            ? (_boards[Pieces.BlackBishop] | _boards[Pieces.BlackKnight]).Count() < 2
            : (_boards[Pieces.BlackBishop] | _boards[Pieces.BlackKnight]).Count() < 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEndGameForWhite()
    {
        var wqr = (_boards[Pieces.WhiteQueen] | _boards[Pieces.WhiteRook]).Count();

        return wqr <= 1 && (wqr == 1
            ? (_boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteKnight]).Count() < 2
            : (_boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteKnight]).Count() < 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanWhitePromote() => (_rank6 & _boards[Pieces.WhitePawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanBlackPromote() => (_rank1 & _boards[Pieces.BlackPawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetRank(int rank) => _ranks[rank];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetFile(int file) => _files[file];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackPass(byte position) => (_blackPassedPawns[position] & _boards[Pieces.WhitePawn]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhitePass(byte position) => (_whitePassedPawns[position] & _boards[Pieces.BlackPawn]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteOver(BitBoard opponentPawns) => (_boards[Pieces.WhitePawn] & opponentPawns).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackOver(BitBoard opponentPawns) => (_boards[Pieces.BlackPawn] & opponentPawns).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDraw()
    {
        if ((_boards[Pieces.WhitePawn] | _boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen] | _boards[Pieces.BlackPawn] | _boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen]).Any())
            return false;

        if ((_boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop]).Count() < 2 && (_boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop]).Count() < 2)
            return true;

        if ((_boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop] | _boards[Pieces.BlackBishop]).IsZero())
            return _boards[Pieces.BlackKnight].Count() < 3;

        if ((_boards[Pieces.BlackKnight] | _boards[Pieces.WhiteBishop] | _boards[Pieces.BlackBishop]).IsZero())
            return _boards[Pieces.WhiteKnight].Count() < 3;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackAttacksTo(byte to) => (_whiteKnightPatterns[to] & _boards[Pieces.BlackKnight]).Any()
            || (to.BishopAttacks(_occupied) & (_boards[Pieces.BlackBishop] | _boards[Pieces.BlackQueen])).Any()
            || (to.RookAttacks(_occupied) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).Any()
            || (_whitePawnPatterns[to] & _boards[Pieces.BlackPawn]).Any()
            || (_whiteKingPatterns[to] & _boards[Pieces.BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteAttacksTo(byte to) => (_blackKnightPatterns[to] & _boards[Pieces.WhiteKnight]).Any()
        || (to.BishopAttacks(_occupied) & (_boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteQueen])).Any()
        || (to.RookAttacks(_occupied) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).Any()
        || (_blackPawnPatterns[to] & _boards[Pieces.WhitePawn]).Any()
        || (_blackKingPatterns[to] & _boards[Pieces.WhiteKing]).Any();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheckToWhite() => IsBlackAttacksTo(_boards[Pieces.WhiteKing].BitScanForward());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheckToBlack() => IsWhiteAttacksTo(_boards[Pieces.BlackKing].BitScanForward());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBehindBlackPassed(byte from, byte to)
    {
        if ((_blackFacing[from] & _boards[Pieces.BlackPawn]).Any())
            return false;

        BitBoard bitBoard = _blackFacing[to] & _boards[Pieces.BlackPawn];
        if (bitBoard.IsZero())
            return false;

        var coordinate = bitBoard.BitScanForward();

        return (_blackFacing[coordinate] & _boards[Pieces.BlackPawn]).IsZero() && (_blackPassedPawns[coordinate] & _boards[Pieces.WhitePawn]).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBehindWhitePassed(byte from, byte to)
    {
        if ((_whiteFacing[from] & _boards[Pieces.WhitePawn]).Any())
            return false;

        BitBoard bitBoard = _whiteFacing[to] & _boards[Pieces.WhitePawn];
        if (bitBoard.IsZero())
            return false;

        var coordinate = bitBoard.BitScanForward();

        return (_whiteFacing[coordinate] & _boards[Pieces.WhitePawn]).IsZero() && (_whitePassedPawns[coordinate] & _boards[Pieces.BlackPawn]).IsZero();
    }

    #endregion

    #region Mobility

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenMobility(byte to) => (to.QueenAttacks(_occupied) & (_empty.Remove(_whitePawnAttacks)
            | _whiteKingZone)).Count() *
        _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookMobility(byte to) => (to.RookAttacks(_occupied) & (_empty.Remove(_whitePawnAttacks)
            | _whiteKingZone)).Count() *
       _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopMobility(byte to) => (to.BishopAttacks(_occupied) & (_empty.Remove(_whitePawnAttacks) | _boards[Pieces.WhiteRook] | _boards[Pieces.WhiteKnight]
            | _whiteKingZone))
            .Count() * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackKnightMobility(byte to) => (_blackKnightPatterns[to] & (_empty.Remove(_whitePawnAttacks) | _boards[Pieces.WhiteQueen] | _boards[Pieces.WhiteRook] | _boards[Pieces.WhiteBishop]
            | _whiteKingZone))
            .Count() * _evaluationService.GetKnightMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenMobility(byte to) => (to.QueenAttacks(_occupied) & (_empty.Remove(_blackPawnAttacks)
            | _blackKingZone)).Count() *
        _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookMobility(byte to) => (to.RookAttacks(_occupied) & (_empty.Remove(_blackPawnAttacks)
            | _blackKingZone)).Count() *
        _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopMobility(byte to) => (to.BishopAttacks(_occupied) & (_empty.Remove(_blackPawnAttacks) | _boards[Pieces.BlackRook] | _boards[Pieces.BlackKnight]
            | _blackKingZone)).Count() *
        _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteKnightMobility(byte to) => (_whiteKnightPatterns[to]
            & (_empty.Remove(_blackPawnAttacks) | _boards[Pieces.BlackQueen] | _boards[Pieces.BlackRook] | _boards[Pieces.BlackBishop]
            | _blackKingZone)).Count()
            * _evaluationService.GetKnightMobilityValue();

    #endregion

    #region SEE

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int StaticExchange(AttackBase attack)
    {
        _attackEvaluationService.Initialize(_boards);
        return _attackEvaluationService.StaticExchange(attack);
    }

    #endregion

    #region Castle

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoWhiteSmallCastle()
    {
        _pieces[Squares.G1] = Pieces.WhiteKing;
        _pieces[Squares.F1] = Pieces.WhiteRook;

        _hash = _hash ^ _hashTable[Squares.H1][Pieces.WhiteRook] ^ _hashTable[Squares.F1][Pieces.WhiteRook];
        _hash = _hash ^ _hashTable[Squares.E1][Pieces.WhiteKing] ^ _hashTable[Squares.G1][Pieces.WhiteKing];

        _boards[Pieces.WhiteKing] ^= _whiteSmallCastleKing;
        _boards[Pieces.WhiteRook] ^= _whiteSmallCastleRook;

        _whites ^= _whiteSmallCastleKing;
        _whites ^= _whiteSmallCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoBlackSmallCastle()
    {
        _pieces[Squares.G8] = Pieces.BlackKing;
        _pieces[Squares.F8] = Pieces.BlackRook;

        _hash = _hash ^ _hashTable[Squares.H8][Pieces.BlackRook] ^ _hashTable[Squares.F8][Pieces.BlackRook];
        _hash = _hash ^ _hashTable[Squares.E8][Pieces.BlackKing] ^ _hashTable[Squares.G8][Pieces.BlackKing];

        _boards[Pieces.BlackKing] ^= _blackSmallCastleKing;
        _boards[Pieces.BlackRook] ^= _blackSmallCastleRook;

        _blacks ^= _blackSmallCastleKing;
        _blacks ^= _blackSmallCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoBlackBigCastle()
    {
        _pieces[Squares.C8] = Pieces.BlackKing;
        _pieces[Squares.D8] = Pieces.BlackRook;

        _hash = _hash ^ _hashTable[Squares.A8][Pieces.BlackRook] ^ _hashTable[Squares.D8][Pieces.BlackRook];
        _hash = _hash ^ _hashTable[Squares.E8][Pieces.BlackKing] ^ _hashTable[Squares.C8][Pieces.BlackKing];

        _boards[Pieces.BlackKing] ^= _blackBigCastleKing;
        _boards[Pieces.BlackRook] ^= _blackBigCastleRook;

        _blacks ^= _blackBigCastleKing;
        _blacks ^= _blackBigCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoWhiteBigCastle()
    {
        _pieces[Squares.C1] = Pieces.WhiteKing;
        _pieces[Squares.D1] = Pieces.WhiteRook;

        _hash = _hash ^ _hashTable[Squares.A1][Pieces.WhiteRook] ^ _hashTable[Squares.D1][Pieces.WhiteRook];
        _hash = _hash ^ _hashTable[Squares.E1][Pieces.WhiteKing] ^ _hashTable[Squares.C1][Pieces.WhiteKing];

        _boards[Pieces.WhiteKing] ^= _whiteBigCastleKing;
        _boards[Pieces.WhiteRook] ^= _whiteBigCastleRook;

        _whites ^= _whiteBigCastleKing;
        _whites ^= _whiteBigCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoWhiteSmallCastle()
    {
        _pieces[Squares.E1] = Pieces.WhiteKing;
        _pieces[Squares.H1] = Pieces.WhiteRook;

        _hash = _hash ^ _hashTable[Squares.F1][Pieces.WhiteRook] ^ _hashTable[Squares.H1][Pieces.WhiteRook];
        _hash = _hash ^ _hashTable[Squares.G1][Pieces.WhiteKing] ^ _hashTable[Squares.E1][Pieces.WhiteKing];

        _boards[Pieces.WhiteKing] ^= _whiteSmallCastleKing;
        _boards[Pieces.WhiteRook] ^= _whiteSmallCastleRook;

        _whites ^= _whiteSmallCastleKing;
        _whites ^= _whiteSmallCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoBlackSmallCastle()
    {
        _pieces[Squares.E8] = Pieces.BlackKing;
        _pieces[Squares.H8] = Pieces.BlackRook;

        _hash = _hash ^ _hashTable[Squares.F8][Pieces.BlackRook] ^ _hashTable[Squares.H8][Pieces.BlackRook];
        _hash = _hash ^ _hashTable[Squares.G8][Pieces.BlackKing] ^ _hashTable[Squares.E8][Pieces.BlackKing];

        _boards[Pieces.BlackKing] ^= _blackSmallCastleKing;
        _boards[Pieces.BlackRook] ^= _blackSmallCastleRook;

        _blacks ^= _blackSmallCastleKing;
        _blacks ^= _blackSmallCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoWhiteBigCastle()
    {
        _pieces[Squares.E1] = Pieces.WhiteKing;
        _pieces[Squares.A1] = Pieces.WhiteRook;

        _hash = _hash ^ _hashTable[Squares.D1][Pieces.WhiteRook] ^ _hashTable[Squares.A1][Pieces.WhiteRook];
        _hash = _hash ^ _hashTable[Squares.C1][Pieces.WhiteKing] ^ _hashTable[Squares.E1][Pieces.WhiteKing];

        _boards[Pieces.WhiteKing] ^= _whiteBigCastleKing;
        _boards[Pieces.WhiteRook] ^= _whiteBigCastleRook;

        _whites ^= _whiteBigCastleKing;
        _whites ^= _whiteBigCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoBlackBigCastle()
    {
        _pieces[Squares.E8] = Pieces.BlackKing;
        _pieces[Squares.A8] = Pieces.BlackRook;

        _hash = _hash ^ _hashTable[Squares.D8][Pieces.BlackRook] ^ _hashTable[Squares.A8][Pieces.BlackRook];
        _hash = _hash ^ _hashTable[Squares.C8][Pieces.BlackKing] ^ _hashTable[Squares.E8][Pieces.BlackKing];

        _boards[Pieces.BlackKing] ^= _blackBigCastleKing;
        _boards[Pieces.BlackRook] ^= _blackBigCastleRook;

        _blacks ^= _blackBigCastleKing;
        _blacks ^= _blackBigCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackSmallCastle() => _moveHistory.CanDoBlackSmallCastle() && _empty.IsSet(_blackSmallCastleCondition) && _boards[Pieces.BlackRook].IsSet(Squares.H8) && !_moveHistory.IsLastMoveWasCheck();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteSmallCastle() => _moveHistory.CanDoWhiteSmallCastle() && _empty.IsSet(_whiteSmallCastleCondition) && _boards[Pieces.WhiteRook].IsSet(Squares.H1) && !_moveHistory.IsLastMoveWasCheck();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackBigCastle() => _moveHistory.CanDoBlackBigCastle() && _empty.IsSet(_blackBigCastleCondition) && _boards[Pieces.BlackRook].IsSet(Squares.A8) && !_moveHistory.IsLastMoveWasCheck();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteBigCastle() => _moveHistory.CanDoWhiteBigCastle() && _empty.IsSet(_whiteBigCastleCondition) && _boards[Pieces.WhiteRook].IsSet(Squares.A1) && !_moveHistory.IsLastMoveWasCheck();

    #endregion

    #region Evaluation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Round(int value) => value + _round[value % 10];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Evaluate()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();
        _whiteKingZone = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];
        _blackKingZone = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];
        var phase = _moveHistory.GetPhase();

        _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);

        return phase == Phase.Middle ? EvaluateMiddle() : phase == Phase.End ? EvaluateEnd() : EvaluateOpening();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EvaluateOpposite()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();
        _whiteKingZone = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];
        _blackKingZone = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];
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
            value += EvaluateWhiteRookMiddle();

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
    private int EvaluateWhitePawnOpening()
    {
        int value = 0;

        var bits = _boards[Pieces.WhitePawn];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhitePawnFullValue(coordinate);

            if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_whiteDoublePawns[coordinate] & _boards[Pieces.WhitePawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }


            if ((_whiteIsolatedPawns[coordinate] & _boards[Pieces.WhitePawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else
            {
                for (byte c = 0; c < _whiteBackwardPawns[coordinate].Count; c++)
                {
                    if ((_whiteBackwardPawns[coordinate][c].Key & _boards[Pieces.WhitePawn]).IsZero() &&
                        (_whiteBackwardPawns[coordinate][c].Value & _boards[Pieces.BlackPawn]).Any())
                    {
                        value -= _evaluationService.GetBackwardPawnValue();
                        break;
                    }
                }
            }
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhitePawnMiddle() => GetWhitePawnValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhitePawnEnd() => GetWhitePawnValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKnightOpening() => GetWhiteKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKnightMiddle() => GetWhiteKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKnightEnd() => GetWhiteKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteBishopOpening() => GetWhiteBishopValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteBishopMiddle() => GetWhiteBishopValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteBishopEnd()
    {
        var bits = _boards[Pieces.WhiteBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhiteBishopFullValue(coordinate);

            value += GetWhiteBishopPinsEnd(coordinate);

            //if ((_whiteMinorDefense[coordinate] & _boards[Pieces.WhitePawn]).Any())
            //{
            //    value += _evaluationService.GetMinorDefendedByPawnValue();
            //}

            value += GetWhiteBishopMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopBattary(byte coordinate)
    {
        if (_boards[Pieces.WhiteQueen].IsZero()) return 0;

        var pattern = _whiteBishopPatterns[coordinate] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        return (coordinate.XrayBishopAttacks(_occupied, _boards[Pieces.WhiteQueen]) & pattern).Any() ? _evaluationService.GetQueenBattaryValue() : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteRookOpening()
    {
        int i = -1;
        int value = 0;

        var king = _boards[Pieces.BlackKing].BitScanForward();
        var bits = _boards[Pieces.WhiteRook];

        while (bits.Any())
        {
            i++;
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhiteRookFullValue(coordinate);

            if ((coordinate > Squares.H2 && (_whiteFacing[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero()) ||
                (_rookFiles[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.WhiteRook]).Any()
                    && (_rookFiles[coordinate] & _boards[Pieces.WhiteRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnOpenFileValue();
                }
            }
            else if ((coordinate > Squares.H2 && (_whiteFacing[coordinate] & _boards[Pieces.WhitePawn]).IsZero()) ||
                (_rookFiles[coordinate] & _boards[Pieces.WhitePawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.WhiteRook]).Any()
                    && (_rookFiles[coordinate] & _boards[Pieces.WhiteRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                }
            }
            if (i > 0 && coordinate < Squares.A2 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.WhiteRook]).Any()
                    && (_rookRanks[coordinate] & _boards[Pieces.WhiteRook]).Any())
            {
                value += _evaluationService.GetConnectedRooksOnFirstRankValue();
            }

            value += GetWhiteRookPinsOpening(coordinate);

            if ((_whiteRookKingPattern[coordinate] & _boards[Pieces.WhiteKing]).Any() &&
                (_whiteRookPawnPattern[coordinate] & _boards[Pieces.WhitePawn]).Any())
            {
                value -= _evaluationService.GetRookBlockedByKingValue();
            }

            //value += GetWhiteRookMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookPinsEnd(byte coordinate) => GetWhiteRookDiscoveredCheck(coordinate)
                 + GetWhiteRookAbsolutePin(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookPinsOpening(byte coordinate) => GetWhiteRookDiscoveredCheck(coordinate)
                 + GetWhiteRookDiscoveredAttack(coordinate)
                 + GetWhiteRookAbsolutePin(coordinate)
                 + GetWhiteRookPartialPin(coordinate)
                 + GetWhiteRookBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookBattary(byte coordinate)
    {
        var pattern = _whiteRookPatterns[coordinate] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[Pieces.WhiteQueen].Any() && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.WhiteQueen]) & pattern).Any())
            return _evaluationService.GetQueenBattaryValue();

        if (_boards[Pieces.WhiteRook].Count() > 1 && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.WhiteRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookPartialPin(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.BlackQueen];
        if (bit.IsZero() || !_whiteRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.BlackQueen];
        if (bit.IsZero() || !_whiteRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookAbsolutePin(byte coordinate)
    {
        if (!_whiteRookPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookDiscoveredCheck(byte coordinate)
    {
        if (!_whiteRookPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteRookMiddle() => EvaluateWhiteRookOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteRookEnd()
    {
        int value = 0;
        var bits = _boards[Pieces.WhiteRook];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhiteRookFullValue(coordinate);

            if ((_rookFiles[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn]))
                .IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();
            }
            else if ((_rookFiles[coordinate] & _boards[Pieces.WhitePawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();
            }

            value += GetWhiteRookPinsEnd(coordinate);

            //value += GetWhiteRookMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteQueenOpening() => EvaluateWhiteQueen();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteQueen()
    {
        int value = 0;
        var bits = _boards[Pieces.WhiteQueen];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhiteQueenFullValue(coordinate);

            value += GetWhiteQueenPins(coordinate);

            //value += GetWhiteQueenMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenPins(byte coordinate) => GetWhiteQueenDiscoveredCheck(coordinate)
                 + GetWhiteQueenAbsolutePin(coordinate)
                 + GetWhiteQueenBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenBattary(byte coordinate)
    {
        var pattern = _whiteQueenPatterns[coordinate] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[Pieces.WhiteRook].Any() && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.WhiteRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        if (_boards[Pieces.WhiteBishop].Any() && (coordinate.XrayBishopAttacks(_occupied, _boards[Pieces.WhiteBishop]) & pattern).Any())
            return _evaluationService.GetBishopBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenAbsolutePin(byte coordinate)
    {
        if (!_whiteQueenPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenDiscoveredCheck(byte coordinate)
    {
        if (!_whiteQueenPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook] | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteQueenMiddle() => EvaluateWhiteQueen();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteQueenEnd() => EvaluateWhiteQueen();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingOpening()
    {
        var kingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        return _evaluationService.GetWhiteKingFullValue(kingPosition)
            + WhiteKingShieldOpeningValue(kingPosition)
            + WhiteKingZoneAttack();
        //- WhiteKingOpenValue(kingPosition);
        //- WhiteKingAttackValue(kingPosition);
        //+ WhiteDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingMiddle()
    {
        var kingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        return _evaluationService.GetWhiteKingFullValue(kingPosition)
            + WhiteKingShieldMiddleValue(kingPosition)
            + WhiteKingZoneAttack();
        //- WhiteKingOpenValue(kingPosition);
        //- WhiteKingAttackValue(kingPosition)
        //+ WhiteDistanceToQueen(kingPosition);
    }

    private int WhiteKingZoneAttack()
    {
        int valueOfAttacks = 0;
        BitBoard attackPattern;
        BitBoardList boards = stackalloc BitBoard[8];

        var bits = _boards[Pieces.WhiteKnight];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = _whiteKnightPatterns[position] & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetKnightAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.WhiteBishop];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.BishopAttacks(_occupied) & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetBishopAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.WhiteRook];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.RookAttacks(_occupied) & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetRookAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.WhiteQueen];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.QueenAttacks(_occupied) & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetQueenAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        if (boards.Count < 1) return 0;

        attackPattern = _whitePawnAttacks & _blackKingZone;
        if (attackPattern.Any())
        {
            valueOfAttacks++;
            boards.Add(attackPattern);
            return boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
        }

        return boards.Count < 2
            ? 0
            : boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingEnd()
    {
        var kingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        return _evaluationService.GetWhiteKingFullValue(kingPosition)
            - KingPawnTrofism(kingPosition);
        //+ WhiteDistanceToQueen(kingPosition);
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
            value += EvaluateBlackRookMiddle();

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
    private int EvaluateBlackPawnOpening()
    {
        int value = 0;
        var bits = _boards[Pieces.BlackPawn];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackPawnFullValue(coordinate);
            if ((_blackBlockedPawns[coordinate] & _whites).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_blackDoublePawns[coordinate] & _boards[Pieces.BlackPawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_blackIsolatedPawns[coordinate] & _boards[Pieces.BlackPawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else
            {
                for (byte c = 0; c < _blackBackwardPawns[coordinate].Count; c++)
                {
                    if ((_blackBackwardPawns[coordinate][c].Key & _boards[Pieces.BlackPawn]).IsZero() &&
                        (_blackBackwardPawns[coordinate][c].Value & _boards[Pieces.WhitePawn]).Any())
                    {
                        value -= _evaluationService.GetBackwardPawnValue();
                        break;
                    }
                }
            }
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackPawnMiddle() => GetBlackPawnValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackPawnEnd() => GetBlackPawnValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKnightOpening() => GetBlackKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKnightMiddle() => GetBlackKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKnightEnd() => GetBlackKnightValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackBishopOpening() => GetBlackBishopValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackBishopMiddle() => GetBlackBishopValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackBishopEnd()
    {
        var bits = _boards[Pieces.BlackBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackBishopFullValue(coordinate);

            value += GetBlackBishopPinsEnd(coordinate);

            //if ((_blackMinorDefense[coordinate] & _boards[Pieces.BlackPawn]).Any())
            //{
            //    value += _evaluationService.GetMinorDefendedByPawnValue();
            //}

            value += GetBlackBishopMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopPinsOpening(byte coordinate) => GetBlackBishopDiscoveredCheck(coordinate)
               + GetBlackBishopDiscoveredAttack(coordinate)
               + GetBlackBishopAbsolutePin(coordinate)
               + GetBlackBishopPartialPin(coordinate)
               + GetBlackBishopBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopPinsEnd(byte coordinate) => GetBlackBishopDiscoveredCheck(coordinate)
                 + GetBlackBishopAbsolutePin(coordinate)
                 + GetBlackBishopBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopBattary(byte coordinate)
    {
        if (_boards[Pieces.BlackQueen].IsZero()) return 0;

        var pattern = _blackBishopPatterns[coordinate] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        return (coordinate.XrayBishopAttacks(_occupied, _boards[Pieces.BlackQueen]) & pattern).Any() ? _evaluationService.GetQueenBattaryValue() : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackRookOpening()
    {
        int value = 0;
        int i = -1;
        var king = _boards[Pieces.WhiteKing].BitScanForward();
        var bits = _boards[Pieces.BlackRook];
        while (bits.Any())
        {
            i++;
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackRookFullValue(coordinate);

            if ((coordinate < Squares.A7 && (_blackFacing[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero()) ||
                (_rookFiles[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();

                if ((_whiteKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.BlackRook]).Any()
                    && (_rookFiles[coordinate] & _boards[Pieces.BlackRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnOpenFileValue();
                }
            }
            else if ((coordinate < Squares.A7 && (_blackFacing[coordinate] & _boards[Pieces.BlackPawn]).IsZero()) ||
                (_rookFiles[coordinate] & _boards[Pieces.BlackPawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();

                if ((_whiteKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.BlackRook]).Any()
                    && (_rookFiles[coordinate] & _boards[Pieces.BlackRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                }
            }
            if (i > 0 && coordinate > Squares.H7 && (coordinate.RookAttacks(_occupied) & _boards[Pieces.BlackRook]).Any()
                    && (_rookRanks[coordinate] & _boards[Pieces.BlackRook]).Any())
            {
                value += _evaluationService.GetConnectedRooksOnFirstRankValue();
            }

            value += GetBlackRookPinsOpening(coordinate);

            if ((_blackRookKingPattern[coordinate] & _boards[Pieces.BlackKing]).Any() &&
                (_blackRookPawnPattern[coordinate] & _boards[Pieces.BlackPawn]).Any())
            {
                value -= _evaluationService.GetRookBlockedByKingValue();
            }

            //value += GetBlackRookMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookPartialPin(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.WhiteQueen];
        if (bit.IsZero() || !_blackRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.WhiteQueen];
        if (bit.IsZero() || !_blackRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookAbsolutePin(byte coordinate)
    {
        if (!_blackRookPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookDiscoveredCheck(byte coordinate)
    {
        if (!_blackRookPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackRookMiddle() => EvaluateBlackRookOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackRookEnd()
    {
        int value = 0;
        var bits = _boards[Pieces.BlackRook];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackRookFullValue(coordinate);

            if ((_rookFiles[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn]))
                .IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();
            }
            else if ((_rookFiles[coordinate] & _boards[Pieces.BlackPawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();
            }

            value += GetBlackRookPinsEnd(coordinate);

            //value += GetBlackRookMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookBattary(byte coordinate)
    {
        var pattern = _blackRookPatterns[coordinate] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[Pieces.BlackQueen].Any() && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.BlackQueen]) & pattern).Any())
            return _evaluationService.GetQueenBattaryValue();

        if (_boards[Pieces.BlackRook].Count() > 1 && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.BlackRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();


        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookPinsOpening(byte coordinate) => GetBlackRookDiscoveredCheck(coordinate)
                     + GetBlackRookDiscoveredAttack(coordinate)
                     + GetBlackRookAbsolutePin(coordinate)
                     + GetBlackRookPartialPin(coordinate)
                     + GetBlackRookBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookPinsEnd(byte coordinate) => GetBlackRookDiscoveredCheck(coordinate)
                 + GetBlackRookAbsolutePin(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackQueen()
    {
        int value = 0;
        var bits = _boards[Pieces.BlackQueen];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackQueenFullValue(coordinate);

            value += GetBlackQueenPins(coordinate);

            //value += GetBlackQueenMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackQueenOpening() => EvaluateBlackQueen();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenPins(byte coordinate) => GetBlackQueenDiscoveredCheck(coordinate)
                 + GetBlackQueenAbsolutePin(coordinate)
                 + GetBlackQueenBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenBattary(byte coordinate)
    {
        var pattern = _blackQueenPatterns[coordinate] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[Pieces.BlackRook].Any() && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.BlackRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        if (_boards[Pieces.BlackBishop].Any() && (coordinate.XrayBishopAttacks(_occupied, _boards[Pieces.BlackBishop]) & pattern).Any())
            return _evaluationService.GetBishopBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenAbsolutePin(byte coordinate)
    {
        if (!_blackQueenPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenDiscoveredCheck(byte coordinate)
    {
        if (!_blackQueenPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook] | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackQueenMiddle() => EvaluateBlackQueen();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackQueenEnd() => EvaluateBlackQueen();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingOpening()
    {
        var kingPosition = _boards[Pieces.BlackKing].BitScanForward();
        return _evaluationService.GetBlackKingFullValue(kingPosition)
            + BlackKingShieldOpeningValue(kingPosition)
            + BlackKingZoneAttack();
        //- BlackKingOpenValue(kingPosition);
        //- BlackKingAttackValue(kingPosition)
        // BlackDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingMiddle()
    {
        var kingPosition = _boards[Pieces.BlackKing].BitScanForward();
        return _evaluationService.GetBlackKingFullValue(kingPosition)
            + BlackKingShieldMiddleValue(kingPosition)
            + BlackKingZoneAttack();
        //- BlackKingOpenValue(kingPosition);
        //- BlackKingAttackValue(kingPosition);
        //BlackDistanceToQueen(kingPosition);
    }
    private int BlackKingZoneAttack()
    {
        int valueOfAttacks = 0;
        BitBoard attackPattern;
        BitBoardList boards = stackalloc BitBoard[8];

        var bits = _boards[Pieces.BlackKnight];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = _blackKnightPatterns[position] & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetKnightAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.BlackBishop];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.BishopAttacks(_occupied) & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetBishopAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.BlackRook];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.RookAttacks(_occupied) & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetRookAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[Pieces.BlackQueen];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.QueenAttacks(_occupied) & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetQueenAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        if (boards.Count < 1) return 0;

        attackPattern = _blackPawnAttacks & _whiteKingZone;
        if (attackPattern.Any())
        {
            valueOfAttacks++;
            boards.Add(attackPattern);
            return boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
        }

        return boards.Count < 2
            ? 0
            : boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingEnd()
    {
        var kingPosition = _boards[Pieces.BlackKing].BitScanForward();
        return _evaluationService.GetBlackKingFullValue(kingPosition)
            - KingPawnTrofism(kingPosition);
        //+ BlackDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetStaticValue()
    {
        var _phase = _moveHistory.GetPhase();
        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        return GetWhiteStaticValue() - GetBlackStaticValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetKingSafetyValue()
    {
        var _phase = _moveHistory.GetPhase();
        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        return 0; //WhiteMiddleKingSafety(_boards[5].BitScanForward()) - BlackMiddleKingSafety(_boards[11].BitScanForward());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPawnValue()
    {
        var _phase = _moveHistory.GetPhase();
        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        return GetWhitePawnValue() - GetBlackPawnValue();
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetDistance(byte from, BitBoard bits)
    {
        int value = 0;
        var distance = _evaluationService.Distance(from);
        while (bits.Any())
        {
            byte position = bits.BitScanForward();
            value += distance[position];
            bits = bits.Remove(position);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int KingPawnTrofism(byte kingPosition) => _trofismCoefficient * GetDistance(kingPosition, _boards[0] | _boards[6]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackKingShieldOpeningValue(byte kingPosition) => _moveHistory.CanDoBlackCastle() ? 0 : BlackKingShieldMiddleValue(kingPosition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackKingShieldMiddleValue(byte kingPosition)
    {
        var pawns = _boards[Pieces.BlackPawn];

        return (_blackPawnShield7[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield2Value() +
            (_blackPawnShield6[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield3Value() +
            (_blackPawnShield5[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield4Value() +
            (_blackPawnKingShield7[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield2Value() +
            (_blackPawnKingShield6[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield3Value() +
            (_blackPawnKingShield5[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield4Value();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopValue()
    {
        var bits = _boards[Pieces.BlackBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackBishopFullValue(coordinate);

            value += GetBlackBishopPinsOpening(coordinate);

            //if ((_blackMinorDefense[coordinate] & _boards[Pieces.BlackPawn]).Any())
            //{
            //    value += _evaluationService.GetMinorDefendedByPawnValue();
            //}

            value += GetBlackBishopMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopPartialPin(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen];
        if (bit.IsZero() || !_blackBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen];
        if (bit.IsZero() || !_blackBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook] | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopAbsolutePin(byte coordinate)
    {
        if (!_blackBishopPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any())
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopDiscoveredCheck(byte coordinate)
    {
        if (!_blackBishopPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook] | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackKnightValue()
    {
        int value = 0;
        var bits = _boards[Pieces.BlackKnight];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackKnightFullValue(coordinate);

            //if ((_blackMinorDefense[coordinate] & _boards[Pieces.BlackPawn]).Any())
            //{
            //    value += _evaluationService.GetMinorDefendedByPawnValue();
            //}

            value += GetBlackKnightMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackPawnValue()
    {
        if (_boards[Pieces.BlackPawn].IsZero()) return _evaluationService.GetNoPawnsValue();

        int value = 0;
        var bits = _boards[Pieces.BlackPawn];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackPawnFullValue(coordinate);
            if ((_blackBlockedPawns[coordinate] & _whites).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_blackDoublePawns[coordinate] & _boards[Pieces.BlackPawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_blackFacing[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero()
                && (_blackPassedPawns[coordinate] & _boards[Pieces.WhitePawn]).IsZero())
            {
                var pp = _evaluationService.GetBlackPassedPawnValue(coordinate);
                if (pp > 0)
                {
                    value += pp;
                    //if ((_blackCandidatePawnsAttackBack[coordinate] & _boards[Pieces.BlackPawn]).Any())
                    //{
                    //    value += _evaluationService.GetProtectedPassedPawnValue();
                    //}
                }
            }


            if ((_blackIsolatedPawns[coordinate] & _boards[Pieces.BlackPawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else
            {
                for (byte c = 0; c < _blackBackwardPawns[coordinate].Count; c++)
                {
                    if ((_blackBackwardPawns[coordinate][c].Key & _boards[Pieces.BlackPawn]).IsZero() &&
                        (_blackBackwardPawns[coordinate][c].Value & _boards[Pieces.WhitePawn]).Any())
                    {
                        value -= _evaluationService.GetBackwardPawnValue();
                        break;
                    }
                }
            }
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingShieldOpeningValue(byte kingPosition) => _moveHistory.CanDoWhiteCastle() ? 0 : WhiteKingShieldMiddleValue(kingPosition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingShieldMiddleValue(byte kingPosition)
    {
        var pawns = _boards[Pieces.WhitePawn];

        return (_whitePawnShield2[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield2Value() +
            (_whitePawnShield3[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield3Value() +
            (_whitePawnShield4[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield4Value() +
            (_whitePawnKingShield2[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield2Value() +
            (_whitePawnKingShield3[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield3Value() +
            (_whitePawnKingShield4[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield4Value();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopValue()
    {
        var bits = _boards[Pieces.WhiteBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhiteBishopFullValue(coordinate);

            value += GetWhiteBishopPinsOpening(coordinate);

            //if ((_whiteMinorDefense[coordinate] & _boards[Pieces.WhitePawn]).Any())
            //{
            //    value += _evaluationService.GetMinorDefendedByPawnValue();
            //}

            value += GetWhiteBishopMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopPinsEnd(byte coordinate) => GetWhiteBishopDiscoveredCheck(coordinate)
                 + GetWhiteBishopAbsolutePin(coordinate)
                 + GetWhiteBishopBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopPinsOpening(byte coordinate) => GetWhiteBishopDiscoveredCheck(coordinate)
                 + GetWhiteBishopDiscoveredAttack(coordinate)
                 + GetWhiteBishopAbsolutePin(coordinate)
                 + GetWhiteBishopPartialPin(coordinate)
                 + GetWhiteBishopBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetWhiteMovablePawns() => ((_boards[Pieces.WhitePawn] << 8) & _empty) >> 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetBlackMovablePawns() => ((_boards[Pieces.BlackPawn] >> 8) & _empty) << 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen];
        if (bit.IsZero() || !_whiteBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook] | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopPartialPin(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen];
        if (bit.IsZero() || !_whiteBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopAbsolutePin(byte coordinate)
    {
        if (!_whiteBishopPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopDiscoveredCheck(byte coordinate)
    {
        if (!_whiteBishopPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook] | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteKnightValue()
    {
        int value = 0;

        var bits = _boards[Pieces.WhiteKnight];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();

            value += _evaluationService.GetWhiteKnightFullValue(coordinate);
            //if ((_whiteMinorDefense[coordinate] & _boards[Pieces.WhitePawn]).Any())
            //{
            //    value += _evaluationService.GetMinorDefendedByPawnValue();
            //}

            value += GetWhiteKnightMobility(coordinate);

            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhitePawnValue()
    {
        if (_boards[Pieces.WhitePawn].IsZero())
            return _evaluationService.GetNoPawnsValue();

        int value = 0;

        var bits = _boards[Pieces.WhitePawn];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhitePawnFullValue(coordinate);

            if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_whiteDoublePawns[coordinate] & _boards[Pieces.WhitePawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_whiteFacing[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero()
                && (_whitePassedPawns[coordinate] & _boards[Pieces.BlackPawn]).IsZero())
            {
                var pp = _evaluationService.GetWhitePassedPawnValue(coordinate);
                if (pp > 0)
                {
                    value += pp;
                    //if ((_whiteCandidatePawnsAttackBack[coordinate] & _boards[Pieces.WhitePawn]).Any())
                    //{
                    //    value += _evaluationService.GetProtectedPassedPawnValue();
                    //}
                }
            }


            if ((_whiteIsolatedPawns[coordinate] & _boards[Pieces.WhitePawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else
            {
                for (byte c = 0; c < _whiteBackwardPawns[coordinate].Count; c++)
                {
                    if ((_whiteBackwardPawns[coordinate][c].Key & _boards[Pieces.WhitePawn]).IsZero() &&
                        (_whiteBackwardPawns[coordinate][c].Value & _boards[Pieces.BlackPawn]).Any())
                    {
                        value -= _evaluationService.GetBackwardPawnValue();
                        break;
                    }
                }
            }
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    #endregion

    #region Private

    private void SetBoards()
    {
        _boards = new BitBoard[12];
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

        _rookFiles = new BitBoard[64];
        for (byte i = 0; i < _rookFiles.Length; i++)
        {
            _rookFiles[i] = _files[i % 8] ^ i.AsBitBoard();
        }

        _rookRanks = new BitBoard[64];
        for (byte i = 0; i < _rookRanks.Length; i++)
        {
            _rookRanks[i] = _ranks[i / 8] ^ i.AsBitBoard();
        }

        _notFileA = ~_files[0];
        _notFileH = ~_files[7];

        _rank1 = _ranks[1];
        _rank6 = _ranks[6];
        _notRank1 = ~_ranks[1];
        _notRank6 = ~_ranks[6];
    }

    private void SetCastles()
    {
        _whiteSmallCastleCondition = new BitBoard();
        _whiteSmallCastleCondition = _whiteSmallCastleCondition.Set(5, 6);

        _whiteBigCastleCondition = new BitBoard();
        _whiteBigCastleCondition = _whiteBigCastleCondition.Set(1, 2, 3);

        _blackSmallCastleCondition = new BitBoard();
        _blackSmallCastleCondition = _blackSmallCastleCondition.Set(61, 62);

        _blackBigCastleCondition = new BitBoard();
        _blackBigCastleCondition = _blackBigCastleCondition.Set(57, 58, 59);

        _whiteBigCastleKing = new BitBoard();
        _whiteBigCastleKing = _whiteBigCastleKing.Or(4, 2);

        _whiteBigCastleRook = new BitBoard();
        _whiteBigCastleRook = _whiteBigCastleRook.Or(0, 3);

        _whiteSmallCastleKing = new BitBoard();
        _whiteSmallCastleKing = _whiteSmallCastleKing.Or(4, 6);

        _whiteSmallCastleRook = new BitBoard();
        _whiteSmallCastleRook = _whiteSmallCastleRook.Or(5, 7);

        _blackBigCastleKing = new BitBoard();
        _blackBigCastleKing = _blackBigCastleKing.Or(58, 60);

        _blackBigCastleRook = new BitBoard();
        _blackBigCastleRook = _blackBigCastleRook.Or(56, 59);

        _blackSmallCastleKing = new BitBoard();
        _blackSmallCastleKing = _blackSmallCastleKing.Or(60, 62);

        _blackSmallCastleRook = new BitBoard();
        _blackSmallCastleRook = _blackSmallCastleRook.Or(61, 63);
    }

    #endregion

    public override string ToString()
    {
        char[] pieceUnicodeChar =
        {
            '\u2659', '\u2658', '\u2657', '\u2656', '\u2655', '\u2654',
            '\u265F', '\u265E', '\u265D', '\u265C', '\u265B', '\u265A', ' '
        };
        var piecesNames = pieceUnicodeChar.Select(c => c.ToString()).ToArray();

        StringBuilder builder = new();
        for (short y = 7; y >= 0; y--)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte i = (byte)(y * 8 + x);
                string v = piecesNames.Last();
                if (!_empty.IsSet(i.AsBitBoard()))
                {
                    v = piecesNames[_pieces[i]];
                }

                builder.Append($"[ {v} ]");
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteMoveLigal(MoveBase move)
    {
        move.Make();

        bool isLegal = !IsCheckToWhite();

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteLigal(MoveBase move)
    {
        move.Make();

        bool isLegal = !IsWhiteNotLegal(move);

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteCastleLigal(MoveBase move, byte rook)
    {
        move.Make();

        bool isLegal = !IsWhiteCastleNotLegal(move.To, rook);

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteNotLegal(MoveBase move) => IsBlackAttacksTo(GetWhiteKingPosition()) ||
            (move.IsCastle && IsBlackAttacksTo(move.To == Squares.C1 ? Squares.D1 : Squares.F1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteCastleNotLegal(byte king, byte rook) => IsBlackAttacksTo(king) || IsBlackAttacksTo(rook);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackMoveLigal(MoveBase move)
    {
        move.Make();

        bool isLegal = !IsCheckToBlack();

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackCastleLigal(MoveBase move, byte rook)
    {
        move.Make();

        bool isLegal = !IsBlackCastleNotLegal(move.To, rook);

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackLigal(MoveBase move)
    {
        move.Make();

        bool isLegal = !IsBlackNotLegal(move);

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackCastleNotLegal(byte king, byte rook) => IsWhiteAttacksTo(king) || IsWhiteAttacksTo(rook);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackNotLegal(MoveBase move) => IsWhiteAttacksTo(GetBlackKingPosition()) ||
             (move.IsCastle && IsWhiteAttacksTo(move.To == Squares.C8 ? Squares.D8 : Squares.F8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnyWhiteAttackTo(byte to) => AnyWhitePawnAttackTo(to) ||
            AnyWhiteKnightAttackTo(to) ||
            AnyWhiteBishopAttackTo(to) ||
            AnyWhiteRookAttackTo(to) ||
            AnyWhiteQueenAttackTo(to) ||
            AnyWhiteKingAttackTo(to);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteKingAttackTo(byte to)
    {
        byte from = _boards[Pieces.WhiteKing].BitScanForward();
        return _whiteKingPatterns[from].IsSet(to) && IsWhiteMoveLigal(_moveProvider.GetWhiteKingAttacks(from, to));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteQueenAttackTo(byte to)
    {
        var fromBoard = _boards[Pieces.WhiteQueen] & to.QueenAttacks(_occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsWhiteMoveLigal(_moveProvider.GetWhiteQueenAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteRookAttackTo(byte to)
    {
        var fromBoard = _boards[Pieces.WhiteRook] & to.RookAttacks(_occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsWhiteMoveLigal(_moveProvider.GetWhiteRookAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteBishopAttackTo(byte to)
    {
        var fromBoard = _boards[Pieces.WhiteBishop] & to.BishopAttacks(_occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsWhiteMoveLigal(_moveProvider.GetWhiteBishopAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteKnightAttackTo(byte to)
    {
        var fromBoard = _boards[Pieces.WhiteKnight] & _whiteKnightPatterns[to];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsWhiteMoveLigal(_moveProvider.GetWhiteKnightAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhitePawnAttackTo(byte to)
    {
        var fromBoard = _boards[Pieces.WhitePawn].Remove(_rank6) & _blackPawnPatterns[to];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsWhiteMoveLigal(_moveProvider.GetWhitePawnAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnyBlackAttackTo(byte to) => AnyBlackPawnAttackTo(to) ||
            AnyBlackKnightAttackTo(to) ||
            AnyBlackBishopAttackTo(to) ||
            AnyBlackRookAttackTo(to) ||
            AnyBlackQueenAttackTo(to) ||
            AnyBlackKingAttackTo(to);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackKingAttackTo(byte to)
    {
        byte from = _boards[Pieces.BlackKing].BitScanForward();
        return _blackKingPatterns[from].IsSet(to) && IsBlackMoveLigal(_moveProvider.GetBlackKingAttacks(from, to));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackQueenAttackTo(byte to)
    {
        var fromBoard = _boards[Pieces.BlackQueen] & to.QueenAttacks(_occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsBlackMoveLigal(_moveProvider.GetBlackQueenAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackRookAttackTo(byte to)
    {
        var fromBoard = _boards[Pieces.BlackRook] & to.RookAttacks(_occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsBlackMoveLigal(_moveProvider.GetBlackRookAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackBishopAttackTo(byte to)
    {
        var fromBoard = _boards[Pieces.BlackBishop] & to.BishopAttacks(_occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsBlackMoveLigal(_moveProvider.GetBlackBishopAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackKnightAttackTo(byte to)
    {
        var fromBoard = _boards[Pieces.BlackKnight] & _blackKnightPatterns[to];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsBlackMoveLigal(_moveProvider.GetBlackKnightAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackPawnAttackTo(byte to)
    {
        var fromBoard = _boards[Pieces.BlackPawn].Remove(_rank1) & _whitePawnPatterns[to];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsBlackMoveLigal(_moveProvider.GetBlackPawnAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AttackBase GetWhiteAttackToForPromotion(byte to) => GetWhiteKnightAttacksTo(to, out AttackBase attack) ||
        GetWhiteBishopAttacksTo(to, out attack) ||
        GetWhiteRookAttacksTo(to, out attack) ||
        GetWhiteQueenAttacksTo(to, out attack) ||
        GetWhiteKingAttacksTo(to, out attack)
            ? attack
            : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteKnightAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = _whiteKnightPatterns[to] & _boards[Pieces.WhiteKnight];

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetWhiteKnightAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteQueenAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.QueenAttacks(_occupied) & _boards[Pieces.WhiteQueen];

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetWhiteQueenAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteBishopAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.BishopAttacks(_occupied) & _boards[Pieces.WhiteBishop];

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetWhiteBishopAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteRookAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.RookAttacks(_occupied) & _boards[Pieces.WhiteRook];

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetWhiteRookAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteKingAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = _whiteKingPatterns[to] & _boards[Pieces.WhiteKing];

        if (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetWhiteKingAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                return true;
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AttackBase GetBlackAttackToForPromotion(byte to) => GetBlackKnightAttacksTo(to, out AttackBase attack) ||
        GetBlackBishopAttacksTo(to, out attack) ||
        GetBlackRookAttacksTo(to, out attack) ||
        GetBlackQueenAttacksTo(to, out attack) ||
        GetBlackKingAttacksTo(to, out attack)
            ? attack
            : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackKnightAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = _blackKnightPatterns[to] & _boards[Pieces.BlackKnight];

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetBlackKnightAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackQueenAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.QueenAttacks(_occupied) & _boards[Pieces.BlackQueen];

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetBlackQueenAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackBishopAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.BishopAttacks(_occupied) & _boards[Pieces.BlackBishop];

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetBlackBishopAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackRookAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.RookAttacks(_occupied) & _boards[Pieces.BlackRook];

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetBlackRookAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackKingAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = _blackKingPatterns[to] & _boards[Pieces.BlackKing];

        if (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetBlackKingAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                return true;
        }

        attack = null;
        return false;
    }
}
