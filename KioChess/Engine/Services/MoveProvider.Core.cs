using Engine.DataStructures;
using Engine.DataStructures.Moves.Arrays;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Boards.Buffers;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services.Evaluation;
using System.Runtime.CompilerServices;

namespace Engine.Services;

[SkipLocalsInit]
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

    private readonly List<int> see;

    private readonly PromotionList _emptyPromotions = [];
    private readonly PromotionAttackList _emptyPromotionAttacks = [];

    private PromotionListArray _whitePromotions;
    private PromotionAttackListArray _whitePromotionAttacks;

    private PromotionListArray _blackPromotions;
    private PromotionAttackListArray _blackPromotionAttacks;

    #endregion

    private readonly MoveBase[] _all;

    private readonly CellBuffer<BitBoard> _whitePawnPatterns;
    private readonly CellBuffer<BitBoard> _whiteKnightPatterns;
    private readonly CellBuffer<BitBoard> _whiteBishopPatterns;
    private readonly CellBuffer<BitBoard> _whiteRookPatterns;
    private readonly CellBuffer<BitBoard> _whiteQueenPatterns;
    private readonly CellBuffer<BitBoard> _whiteKingPatterns;
    private readonly CellBuffer<BitBoard> _blackPawnPatterns;
    private readonly CellBuffer<BitBoard> _blackKnightPatterns;
    private readonly CellBuffer<BitBoard> _blackBishopPatterns;
    private readonly CellBuffer<BitBoard> _blackRookPatterns;
    private readonly CellBuffer<BitBoard> _blackQueenPatterns;
    private readonly CellBuffer<BitBoard> _blackKingPatterns;
    private readonly BitBoard[][] _attackPatterns;

    private static readonly int _squaresNumber = 64;
    private readonly int _piecesNumbers = 12;

    private Board _board;

    public MoveProvider(IConfigurationProvider configurationProvider, IStaticValueProvider staticValueProvider)
    {
        var _moves = new DynamicArray<MoveList>[_piecesNumbers][];
        var _attacks = new DynamicArray<AttackList>[_piecesNumbers][];
        var _promotionAttacks = new DynamicArray<PromotionAttackList>[_piecesNumbers][];
        var _promotions = new DynamicArray<PromotionList>[_piecesNumbers][];
        var _movesTemp = new List<List<MoveBase>>[_piecesNumbers][];
        var _attacksTemp = new List<List<AttackBase>>[_piecesNumbers][];
        var _promotionsAttackTemp = new List<List<PromotionAttack>>[_piecesNumbers][];
        var _promotionsTemp = new List<List<PromotionMove>>[_piecesNumbers][];
        _attackPatterns = new BitBoard[_piecesNumbers][];
        var _attacksTo = new AttackBase[_piecesNumbers][][][];

        var es = new EvaluationServiceOpening(configurationProvider, staticValueProvider);

        see =
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
            _attackPatterns[piece] = new BitBoard[_squaresNumber];
            _attacksTo[piece] = new AttackBase[_squaresNumber][][];
            for (int square = 0; square < _squaresNumber; square++)
            {
                _movesTemp[piece][square] = [];
                _attacksTemp[piece][square] = [];
                _promotionsTemp[piece][square] = [];
                _promotionsAttackTemp[piece][square] = [];
                _attackPatterns[piece][square] = new BitBoard(0);
            }

            SetMoves((byte)piece, _movesTemp, _promotionsTemp);
            SetAttacks((byte)piece, _attacksTemp, _promotionsAttackTemp);

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
            SetAttackPatterns((byte)piece, _attacksTemp);
        }

        SetPawnAttackPatterns();

        _whitePawnPatterns = new();
        SetPieceAttackPatterns(ref _whitePawnPatterns, _attackPatterns[Pieces.WhitePawn]);

        _whiteKnightPatterns = new();
        SetPieceAttackPatterns(ref _whiteKnightPatterns, _attackPatterns[Pieces.WhiteKnight]);

        _whiteBishopPatterns = new();
        SetPieceAttackPatterns(ref _whiteBishopPatterns, _attackPatterns[Pieces.WhiteBishop]);

        _whiteRookPatterns = new();
        SetPieceAttackPatterns(ref _whiteRookPatterns, _attackPatterns[Pieces.WhiteRook]);

        _whiteQueenPatterns = new();
        SetPieceAttackPatterns(ref _whiteQueenPatterns, _attackPatterns[Pieces.WhiteQueen]);

        _whiteKingPatterns = new();
        SetPieceAttackPatterns(ref _whiteKingPatterns, _attackPatterns[Pieces.WhiteKing]);

        _blackPawnPatterns = new();
        SetPieceAttackPatterns(ref _blackPawnPatterns, _attackPatterns[Pieces.BlackPawn]);

        _blackKnightPatterns = new();
        SetPieceAttackPatterns(ref _blackKnightPatterns, _attackPatterns[Pieces.BlackKnight]);

        _blackBishopPatterns = new();
        SetPieceAttackPatterns(ref _blackBishopPatterns, _attackPatterns[Pieces.BlackBishop]);

        _blackRookPatterns = new();
        SetPieceAttackPatterns(ref _blackRookPatterns, _attackPatterns[Pieces.BlackRook]);

        _blackQueenPatterns = new();
        SetPieceAttackPatterns(ref _blackQueenPatterns, _attackPatterns[Pieces.BlackQueen]);

        _blackKingPatterns = new();
        SetPieceAttackPatterns(ref _blackKingPatterns, _attackPatterns[Pieces.BlackKing]);

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
        MoveList.Moves = _all;
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
                move.Turn = Models.Enums.Turn.White;
            }
            else
            {
                move.IsWhite = false;
                move.IsBlack = true;
                move.Turn = Models.Enums.Turn.Black;
            }

            move.IsFutile = !move.IsAttack && !move.IsPromotion;
            move.IsQuiet = !move.IsAttack && !move.IsPromotion;

            move.IsIrreversible = move.IsAttack || move.IsCastle || move.Piece == Pieces.WhitePawn || move.Piece == Pieces.BlackPawn;
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
        SetPawnOver();
    }

    #region Public API

    public int MovesCount => _all.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase Get(short key) => _all[key];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsAttack(short key) => _all[key].IsAttack;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<MoveBase> GetAll() => _all;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAttacks(byte piece, byte cell, AttackList attackList)
    {
        attackList.Clear();

        switch (piece)
        {
            case Pieces.WhitePawn:
                if (cell < Squares.A7)
                {
                    GetWhitePawnAttacks(cell.AsBitBoard(), attackList); 
                }
                break;
            case Pieces.BlackPawn:
                if (cell > Squares.H2)
                {
                    GetBlackPawnAttacks(cell.AsBitBoard(), attackList); 
                }
                break;
            case Pieces.WhiteKnight:
                GetWhiteKnightAttacks(cell.AsBitBoard(), attackList);
                break;
            case Pieces.BlackKnight:
                GetBlackKnightAttacks(cell.AsBitBoard(), attackList);
                break;
            case Pieces.WhiteBishop:
                GetWhiteBishopAttacks(cell.AsBitBoard(), attackList);
                break;
            case Pieces.BlackBishop:
                GetBlackBishopAttacks(cell.AsBitBoard(), attackList);
                break;
            case Pieces.WhiteRook:
                GetWhiteRookAttacks(cell.AsBitBoard(), attackList);
                break;
            case Pieces.BlackRook:
                GetBlackRookAttacks(cell.AsBitBoard(), attackList);
                break;
            case Pieces.WhiteQueen:
                GetWhiteQueenAttacks(cell.AsBitBoard(), attackList);
                break;
            case Pieces.BlackQueen:
                GetBlackQueenAttacks(cell.AsBitBoard(), attackList);
                break;
            case Pieces.WhiteKing:
                GetWhiteKingAttacks(cell.AsBitBoard(), attackList);
                break;
            case Pieces.BlackKing:
                GetBlackKingAttacks(cell.AsBitBoard(), attackList);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetMoves(byte piece, byte cell, MoveList moveList)
    {
        moveList.Clear();
        switch (piece)
        {
            case Pieces.WhitePawn:
                if (cell < Squares.A7)
                {
                    GetWhitePawnMoves(cell.AsBitBoard(), moveList);
                }
                break;
            case Pieces.BlackPawn:
                if (cell > Squares.H2)
                {
                    GetBlackPawnMoves(cell.AsBitBoard(), moveList);
                }
                break;
            case Pieces.WhiteKnight:
                GetWhiteKnightMoves(cell.AsBitBoard(), moveList);
                break;
            case Pieces.BlackKnight:
                GetBlackKnightMoves(cell.AsBitBoard(), moveList);
                break;
            case Pieces.WhiteBishop:
                GetWhiteBishopMoves(cell.AsBitBoard(), moveList);
                break;
            case Pieces.BlackBishop:
                GetBlackBishopMoves(cell.AsBitBoard(), moveList);
                break;
            case Pieces.WhiteRook:
                GetWhiteRookMoves(cell.AsBitBoard(), moveList);
                break;
            case Pieces.BlackRook:
                GetBlackRookMoves(cell.AsBitBoard(), moveList);
                break;
            case Pieces.WhiteQueen:
                GetWhiteQueenMoves(cell.AsBitBoard(), moveList);
                break;
            case Pieces.BlackQueen:
                GetBlackQueenMoves(cell.AsBitBoard(), moveList);
                break;
            case Pieces.WhiteKing:
                GetWhiteKingMoves(cell.AsBitBoard(), moveList);
                break;
            case Pieces.BlackKing:
                GetBlackKingMoves(cell.AsBitBoard(), moveList);
                break;
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
