using Engine.DataStructures;
using Engine.Models.Enums;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services.Evaluation;
using System.Runtime.CompilerServices;
using Engine.Models.Boards.Buffers;
using Engine.Models.Boards.Structures;
using Engine.DataStructures.Moves.Arrays;

namespace Engine.Services;

public partial class MoveProvider
{
    #region Moves

    private BitBoard _whitePawnRank2;
    private BitBoard _whitePawnRank4;

    private MoveArray[] _whitePawnMoves;
    private MoveArray[] _whiteKnightMoves;
    private MoveArray[] _whiteBishopMoves;
    private MoveArray[] _whiteRookMoves;
    private MoveArray[] _whiteQueenMoves;
    private MoveArray[] _whiteKingMoves;

    private BitBoard _blackPawnRank7;
    private BitBoard _blackPawnRank5;

    private MoveArray[] _blackPawnMoves;
    private MoveArray[] _blackKnightMoves;
    private MoveArray[] _blackBishopMoves;
    private MoveArray[] _blackRookMoves;
    private MoveArray[] _blackQueenMoves;
    private MoveArray[] _blackKingMoves;

    #endregion

    #region Attacks

    private PawnOverAttackArray _whitePawnOverAttacks;
    private PawnOverAttackArray _blackPawnOverAttacks;

    private AttackArray[] _whitePawnAttacks;
    private AttackArray[] _whiteKnightAttacks;
    private AttackArray[] _whiteBishopAttacks;
    private AttackArray[] _whiteRookAttacks;
    private AttackArray[] _whiteQueenAttacks;
    private AttackArray[] _whiteKingAttacks;

    private AttackArray[] _blackPawnAttacks;
    private AttackArray[] _blackKnightAttacks;
    private AttackArray[] _blackBishopAttacks;
    private AttackArray[] _blackRookAttacks;
    private AttackArray[] _blackQueenAttacks;
    private AttackArray[] _blackKingAttacks;

    #endregion

    #region Promotions

    private readonly PromotionList _emptyPromotions = new PromotionList(0);
    private readonly PromotionAttackList _emptyPromotionAttacks = new PromotionAttackList(0);

    private PromotionListArray _whitePromotions;
    private PromotionAttackListArray _whitePromotionAttacks;

    private PromotionListArray _blackPromotions;
    private PromotionAttackListArray _blackPromotionAttacks;

    #endregion

    private readonly MoveBase[] _all;

    private readonly CellBuffer<BitBoard> _whitePawnPatterns;
    private readonly CellBuffer<BitBoard> _whiteKnightPatterns;
    private readonly CellBuffer<BitBoard> _whiteKingPatterns;
    private readonly CellBuffer<BitBoard> _blackPawnPatterns;
    private readonly CellBuffer<BitBoard> _blackKnightPatterns;
    private readonly CellBuffer<BitBoard> _blackKingPatterns;
    private readonly CellBuffer<CellBuffer<BitBoard>> _attackPatterns;

    private static readonly int _squaresNumber = 64;
    private readonly int _piecesNumbers = 12;

    private Board _board;

    public MoveProvider(IConfigurationProvider configurationProvider, IStaticValueProvider staticValueProvider)
    {
        var _moves = new DynamicArray<MoveList>[_piecesNumbers][];
        var _attacks = new DynamicArray<AttackList>[_piecesNumbers][];
        var _promotionAttacks = new DynamicArray<PromotionAttackList>[_piecesNumbers][];
        var _promotions = new DynamicArray<PromotionList>[_piecesNumbers][];
        List<List<MoveBase>>[][] _movesTemp = new List<List<MoveBase>>[_piecesNumbers][];
        List<List<AttackBase>>[][] _attacksTemp = new List<List<AttackBase>>[_piecesNumbers][];
        List<List<PromotionAttack>>[][] _promotionsAttackTemp = new List<List<PromotionAttack>>[_piecesNumbers][];
        List<List<PromotionMove>>[][] _promotionsTemp = new List<List<PromotionMove>>[_piecesNumbers][];
        _attackPatterns = new CellBuffer<CellBuffer<BitBoard>>();
        var _attacksTo = new AttackBase[_piecesNumbers][][][];

        var es = new EvaluationServiceOpening(configurationProvider, staticValueProvider);

        List<int> see =
        [
            es.GetPieceValue(Pieces.WhiteQueen) - es.GetPieceValue(Pieces.WhitePawn),
            es.GetPieceValue(Pieces.WhiteRook) - es.GetPieceValue(Pieces.WhitePawn),
            es.GetPieceValue(Pieces.WhiteBishop) - es.GetPieceValue(Pieces.WhitePawn),
            es.GetPieceValue(Pieces.WhiteKnight) - es.GetPieceValue(Pieces.WhitePawn)
        ];

        AttackBase.CapturedValue = new int[12];
        for (byte i = 0; i < 12; i++)
        {
            AttackBase.CapturedValue[i] = es.GetPieceValue(i);
        }

        foreach (var piece in Enumerable.Range(0, 12))
        {
            _moves[piece] = new DynamicArray<MoveList>[_squaresNumber];
            _attacks[piece] = new DynamicArray<AttackList>[_squaresNumber];
            _promotionAttacks[piece] = new DynamicArray<PromotionAttackList>[_squaresNumber];
            _promotions[piece] = new DynamicArray<PromotionList>[_squaresNumber];
            _movesTemp[piece] = new List<List<MoveBase>>[_squaresNumber];
            _attacksTemp[piece] = new List<List<AttackBase>>[_squaresNumber];
            _promotionsAttackTemp[piece] = new List<List<PromotionAttack>>[_squaresNumber];
            _promotionsTemp[piece] = new List<List<PromotionMove>>[_squaresNumber];
            _attackPatterns[piece] = new CellBuffer<BitBoard>();
            _attacksTo[piece] = new AttackBase[_squaresNumber][][];
            for (int square = 0; square < _squaresNumber; square++)
            {
                _movesTemp[piece][square] = [];
                _attacksTemp[piece][square] = [];
                _promotionsTemp[piece][square] = [];
                _promotionsAttackTemp[piece][square] = [];
                _attackPatterns[piece][square] = new BitBoard(0);
            }

            SetMoves((byte)piece, _movesTemp, _promotionsTemp, see);
            SetAttacks((byte)piece, _attacksTemp, _promotionsAttackTemp, see);

            for (int i = 0; i < _squaresNumber; i++)
            {
                Dictionary<byte, AttackBase[]> attacksTo = _attacksTemp[piece][i].SelectMany(m => m)
                    .GroupBy(g => g.To)
                    .ToDictionary(key => key.Key, v => v.ToArray());
                AttackBase[][] aTo = new AttackBase[_squaresNumber][];
                for (byte q = 0; q < aTo.Length; q++)
                {
                    if (attacksTo.TryGetValue(q, out var list))
                    {
                        aTo[q] = list;
                    }
                }

                _attacksTo[piece][i] = aTo;
            }

            if (piece == Pieces.WhitePawn || piece == Pieces.BlackPawn)
            {
                for (int i = 0; i < _squaresNumber; i++)
                {
                    Dictionary<byte, PromotionAttack[]> attacksTo = _promotionsAttackTemp[piece][i].SelectMany(m => m)
                        .GroupBy(g => g.To)
                        .ToDictionary(key => key.Key, v => v.ToArray());
                    PromotionAttack[][] aTo = new PromotionAttack[_squaresNumber][];
                    for (byte q = 0; q < aTo.Length; q++)
                    {
                        if (attacksTo.TryGetValue(q, out var list))
                        {
                            aTo[q] = list;
                        }
                    }

                    _attacksTo[piece][i] = aTo;
                }
            }
        }

        foreach (var piece in Enumerable.Range(0, 12))
        {
            foreach (List<List<AttackBase>> attacks in _attacksTemp[piece])
            {
                if (attacks == null) continue;

                foreach (var moves in attacks)
                {
                    foreach (var move in moves)
                    {
                        _attackPatterns[piece][move.From] = _attackPatterns[piece][move.From] | move.EmptyBoard | move.To.AsBitBoard();
                    }
                }
            }
        }

        _attackPatterns[Pieces.WhitePawn][Squares.A1] = Squares.B2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.B1] = Squares.A2.AsBitBoard() | Squares.C2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.C1] = Squares.B2.AsBitBoard() | Squares.D2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.D1] = Squares.C2.AsBitBoard() | Squares.E2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.E1] = Squares.D2.AsBitBoard() | Squares.F2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.F1] = Squares.E2.AsBitBoard() | Squares.G2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.G1] = Squares.F2.AsBitBoard() | Squares.H2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.H1] = Squares.G2.AsBitBoard();

        _attackPatterns[Pieces.BlackPawn][Squares.A8] = Squares.B7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.B8] = Squares.A7.AsBitBoard() | Squares.C7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.C8] = Squares.B7.AsBitBoard() | Squares.D7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.D8] = Squares.C7.AsBitBoard() | Squares.E7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.E8] = Squares.D7.AsBitBoard() | Squares.F7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.F8] = Squares.E7.AsBitBoard() | Squares.G7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.G8] = Squares.F7.AsBitBoard() | Squares.H7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.H8] = Squares.G7.AsBitBoard();

        _attackPatterns[Pieces.WhitePawn][Squares.A7] = Squares.B8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.B7] = Squares.A8.AsBitBoard() | Squares.C8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.C7] = Squares.B8.AsBitBoard() | Squares.D8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.D7] = Squares.C8.AsBitBoard() | Squares.E8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.E7] = Squares.D8.AsBitBoard() | Squares.F8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.F7] = Squares.E8.AsBitBoard() | Squares.G8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.G7] = Squares.F8.AsBitBoard() | Squares.H8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.H7] = Squares.G8.AsBitBoard();

        _attackPatterns[Pieces.BlackPawn][Squares.A2] = Squares.B1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.B2] = Squares.A1.AsBitBoard() | Squares.C1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.C2] = Squares.B1.AsBitBoard() | Squares.D1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.D2] = Squares.C1.AsBitBoard() | Squares.E1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.E2] = Squares.D1.AsBitBoard() | Squares.F1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.F2] = Squares.E1.AsBitBoard() | Squares.G1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.G2] = Squares.F1.AsBitBoard() | Squares.H1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.H2] = Squares.G1.AsBitBoard();

        _whitePawnPatterns = new();
        _whitePawnPatterns = _attackPatterns[Pieces.WhitePawn];

        _whiteKnightPatterns = new();
        _whiteKnightPatterns = _attackPatterns[Pieces.WhiteKnight];

        _whiteKingPatterns = new();
        _whiteKingPatterns = _attackPatterns[Pieces.WhiteKing];

        _blackPawnPatterns = new();
        _blackPawnPatterns = _attackPatterns[Pieces.BlackPawn];

        _blackKnightPatterns = new();
        _blackKnightPatterns = _attackPatterns[Pieces.BlackKnight];

        _blackKingPatterns = new();
        _blackKingPatterns = _attackPatterns[Pieces.BlackKing];

        List<MoveBase> all = [];
        for (var i = 0; i < _attacksTemp.Length; i++)
        {
            for (var j = 0; j < _attacksTemp[i].Length; j++)
            {
                for (var k = 0; k < _attacksTemp[i][j].Count; k++)
                {
                    foreach (var attack in _attacksTemp[i][j][k])
                    {
                        all.Add(attack);
                    }
                }
            }
        }
        for (var i = 0; i < _promotionsAttackTemp.Length; i++)
        {
            for (var j = 0; j < _promotionsAttackTemp[i].Length; j++)
            {
                for (var k = 0; k < _promotionsAttackTemp[i][j].Count; k++)
                {
                    foreach (var attack in _promotionsAttackTemp[i][j][k])
                    {
                        all.Add(attack);
                    }
                }
            }
        }

        for (var i = 0; i < _promotionsTemp.Length; i++)
        {
            for (var j = 0; j < _promotionsTemp[i].Length; j++)
            {
                for (var k = 0; k < _promotionsTemp[i][j].Count; k++)
                {
                    foreach (var move in _promotionsTemp[i][j][k])
                    {
                        all.Add(move);
                    }
                }
            }
        }

        for (var i = 0; i < _movesTemp.Length; i++)
        {
            for (var j = 0; j < _movesTemp[i].Length; j++)
            {
                for (var k = 0; k < _movesTemp[i][j].Count; k++)
                {
                    foreach (var move in _movesTemp[i][j][k])
                    {
                        all.Add(move);
                    }
                }
            }
        }

        HashSet<byte> whitePromotion = [Squares.A6, Squares.B6, Squares.C6, Squares.D6, Squares.E6, Squares.F6, Squares.G6, Squares.H6,];
        HashSet<byte> blackPromotion = [Squares.A3, Squares.B3, Squares.C3, Squares.D3, Squares.E3, Squares.F3, Squares.G3, Squares.H3,];
        _all = all.ToArray();
        //MoveList.Moves = _all;
        ushort quietKey = 0;
        ushort lowSeeKey = 1;
        int quietCount = 0;
        int nonQuietCount = 0;
        for (var i = 0; i < _all.Length; i++)
        {
            var move = _all[i];
            move.Key = (short)i;

            move.IsPromotionExtension = (move.Piece == Pieces.BlackPawn && blackPromotion.Contains(move.From)) || (move.Piece == Pieces.WhitePawn && whitePromotion.Contains(move.From));

            if (move.IsPromotionExtension)
            {
                move.CanNotReduceNext = true;
                move.CanReduce = false;
            }
            else
            {
                move.CanNotReduceNext = false;
                move.CanReduce = !move.IsAttack && !move.IsPromotion;
            }

            if (move.Piece.IsWhite())
            {
                move.IsWhite = true;
                move.IsBlack = false;
                move.Turn = Turn.White;
            }
            else
            {
                move.IsWhite = false;
                move.IsBlack = true;
                move.Turn = Turn.Black;
            }

            move.IsFutile = !move.IsAttack && !move.IsPromotion;
            move.IsQuiet = !move.IsAttack && !move.IsPromotion;

            move.IsIrreversible = move.IsAttack || move.IsCastle || move.Piece == Pieces.WhitePawn || move.Piece == Pieces.BlackPawn;

            if (move.IsQuiet)
            {
                move.QuietKey = quietKey++;
                quietCount++;
            }
            else
            {
                move.LowSeeKey = lowSeeKey++;
                nonQuietCount++;
            }
        }

        var promotions = _all.OfType<PromotionMove>();
        foreach (var move in promotions)
        {
            var isLowPiece = move.PromotionPiece == Pieces.WhiteKnight || move.PromotionPiece == Pieces.WhiteBishop || move.PromotionPiece == Pieces.BlackKnight || move.PromotionPiece == Pieces.WhiteBishop;
            move.CanReduce = isLowPiece;
            move.IsFutile = isLowPiece;
            move.CanNotReduceNext = !isLowPiece;
        }

        var promotionAttacks = _all.OfType<PromotionAttack>();
        foreach (var move in promotionAttacks)
        {
            var isLowPiece = move.PromotionPiece == Pieces.WhiteKnight || move.PromotionPiece == Pieces.WhiteBishop || move.PromotionPiece == Pieces.BlackKnight || move.PromotionPiece == Pieces.WhiteBishop;
            move.CanReduce = isLowPiece;
            move.IsFutile = isLowPiece;
            move.CanNotReduceNext = !isLowPiece;
        }

        SetMoves();
        SetPromotions();
        SetAttacks();
        SetPromotionAttacks();
        SetPawnOver();

        MovesCount = _all.Length;
        QuietCount = quietCount;
        NonQuietCount = nonQuietCount;
    }

    #region Public API

    public int MovesCount;
    public int QuietCount;
    public int NonQuietCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase Get(short key) => _all[key];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsAttack(short key) => _all[key].IsAttack;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<MoveBase> GetAll() => _all;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetPromotions(byte piece, byte cell, PromotionList promotions)
    {
        promotions.Clear();

        PromotionArray lists;
        if (piece == Pieces.WhitePawn && cell > Squares.H6)
            lists = _whitePromotions[cell];
        else if (piece == Pieces.BlackPawn && cell < Squares.A3)
            lists = _blackPromotions[cell];
        else
            return;

        var array = lists.GetAll();

        for (byte i = 0; i < array.Length; i++)
        {
            var list = array[i];
            if (list == null) continue;
            if (list[0].IsLegal())
                promotions.Add(list);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetPromotions(byte piece, byte cell, List<PromotionAttackList> promotions)
    {
        promotions[0].Clear();
        promotions[1].Clear();

        PromotionAttackArray lists;
        if (piece == Pieces.WhitePawn && cell > Squares.H6)
            lists = _whitePromotionAttacks[cell];
        else if (piece == Pieces.BlackPawn && cell < Squares.A3)
            lists = _blackPromotionAttacks[cell];
        else
            return;

        var array = lists.GetAll();

        for (byte i = 0; i < array.Length; i++)
        {
            var list = array[i];
            if (list == null) continue;
            if (list.Count > 0 && list[0].IsLegal())
                promotions[i].Add(list);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAttacks(byte piece, byte cell, AttackList attackList)
    {
        attackList.Clear();
        AttackArray lists;
        switch (piece)
        {
            case Pieces.WhitePawn:
                lists = _whitePawnAttacks[cell];
                break;
            case Pieces.WhiteKnight:
                lists = _whiteKnightAttacks[cell];
                break;
            case Pieces.WhiteBishop:
                lists = _whiteBishopAttacks[cell];
                break;
            case Pieces.WhiteRook:
                lists = _whiteRookAttacks[cell];
                break;
            case Pieces.WhiteQueen:
                lists = _whiteQueenAttacks[cell];
                break;
            case Pieces.WhiteKing:
                lists = _whiteKingAttacks[cell];
                break;
            case Pieces.BlackPawn:
                lists = _blackPawnAttacks[cell];
                break;
            case Pieces.BlackKnight:

                lists = _blackKnightAttacks[cell];
                break;
            case Pieces.BlackBishop:
                lists = _blackBishopAttacks[cell];
                break;
            case Pieces.BlackRook:
                lists = _blackRookAttacks[cell];
                break;
            case Pieces.BlackQueen:
                lists = _blackQueenAttacks[cell];
                break;
            case Pieces.BlackKing:
                lists = _blackKingAttacks[cell];
                break;
            default:
                return;
        }
        var array = lists.GetAll();
        for (byte i = 0; i < array.Length; i++)
        {
            var m = array[i];
            if (m?.IsLegal() == true)
                attackList.Add(m);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<MoveBase> GetMoves(byte piece, byte cell)
    {
        MoveArray lists;
        switch (piece)
        {
            case Pieces.WhitePawn:
                lists = _whitePawnMoves[cell];
                break;
            case Pieces.WhiteKnight:
                lists = _whiteKnightMoves[cell];
                break;
            case Pieces.WhiteBishop:
                lists = _whiteBishopMoves[cell];
                break;
            case Pieces.WhiteRook:
                lists = _whiteRookMoves[cell];
                break;
            case Pieces.WhiteQueen:
                lists = _whiteQueenMoves[cell];
                break;
            case Pieces.WhiteKing:
                lists = _whiteKingMoves[cell];
                break;
            case Pieces.BlackPawn:
                lists = _blackPawnMoves[cell];
                break;
            case Pieces.BlackKnight:
                lists = _blackKnightMoves[cell];
                break;
            case Pieces.BlackBishop:
                lists = _blackBishopMoves[cell];
                break;
            case Pieces.BlackRook:
                lists = _blackRookMoves[cell];
                break;
            case Pieces.BlackQueen:
                lists = _blackQueenMoves[cell];
                break;
            case Pieces.BlackKing:
                lists = _blackKingMoves[cell];
                break;
            default:
                yield break;
        }

        var array = lists.GetAll();
        for (byte i = 0; i < array.Length; i++)
        {
            var m = array[i];
            if (m.IsLegal())
                yield return m;
            else
            {
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetMoves(byte piece, byte cell, MoveList moveList)
    {
        moveList.Clear();
        MoveArray lists;
        switch (piece)
        {
            case Pieces.WhitePawn:
                lists = _whitePawnMoves[cell];
                break;
            case Pieces.WhiteKnight:
                lists = _whiteKnightMoves[cell];
                break;
            case Pieces.WhiteBishop:
                lists = _whiteBishopMoves[cell];
                break;
            case Pieces.WhiteRook:
                lists = _whiteRookMoves[cell];
                break;
            case Pieces.WhiteQueen:
                lists = _whiteQueenMoves[cell];
                break;
            case Pieces.WhiteKing:
                lists = _whiteKingMoves[cell];
                break;
            case Pieces.BlackPawn:
                lists = _blackPawnMoves[cell];
                break;
            case Pieces.BlackKnight:
                lists = _blackKnightMoves[cell];
                break;
            case Pieces.BlackBishop:
                lists = _blackBishopMoves[cell];
                break;
            case Pieces.BlackRook:
                lists = _blackRookMoves[cell];
                break;
            case Pieces.BlackQueen:
                lists = _blackQueenMoves[cell];
                break;
            case Pieces.BlackKing:
                lists = _blackKingMoves[cell];
                break;
            default:
                return;
        }

        var array = lists.GetAll();
        for (byte i = 0; i < array.Length; i++)
        {
            var m = array[i];
            if (m?.IsLegal() == true)
                moveList.Add(m);
        }
    }

    public void SetBoard(Board b)
    {
        _board = b;
        _whitePawnRank2 = b.GetRank(1);
        _whitePawnRank4 = b.GetRank(3);
        _blackPawnRank5 = b.GetRank(4);
        _blackPawnRank7 = b.GetRank(6);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetAttackPattern(byte piece, byte position) => _attackPatterns[piece][position];

    #endregion
}
