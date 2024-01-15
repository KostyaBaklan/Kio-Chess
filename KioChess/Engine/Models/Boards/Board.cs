using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Services.Evaluation;

namespace Engine.Models.Boards;

public class Board 
{
    #region Pieces

    const byte WhitePawn = 0;
    const byte WhiteKnight = 1;
    const byte WhiteBishop = 2;
    const byte WhiteRook = 3;
    const byte WhiteQueen = 4;
    const byte WhiteKing = 5;
    const byte BlackPawn = 6;
    const byte BlackKnight = 7;
    const byte BlackBishop = 8;
    const byte BlackRook = 9;
    const byte BlackQueen = 10;
    const byte BlackKing = 11;

    #endregion

    #region Squares

    const byte A1 = 0;
    const byte B1 = 1;
    const byte C1 = 2;
    const byte D1 = 3;
    const byte E1 = 4;
    const byte F1 = 5;
    const byte G1 = 6;
    const byte H1 = 7;
    const byte A2 = 8;
    const byte B2 = 9;
    const byte C2 = 10;
    const byte D2 = 11;
    const byte E2 = 12;
    const byte F2 = 13;
    const byte G2 = 14;
    const byte H2 = 15;
    const byte A3 = 16;
    const byte B3 = 17;
    const byte C3 = 18;
    const byte D3 = 19;
    const byte E3 = 20;
    const byte F3 = 21;
    const byte G3 = 22;
    const byte H3 = 23;
    const byte A4 = 24;
    const byte B4 = 25;
    const byte C4 = 26;
    const byte D4 = 27;
    const byte E4 = 28;
    const byte F4 = 29;
    const byte G4 = 30;
    const byte H4 = 31;
    const byte A5 = 32;
    const byte B5 = 33;
    const byte C5 = 34;
    const byte D5 = 35;
    const byte E5 = 36;
    const byte F5 = 37;
    const byte G5 = 38;
    const byte H5 = 39;
    const byte A6 = 40;
    const byte B6 = 41;
    const byte C6 = 42;
    const byte D6 = 43;
    const byte E6 = 44;
    const byte F6 = 45;
    const byte G6 = 46;
    const byte H6 = 47;
    const byte A7 = 48;
    const byte B7 = 49;
    const byte C7 = 50;
    const byte D7 = 51;
    const byte E7 = 52;
    const byte F7 = 53;
    const byte G7 = 54;
    const byte H7 = 55;
    const byte A8 = 56;
    const byte B8 = 57;
    const byte C8 = 58;
    const byte D8 = 59;
    const byte E8 = 60;
    const byte F8 = 61;
    const byte G8 = 62;
    const byte H8 = 63;

    #endregion

    #region Fields

    private byte _phase = Phase.Opening;

    private ulong _hash;
    private ulong[][] _hashTable;

    private BitBoard _empty;
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

    private BitBoard[] _notRanks;
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
    private BitBoard[] _whiteRookKingPattern;
    private BitBoard[] _whiteRookPawnPattern;
    private BitBoard[] _blackRookKingPattern;
    private BitBoard[] _blackRookPawnPattern;

    private BitBoard[] _whitePawnStormFile4;
    private BitBoard[] _whitePawnStormFile5;
    private BitBoard[] _whitePawnStormFile6;
    private BitBoard[] _blackPawnStormFile4;
    private BitBoard[] _blackPawnStormFile5;
    private BitBoard[] _blackPawnStormFile6;

    private BitBoard[] _whiteRookFileBlocking;
    private BitBoard[] _whiteRookRankBlocking;
    private BitBoard[] _blackRookFileBlocking;
    private BitBoard[] _blackRookRankBlocking;

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

    private readonly int[] _round = new int[] { 0, -1, -2, 2, 1, 0, -1, -2, 2, 1 };

    private PositionsList _positionList;
    private readonly MoveProvider _moveProvider;
    private readonly IMoveHistoryService _moveHistory;
    private EvaluationServiceBase _evaluationService;
    private readonly IEvaluationServiceFactory _evaluationServiceFactory;
    private readonly AttackEvaluationService _attackEvaluationService;

    #endregion

    #region CTOR

    public Board()
    {
        _pieces = new byte[64];
        _positionList = new PositionsList();

        MoveBase.Board = this;

        SetBoards();

        SetFilesAndRanks();

        SetCastles();

        _moveProvider = ServiceLocator.Current.GetInstance<MoveProvider>();
        _moveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        _evaluationServiceFactory = ServiceLocator.Current.GetInstance<IEvaluationServiceFactory>();
        _attackEvaluationService = new AttackEvaluationService(_evaluationServiceFactory,_moveProvider);
        _attackEvaluationService.SetBoard(this);

        HashSet<ulong> set = new HashSet<ulong>();

        InitializeZoobrist(set);

        _moveProvider.SetBoard(this);


        _whiteQueenOpening = D1.AsBitBoard() | E1.AsBitBoard() | C1.AsBitBoard() |
                             D2.AsBitBoard() | E2.AsBitBoard() | C2.AsBitBoard();

        _blackQueenOpening = D8.AsBitBoard() | E8.AsBitBoard() | C8.AsBitBoard() |
                             D7.AsBitBoard() | E7.AsBitBoard() | C7.AsBitBoard();

        SetKingSafety();

        SetPawnProperties();

        SetKingRookPatterns();

        SetRookBlocking();

        SetForwards();

        SetAttackPatterns();
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
            _whitePawnPatterns[i] = _moveProvider.GetAttackPattern(WhitePawn, i);
            _whiteKnightPatterns[i] = _moveProvider.GetAttackPattern(WhiteKnight, i);
            _whiteBishopPatterns[i] = _moveProvider.GetAttackPattern(WhiteBishop, i);
            _whiteRookPatterns[i] = _moveProvider.GetAttackPattern(WhiteRook, i);
            _whiteQueenPatterns[i] = _moveProvider.GetAttackPattern(WhiteQueen, i);
            _whiteKingPatterns[i] = _moveProvider.GetAttackPattern(WhiteKing, i);

            _blackPawnPatterns[i] = _moveProvider.GetAttackPattern(BlackPawn, i);
            _blackKnightPatterns[i] = _moveProvider.GetAttackPattern(BlackKnight, i);
            _blackBishopPatterns[i] = _moveProvider.GetAttackPattern(BlackBishop, i);
            _blackRookPatterns[i] = _moveProvider.GetAttackPattern(BlackRook, i);
            _blackQueenPatterns[i] = _moveProvider.GetAttackPattern(BlackQueen, i);
            _blackKingPatterns[i] = _moveProvider.GetAttackPattern(BlackKing, i);
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

    private void SetForwards()
    {
        var evaluationServices = _evaluationServiceFactory.GetEvaluationServices();

        var moves = _moveProvider.GetAll();

        foreach (var move in moves)
        {
            if (move.IsPromotion || move.IsCastle)
            {
                for (int i = 0; i < 3; i++)
                {
                    move.IsForward[i] = true;
                }
            }
            else if (move.IsAttack)
            {
                continue;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    move.IsForward[i] = evaluationServices[i].IsForward(move);
                }
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
            _whiteRookRankBlocking[i] = _moveProvider.GetAttackPattern(BlackPawn, (byte)(i + 8));
        }
        for (int i = 8; i < 56; i++)
        {
            _blackRookRankBlocking[i] = _moveProvider.GetAttackPattern(WhitePawn, (byte)(i - 8));
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
        var bitBoard = _blackRookPawnPattern[A8];
        bitBoard = bitBoard | A7.AsBitBoard() | A6.AsBitBoard();
        _blackRookPawnPattern[A8] = bitBoard;

        bitBoard = _blackRookPawnPattern[B8];
        bitBoard = bitBoard | B7.AsBitBoard() | B6.AsBitBoard();
        _blackRookPawnPattern[B8] = bitBoard;

        bitBoard = _blackRookPawnPattern[C8];
        bitBoard = bitBoard | C7.AsBitBoard() | C6.AsBitBoard();
        _blackRookPawnPattern[C8] = bitBoard;

        bitBoard = _blackRookPawnPattern[H8];
        bitBoard = bitBoard | H7.AsBitBoard() | H6.AsBitBoard();
        _blackRookPawnPattern[H8] = bitBoard;

        bitBoard = _blackRookPawnPattern[G8];
        bitBoard = bitBoard | G7.AsBitBoard() | G6.AsBitBoard();
        _blackRookPawnPattern[G8] = bitBoard;
    }

    private void SetWhiteRookPawnPattern()
    {
        var bitBoard = _whiteRookPawnPattern[A1];
        bitBoard = bitBoard | A2.AsBitBoard() | A3.AsBitBoard();
        _whiteRookPawnPattern[A1] = bitBoard;

        bitBoard = _whiteRookPawnPattern[B1];
        bitBoard = bitBoard | B2.AsBitBoard() | B3.AsBitBoard();
        _whiteRookPawnPattern[B1] = bitBoard;

        bitBoard = _whiteRookPawnPattern[C1];
        bitBoard = bitBoard | C2.AsBitBoard() | C3.AsBitBoard();
        _whiteRookPawnPattern[C1] = bitBoard;

        bitBoard = _whiteRookPawnPattern[H1];
        bitBoard = bitBoard | H2.AsBitBoard() | H3.AsBitBoard();
        _whiteRookPawnPattern[H1] = bitBoard;

        bitBoard = _whiteRookPawnPattern[G1];
        bitBoard = bitBoard | G2.AsBitBoard() | G3.AsBitBoard();
        _whiteRookPawnPattern[G1] = bitBoard;
    }

    private void SetBlackRookKingPattern()
    {
        var bitBoard = _blackRookKingPattern[A8];
        bitBoard = bitBoard | B8.AsBitBoard() | C8.AsBitBoard() | D8.AsBitBoard();
        _blackRookKingPattern[A8] = bitBoard;

        bitBoard = _blackRookKingPattern[B8];
        bitBoard = bitBoard | C8.AsBitBoard() | D8.AsBitBoard();
        _blackRookKingPattern[B8] = bitBoard;

        bitBoard = _blackRookKingPattern[C8];
        bitBoard = bitBoard | D8.AsBitBoard();
        _blackRookKingPattern[C8] = bitBoard;

        bitBoard = _blackRookKingPattern[H8];
        bitBoard = bitBoard | G8.AsBitBoard() | F8.AsBitBoard();
        _blackRookKingPattern[H8] = bitBoard;

        bitBoard = _blackRookKingPattern[G8];
        bitBoard = bitBoard | F8.AsBitBoard();
        _blackRookKingPattern[G8] = bitBoard;
    }

    private void SetWhiteRookKingPattern()
    {
        var bitBoard = _whiteRookKingPattern[A1];
        bitBoard = bitBoard | B1.AsBitBoard() | C1.AsBitBoard() | D1.AsBitBoard();
        _whiteRookKingPattern[A1] = bitBoard;

        bitBoard = _whiteRookKingPattern[B1];
        bitBoard = bitBoard | C1.AsBitBoard() | D1.AsBitBoard();
        _whiteRookKingPattern[B1] = bitBoard;

        bitBoard = _whiteRookKingPattern[C1];
        bitBoard = bitBoard | D1.AsBitBoard();
        _whiteRookKingPattern[C1] = bitBoard;

        bitBoard = _whiteRookKingPattern[H1];
        bitBoard = bitBoard | G1.AsBitBoard() | F1.AsBitBoard();
        _whiteRookKingPattern[H1] = bitBoard;

        bitBoard = _whiteRookKingPattern[G1];
        bitBoard = bitBoard | F1.AsBitBoard();
        _whiteRookKingPattern[G1] = bitBoard;
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

        for (byte i = 8; i < 48; i++)
        {
            BitBoard b = new BitBoard();
            for (byte j = (byte)(i + 8); j < 56; j += 8)
            {
                b |= j.AsBitBoard();
            }
            _whiteFacing[i] = b;
        }
        for (byte i = 16; i < 56; i++)
        {
            BitBoard b = new BitBoard();
            for (byte j = (byte)(i - 8); j >= 8; j -= 8)
            {
                b |= j.AsBitBoard();
            }
            _blackFacing[i] = b;
        }

        for (byte i = 16; i < 64; i++)
        {
            _whiteMinorDefense[i] = _moveProvider.GetAttackPattern(BlackPawn, i);
        }
        for (byte i = 0; i < 48; i++)
        {
            _blackMinorDefense[i] = _moveProvider.GetAttackPattern(WhitePawn, i);
        }

        BitBoard ones = new BitBoard();
        ones = ~ones;

        for (byte i = 8; i < 56; i++)
        {
            var f = i % 8;
            var r = i / 8;

            _whiteBlockedPawns[i] = ((byte)(i + 8)).AsBitBoard();
            _blackBlockedPawns[i] = ((byte)(i - 8)).AsBitBoard();

            _whiteDoublePawns[i] = _files[f] ^ i.AsBitBoard();
            _blackDoublePawns[i] = _files[f] ^ i.AsBitBoard();

            _whiteCandidatePawnsAttackFront[i] = _moveProvider.GetAttackPattern(WhitePawn, i);
            _whiteCandidatePawnsAttackBack[i] = _moveProvider.GetAttackPattern(BlackPawn, i);
            _blackCandidatePawnsAttackBack[i] = _moveProvider.GetAttackPattern(WhitePawn, i);
            _blackCandidatePawnsAttackFront[i] = _moveProvider.GetAttackPattern(BlackPawn, i);

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
            _whiteBackwardPawns[i] = new List<KeyValuePair<BitBoard, BitBoard>>();
            _blackBackwardPawns[i] = new List<KeyValuePair<BitBoard, BitBoard>>();
        }

        for (byte i = 8; i < 16; i++)
        {
            byte w = (byte)(i + 8);
            var bb = _moveProvider.GetAttackPattern(WhitePawn, w);
            var wb = _blackPassedPawns[w];
            for (int j = i; j >= 0; j -= 8)
            {
                wb ^= j.AsBitBoard();
            }

            _whiteBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));

            w = (byte)(i + 16);
            bb = _moveProvider.GetAttackPattern(WhitePawn, w);
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
            var bb = _moveProvider.GetAttackPattern(WhitePawn, w);
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
            var bb = _moveProvider.GetAttackPattern(BlackPawn, w);
            var wb = _whitePassedPawns[w];
            for (int j = i; j < 56; j += 8)
            {
                wb ^= j.AsBitBoard();
            }

            _blackBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));

            w = (byte)(i - 16);
            bb = _moveProvider.GetAttackPattern(BlackPawn, w);
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
            var bb = _moveProvider.GetAttackPattern(BlackPawn, w);
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
            _whiteKingShield[i] = _moveProvider.GetAttackPattern(WhiteKing, i);
            _blackKingShield[i] = _moveProvider.GetAttackPattern(BlackKing, i);
        }

        _whiteKingFace = new BitBoard[64];
        for (byte i = 0; i < 32; i++)
        {
            _whiteKingFace[i] = _moveProvider.GetAttackPattern(WhiteKing, i) &
                                _ranks[i / 8 + 1];
        }

        _blackKingFace = new BitBoard[64];
        for (byte i = 32; i < 64; i++)
        {
            _blackKingFace[i] = _moveProvider.GetAttackPattern(BlackKing, i) &
                                _ranks[i / 8 - 1];
        }

        _whiteKingFaceShield = new BitBoard[64];
        for (byte i = 0; i < 32; i++)
        {
            _whiteKingFaceShield[i] = _moveProvider.GetAttackPattern(WhiteKing, (byte)(i + 8)) &
                                      _ranks[i / 8 + 2];
        }

        _blackKingFaceShield = new BitBoard[64];
        for (byte i = 32; i < 64; i++)
        {
            _blackKingFaceShield[i] = _moveProvider.GetAttackPattern(BlackKing, (byte)(i - 8)) &
                                      _ranks[i / 8 - 2];
        }

        _whiteKingOpenFile = new BitBoard[64][];
        for (byte i = 0; i < _whiteKingOpenFile.Length; i++)
        {
            var rank = i % 8;
            if (rank == 0)
            {
                _whiteKingOpenFile[i] = new[] { _files[0] ^ i.AsBitBoard(), _files[1] };
            }
            else if (rank == 7)
            {
                _whiteKingOpenFile[i] = new[] { _files[7] ^ i.AsBitBoard(), _files[6] };
            }
            else
            {
                _whiteKingOpenFile[i] = new[]
                    {_files[rank - 1], _files[rank] ^ i.AsBitBoard(), _files[rank + 1]};
            }
        }

        _blackKingOpenFile = new BitBoard[64][];
        for (byte i = 0; i < _blackKingOpenFile.Length; i++)
        {
            var rank = i % 8;
            if (rank == 0)
            {
                _blackKingOpenFile[i] = new[] { _files[0] ^ i.AsBitBoard(), _files[1] };
            }
            else if (rank == 7)
            {
                _blackKingOpenFile[i] = new[] { _files[7] ^ i.AsBitBoard(), _files[6] };
            }
            else
            {
                _blackKingOpenFile[i] = new[]
                    {_files[rank - 1], _files[rank] ^ i.AsBitBoard(), _files[rank + 1]};
            }
        }

        SetWhitePawnStorm();

        SetBlackPawnStorm();
    }

    private void SetBlackPawnStorm()
    {
        _blackPawnStormFile4 = new BitBoard[64];
        _blackPawnStormFile5 = new BitBoard[64];
        _blackPawnStormFile6 = new BitBoard[64];
        for (byte i = 0; i < 64; i++)
        {
            if (i > 47)
            {
                var rank = i % 8;
                if (rank == 0)
                {
                    _blackPawnStormFile4[i] = (_files[0] | _files[1]) & _ranks[3];
                    _blackPawnStormFile5[i] = (_files[0] | _files[1]) & _ranks[4];
                    _blackPawnStormFile6[i] = (_files[0] | _files[1]) & _ranks[5];
                }
                else if (rank == 7)
                {
                    _blackPawnStormFile4[i] = (_files[6] | _files[7]) & _ranks[3];
                    _blackPawnStormFile5[i] = (_files[6] | _files[7]) & _ranks[4];
                    _blackPawnStormFile6[i] = (_files[6] | _files[7]) & _ranks[5];
                }
                else
                {
                    _blackPawnStormFile4[i] = (_files[rank - 1] | _files[rank] | _files[rank + 1]) & _ranks[3];
                    _blackPawnStormFile5[i] = (_files[rank - 1] | _files[rank] | _files[rank + 1]) & _ranks[4];
                    _blackPawnStormFile6[i] = (_files[rank - 1] | _files[rank] | _files[rank + 1]) & _ranks[5];
                }
            }
            else
            {
                _blackPawnStormFile4[i] = new BitBoard();
                _blackPawnStormFile5[i] = new BitBoard();
                _blackPawnStormFile6[i] = new BitBoard();
            }
        }
    }

    private void SetWhitePawnStorm()
    {
        _whitePawnStormFile4 = new BitBoard[64];
        _whitePawnStormFile5 = new BitBoard[64];
        _whitePawnStormFile6 = new BitBoard[64];
        for (byte i = 0; i < 64; i++)
        {
            if (i < 16)
            {
                var rank = i % 8;
                if (rank == 0)
                {
                    _whitePawnStormFile4[i] = (_files[0] | _files[1]) & _ranks[4];
                    _whitePawnStormFile5[i] = (_files[0] | _files[1]) & _ranks[3];
                    _whitePawnStormFile6[i] = (_files[0] | _files[1]) & _ranks[2];
                }
                else if (rank == 7)
                {
                    _whitePawnStormFile4[i] = (_files[6] | _files[7]) & _ranks[4];
                    _whitePawnStormFile5[i] = (_files[6] | _files[7]) & _ranks[3];
                    _whitePawnStormFile6[i] = (_files[6] | _files[7]) & _ranks[2];
                }
                else
                {
                    _whitePawnStormFile4[i] = (_files[rank - 1] | _files[rank] | _files[rank + 1]) & _ranks[4];
                    _whitePawnStormFile5[i] = (_files[rank - 1] | _files[rank] | _files[rank + 1]) & _ranks[3];
                    _whitePawnStormFile6[i] = (_files[rank - 1] | _files[rank] | _files[rank + 1]) & _ranks[2];
                }
            }
            else
            {
                _whitePawnStormFile4[i] = new BitBoard();
                _whitePawnStormFile5[i] = new BitBoard();
                _whitePawnStormFile6[i] = new BitBoard();
            }
        }
    }

    #endregion

    #region Implementation of Board

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackPawn(byte to) => (_whitePawnPatterns[to] & _boards[BlackPawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackKnight(byte to) => (_whiteKnightPatterns[to] & _boards[BlackKnight]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackBishop(byte to) => (to.BishopAttacks(~_empty) & _boards[BlackBishop]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhiteBishop(byte to) => (to.BishopAttacks(~_empty) & _boards[WhiteBishop]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhitePawn(byte to) => (_blackPawnPatterns[to] & _boards[WhitePawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhiteKnight(byte to) => (_blackKnightPatterns[to] & _boards[WhiteKnight]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookOnSeven(byte from, byte to) => (_ranks[6] & from.AsBitBoard()).IsZero() && (_ranks[6] & from.AsBitBoard()).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookOnSeven(byte from, byte to) => (_ranks[1] & from.AsBitBoard()).IsZero() && (_ranks[1] & from.AsBitBoard()).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDoubleBlackRook(byte from, byte to) => (from.RookAttacks(~_empty) & (_boards[BlackRook] | _boards[BlackQueen])).IsZero() &&
            (to.RookAttacks(~_empty) & (_boards[BlackRook] | _boards[BlackQueen])).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDoubleWhiteRook(byte from, byte to) => (from.RookAttacks(~_empty) & (_boards[WhiteRook] | _boards[WhiteQueen])).IsZero() &&
            (to.RookAttacks(~_empty) & (_boards[WhiteRook] | _boards[WhiteQueen])).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookOnOpenFile(byte from, byte to) => (_rookFiles[from] & (_boards[WhitePawn] | _boards[BlackPawn])).Any() && (_rookFiles[to] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookOnOpenFile(byte from, byte to) => (_rookFiles[from] & (_boards[WhitePawn] | _boards[BlackPawn])).Any() && (_rookFiles[to] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackCandidate(byte from, byte to)
    {
        if ((_blackFacing[from] & (_boards[WhitePawn] | _boards[BlackPawn])).Any()) return false;

        return (_blackCandidatePawnsFront[from] & _boards[WhitePawn]).Count() < (_blackCandidatePawnsBack[from] & _boards[BlackPawn]).Count() &&
                (_blackCandidatePawnsAttackFront[from] & _boards[WhitePawn]).Count() <= (_blackCandidatePawnsAttackBack[from] & _boards[BlackPawn]).Count()
                &&
                (_blackCandidatePawnsFront[to] & _boards[WhitePawn]).Count() < (_blackCandidatePawnsBack[to] & _boards[BlackPawn]).Count() &&
                (_blackCandidatePawnsAttackFront[to] & _boards[WhitePawn]).Count() <= (_blackCandidatePawnsAttackBack[to] & _boards[BlackPawn]).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteCandidate(byte from, byte to)
    {
        if ((_whiteFacing[from] & (_boards[WhitePawn] | _boards[BlackPawn])).Any()) return false;

        return (_whiteCandidatePawnsFront[from] & _boards[BlackPawn]).Count() < (_whiteCandidatePawnsBack[from] & _boards[WhitePawn]).Count() &&
                (_whiteCandidatePawnsAttackFront[from] & _boards[BlackPawn]).Count() <= (_whiteCandidatePawnsAttackBack[from] & _boards[WhitePawn]).Count()
                &&
                (_whiteCandidatePawnsFront[to] & _boards[BlackPawn]).Count() < (_whiteCandidatePawnsBack[to] & _boards[WhitePawn]).Count() &&
                (_whiteCandidatePawnsAttackFront[to] & _boards[BlackPawn]).Count() <= (_whiteCandidatePawnsAttackBack[to] & _boards[WhitePawn]).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackPawnStorm(byte from) => (_blackPassedPawns[from] & _boards[WhiteKing]).Any() && (_whitePassedPawns[from] & _boards[BlackKing]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhitePawnStorm(byte from) => (_whitePassedPawns[from] & _boards[BlackKing]).Any() && (_blackPassedPawns[from] & _boards[WhiteKing]).IsZero();

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

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWhite(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];
        _pieces[square] = piece;

        BitBoard bitBoard = square.AsBitBoard();

        _boards[piece] |= bitBoard;
        _whites |= bitBoard;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveWhite(byte piece, byte from, byte to)
    {
        _hash = _hash ^ _hashTable[from][piece] ^ _hashTable[to][piece];
        _pieces[to] = piece;

        BitBoard bitBoard = from.AsBitBoard() | to.AsBitBoard();

        _boards[piece] ^= bitBoard;
        _whites ^= bitBoard;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveBlack(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];

        var bit = ~square.AsBitBoard();

        _boards[piece] &= bit;
        _blacks &= bit;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBlack(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];
        _pieces[square] = piece;

        BitBoard bitBoard = square.AsBitBoard();

        _boards[piece] |= bitBoard;
        _blacks |= bitBoard;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveBlack(byte piece, byte from, byte to)
    {
        _hash = _hash ^ _hashTable[from][piece] ^ _hashTable[to][piece];
        _pieces[to] = piece;

        BitBoard bitBoard = from.AsBitBoard() | to.AsBitBoard();

        _boards[piece] ^= bitBoard;
        _blacks ^= bitBoard;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetWhiteKingPosition() => _boards[WhiteKing].BitScanForward();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBlackKingPosition() => _boards[BlackKing].BitScanForward();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PositionsList GetPiecePositions(byte index)
    {
        _boards[index].GetPositions(_positionList);
        return _positionList;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetSquares(byte index, SquareList squares) => _boards[index].GetPositions(squares);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhitePawnSquares(SquareList squares) => (_notRanks[6] & _boards[WhitePawn]).GetPositions(squares);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackPawnSquares(SquareList squares) => (_notRanks[1] & _boards[BlackPawn]).GetPositions(squares);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhitePromotionSquares(SquareList squares) => (_ranks[6] & _boards[WhitePawn]).GetPositions(squares);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackPromotionSquares(SquareList squares) => (_ranks[1] & _boards[BlackPawn]).GetPositions(squares);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetKey() => _hash;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetOccupied() => ~_empty;

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
    public BitBoard GetWhitePawnAttacks() => ((_boards[WhitePawn] & _notFileA) << 7) |
               ((_boards[WhitePawn] & _notFileH) << 9);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPawnAttacks() => ((_boards[BlackPawn] & _notFileA) >> 9) |
               ((_boards[BlackPawn] & _notFileH) >> 7);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte UpdatePhase()
    {
        var ply = _moveHistory.GetPly();
        _phase = ply < 16 ? Phase.Opening : ply > 35 && IsEndGame() ? Phase.End : Phase.Middle;
        return _phase;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEndGame() => IsEndGameForWhite() && IsEndGameForBlack();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEndGameForBlack()
    {
        var bqr = (_boards[BlackQueen] | _boards[BlackRook]).Count();

        if (bqr > 1) return false;

        return bqr == 1
            ? (_boards[BlackBishop] | _boards[BlackKnight]).Count() < 2
            : (_boards[BlackBishop] | _boards[BlackKnight]).Count() < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEndGameForWhite()
    {
        var wqr = (_boards[WhiteQueen] | _boards[WhiteRook]).Count();

        if (wqr > 1) return false;

        return wqr == 1
            ? (_boards[WhiteBishop] | _boards[WhiteKnight]).Count() < 2
            : (_boards[WhiteBishop] | _boards[WhiteKnight]).Count() < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanWhitePromote() => (_ranks[6] & _boards[WhitePawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanBlackPromote() => (_ranks[1] & _boards[BlackPawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetRank(int rank) => _ranks[rank];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPhase() => _phase;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackPass(byte position) => (_blackPassedPawns[position] & _boards[WhitePawn]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhitePass(byte position) => (_whitePassedPawns[position] & _boards[BlackPawn]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteOver(BitBoard opponentPawns) => (_boards[WhitePawn] & opponentPawns).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackOver(BitBoard opponentPawns) => (_boards[BlackPawn] & opponentPawns).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDraw() => (_boards[WhitePawn] | _boards[WhiteRook] | _boards[WhiteQueen] | _boards[BlackPawn] | _boards[BlackRook] | _boards[BlackQueen]).IsZero()
            && (_boards[WhiteKnight] | _boards[WhiteBishop]).Count() < 2 && (_boards[BlackKnight] | _boards[BlackBishop]).Count() < 2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackAttacksTo(byte to) => (_whiteKnightPatterns[to] & _boards[BlackKnight]).Any()
            || (to.BishopAttacks(~_empty) & (_boards[BlackBishop] | _boards[BlackQueen])).Any()
            || (to.RookAttacks(~_empty) & (_boards[BlackRook] | _boards[BlackQueen])).Any()
            || (_whitePawnPatterns[to] & _boards[BlackPawn]).Any()
            || (_whiteKingPatterns[to] & _boards[BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteAttacksTo(byte to) => (_blackKnightPatterns[to] & _boards[WhiteKnight]).Any()
        || (to.BishopAttacks(~_empty) & (_boards[WhiteBishop] | _boards[WhiteQueen])).Any()
        || (to.RookAttacks(~_empty) & (_boards[WhiteRook] | _boards[WhiteQueen])).Any()
        || (_blackPawnPatterns[to] & _boards[WhitePawn]).Any()
        || (_blackKingPatterns[to] & _boards[WhiteKing]).Any();

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
        _pieces[G1] = WhiteKing;
        _pieces[F1] = WhiteRook;

        _hash = _hash ^ _hashTable[H1][WhiteRook] ^ _hashTable[F1][WhiteRook];
        _hash = _hash ^ _hashTable[E1][WhiteKing] ^ _hashTable[G1][WhiteKing];

        _boards[WhiteKing] ^= _whiteSmallCastleKing;
        _boards[WhiteRook] ^= _whiteSmallCastleRook;

        _whites ^= _whiteSmallCastleKing;
        _whites ^= _whiteSmallCastleRook;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoBlackSmallCastle()
    {
        _pieces[G8] = BlackKing;
        _pieces[F8] = BlackRook;

        _hash = _hash ^ _hashTable[H8][BlackRook] ^ _hashTable[F8][BlackRook];
        _hash = _hash ^ _hashTable[E8][BlackKing] ^ _hashTable[G8][BlackKing];

        _boards[BlackKing] ^= _blackSmallCastleKing;
        _boards[BlackRook] ^= _blackSmallCastleRook;

        _blacks ^= _blackSmallCastleKing;
        _blacks ^= _blackSmallCastleRook;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoBlackBigCastle()
    {
        _pieces[C8] = BlackKing;
        _pieces[D8] = BlackRook;

        _hash = _hash ^ _hashTable[A8][BlackRook] ^ _hashTable[D8][BlackRook];
        _hash = _hash ^ _hashTable[E8][BlackKing] ^ _hashTable[C8][BlackKing];

        _boards[BlackKing] ^= _blackBigCastleKing;
        _boards[BlackRook] ^= _blackBigCastleRook;

        _blacks ^= _blackBigCastleKing;
        _blacks ^= _blackBigCastleRook;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoWhiteBigCastle()
    {
        _pieces[C1] = WhiteKing;
        _pieces[D1] = WhiteRook;

        _hash = _hash ^ _hashTable[A1][WhiteRook] ^ _hashTable[D1][WhiteRook];
        _hash = _hash ^ _hashTable[E1][WhiteKing] ^ _hashTable[C1][WhiteKing];

        _boards[WhiteKing] ^= _whiteBigCastleKing;
        _boards[WhiteRook] ^= _whiteBigCastleRook;

        _whites ^= _whiteBigCastleKing;
        _whites ^= _whiteBigCastleRook;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoWhiteSmallCastle()
    {
        _pieces[E1] = WhiteKing;
        _pieces[H1] = WhiteRook;

        _hash = _hash ^ _hashTable[F1][WhiteRook] ^ _hashTable[H1][WhiteRook];
        _hash = _hash ^ _hashTable[G1][WhiteKing] ^ _hashTable[E1][WhiteKing];

        _boards[WhiteKing] ^= _whiteSmallCastleKing;
        _boards[WhiteRook] ^= _whiteSmallCastleRook;

        _whites ^= _whiteSmallCastleKing;
        _whites ^= _whiteSmallCastleRook;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoBlackSmallCastle()
    {
        _pieces[E8] = BlackKing;
        _pieces[H8] = BlackRook;

        _hash = _hash ^ _hashTable[F8][BlackRook] ^ _hashTable[H8][BlackRook];
        _hash = _hash ^ _hashTable[G8][BlackKing] ^ _hashTable[E8][BlackKing];

        _boards[BlackKing] ^= _blackSmallCastleKing;
        _boards[BlackRook] ^= _blackSmallCastleRook;

        _blacks ^= _blackSmallCastleKing;
        _blacks ^= _blackSmallCastleRook;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoWhiteBigCastle()
    {
        _pieces[E1] = WhiteKing;
        _pieces[A1] = WhiteRook;

        _hash = _hash ^ _hashTable[D1][WhiteRook] ^ _hashTable[A1][WhiteRook];
        _hash = _hash ^ _hashTable[C1][WhiteKing] ^ _hashTable[E1][WhiteKing];

        _boards[WhiteKing] ^= _whiteBigCastleKing;
        _boards[WhiteRook] ^= _whiteBigCastleRook;

        _whites ^= _whiteBigCastleKing;
        _whites ^= _whiteBigCastleRook;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoBlackBigCastle()
    {
        _pieces[E8] = BlackKing;
        _pieces[A8] = BlackRook;

        _hash = _hash ^ _hashTable[D8][BlackRook] ^ _hashTable[A8][BlackRook];
        _hash = _hash ^ _hashTable[C8][BlackKing] ^ _hashTable[E8][BlackKing];

        _boards[BlackKing] ^= _blackBigCastleKing;
        _boards[BlackRook] ^= _blackBigCastleRook;

        _blacks ^= _blackBigCastleKing;
        _blacks ^= _blackBigCastleRook;

        _empty = ~(_whites | _blacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackSmallCastle() => _moveHistory.CanDoBlackSmallCastle() && _empty.IsSet(_blackSmallCastleCondition) && _boards[BlackRook].IsSet(BitBoards.H8) && CanDoBlackCastle(E8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteSmallCastle() => _moveHistory.CanDoWhiteSmallCastle() && _empty.IsSet(_whiteSmallCastleCondition) && _boards[WhiteRook].IsSet(BitBoards.H1) && CanDoWhiteCastle(E1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackBigCastle() => _moveHistory.CanDoBlackBigCastle() && _empty.IsSet(_blackBigCastleCondition) && _boards[BlackRook].IsSet(BitBoards.A8) && CanDoBlackCastle(E8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteBigCastle() => _moveHistory.CanDoWhiteBigCastle() && _empty.IsSet(_whiteBigCastleCondition) && _boards[WhiteRook].IsSet(BitBoards.A1) && CanDoWhiteCastle(E1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteCastle(byte to) => ((_whiteKnightPatterns[to] & _boards[BlackKnight])
                | (to.BishopAttacks(~_empty) & (_boards[BlackBishop] | _boards[BlackQueen]))
                | (to.RookAttacks(~_empty) & (_boards[BlackRook] | _boards[BlackQueen]))
                | (_whitePawnPatterns[to] & _boards[BlackPawn])).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackCastle(byte to) => ((_blackKnightPatterns[to] & _boards[WhiteKnight])
                | (to.BishopAttacks(~_empty) & (_boards[WhiteBishop] | _boards[WhiteQueen]))
                | (to.RookAttacks(~_empty) & (_boards[WhiteRook] | _boards[WhiteQueen]))
                | (_blackPawnPatterns[to] & _boards[WhitePawn])).IsZero();

    #endregion

    #region Evaluation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Evaluate()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();

        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        if (_phase == Phase.Opening)
            return EvaluateOpening();
        if (_phase == Phase.Middle)
            return EvaluateMiddle();
        return EvaluateEnd();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EvaluateOpposite()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();

        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        if (_phase == Phase.Opening)
            return EvaluateOpeningOpposite();
        if (_phase == Phase.Middle)
            return EvaluateMiddleOpposite();
        return EvaluateEndOpposite();
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

        if (_boards[WhiteKnight].Any())
            value += EvaluateWhiteKnightOpening();

        if (_boards[WhiteBishop].Any())
            value += EvaluateWhiteBishopOpening();

        if (_boards[WhiteRook].Any())
            value += EvaluateWhiteRookOpening();

        if (_boards[WhiteQueen].Any())
            value += EvaluateWhiteQueenOpening();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteMiddle()
    {
        var value = EvaluateWhitePawnMiddle() + EvaluateWhiteKingMiddle();

        if (_boards[WhiteKnight].Any())
            value += EvaluateWhiteKnightMiddle();

        if (_boards[WhiteBishop].Any())
            value += EvaluateWhiteBishopMiddle();

        if (_boards[WhiteRook].Any())
            value += EvaluateWhiteRookMiddle();

        if (_boards[WhiteQueen].Any())
            value += EvaluateWhiteQueenMiddle();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteEnd()
    {
        var value = EvaluateWhitePawnEnd() + EvaluateWhiteKingEnd();

        if (_boards[WhiteKnight].Any())
            value += EvaluateWhiteKnightEnd();

        if (_boards[WhiteBishop].Any())
            value += EvaluateWhiteBishopEnd();

        if (_boards[WhiteRook].Any())
            value += EvaluateWhiteRookEnd();

        if (_boards[WhiteQueen].Any())
            value += EvaluateWhiteQueenEnd();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenMobility(byte to) => _blackQueenPatterns[to]
            .Remove((_empty & to.QueenAttacks(~_empty)).Remove(_whitePawnAttacks))
            .Count() * _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookMobility(byte to) => _blackRookPatterns[to]
            .Remove((_empty & to.RookAttacks(~_empty)).Remove(_whitePawnAttacks))
            .Count() * _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopMobility(byte to) => _blackBishopPatterns[to]
            .Remove((_empty & to.BishopAttacks(~_empty)).Remove(_whitePawnAttacks))
            .Count() * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackKnightMobility(byte to)
    {
        var attackPattern = _blackKnightPatterns[to];
        return attackPattern.Remove((_empty & attackPattern)
            .Remove(_whitePawnAttacks))
            .Count() * _evaluationService.GetKnightMobilityValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenMobility(byte to) => _whiteQueenPatterns[to]
            .Remove((_empty & to.QueenAttacks(~_empty)).Remove(_blackPawnAttacks))
            .Count() * _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookMobility(byte to) => _whiteRookPatterns[to]
            .Remove((_empty & to.RookAttacks(~_empty)).Remove(_blackPawnAttacks))
            .Count() * _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopMobility(byte to) => _whiteBishopPatterns[to]
            .Remove((_empty & to.BishopAttacks(~_empty)).Remove(_blackPawnAttacks))
            .Count() * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteKnightMobility(byte to)
    {
        var attackPattern = _whiteKnightPatterns[to];
        return attackPattern.Remove((_empty & attackPattern)
            .Remove(_blackPawnAttacks))
            .Count() * _evaluationService.GetKnightMobilityValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhitePawnOpening()
    {
        int value = 0;

        BitList positions = stackalloc byte[8];

        _boards[WhitePawn].GetPositions(ref positions);

        for (byte i = 0; i < positions.Count; i++)
        {
            byte coordinate = positions[i];
            value += _evaluationService.GetFullValue(WhitePawn, coordinate);

            if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_whiteIsolatedPawns[coordinate] & _boards[WhitePawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }

            if ((_whiteDoublePawns[coordinate] & _boards[WhitePawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            for (byte c = 0; c < _whiteBackwardPawns[coordinate].Count; c++)
            {
                if ((_whiteBackwardPawns[coordinate][c].Key & _boards[WhitePawn]).IsZero() &&
                    (_whiteBackwardPawns[coordinate][c].Value & _boards[BlackPawn]).Any())
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                    break;
                }
            }
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
        _boards[WhiteBishop].GetPositions(_positionList);

        int value = 0;

        if (_positionList.Count > 1)
        {
            value += _evaluationService.GetDoubleBishopValue();
        }

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(WhiteBishop, coordinate);
            if ((_whiteMinorDefense[coordinate] & _boards[WhitePawn]).Any())
            {
                value += _evaluationService.GetMinorDefendedByPawnValue();
            }

            value -= GetWhiteBishopMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteRookOpening()
    {
        _boards[WhiteRook].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(WhiteRook, coordinate);

            if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                .IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();
            }
            else if ((_rookFiles[coordinate] & _boards[WhitePawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();
            }

            if (_boards[BlackQueen].Any() && _rookFiles[coordinate].IsSet(_boards[BlackQueen]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            if (_rookFiles[coordinate].IsSet(_boards[BlackKing]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            if ((coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any() && (_rookRanks[coordinate] & _boards[WhiteRook]).Any())
            {
                value += _evaluationService.GetDoubleRookHorizontalValue();
            }

            if ((_whiteRookKingPattern[coordinate] & _boards[WhiteKing]).Any() &&
                (_whiteRookPawnPattern[coordinate] & _boards[WhitePawn]).Any())
            {
                value -= _evaluationService.GetRookBlockedByKingValue();
            }

            value -= GetWhiteRookMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteRookMiddle()
    {
        _boards[WhiteRook].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(WhiteRook, coordinate);

            if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                .IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();
            }
            else if ((_rookFiles[coordinate] & _boards[WhitePawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();
            }

            if (_boards[BlackQueen].Any() && _rookFiles[coordinate].IsSet(_boards[BlackQueen]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            if (_rookFiles[coordinate].IsSet(_boards[BlackKing]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            if ((coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any() && (_rookFiles[coordinate] & _boards[WhiteRook]).Any())
            {
                value += _evaluationService.GetDoubleRookVerticalValue();
            }

            if ((coordinate.RookAttacks(~_empty) & _boards[WhiteQueen]).Any()
                && (_rookFiles[coordinate] & _boards[WhiteQueen]).Any())
            {
                value += _evaluationService.GetDoubleRookVerticalValue();
            }

            if ((_whiteRookKingPattern[coordinate] & _boards[WhiteKing]).Any() &&
                (_whiteRookPawnPattern[coordinate] & _boards[WhitePawn]).Any())
            {
                value -= _evaluationService.GetRookBlockedByKingValue();
            }

            value -= GetWhiteRookMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteRookEnd()
    {
        _boards[WhiteRook].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(WhiteRook, coordinate);

            if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                .IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();
            }
            else if ((_rookFiles[coordinate] & _boards[WhitePawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();
            }

            if ((coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any())
            {
                if ((_rookFiles[coordinate] & _boards[WhiteRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookVerticalValue();
                }
                else if ((_rookRanks[coordinate] & _boards[WhiteRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookHorizontalValue();
                }
            }

            if ((coordinate.RookAttacks(~_empty) & _boards[WhiteQueen]).Any()
                && (_rookFiles[coordinate] & _boards[WhiteQueen]).Any())
            {
                value += _evaluationService.GetDoubleRookVerticalValue();
            }

            value -= GetWhiteRookMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteQueenOpening()
    {
        _boards[WhiteQueen].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(WhiteQueen, coordinate);

            if (_whiteQueenPatterns[coordinate].IsSet(_boards[BlackKing]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            //value -= GetWhiteQueenMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteQueenMiddle()
    {
        _boards[WhiteQueen].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(WhiteQueen, coordinate);

            if (_whiteQueenPatterns[coordinate].IsSet(_boards[BlackKing]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            if ((coordinate.BishopAttacks(~_empty) & _boards[WhiteBishop]).Any())
            {
                value += _evaluationService.GetBattaryValue();
            }

            value -= GetWhiteQueenMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteQueenEnd()
    {
        _boards[WhiteQueen].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(WhiteQueen, coordinate);

            value -= GetWhiteQueenMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingOpening()
    {
        var kingPosition = _boards[WhiteKing].BitScanForward();
        return _evaluationService.GetFullValue(WhiteKing, kingPosition) + WhiteOpeningKingSafety(kingPosition) - WhitePawnStorm(kingPosition)
            + WhiteDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingMiddle()
    {
        var kingPosition = _boards[WhiteKing].BitScanForward();
        return _evaluationService.GetFullValue(WhiteKing, kingPosition) + WhiteMiddleKingSafety(kingPosition) - WhitePawnStorm(kingPosition)
            + WhiteDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteDistanceToQueen(byte kingPosition)
    {
        if (_boards[BlackQueen].IsZero()) return _evaluationService.GetQueenDistanceToKingValue() * 14;

        BitList list = stackalloc byte[4];

        _boards[BlackQueen].GetPositions(ref list);

        short value = _evaluationService.GetDistance(kingPosition, list[0]);

        for (byte position = 1; position < list.Count; position++)
        {
            value -= _evaluationService.GetDistance(kingPosition, list[position]);
        }

        return _evaluationService.GetQueenDistanceToKingValue() * value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhitePawnStorm(byte kingPosition)
    {
        if (kingPosition < 16)
        {
            var value = (_whitePawnStormFile4[kingPosition] & _boards[BlackPawn]).Count() * _evaluationService.GetPawnStormValue4() +
                (_whitePawnStormFile5[kingPosition] & _boards[BlackPawn]).Count() * _evaluationService.GetPawnStormValue5() +
                (_whitePawnStormFile6[kingPosition] & _boards[BlackPawn]).Count() * _evaluationService.GetPawnStormValue6();
            return value;
        }

        return 10;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingEnd()
    {
        var kingPosition = _boards[WhiteKing].BitScanForward();
        return _evaluationService.GetFullValue(WhiteKing, kingPosition) - KingPawnTrofism(kingPosition)
            + WhiteDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackOpening()
    {
        var value = EvaluateBlackPawnOpening() + EvaluateBlackKingOpening();

        if (_boards[BlackKnight].Any())
            value += EvaluateBlackKnightOpening();

        if (_boards[BlackBishop].Any())
            value += EvaluateBlackBishopOpening();

        if (_boards[BlackRook].Any())
            value += EvaluateBlackRookOpening();

        if (_boards[BlackQueen].Any())
            value += EvaluateBlackQueenOpening();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackMiddle()
    {
        var value = EvaluateBlackPawnMiddle() + EvaluateBlackKingMiddle();

        if (_boards[BlackKnight].Any())
            value += EvaluateBlackKnightMiddle();

        if (_boards[BlackBishop].Any())
            value += EvaluateBlackBishopMiddle();

        if (_boards[BlackRook].Any())
            value += EvaluateBlackRookMiddle();

        if (_boards[BlackQueen].Any())
            value += EvaluateBlackQueenMiddle();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackEnd()
    {
        var value = EvaluateBlackPawnEnd() + EvaluateBlackKingEnd();

        if (_boards[BlackKnight].Any())
            value += EvaluateBlackKnightEnd();

        if (_boards[BlackBishop].Any())
            value += EvaluateBlackBishopEnd();

        if (_boards[BlackRook].Any())
            value += EvaluateBlackRookEnd();

        if (_boards[BlackQueen].Any())
            value += EvaluateBlackQueenEnd();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackPawnOpening()
    {
        int value = 0;

        BitList positions = stackalloc byte[8];

        _boards[BlackPawn].GetPositions(ref positions);

        for (byte i = 0; i < positions.Count; i++)
        {
            byte coordinate = positions[i];
            value += _evaluationService.GetFullValue(BlackPawn, coordinate);
            if ((_blackBlockedPawns[coordinate] & _whites).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_blackIsolatedPawns[coordinate] & _boards[BlackPawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }

            if ((_blackDoublePawns[coordinate] & _boards[BlackPawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            for (byte c = 0; c < _blackBackwardPawns[coordinate].Count; c++)
            {
                if ((_blackBackwardPawns[coordinate][c].Key & _boards[BlackPawn]).IsZero() &&
                    (_blackBackwardPawns[coordinate][c].Value & _boards[WhitePawn]).Any())
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                    break;
                }
            }
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
        _boards[BlackBishop].GetPositions(_positionList);

        int value = 0;
        if (_positionList.Count > 1)
        {
            value += _evaluationService.GetDoubleBishopValue();
        }

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(BlackBishop, coordinate);

            if ((_blackMinorDefense[coordinate] & _boards[BlackPawn]).Any())
            {
                value += _evaluationService.GetMinorDefendedByPawnValue();
            }

            value -= GetBlackBishopMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackRookOpening()
    {
        _boards[BlackRook].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(BlackRook, coordinate);

            if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                .IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();
            }
            else if ((_rookFiles[coordinate] & _boards[BlackPawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();
            }

            if (_boards[WhiteQueen].Any() && _rookFiles[coordinate].IsSet(_boards[WhiteQueen]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            if (_rookFiles[coordinate].IsSet(_boards[WhiteKing]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            if ((coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any() && (_rookRanks[coordinate] & _boards[BlackRook]).Any())
            {
                value += _evaluationService.GetDoubleRookHorizontalValue();
            }

            if ((_blackRookKingPattern[coordinate] & _boards[BlackKing]).Any() &&
                (_blackRookPawnPattern[coordinate] & _boards[BlackPawn]).Any())
            {
                value -= _evaluationService.GetRookBlockedByKingValue();
            }

            value -= GetBlackRookMobility(coordinate);

        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackRookMiddle()
    {
        _boards[BlackRook].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(BlackRook, coordinate);

            if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                .IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();
            }
            else if ((_rookFiles[coordinate] & _boards[BlackPawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();
            }

            if (_boards[WhiteQueen].Any() && _rookFiles[coordinate].IsSet(_boards[WhiteQueen]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            if (_rookFiles[coordinate].IsSet(_boards[WhiteKing]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            if ((coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any() && (_rookFiles[coordinate] & _boards[BlackRook]).Any())
            {
                value += _evaluationService.GetDoubleRookVerticalValue();
            }

            if ((coordinate.RookAttacks(~_empty) & _boards[BlackQueen]).Any()
                && (_rookFiles[coordinate] & _boards[BlackQueen]).Any())
            {
                value += _evaluationService.GetDoubleRookVerticalValue();
            }

            if ((_blackRookKingPattern[coordinate] & _boards[BlackKing]).Any() &&
                (_blackRookPawnPattern[coordinate] & _boards[BlackPawn]).Any())
            {
                value -= _evaluationService.GetRookBlockedByKingValue();
            }

            value -= GetBlackRookMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackRookEnd()
    {
        _boards[BlackRook].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(BlackRook, coordinate);

            if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                .IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();
            }
            else if ((_rookFiles[coordinate] & _boards[BlackPawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();
            }

            if ((coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any())
            {
                if ((_rookFiles[coordinate] & _boards[BlackRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookVerticalValue();
                }
                else if ((_rookRanks[coordinate] & _boards[BlackRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookHorizontalValue();
                }
            }

            if ((coordinate.RookAttacks(~_empty) & _boards[BlackQueen]).Any()
                && (_rookFiles[coordinate] & _boards[BlackQueen]).Any())
            {
                value += _evaluationService.GetDoubleRookVerticalValue();
            }

            value -= GetBlackRookMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackQueenOpening()
    {
        _boards[BlackQueen].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(BlackQueen, coordinate);

            if (_blackQueenPatterns[coordinate].IsSet(_boards[WhiteKing]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            //value -= GetBlackQueenMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackQueenMiddle()
    {
        _boards[BlackQueen].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(BlackQueen, coordinate);

            if (_blackQueenPatterns[coordinate].IsSet(_boards[WhiteKing]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            if ((coordinate.BishopAttacks(~_empty) & _boards[BlackBishop]).Any())
            {
                value += _evaluationService.GetBattaryValue();
            }

            value -= GetBlackQueenMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackQueenEnd()
    {
        _boards[BlackQueen].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(BlackQueen, coordinate);

            value -= GetBlackQueenMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingOpening()
    {
        var kingPosition = _boards[BlackKing].BitScanForward();
        return _evaluationService.GetFullValue(BlackKing, kingPosition) + BlackOpeningKingSafety(kingPosition) - BlackPawnStorm(kingPosition) + BlackDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingMiddle()
    {
        var kingPosition = _boards[BlackKing].BitScanForward();
        return _evaluationService.GetFullValue(BlackKing, kingPosition) + BlackMiddleKingSafety(kingPosition) - BlackPawnStorm(kingPosition) + BlackDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackPawnStorm(byte kingPosition)
    {
        if (kingPosition > 47)
        {
            var value = (_blackPawnStormFile4[kingPosition] & _boards[WhitePawn]).Count() * _evaluationService.GetPawnStormValue4() +
                (_blackPawnStormFile5[kingPosition] & _boards[WhitePawn]).Count() * _evaluationService.GetPawnStormValue5() +
                (_blackPawnStormFile6[kingPosition] & _boards[WhitePawn]).Count() * _evaluationService.GetPawnStormValue6();
            return value;
        }

        return 10;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackDistanceToQueen(byte kingPosition)
    {
        if (_boards[WhiteQueen].IsZero()) return _evaluationService.GetQueenDistanceToKingValue() * 14;

        BitList list = stackalloc byte[4];

        _boards[WhiteQueen].GetPositions(ref list);

        short value = _evaluationService.GetDistance(kingPosition, list[0]);

        for (byte position = 1; position < list.Count; position++)
        {
            value -= _evaluationService.GetDistance(kingPosition, list[position]);
        }

        return _evaluationService.GetQueenDistanceToKingValue() * value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingEnd()
    {
        var kingPosition = _boards[BlackKing].BitScanForward();
        return _evaluationService.GetFullValue(BlackKing, kingPosition) - KingPawnTrofism(kingPosition) + BlackDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetStaticValue()
    {
        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        return GetWhiteStaticValue() - GetBlackStaticValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetKingSafetyValue()
    {
        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        return WhiteMiddleKingSafety(_boards[5].BitScanForward()) - BlackMiddleKingSafety(_boards[11].BitScanForward());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPawnValue()
    {
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
    private short KingPawnTrofism(byte kingPosition)
    {
        BitList positions = stackalloc byte[16];

        (_boards[0] | _boards[6]).GetPositions(ref positions);

        return _evaluationService.Distance(kingPosition, positions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackOpeningKingSafety(byte kingPosition) => BlackKingShieldOpeningValue(kingPosition) - BlackKingAttackValue(kingPosition) - BlackKingOpenValue(kingPosition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackMiddleKingSafety(byte kingPosition) => BlackKingShieldMiddleValue(kingPosition) - BlackKingAttackValue(kingPosition) - BlackKingOpenValue(kingPosition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private short BlackKingOpenValue(byte kingPosition)
    {
        short value = 0;
        var boards = _blackKingOpenFile[kingPosition];
        for (byte i = 0; i < boards.Length; i++)
        {
            if ((boards[i] & _blacks).IsZero())
            {
                value += _evaluationService.GetKingZoneOpenFileValue();
            }
        }
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackKingAttackValue(byte kingPosition)
    {
        byte attackingPiecesCount = 0;
        int valueOfAttacks = 0;
        var shield = _blackKingShield[kingPosition];
        BitList positions = stackalloc byte[4];

        _boards[WhiteKnight].GetPositions(ref positions);
        for (byte i = 0; i < positions.Count; i++)
        {
            var attackPattern = _whiteKnightPatterns[positions[i]] & shield;
            if (!attackPattern.Any()) continue;

            attackingPiecesCount++;
            valueOfAttacks += attackPattern.Count() * _evaluationService.GetKnightAttackValue();
        }

        _boards[WhiteBishop].GetPositions(ref positions);
        for (byte i = 0; i < positions.Count; i++)
        {
            var bishopAttacks = positions[i].BishopAttacks(~_empty) & shield;
            if (bishopAttacks.Any())
            {
                attackingPiecesCount++;
                valueOfAttacks += bishopAttacks.Count() * _evaluationService.GetBishopAttackValue();
            }
        }

        _boards[WhiteRook].GetPositions(ref positions);
        for (byte i = 0; i < positions.Count; i++)
        {
            var rookAttacks = positions[i].RookAttacks(~_empty) & shield;
            if (rookAttacks.Any())
            {
                attackingPiecesCount++;
                valueOfAttacks += rookAttacks.Count() * _evaluationService.GetRookAttackValue();
            }
        }

        _boards[WhiteQueen].GetPositions(ref positions);
        for (byte i = 0; i < positions.Count; i++)
        {
            var queenAttacks = positions[i].QueenAttacks(~_empty) & shield;
            if (queenAttacks.Any())
            {
                attackingPiecesCount++;
                valueOfAttacks += queenAttacks.Count() * _evaluationService.GetQueenAttackValue();
            }
        }

        if (attackingPiecesCount < 2) return 0;

        return Round(valueOfAttacks * _evaluationService.GetAttackWeight(attackingPiecesCount));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackKingShieldOpeningValue(byte kingPosition)
    {
        if (_moveHistory.CanDoBlackCastle()) return 0;

        return _evaluationService.GetKingShieldFaceValue() * (_blackKingFace[kingPosition] & _blacks).Count() +
            _evaluationService.GetKingShieldPreFaceValue() * (_blackKingFaceShield[kingPosition] & _blacks).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int BlackKingShieldMiddleValue(byte kingPosition) => _evaluationService.GetKingShieldFaceValue() * (_blackKingFace[kingPosition] & _blacks).Count() +
            _evaluationService.GetKingShieldPreFaceValue() * (_blackKingFaceShield[kingPosition] & _blacks).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopValue()
    {
        _boards[BlackBishop].GetPositions(_positionList);

        int value = 0;
        if (_positionList.Count > 1)
        {
            value += _evaluationService.GetDoubleBishopValue();
        }

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(BlackBishop, coordinate);

            if ((_blackMinorDefense[coordinate] & _boards[BlackPawn]).Any())
            {
                value += _evaluationService.GetMinorDefendedByPawnValue();
            }

            if (_boards[WhiteQueen].Any() && _blackBishopPatterns[coordinate].IsSet(_boards[WhiteQueen]))
            {
                value += _evaluationService.GetRentgenValue();
            }
            if (_blackBishopPatterns[coordinate].IsSet(_boards[WhiteKing]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            value -= GetBlackBishopMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackKnightValue()
    {
        _boards[BlackKnight].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(BlackKnight, coordinate);

            if ((_blackMinorDefense[coordinate] & _boards[BlackPawn]).Any())
            {
                value += _evaluationService.GetMinorDefendedByPawnValue();
            }

            value -= GetBlackKnightMobility(coordinate);
        }
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackPawnValue()
    {
        if (_boards[BlackPawn].IsZero())
        {
            return _evaluationService.GetNoPawnsValue();
        }

        int value = 0;

        BitList positions = stackalloc byte[8];

        _boards[BlackPawn].GetPositions(ref positions);

        for (byte i = 0; i < positions.Count; i++)
        {
            byte coordinate = positions[i];
            value += _evaluationService.GetFullValue(BlackPawn, coordinate);
            if ((_blackBlockedPawns[coordinate] & _whites).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_blackIsolatedPawns[coordinate] & _boards[BlackPawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }

            if ((_blackDoublePawns[coordinate] & _boards[BlackPawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_blackFacing[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero())
            {
                if ((_blackPassedPawns[coordinate] & _boards[WhitePawn]).IsZero())
                {
                    var pp = _evaluationService.GetBlackPassedPawnValue(coordinate);
                    if (pp > 0)
                    {
                        value += pp;
                        if ((_blackCandidatePawnsAttackBack[coordinate] & _boards[BlackPawn]).Any())
                        {
                            value += _evaluationService.GetProtectedPassedPawnValue();
                        }
                    }
                }
                else if ((_blackCandidatePawnsFront[coordinate] & _boards[WhitePawn]).Count() < (_blackCandidatePawnsBack[coordinate] & _boards[BlackPawn]).Count() &&
                    (_blackCandidatePawnsAttackFront[coordinate] & _boards[WhitePawn]).Count() <= (_blackCandidatePawnsAttackBack[coordinate] & _boards[BlackPawn]).Count())
                {
                    value += _evaluationService.GetBlackCandidatePawnValue(coordinate);
                }
                else
                {
                    value += _evaluationService.GetOpenPawnValue();
                }
            }

            for (byte c = 0; c < _blackBackwardPawns[coordinate].Count; c++)
            {
                if ((_blackBackwardPawns[coordinate][c].Key & _boards[BlackPawn]).IsZero() &&
                    (_blackBackwardPawns[coordinate][c].Value & _boards[WhitePawn]).Any())
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                    break;
                }
            }
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteOpeningKingSafety(byte kingPosition) => WhiteKingShieldOpeningValue(kingPosition) - WhiteKingAttackValue(kingPosition) - WhiteKingOpenValue(kingPosition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteMiddleKingSafety(byte kingPosition) => WhiteKingShieldMiddleValue(kingPosition) - WhiteKingAttackValue(kingPosition) - WhiteKingOpenValue(kingPosition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingOpenValue(byte kingPosition)
    {
        short value = 0;
        var boards = _whiteKingOpenFile[kingPosition];
        for (byte i = 0; i < boards.Length; i++)
        {
            if ((boards[i] & _whites).IsZero())
            {
                value += _evaluationService.GetKingZoneOpenFileValue();
            }
        }
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingAttackValue(byte kingPosition)
    {
        byte attackingPiecesCount = 0;
        int valueOfAttacks = 0;
        var shield = _whiteKingShield[kingPosition];
        BitList positions = stackalloc byte[4];

        _boards[BlackKnight].GetPositions(ref positions);
        for (byte i = 0; i < positions.Count; i++)
        {
            var attackPattern = _blackKnightPatterns[positions[i]] & shield;
            if (!attackPattern.Any()) continue;

            attackingPiecesCount++;
            valueOfAttacks += attackPattern.Count() * _evaluationService.GetKnightAttackValue();
        }

        _boards[BlackBishop].GetPositions(ref positions);
        for (byte i = 0; i < positions.Count; i++)
        {
            var bishopAttacks = positions[i].BishopAttacks(~_empty) & shield;
            if (bishopAttacks.Any())
            {
                attackingPiecesCount++;
                valueOfAttacks += bishopAttacks.Count() * _evaluationService.GetBishopAttackValue();
            }
        }

        _boards[BlackRook].GetPositions(ref positions);
        for (byte i = 0; i < positions.Count; i++)
        {
            var rookAttacks = positions[i].RookAttacks(~_empty) & shield;
            if (rookAttacks.Any())
            {
                attackingPiecesCount++;
                valueOfAttacks += rookAttacks.Count() * _evaluationService.GetRookAttackValue();
            }
        }

        _boards[BlackQueen].GetPositions(ref positions);
        for (byte i = 0; i < positions.Count; i++)
        {
            var queenAttacks = positions[i].QueenAttacks(~_empty) & shield;
            if (queenAttacks.Any())
            {
                attackingPiecesCount++;
                valueOfAttacks += queenAttacks.Count() * _evaluationService.GetQueenAttackValue();
            }
        }

        if (attackingPiecesCount < 2) return 0;

        return Round(valueOfAttacks * _evaluationService.GetAttackWeight(attackingPiecesCount));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingShieldOpeningValue(byte kingPosition)
    {
        if (_moveHistory.CanDoWhiteCastle()) return 0;

        return _evaluationService.GetKingShieldFaceValue() * (_whiteKingFace[kingPosition] & _whites).Count()
            + _evaluationService.GetKingShieldPreFaceValue() * (_whiteKingFaceShield[kingPosition] & _whites).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingShieldMiddleValue(byte kingPosition) => _evaluationService.GetKingShieldFaceValue() * (_whiteKingFace[kingPosition] & _whites).Count()
            + _evaluationService.GetKingShieldPreFaceValue() * (_whiteKingFaceShield[kingPosition] & _whites).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopValue()
    {
        _boards[WhiteBishop].GetPositions(_positionList);

        int value = 0;

        if (_positionList.Count > 1)
        {
            value += _evaluationService.GetDoubleBishopValue();
        }

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(WhiteBishop, coordinate);
            if ((_whiteMinorDefense[coordinate] & _boards[WhitePawn]).Any())
            {
                value += _evaluationService.GetMinorDefendedByPawnValue();
            }

            if (_boards[BlackQueen].Any() && _whiteBishopPatterns[coordinate].IsSet(_boards[BlackQueen]))
            {
                value += _evaluationService.GetRentgenValue();
            }
            if (_whiteBishopPatterns[coordinate].IsSet(_boards[BlackKing]))
            {
                value += _evaluationService.GetRentgenValue();
            }

            value -= GetWhiteBishopMobility(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteKnightValue()
    {
        _boards[WhiteKnight].GetPositions(_positionList);

        int value = 0;

        for (byte i = 0; i < _positionList.Count; i++)
        {
            byte coordinate = _positionList[i];
            value += _evaluationService.GetFullValue(WhiteKnight, coordinate);
            if ((_whiteMinorDefense[coordinate] & _boards[WhitePawn]).Any())
            {
                value += _evaluationService.GetMinorDefendedByPawnValue();
            }

            value -= GetWhiteKnightMobility(coordinate);
        }
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhitePawnValue()
    {
        if (_boards[WhitePawn].IsZero())
        {
            return _evaluationService.GetNoPawnsValue();
        }

        int value = 0;

        BitList positions = stackalloc byte[8];

        _boards[WhitePawn].GetPositions(ref positions);

        for (byte i = 0; i < positions.Count; i++)
        {
            byte coordinate = positions[i];
            value += _evaluationService.GetFullValue(WhitePawn, coordinate);

            if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_whiteIsolatedPawns[coordinate] & _boards[WhitePawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }

            if ((_whiteDoublePawns[coordinate] & _boards[WhitePawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_whiteFacing[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero())
            {
                if ((_whitePassedPawns[coordinate] & _boards[BlackPawn]).IsZero())
                {
                    var pp = _evaluationService.GetWhitePassedPawnValue(coordinate);
                    if (pp > 0)
                    {
                        value += pp;
                        if ((_whiteCandidatePawnsAttackBack[coordinate] & _boards[WhitePawn]).Any())
                        {
                            value += _evaluationService.GetProtectedPassedPawnValue();
                        }
                    }
                }
                else if ((_whiteCandidatePawnsFront[coordinate] & _boards[BlackPawn]).Count() < (_whiteCandidatePawnsBack[coordinate] & _boards[WhitePawn]).Count() &&
                    (_whiteCandidatePawnsAttackFront[coordinate] & _boards[BlackPawn]).Count() <= (_whiteCandidatePawnsAttackBack[coordinate] & _boards[WhitePawn]).Count())
                {
                    value += _evaluationService.GetWhiteCandidatePawnValue(coordinate);
                }
                else
                {
                    value += _evaluationService.GetOpenPawnValue();
                }
            }

            for (byte c = 0; c < _whiteBackwardPawns[coordinate].Count; c++)
            {
                if ((_whiteBackwardPawns[coordinate][c].Key & _boards[WhitePawn]).IsZero() &&
                    (_whiteBackwardPawns[coordinate][c].Value & _boards[BlackPawn]).Any())
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                    break;
                }
            }
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Round(double v) => (int)Math.Round(v, 0, MidpointRounding.AwayFromZero);

    #endregion

    #region Private

    private void SetBoards()
    {
        _boards = new BitBoard[12];
        _boards[WhitePawn] = _boards[WhitePawn].Set(Enumerable.Range(8, 8).ToArray());
        _boards[WhiteKnight] = _boards[WhiteKnight].Set(1, 6);
        _boards[WhiteBishop] = _boards[WhiteBishop].Set(2, 5);
        _boards[WhiteRook] = _boards[WhiteRook].Set(0, 7);
        _boards[WhiteQueen] = _boards[WhiteQueen].Set(3);
        _boards[WhiteKing] = _boards[WhiteKing].Set(4);

        _whites = _boards[WhitePawn] |
                  _boards[WhiteKnight] |
                  _boards[WhiteBishop] |
                  _boards[WhiteRook] |
                  _boards[WhiteQueen] |
                  _boards[WhiteKing];

        _boards[BlackPawn] =
            _boards[BlackPawn].Set(Enumerable.Range(48, 8).ToArray());
        _boards[BlackRook] = _boards[BlackRook].Set(56, 63);
        _boards[BlackKnight] = _boards[BlackKnight].Set(57, 62);
        _boards[BlackBishop] = _boards[BlackBishop].Set(58, 61);
        _boards[BlackQueen] = _boards[BlackQueen].Set(59);
        _boards[BlackKing] = _boards[BlackKing].Set(60);

        _blacks = _boards[BlackPawn] |
                  _boards[BlackRook] |
                  _boards[BlackKnight] |
                  _boards[BlackBishop] |
                  _boards[BlackQueen] |
                  _boards[BlackKing];

        _empty = ~(_whites | _blacks);

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
        BitBoard rank = new BitBoard(0);
        rank = rank.Set(Enumerable.Range(0, 8).ToArray());
        _ranks = new BitBoard[8];
        _notRanks = new BitBoard[8];
        for (var i = 0; i < _ranks.Length; i++)
        {
            _ranks[i] = rank;
            rank = rank << 8;
        }

        _files = new BitBoard[8];
        BitBoard file = new BitBoard(0);
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

        for (int i = 0; i < _notRanks.Length; i++)
        {
            _notRanks[i] = ~_ranks[i];
        }
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

        StringBuilder builder = new StringBuilder();
        for (byte y = 7; y >= 0; y--)
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
}
