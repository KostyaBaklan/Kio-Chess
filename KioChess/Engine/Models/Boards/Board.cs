using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Services.Evaluation;

namespace Engine.Models.Boards;
public class DuplicateKeyComparer<TKey>
            :
         IComparer<TKey> where TKey : IComparable
{
    #region IComparer<TKey> Members

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        return result == 0 ? -1 : result;
    }

    #endregion
}
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

    private static byte One = 1;
    private static byte Zero = 0;

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

    private PositionsList _positionList;
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

        var round = new int[] { 0, -1, -2, 2, 1, 0, -1, -2, 2, 1 };
        _round = Enumerable.Range(0, 2000).Select(i =>
        {
            return i + round[i % 10];
        }).ToArray();

        MoveBase.Board = this;

        SetBoards();

        SetFilesAndRanks();

        SetCastles();

        _moveProvider = ServiceLocator.Current.GetInstance<MoveProvider>();
        _moveHistory = ServiceLocator.Current.GetInstance<MoveHistoryService>();
        _evaluationServiceFactory = ServiceLocator.Current.GetInstance<IEvaluationServiceFactory>();
        _attackEvaluationService = new AttackEvaluationService(_evaluationServiceFactory, _moveProvider);
        _attackEvaluationService.SetBoard(this);
        _moveHistory.SetBoard(this);

        _trofismCoefficient = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
            .Evaluation.Static.KingSafety.TrofismCoefficientValue;

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

        for (byte i = 0; i < 48; i++)
        {
            BitBoard b = new BitBoard();
            for (byte j = (byte)(i + 8); j < 56; j += 8)
            {
                b |= j.AsBitBoard();
            }
            _whiteFacing[i] = b;
        }
        for (byte i = 16; i < 64; i++)
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
                _whiteKingOpenFile[i] = new BitBoard[2];

                BitBoard b = new BitBoard();
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

                BitBoard b = new BitBoard();
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

                BitBoard b = new BitBoard();
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

                BitBoard b = new BitBoard();
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

                BitBoard b = new BitBoard();
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

                BitBoard b = new BitBoard();
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

        SetWhitePawnStorm();

        SetBlackPawnStorm();

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

    #region Implementation of IBoard

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopBattary(byte to)
    {
        if (_boards[WhiteQueen].IsZero()) return false;

        var pattern = _whiteBishopPatterns[to] & _blackKingPatterns[_boards[BlackKing].BitScanForward()];

        return pattern.Any() && (to.XrayBishopAttacks(~_empty, _boards[WhiteQueen]) & pattern).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteRookBattary(byte to)
    {
        var pattern = _whiteRookPatterns[to] & _blackKingPatterns[_boards[BlackKing].BitScanForward()];

        return pattern.Any() && _boards[WhiteQueen].Any() && (to.XrayRookAttacks(~_empty, _boards[WhiteQueen]) & pattern).Any() || (_boards[WhiteRook].Count() > 1 && (to.XrayRookAttacks(~_empty, _boards[WhiteRook]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteQueenBattary(byte to)
    {
        var pattern = _whiteQueenPatterns[to] & _blackKingPatterns[_boards[BlackKing].BitScanForward()];

        return pattern.Any() && _boards[WhiteRook].Any() && (to.XrayRookAttacks(~_empty, _boards[WhiteRook]) & pattern).Any() || (_boards[WhiteBishop].Any() && (to.XrayBishopAttacks(~_empty, _boards[WhiteBishop]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopBattary(byte to)
    {
        if (_boards[BlackQueen].IsZero()) return false;

        var pattern = _blackBishopPatterns[to] & _whiteKingPatterns[_boards[WhiteKing].BitScanForward()];

        return pattern.Any() && (to.XrayBishopAttacks(~_empty, _boards[BlackQueen]) & pattern).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackQueenBattary(byte to)
    {
        var pattern = _blackQueenPatterns[to] & _whiteKingPatterns[_boards[WhiteKing].BitScanForward()];

        return pattern.Any() && _boards[BlackRook].Any() && (to.XrayRookAttacks(~_empty, _boards[BlackRook]) & pattern).Any() || (_boards[BlackBishop].Any() && (to.XrayBishopAttacks(~_empty, _boards[BlackBishop]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackRookBattary(byte to)
    {
        var pattern = _blackRookPatterns[to] & _whiteKingPatterns[_boards[WhiteKing].BitScanForward()];

        return pattern.Any() && _boards[BlackQueen].Any() && (to.XrayRookAttacks(~_empty, _boards[BlackQueen]) & pattern).Any() || (_boards[BlackRook].Count() > 1 && (to.XrayRookAttacks(~_empty, _boards[BlackRook]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackQueenPin(byte to) => (to.XrayRookAttacks(~_empty, _whites) & (_boards[WhiteKing])).Any()
            || (to.XrayRookAttacks(~_empty, _blacks.Remove(_boards[BlackPawn])) & _boards[WhiteKing]).Any()
            || (to.XrayBishopAttacks(~_empty, _whites) & (_boards[WhiteKing])).Any()
            || (to.XrayBishopAttacks(~_empty, _blacks.Remove(_boards[BlackPawn])) & _boards[WhiteKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteQueenPin(byte to) => (to.XrayRookAttacks(~_empty, _blacks) & (_boards[BlackKing])).Any()
            || (to.XrayRookAttacks(~_empty, _whites.Remove(_boards[WhitePawn])) & _boards[BlackKing]).Any()
            || (to.XrayBishopAttacks(~_empty, _blacks) & (_boards[BlackKing])).Any()
            || (to.XrayBishopAttacks(~_empty, _whites.Remove(_boards[WhitePawn])) & _boards[BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackRookPin(byte to) => (to.XrayRookAttacks(~_empty, _whites) & (_boards[WhiteKing] | _boards[WhiteQueen])).Any() || (to.XrayRookAttacks(~_empty, _blacks.Remove(_boards[BlackPawn])) & _boards[WhiteKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteRookPin(byte to) => (to.XrayRookAttacks(~_empty, _blacks) & (_boards[BlackKing] | _boards[BlackQueen])).Any() || (to.XrayRookAttacks(~_empty, _whites.Remove(_boards[WhitePawn])) & _boards[BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopPin(byte to) => (to.XrayBishopAttacks(~_empty, _whites) & (_boards[WhiteKing] | _boards[WhiteQueen] | _boards[WhiteRook])).Any() || (to.XrayBishopAttacks(~_empty, _blacks.Remove(_boards[BlackPawn])) & _boards[WhiteKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopPin(byte to) => (to.XrayBishopAttacks(~_empty, _blacks) & (_boards[BlackKing] | _boards[BlackQueen] | _boards[BlackRook])).Any() || (to.XrayBishopAttacks(~_empty, _whites.Remove(_boards[WhitePawn])) & _boards[BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopAttacksHardPiece(byte to) => (to.BishopAttacks(~_empty) & (_boards[WhiteRook] | _boards[WhiteQueen])).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackKnightAttacksHardPiece(byte to) => (_blackKnightPatterns[to] & (_boards[WhiteRook] | _boards[WhiteQueen])).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopAttacksHardPiece(byte to) => (to.BishopAttacks(~_empty) & (_boards[BlackRook] | _boards[BlackQueen])).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteKnightAttacksHardPiece(byte to) => (_whiteKnightPatterns[to] & (_boards[BlackRook] | _boards[BlackQueen])).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackPawnFork(byte to) => (_blackPawnPatterns[to] & _whites.Remove(_boards[WhitePawn])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhitePawnFork(byte to) => (_whitePawnPatterns[to] & _blacks.Remove(_boards[BlackPawn])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackKnightFork(byte to) => (_blackKnightPatterns[to] & (_boards[WhiteRook] | _boards[WhiteQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopFork(byte to) => (to.BishopAttacks(_empty) & (_boards[BlackRook] | _boards[BlackQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopFork(byte to) => (to.BishopAttacks(_empty) & (_boards[WhiteRook] | _boards[WhiteQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteKnightFork(byte to) => (_whiteKnightPatterns[to] & (_boards[BlackRook] | _boards[BlackQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[WhiteKing].BitScanForward()];

        return (from.BishopAttacks(~_empty) & shield).Count() < (to.BishopAttacks(~_empty) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackKnightAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[WhiteKing].BitScanForward()];

        return (_blackKnightPatterns[from] & shield).Count() < (_blackKnightPatterns[to] & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[BlackKing].BitScanForward()];

        return (from.BishopAttacks(~_empty) & shield).Count() < (to.BishopAttacks(~_empty) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteKnightAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[BlackKing].BitScanForward()];

        return (_whiteKnightPatterns[from] & shield).Count() < (_whiteKnightPatterns[to] & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[BlackKing].BitScanForward()];

        return (from.RookAttacks(~_empty) & shield).Count() < (to.RookAttacks(~_empty) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[WhiteKing].BitScanForward()];

        return (from.RookAttacks(~_empty) & shield).Count() < (to.RookAttacks(~_empty) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteQueenAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[BlackKing].BitScanForward()];

        return (from.QueenAttacks(~_empty) & shield).Count() < (to.QueenAttacks(~_empty) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackQueenAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[WhiteKing].BitScanForward()];

        return (from.QueenAttacks(~_empty) & shield).Count() < (to.QueenAttacks(~_empty) & shield).Count();
    }

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
    public bool IsWhiteRookOnSeven(byte from, byte to) => (_ranks[6] & from.AsBitBoard()).IsZero() && (_ranks[6] & to.AsBitBoard()).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookOnSeven(byte from, byte to) => (_ranks[1] & from.AsBitBoard()).IsZero() && (_ranks[1] & to.AsBitBoard()).Any();

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
    private bool IsLateEndGameForBlack() => (_boards[BlackQueen] | _boards[BlackRook]).IsZero() && (_boards[BlackKnight] | _boards[BlackBishop]).Count() < 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateEndGameForWhite() => (_boards[WhiteQueen] | _boards[WhiteRook]).IsZero() && (_boards[WhiteKnight] | _boards[WhiteBishop]).Count() < 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLateMiddleGame() => IsLateMiddleGameForWhite() || IsLateMiddleGameForBlack();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateMiddleGameForBlack()
    {
        var wq = _boards[BlackQueen].Count();

        if (wq > 1) return (_boards[BlackRook] | _boards[BlackBishop] | _boards[BlackKnight]).IsZero();
        if (wq == 1) return (_boards[BlackRook] | _boards[BlackBishop] | _boards[BlackKnight]).Count() < 2;
        return (_boards[BlackRook] | _boards[BlackBishop] | _boards[BlackKnight]).Count() < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLateMiddleGameForWhite()
    {
        var wq = _boards[WhiteQueen].Count();

        if (wq > 1) return (_boards[WhiteRook] | _boards[WhiteBishop] | _boards[WhiteKnight]).IsZero();
        if (wq == 1) return (_boards[WhiteRook] | _boards[WhiteBishop] | _boards[WhiteKnight]).Count() < 2;
        return (_boards[WhiteRook] | _boards[WhiteBishop] | _boards[WhiteKnight]).Count() < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEndGame() => IsEndGameForWhite() || IsEndGameForBlack();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEndGameForBlack()
    {
        var bqr = (_boards[BlackQueen] | _boards[BlackRook]).Count();

        return bqr > 1
            ? false
            : bqr == 1
            ? (_boards[BlackBishop] | _boards[BlackKnight]).Count() < 2
            : (_boards[BlackBishop] | _boards[BlackKnight]).Count() < 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEndGameForWhite()
    {
        var wqr = (_boards[WhiteQueen] | _boards[WhiteRook]).Count();

        return wqr > 1
            ? false
            : wqr == 1
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
    public BitBoard GetFile(int file) => _files[file];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackPass(byte position) => (_blackPassedPawns[position] & _boards[WhitePawn]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhitePass(byte position) => (_whitePassedPawns[position] & _boards[BlackPawn]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteOver(BitBoard opponentPawns) => (_boards[WhitePawn] & opponentPawns).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackOver(BitBoard opponentPawns) => (_boards[BlackPawn] & opponentPawns).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDraw()
    {
        if ((_boards[WhitePawn] | _boards[WhiteRook] | _boards[WhiteQueen] | _boards[BlackPawn] | _boards[BlackRook] | _boards[BlackQueen]).Any())
            return false;

        if ((_boards[WhiteKnight] | _boards[WhiteBishop]).Count() < 2 && (_boards[BlackKnight] | _boards[BlackBishop]).Count() < 2)
            return true;

        if ((_boards[WhiteKnight] | _boards[WhiteBishop] | _boards[BlackBishop]).IsZero())
            return _boards[BlackKnight].Count() < 3;

        if ((_boards[BlackKnight] | _boards[WhiteBishop] | _boards[BlackBishop]).IsZero())
            return _boards[WhiteKnight].Count() < 3;

        return false;
    }

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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheckToToWhite() => IsBlackAttacksTo(_boards[WhiteKing].BitScanForward());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCheckToBlack() => IsWhiteAttacksTo(_boards[BlackKing].BitScanForward());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBehindBlackPassed(byte from, byte to)
    {
        if ((_blackFacing[from] & _boards[BlackPawn]).Any())
            return false;

        BitBoard bitBoard = _blackFacing[to] & _boards[BlackPawn];
        if (bitBoard.IsZero())
            return false;

        var coordinate = bitBoard.BitScanForward();

        return (_blackFacing[coordinate] & _boards[BlackPawn]).IsZero() && (_blackPassedPawns[coordinate] & _boards[WhitePawn]).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBehindWhitePassed(byte from, byte to)
    {
        if ((_whiteFacing[from] & _boards[WhitePawn]).Any())
            return false;

        BitBoard bitBoard = _whiteFacing[to] & _boards[WhitePawn];
        if (bitBoard.IsZero())
            return false;

        var coordinate = bitBoard.BitScanForward();

        return (_whiteFacing[coordinate] & _boards[WhitePawn]).IsZero() && (_whitePassedPawns[coordinate] & _boards[BlackPawn]).IsZero();
    }

    #endregion

    #region Mobility

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenMobility(byte to) => _round[(to.QueenAttacks(~_empty) & (_empty.Remove(_whitePawnAttacks)
            | _whiteKingZone)).Count() *
        _evaluationService.GetQueenMobilityValue()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookMobility(byte to) => _round[(to.RookAttacks(~_empty) & (_empty.Remove(_whitePawnAttacks)
            | _whiteKingZone)).Count() *
       _evaluationService.GetRookMobilityValue()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopMobility(byte to) => _round[(to.BishopAttacks(~_empty) & (_empty.Remove(_whitePawnAttacks) | _boards[WhiteRook] | _boards[WhiteKnight]
            | _whiteKingZone))
            .Count() * _evaluationService.GetBishopMobilityValue()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackKnightMobility(byte to) => _round[(_blackKnightPatterns[to] & (_empty.Remove(_whitePawnAttacks) | _boards[WhiteQueen] | _boards[WhiteRook] | _boards[WhiteBishop]
            | _whiteKingZone))
            .Count() * _evaluationService.GetKnightMobilityValue()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenMobility(byte to) => _round[(to.QueenAttacks(~_empty) & (_empty.Remove(_blackPawnAttacks)
            | _blackKingZone)).Count() *
        _evaluationService.GetQueenMobilityValue()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookMobility(byte to) => _round[(to.RookAttacks(~_empty) & (_empty.Remove(_blackPawnAttacks)
            | _blackKingZone)).Count() *
        _evaluationService.GetRookMobilityValue()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopMobility(byte to) => _round[(to.BishopAttacks(~_empty) & (_empty.Remove(_blackPawnAttacks) | _boards[BlackRook] | _boards[BlackKnight]
            | _blackKingZone)).Count() *
        _evaluationService.GetBishopMobilityValue()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteKnightMobility(byte to) => _round[(_whiteKnightPatterns[to]
            & (_empty.Remove(_blackPawnAttacks) | _boards[BlackQueen] | _boards[BlackRook] | _boards[BlackBishop]
            | _blackKingZone)).Count()
            * _evaluationService.GetKnightMobilityValue()];

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
    public bool CanDoBlackSmallCastle() => _moveHistory.CanDoBlackSmallCastle() && _empty.IsSet(_blackSmallCastleCondition) && _boards[BlackRook].IsSet(BitBoards.H8) && !_moveHistory.IsLastMoveWasCheck();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteSmallCastle() => _moveHistory.CanDoWhiteSmallCastle() && _empty.IsSet(_whiteSmallCastleCondition) && _boards[WhiteRook].IsSet(BitBoards.H1) && !_moveHistory.IsLastMoveWasCheck();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackBigCastle() => _moveHistory.CanDoBlackBigCastle() && _empty.IsSet(_blackBigCastleCondition) && _boards[BlackRook].IsSet(BitBoards.A8) && !_moveHistory.IsLastMoveWasCheck();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteBigCastle() => _moveHistory.CanDoWhiteBigCastle() && _empty.IsSet(_whiteBigCastleCondition) && _boards[WhiteRook].IsSet(BitBoards.A1) && !_moveHistory.IsLastMoveWasCheck();

    #endregion

    #region Evaluation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Evaluate()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();
        _whiteKingZone = _whiteKingShield[_boards[WhiteKing].BitScanForward()];
        _blackKingZone = _blackKingShield[_boards[BlackKing].BitScanForward()];
        var _phase = _moveHistory.GetPhase();

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
        _whiteKingZone = _whiteKingShield[_boards[WhiteKing].BitScanForward()];
        _blackKingZone = _blackKingShield[_boards[BlackKing].BitScanForward()];
        var _phase = _moveHistory.GetPhase();

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
    private int EvaluateWhitePawnOpening()
    {
        int value = 0;

        var bits = _boards[WhitePawn];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhitePawnFullValue(coordinate);

            if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_whiteDoublePawns[coordinate] & _boards[WhitePawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }


            if ((_whiteIsolatedPawns[coordinate] & _boards[WhitePawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else
            {
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
        var bits = _boards[WhiteBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhiteBishopFullValue(coordinate);

            value += GetWhiteBishopPinsEnd(coordinate);

            //if ((_whiteMinorDefense[coordinate] & _boards[WhitePawn]).Any())
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
        if (_boards[WhiteQueen].IsZero()) return 0;

        var pattern = _whiteBishopPatterns[coordinate] & _blackKingPatterns[_boards[BlackKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        return (coordinate.XrayBishopAttacks(~_empty, _boards[WhiteQueen]) & pattern).Any() ? _evaluationService.GetQueenBattaryValue() : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteRookOpening()
    {
        int i = -1;
        int value = 0;

        var king = _boards[BlackKing].BitScanForward();
        var bits = _boards[WhiteRook];

        while (bits.Any())
        {
            i++;
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhiteRookFullValue(coordinate);

            if ((coordinate > 15 && (_whiteFacing[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero()) ||
                (_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any()
                    && (_rookFiles[coordinate] & _boards[WhiteRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnOpenFileValue();
                }
            }
            else if ((coordinate > 15 && (_whiteFacing[coordinate] & _boards[WhitePawn]).IsZero()) ||
                (_rookFiles[coordinate] & _boards[WhitePawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any()
                    && (_rookFiles[coordinate] & _boards[WhiteRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                }
            }
            if (i > 0 && coordinate < 8 && (coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any()
                    && (_rookRanks[coordinate] & _boards[WhiteRook]).Any())
            {
                value += _evaluationService.GetConnectedRooksOnFirstRankValue();
            }

            value += GetWhiteRookPinsOpening(coordinate);

            //if ((coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any() && (_rookRanks[coordinate] & _boards[WhiteRook]).Any())
            //{
            //    value += _evaluationService.GetDoubleRookHorizontalValue();
            //}

            if ((_whiteRookKingPattern[coordinate] & _boards[WhiteKing]).Any() &&
                (_whiteRookPawnPattern[coordinate] & _boards[WhitePawn]).Any())
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
        var pattern = _whiteRookPatterns[coordinate] & _blackKingPatterns[_boards[BlackKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[WhiteQueen].Any() && (coordinate.XrayRookAttacks(~_empty, _boards[WhiteQueen]) & pattern).Any())
            return _evaluationService.GetQueenBattaryValue();

        if (_boards[WhiteRook].Count() > 1 && (coordinate.XrayRookAttacks(~_empty, _boards[WhiteRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookPartialPin(byte coordinate)
    {
        BitBoard bit = _boards[BlackQueen];
        if (bit.IsZero() || !_whiteRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[BlackKnight] | _boards[BlackBishop];

        var attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[BlackQueen];
        if (bit.IsZero() || !_whiteRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[WhiteKnight] | _boards[WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookAbsolutePin(byte coordinate)
    {
        if (!_whiteRookPatterns[coordinate].IsSet(_boards[BlackKing]))
            return 0;

        var blocker = _boards[BlackKnight] | _boards[BlackBishop];

        var attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & _boards[BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookDiscoveredCheck(byte coordinate)
    {
        if (!_whiteRookPatterns[coordinate].IsSet(_boards[BlackKing]))
            return 0;

        var blocker = _boards[WhiteKnight] | _boards[WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & _boards[BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteRookMiddle()
    {
        int i = -1;
        int value = 0;

        var king = _boards[BlackKing].BitScanForward();
        var bits = _boards[WhiteRook];

        while (bits.Any())
        {
            i++;
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhiteRookFullValue(coordinate);

            if ((coordinate > 15 && (_whiteFacing[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero()) ||
                (_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any()
                    && (_rookFiles[coordinate] & _boards[WhiteRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnOpenFileValue();
                }
            }
            else if ((coordinate > 15 && (_whiteFacing[coordinate] & _boards[WhitePawn]).IsZero()) ||
                (_rookFiles[coordinate] & _boards[WhitePawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any()
                    && (_rookFiles[coordinate] & _boards[WhiteRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                }
            }
            if (i > 0 && coordinate < 8 && (coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any()
                    && (_rookRanks[coordinate] & _boards[WhiteRook]).Any())
            {
                value += _evaluationService.GetConnectedRooksOnFirstRankValue();
            }

            value += GetWhiteRookPinsOpening(coordinate);

            //if ((coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any() && (_rookFiles[coordinate] & _boards[WhiteRook]).Any())
            //{
            //    value += _evaluationService.GetDoubleRookVerticalValue();
            //}

            //if ((coordinate.RookAttacks(~_empty) & _boards[WhiteQueen]).Any()
            //    && (_rookFiles[coordinate] & _boards[WhiteQueen]).Any())
            //{
            //    value += _evaluationService.GetDoubleRookVerticalValue();
            //}

            if ((_whiteRookKingPattern[coordinate] & _boards[WhiteKing]).Any() &&
                (_whiteRookPawnPattern[coordinate] & _boards[WhitePawn]).Any())
            {
                value -= _evaluationService.GetRookBlockedByKingValue();
            }

            //value += GetWhiteRookMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteRookEnd()
    {
        int value = 0;
        var bits = _boards[WhiteRook];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhiteRookFullValue(coordinate);

            if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                .IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();
            }
            else if ((_rookFiles[coordinate] & _boards[WhitePawn]).IsZero())
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
        var bits = _boards[WhiteQueen];
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
        var pattern = _whiteQueenPatterns[coordinate] & _blackKingPatterns[_boards[BlackKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[WhiteRook].Any() && (coordinate.XrayRookAttacks(~_empty, _boards[WhiteRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        if (_boards[WhiteBishop].Any() && (coordinate.XrayBishopAttacks(~_empty, _boards[WhiteBishop]) & pattern).Any())
            return _evaluationService.GetBishopBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenAbsolutePin(byte coordinate)
    {
        if (!_whiteQueenPatterns[coordinate].IsSet(_boards[BlackKing]))
            return 0;

        var blocker = _boards[BlackKnight] | _boards[BlackRook];

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & _boards[BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        blocker = _boards[BlackKnight] | _boards[BlackBishop];

        attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & _boards[BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenDiscoveredCheck(byte coordinate)
    {
        if (!_whiteQueenPatterns[coordinate].IsSet(_boards[BlackKing]))
            return 0;

        var blocker = _boards[WhiteKnight] | _boards[WhiteRook] | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & _boards[BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        blocker = _boards[WhiteKnight] | _boards[WhiteBishop];

        attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & _boards[BlackKing]).Any()) //Discovered Check
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
        var kingPosition = _boards[WhiteKing].BitScanForward();
        return _evaluationService.GetWhiteKingFullValue(kingPosition)
            + WhiteKingShieldOpeningValue(kingPosition)
            + WhiteKingZoneAttack();
        //- WhiteKingOpenValue(kingPosition);
        //- WhiteKingAttackValue(kingPosition);;
        //- WhitePawnStorm(kingPosition)
        //+ WhiteDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKingMiddle()
    {
        var kingPosition = _boards[WhiteKing].BitScanForward();
        return _evaluationService.GetWhiteKingFullValue(kingPosition)
            + WhiteKingShieldMiddleValue(kingPosition)
            + WhiteKingZoneAttack();
        //- WhiteKingOpenValue(kingPosition);
        //- WhiteKingAttackValue(kingPosition)
        //- WhitePawnStorm(kingPosition)
        //+ WhiteDistanceToQueen(kingPosition);
    }

    private int WhiteKingZoneAttack()
    {
        int valueOfAttacks = 0;
        BitBoard attackPattern;
        BitBoardList boards = stackalloc BitBoard[8];

        var bits = _boards[WhiteKnight];
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

        bits = _boards[WhiteBishop];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.BishopAttacks(~_empty) & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetBishopAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[WhiteRook];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.RookAttacks(~_empty) & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetRookAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[WhiteQueen];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.QueenAttacks(~_empty) & _blackKingZone;
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
        return _evaluationService.GetWhiteKingFullValue(kingPosition)
            - KingPawnTrofism(kingPosition);
        //+ WhiteDistanceToQueen(kingPosition);
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
        var bits = _boards[BlackPawn];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackPawnFullValue(coordinate);
            if ((_blackBlockedPawns[coordinate] & _whites).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
            }

            if ((_blackDoublePawns[coordinate] & _boards[BlackPawn]).Any())
            {
                value -= _evaluationService.GetDoubledPawnValue();
            }

            if ((_blackIsolatedPawns[coordinate] & _boards[BlackPawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else
            {
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
        var bits = _boards[BlackBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackBishopFullValue(coordinate);

            value += GetBlackBishopPinsEnd(coordinate);

            //if ((_blackMinorDefense[coordinate] & _boards[BlackPawn]).Any())
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
        if (_boards[BlackQueen].IsZero()) return 0;

        var pattern = _blackBishopPatterns[coordinate] & _whiteKingPatterns[_boards[WhiteKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        return (coordinate.XrayBishopAttacks(~_empty, _boards[BlackQueen]) & pattern).Any() ? _evaluationService.GetQueenBattaryValue() : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackRookOpening()
    {
        int value = 0;
        int i = -1;
        var king = _boards[WhiteKing].BitScanForward();
        var bits = _boards[BlackRook];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackRookFullValue(coordinate);

            if ((coordinate < 48 && (_blackFacing[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero()) ||
                (_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any()
                    && (_rookFiles[coordinate] & _boards[BlackRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnOpenFileValue();
                }
            }
            else if ((coordinate < 48 && (_blackFacing[coordinate] & _boards[BlackPawn]).IsZero()) ||
                (_rookFiles[coordinate] & _boards[BlackPawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any()
                    && (_rookFiles[coordinate] & _boards[BlackRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                }
            }
            if (i > 0 && coordinate < 56 && (coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any()
                    && (_rookRanks[coordinate] & _boards[BlackRook]).Any())
            {
                value += _evaluationService.GetConnectedRooksOnFirstRankValue();
            }

            value += GetBlackRookPinsOpening(coordinate);

            //if ((coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any() && (_rookRanks[coordinate] & _boards[BlackRook]).Any())
            //{
            //    value += _evaluationService.GetDoubleRookHorizontalValue();
            //}

            if ((_blackRookKingPattern[coordinate] & _boards[BlackKing]).Any() &&
                (_blackRookPawnPattern[coordinate] & _boards[BlackPawn]).Any())
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
        BitBoard bit = _boards[WhiteQueen];
        if (bit.IsZero() || !_blackRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[WhiteKnight] | _boards[WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[WhiteQueen];
        if (bit.IsZero() || !_blackRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[BlackKnight] | _boards[BlackBishop];

        var attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookAbsolutePin(byte coordinate)
    {
        if (!_blackRookPatterns[coordinate].IsSet(_boards[WhiteKing]))
            return 0;

        var blocker = _boards[WhiteKnight] | _boards[WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & _boards[WhiteKing]).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookDiscoveredCheck(byte coordinate)
    {
        if (!_blackRookPatterns[coordinate].IsSet(_boards[WhiteKing]))
            return 0;

        var blocker = _boards[BlackKnight] | _boards[BlackBishop];

        var attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & _boards[WhiteKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackRookMiddle()
    {
        int value = 0;
        int i = -1;
        var king = _boards[WhiteKing].BitScanForward();
        var bits = _boards[BlackRook];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackRookFullValue(coordinate);

            if ((coordinate < 48 && (_blackFacing[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero()) ||
                (_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn])).IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any()
                    && (_rookFiles[coordinate] & _boards[BlackRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnOpenFileValue();
                }
            }
            else if ((coordinate < 48 && (_blackFacing[coordinate] & _boards[BlackPawn]).IsZero()) ||
                (_rookFiles[coordinate] & _boards[BlackPawn]).IsZero())
            {
                value += _evaluationService.GetRookOnHalfOpenFileValue();

                if ((_blackKingPatterns[king] & _rookFiles[coordinate]).Any())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileNextToKingValue();
                }

                if (i > 0 && (coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any()
                    && (_rookFiles[coordinate] & _boards[BlackRook]).Any())
                {
                    value += _evaluationService.GetDoubleRookOnHalfOpenFileValue();
                }
            }
            if (i > 0 && coordinate < 56 && (coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any()
                    && (_rookRanks[coordinate] & _boards[BlackRook]).Any())
            {
                value += _evaluationService.GetConnectedRooksOnFirstRankValue();
            }

            value += GetBlackRookPinsOpening(coordinate);

            //if ((coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any() && (_rookFiles[coordinate] & _boards[BlackRook]).Any())
            //{
            //    value += _evaluationService.GetDoubleRookVerticalValue();
            //}

            //if ((coordinate.RookAttacks(~_empty) & _boards[BlackQueen]).Any()
            //    && (_rookFiles[coordinate] & _boards[BlackQueen]).Any())
            //{
            //    value += _evaluationService.GetDoubleRookVerticalValue();
            //}

            if ((_blackRookKingPattern[coordinate] & _boards[BlackKing]).Any() &&
                (_blackRookPawnPattern[coordinate] & _boards[BlackPawn]).Any())
            {
                value -= _evaluationService.GetRookBlockedByKingValue();
            }

            //value += GetBlackRookMobility(coordinate);
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackRookEnd()
    {
        int value = 0;
        var bits = _boards[BlackRook];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackRookFullValue(coordinate);

            if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                .IsZero())
            {
                value += _evaluationService.GetRookOnOpenFileValue();
            }
            else if ((_rookFiles[coordinate] & _boards[BlackPawn]).IsZero())
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
        var pattern = _blackRookPatterns[coordinate] & _whiteKingPatterns[_boards[WhiteKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[BlackQueen].Any() && (coordinate.XrayRookAttacks(~_empty, _boards[BlackQueen]) & pattern).Any())
            return _evaluationService.GetQueenBattaryValue();

        if (_boards[BlackRook].Count() > 1 && (coordinate.XrayRookAttacks(~_empty, _boards[BlackRook]) & pattern).Any())
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
        var bits = _boards[BlackQueen];
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
        var pattern = _blackQueenPatterns[coordinate] & _whiteKingPatterns[_boards[WhiteKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[BlackRook].Any() && (coordinate.XrayRookAttacks(~_empty, _boards[BlackRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        if (_boards[BlackBishop].Any() && (coordinate.XrayBishopAttacks(~_empty, _boards[BlackBishop]) & pattern).Any())
            return _evaluationService.GetBishopBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenAbsolutePin(byte coordinate)
    {
        if (!_blackQueenPatterns[coordinate].IsSet(_boards[WhiteKing]))
            return 0;

        var blocker = _boards[WhiteKnight] | _boards[WhiteRook];

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & _boards[WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        blocker = _boards[WhiteKnight] | _boards[WhiteBishop];

        attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & _boards[WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenDiscoveredCheck(byte coordinate)
    {
        if (!_blackQueenPatterns[coordinate].IsSet(_boards[WhiteKing]))
            return 0;

        var blocker = _boards[BlackKnight] | _boards[BlackRook] | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & _boards[WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        blocker = _boards[BlackKnight] | _boards[BlackBishop];

        attacks = coordinate.XrayRookAttacks(~_empty, blocker);

        if ((attacks & _boards[WhiteKing]).Any()) //Discovered Check
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
        var kingPosition = _boards[BlackKing].BitScanForward();
        return _evaluationService.GetBlackKingFullValue(kingPosition)
            + BlackKingShieldOpeningValue(kingPosition)
            + BlackKingZoneAttack();
        //- BlackKingOpenValue(kingPosition);
        //- BlackKingAttackValue(kingPosition)
        //- BlackPawnStorm(kingPosition) + BlackDistanceToQueen(kingPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKingMiddle()
    {
        var kingPosition = _boards[BlackKing].BitScanForward();
        return _evaluationService.GetBlackKingFullValue(kingPosition)
            + BlackKingShieldMiddleValue(kingPosition)
            + BlackKingZoneAttack();
        //- BlackKingOpenValue(kingPosition);
        //- BlackKingAttackValue(kingPosition);
        //- BlackPawnStorm(kingPosition) + BlackDistanceToQueen(kingPosition);
    }
    private int BlackKingZoneAttack()
    {
        int valueOfAttacks = 0;
        BitBoard attackPattern;
        BitBoardList boards = stackalloc BitBoard[8];

        var bits = _boards[BlackKnight];
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

        bits = _boards[BlackBishop];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.BishopAttacks(~_empty) & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetBishopAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[BlackRook];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.RookAttacks(~_empty) & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetRookAttackValue();
                boards.Add(attackPattern);
            }
            bits = bits.Remove(position);
        }

        bits = _boards[BlackQueen];
        while (bits.Any())
        {
            var position = bits.BitScanForward();
            attackPattern = position.QueenAttacks(~_empty) & _whiteKingZone;
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
        var pawns = _boards[BlackPawn];

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
        var bits = _boards[BlackBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackBishopFullValue(coordinate);

            value += GetBlackBishopPinsOpening(coordinate);

            //if ((_blackMinorDefense[coordinate] & _boards[BlackPawn]).Any())
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
        BitBoard bit = _boards[WhiteRook] | _boards[WhiteQueen];
        if (bit.IsZero() || !_blackBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[WhiteKnight] | _boards[WhiteRook];

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[WhiteRook] | _boards[WhiteQueen];
        if (bit.IsZero() || !_blackBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[BlackKnight] | _boards[BlackRook] | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopAbsolutePin(byte coordinate)
    {
        if (!_blackBishopPatterns[coordinate].IsSet(_boards[WhiteKing]))
            return 0;

        var blocker = _boards[WhiteKnight] | _boards[WhiteRook];

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & _boards[WhiteKing]).Any())
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopDiscoveredCheck(byte coordinate)
    {
        if (!_blackBishopPatterns[coordinate].IsSet(_boards[WhiteKing]))
            return 0;

        var blocker = _boards[BlackKnight] | _boards[BlackRook] | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & _boards[WhiteKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackKnightValue()
    {
        int value = 0;
        var bits = _boards[BlackKnight];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackKnightFullValue(coordinate);

            //if ((_blackMinorDefense[coordinate] & _boards[BlackPawn]).Any())
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
        if (_boards[BlackPawn].IsZero()) return _evaluationService.GetNoPawnsValue();

        int value = 0;
        var bits = _boards[BlackPawn];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetBlackPawnFullValue(coordinate);
            if ((_blackBlockedPawns[coordinate] & _whites).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
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
                        //if ((_blackCandidatePawnsAttackBack[coordinate] & _boards[BlackPawn]).Any())
                        //{
                        //    value += _evaluationService.GetProtectedPassedPawnValue();
                        //}
                    }
                }
                //else if ((_blackCandidatePawnsFront[coordinate] & _boards[WhitePawn]).Count() < (_blackCandidatePawnsBack[coordinate] & _boards[BlackPawn]).Count() &&
                //    (_blackCandidatePawnsAttackFront[coordinate] & _boards[WhitePawn]).Count() <= (_blackCandidatePawnsAttackBack[coordinate] & _boards[BlackPawn]).Count())
                //{
                //    value += _evaluationService.GetBlackCandidatePawnValue(coordinate);
                //}
                //else
                //{
                //    value += _evaluationService.GetOpenPawnValue();
                //}
            }


            if ((_blackIsolatedPawns[coordinate] & _boards[BlackPawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else
            {
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
            bits = bits.Remove(coordinate);
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingShieldOpeningValue(byte kingPosition) => _moveHistory.CanDoWhiteCastle() ? 0 : WhiteKingShieldMiddleValue(kingPosition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WhiteKingShieldMiddleValue(byte kingPosition)
    {
        var pawns = _boards[WhitePawn];

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
        var bits = _boards[WhiteBishop];
        int value = bits.Count() > 1 ? _evaluationService.GetDoubleBishopValue() : 0;

        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhiteBishopFullValue(coordinate);

            value += GetWhiteBishopPinsOpening(coordinate);

            //if ((_whiteMinorDefense[coordinate] & _boards[WhitePawn]).Any())
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
    private BitBoard GetWhiteMovablePawns() => ((_boards[WhitePawn] << 8) & _empty) >> 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetBlackMovablePawns() => ((_boards[BlackPawn] >> 8) & _empty) << 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[BlackRook] | _boards[BlackQueen];
        if (bit.IsZero() || !_whiteBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[WhiteKnight] | _boards[WhiteRook] | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopPartialPin(byte coordinate)
    {
        BitBoard bit = _boards[BlackRook] | _boards[BlackQueen];
        if (bit.IsZero() || !_whiteBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[BlackKnight] | _boards[BlackRook];

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopAbsolutePin(byte coordinate)
    {
        if (!_whiteBishopPatterns[coordinate].IsSet(_boards[BlackKing]))
            return 0;

        var blocker = _boards[BlackKnight] | _boards[BlackRook];

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & _boards[BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopDiscoveredCheck(byte coordinate)
    {
        if (!_whiteBishopPatterns[coordinate].IsSet(_boards[BlackKing]))
            return 0;

        var blocker = _boards[WhiteKnight] | _boards[WhiteRook] | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(~_empty, blocker);

        if ((attacks & _boards[BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteKnightValue()
    {
        int value = 0;

        var bits = _boards[WhiteKnight];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();

            value += _evaluationService.GetWhiteKnightFullValue(coordinate);
            //if ((_whiteMinorDefense[coordinate] & _boards[WhitePawn]).Any())
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
        if (_boards[WhitePawn].IsZero())
        {
            return _evaluationService.GetNoPawnsValue();
        }

        int value = 0;

        var bits = _boards[WhitePawn];
        while (bits.Any())
        {
            var coordinate = bits.BitScanForward();
            value += _evaluationService.GetWhitePawnFullValue(coordinate);

            if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
            {
                value -= _evaluationService.GetBlockedPawnValue();
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
                        //if ((_whiteCandidatePawnsAttackBack[coordinate] & _boards[WhitePawn]).Any())
                        //{
                        //    value += _evaluationService.GetProtectedPassedPawnValue();
                        //}
                    }
                }
                //else if ((_whiteCandidatePawnsFront[coordinate] & _boards[BlackPawn]).Count() < (_whiteCandidatePawnsBack[coordinate] & _boards[WhitePawn]).Count() &&
                //    (_whiteCandidatePawnsAttackFront[coordinate] & _boards[BlackPawn]).Count() <= (_whiteCandidatePawnsAttackBack[coordinate] & _boards[WhitePawn]).Count())
                //{
                //    value += _evaluationService.GetWhiteCandidatePawnValue(coordinate);
                //}
                //else
                //{
                //    value += _evaluationService.GetOpenPawnValue();
                //}
            }


            if ((_whiteIsolatedPawns[coordinate] & _boards[WhitePawn]).IsZero())
            {
                value -= _evaluationService.GetIsolatedPawnValue();
            }
            else
            {
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
            bits = bits.Remove(coordinate);
        }

        return value;
    }

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

        bool isLegal = !IsCheckToToWhite();

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
            (move.IsCastle && IsBlackAttacksTo(move.To == C1 ? D1 : F1));

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
             (move.IsCastle && IsWhiteAttacksTo(move.To == C8 ? D8 : F8));

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
        byte from = _boards[WhiteKing].BitScanForward();
        return _whiteKingPatterns[from].IsSet(to) && IsWhiteMoveLigal(_moveProvider.GetWhiteKingAttacks(from, to));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteQueenAttackTo(byte to)
    {
        var fromBoard = _boards[WhiteQueen] & to.QueenAttacks(~_empty);

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
        var fromBoard = _boards[WhiteRook] & to.RookAttacks(~_empty);

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
        var fromBoard = _boards[WhiteBishop] & to.BishopAttacks(~_empty);

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
        var fromBoard = _boards[WhiteKnight] & _whiteKnightPatterns[to];

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
        var fromBoard = _boards[WhitePawn].Remove(_ranks[6]) & _blackPawnPatterns[to];

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
        byte from = _boards[BlackKing].BitScanForward();
        return _blackKingPatterns[from].IsSet(to) && IsBlackMoveLigal(_moveProvider.GetBlackKingAttacks(from, to));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackQueenAttackTo(byte to)
    {
        var fromBoard = _boards[BlackQueen] & to.QueenAttacks(~_empty);

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
        var fromBoard = _boards[BlackRook] & to.RookAttacks(~_empty);

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
        var fromBoard = _boards[BlackBishop] & to.BishopAttacks(~_empty);

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
        var fromBoard = _boards[BlackKnight] & _blackKnightPatterns[to];

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
        var fromBoard = _boards[BlackPawn].Remove(_ranks[1]) & _whitePawnPatterns[to];

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
    internal AttackBase GetWhiteAttackToForPromotion(byte to)
    {
        AttackBase attack;

        _ = GetWhiteKnightAttacksTo(to, out attack) ||
        GetWhiteBishopAttacksTo(to, out attack) ||
        GetWhiteRookAttacksTo(to, out attack) ||
        GetWhiteQueenAttacksTo(to, out attack) ||
        GetWhiteKingAttacksTo(to, out attack);

        return attack;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteKnightAttacksTo(byte to, out AttackBase attack)
    {
        var fromBoard = _boards[WhiteKnight];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (_whiteKnightPatterns[from].IsSet(to))
            {
                AttackBase a = _moveProvider.GetWhiteKnightAttacks(from, to); //_whiteKnightAttacks[from][to];
                if (IsWhiteMoveLigal(a))
                {
                    attack = a;
                    return true;
                }
            }
            fromBoard = fromBoard.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteQueenAttacksTo(byte to, out AttackBase attack)
    {
        var fromBoard = _boards[WhiteQueen];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (from.QueenAttacks(~_empty).IsSet(to))
            {
                AttackBase a = _moveProvider.GetWhiteQueenAttacks(from, to); //_whiteKnightAttacks[from][to];
                if (IsWhiteMoveLigal(a))
                {
                    attack = a;
                    return true;
                }
            }
            fromBoard = fromBoard.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteBishopAttacksTo(byte to, out AttackBase attack)
    {
        var fromBoard = _boards[WhiteBishop];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (from.BishopAttacks(~_empty).IsSet(to))
            {
                AttackBase a = _moveProvider.GetWhiteBishopAttacks(from, to); //_whiteKnightAttacks[from][to];
                if (IsWhiteMoveLigal(a))
                {
                    attack = a;
                    return true;
                }
            }
            fromBoard = fromBoard.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteRookAttacksTo(byte to, out AttackBase attack)
    {
        var fromBoard = _boards[WhiteRook];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (from.RookAttacks(~_empty).IsSet(to))
            {
                AttackBase a = _moveProvider.GetWhiteRookAttacks(from, to); //_whiteKnightAttacks[from][to];
                if (IsWhiteMoveLigal(a))
                {
                    attack = a;
                    return true;
                }
            }
            fromBoard = fromBoard.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteKingAttacksTo(byte to, out AttackBase attack)
    {
        var fromBoard = _boards[WhiteKing];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (_whiteKingPatterns[from].IsSet(to))
            {
                AttackBase a = _moveProvider.GetWhiteKingAttacks(from, to); //_whiteKnightAttacks[from][to];
                if (IsWhiteMoveLigal(a))
                {
                    attack = a;
                    return true;
                }
            }
            fromBoard = fromBoard.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AttackBase GetBlackAttackToForPromotion(byte to)
    {
        AttackBase attack;

        _ = GetBlackKnightAttacksTo(to, out attack) ||
        GetBlackBishopAttacksTo(to, out attack) ||
        GetBlackRookAttacksTo(to, out attack) ||
        GetBlackQueenAttacksTo(to, out attack) ||
        GetBlackKingAttacksTo(to, out attack);

        return attack;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackKnightAttacksTo(byte to, out AttackBase attack)
    {
        var fromBoard = _boards[BlackKnight];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (_blackKnightPatterns[from].IsSet(to))
            {
                AttackBase a = _moveProvider.GetBlackKnightAttacks(from, to); //_BlackKnightAttacks[from][to];
                if (IsBlackMoveLigal(a))
                {
                    attack = a;
                    return true;
                }
            }
            fromBoard = fromBoard.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackQueenAttacksTo(byte to, out AttackBase attack)
    {
        var fromBoard = _boards[BlackQueen];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (from.QueenAttacks(~_empty).IsSet(to))
            {
                AttackBase a = _moveProvider.GetBlackQueenAttacks(from, to); //_BlackKnightAttacks[from][to];
                if (IsBlackMoveLigal(a))
                {
                    attack = a;
                    return true;
                }
            }
            fromBoard = fromBoard.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackBishopAttacksTo(byte to, out AttackBase attack)
    {
        var fromBoard = _boards[BlackBishop];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (from.BishopAttacks(~_empty).IsSet(to))
            {
                AttackBase a = _moveProvider.GetBlackBishopAttacks(from, to); //_BlackKnightAttacks[from][to];
                if (IsBlackMoveLigal(a))
                {
                    attack = a;
                    return true;
                }
            }
            fromBoard = fromBoard.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackRookAttacksTo(byte to, out AttackBase attack)
    {
        var fromBoard = _boards[BlackRook];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (from.RookAttacks(~_empty).IsSet(to))
            {
                AttackBase a = _moveProvider.GetBlackRookAttacks(from, to); //_BlackKnightAttacks[from][to];
                if (IsBlackMoveLigal(a))
                {
                    attack = a;
                    return true;
                }
            }
            fromBoard = fromBoard.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackKingAttacksTo(byte to, out AttackBase attack)
    {
        var fromBoard = _boards[BlackKing];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (_blackKingPatterns[from].IsSet(to))
            {
                AttackBase a = _moveProvider.GetBlackKingAttacks(from, to); //_BlackKnightAttacks[from][to];
                if (IsBlackMoveLigal(a))
                {
                    attack = a;
                    return true;
                }
            }
            fromBoard = fromBoard.Remove(from);
        }

        attack = null;
        return false;
    }
}
