using Engine.DataStructures;
using Engine.Models.Enums;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services.Evaluation;
using System.Runtime.CompilerServices;

namespace Engine.Services;

public class MoveProvider
{
    #region Moves

    private BitBoard _whitePawnRank2;
    private BitBoard _whitePawnRank4;

    private MoveBase[][] _whitePawnMoves;
    private MoveBase[][] _whiteKnightMoves;
    private MoveBase[][] _whiteBishopMoves;
    private MoveBase[][] _whiteRookMoves;
    private MoveBase[][] _whiteQueenMoves;
    private MoveBase[][] _whiteKingMoves;

    private BitBoard _blackPawnRank7;
    private BitBoard _blackPawnRank5;

    private MoveBase[][] _blackPawnMoves;
    private MoveBase[][] _blackKnightMoves;
    private MoveBase[][] _blackBishopMoves;
    private MoveBase[][] _blackRookMoves;
    private MoveBase[][] _blackQueenMoves;
    private MoveBase[][] _blackKingMoves;

    #endregion

    #region Attacks

    private AttackBase[][] _whitePawnAttacks;
    private AttackList[] _whitePawnOverAttacks;
    private AttackBase[][] _whiteKnightAttacks;
    private AttackBase[][] _whiteBishopAttacks;
    private AttackBase[][] _whiteRookAttacks;
    private AttackBase[][] _whiteQueenAttacks;
    private AttackBase[][] _whiteKingAttacks;

    private AttackBase[][] _blackPawnAttacks;
    private AttackList[] _blackPawnOverAttacks;
    private AttackBase[][] _blackKnightAttacks;
    private AttackBase[][] _blackBishopAttacks;
    private AttackBase[][] _blackRookAttacks;
    private AttackBase[][] _blackQueenAttacks;
    private AttackBase[][] _blackKingAttacks;

    #endregion

    #region Promotions

    private readonly List<int> see;

    private readonly PromotionList _emptyPromotions = [];
    private readonly PromotionAttackList _emptyPromotionAttacks = [];

    private PromotionList[][] _whitePromotions;
    private PromotionAttackList[][] _whitePromotionAttacks;

    private PromotionList[][] _blackPromotions;
    private PromotionAttackList[][] _blackPromotionAttacks;

    #endregion

    private readonly MoveBase[] _all;
    private readonly DynamicArray<MoveList>[][] _moves;
    private readonly DynamicArray<AttackList>[][] _attacks;
    private readonly DynamicArray<PromotionList>[][] _promotions;
    private readonly DynamicArray<PromotionAttackList>[][] _promotionAttacks;
    private List<List<MoveBase>>[][] _movesTemp;
    private List<List<AttackBase>>[][] _attacksTemp;
    private List<List<PromotionMove>>[][] _promotionsTemp;
    private List<List<PromotionAttack>>[][] _promotionsAttackTemp;
    private readonly AttackBase[][][][] _attacksTo;

    private readonly BitBoard[] _whitePawnPatterns;
    private readonly BitBoard[] _whiteKnightPatterns;
    private readonly BitBoard[] _whiteBishopPatterns;
    private readonly BitBoard[] _whiteRookPatterns;
    private readonly BitBoard[] _whiteQueenPatterns;
    private readonly BitBoard[] _whiteKingPatterns;
    private readonly BitBoard[] _blackPawnPatterns;
    private readonly BitBoard[] _blackKnightPatterns;
    private readonly BitBoard[] _blackBishopPatterns;
    private readonly BitBoard[] _blackRookPatterns;
    private readonly BitBoard[] _blackQueenPatterns;
    private readonly BitBoard[] _blackKingPatterns;
    private readonly BitBoard[][] _attackPatterns;

    private static readonly int _squaresNumber = 64;
    private readonly int _piecesNumbers = 12;

    private Board _board;

    public MoveProvider(IConfigurationProvider configurationProvider, IStaticValueProvider staticValueProvider)
    {
        _moves = new DynamicArray<MoveList>[_piecesNumbers][];
        _attacks = new DynamicArray<AttackList>[_piecesNumbers][];
        _promotionAttacks = new DynamicArray<PromotionAttackList>[_piecesNumbers][];
        _promotions = new DynamicArray<PromotionList>[_piecesNumbers][];
        _movesTemp = new List<List<MoveBase>>[_piecesNumbers][];
        _attacksTemp = new List<List<AttackBase>>[_piecesNumbers][];
        _promotionsAttackTemp = new List<List<PromotionAttack>>[_piecesNumbers][];
        _promotionsTemp = new List<List<PromotionMove>>[_piecesNumbers][];
        _attackPatterns = new BitBoard[_piecesNumbers][];
        _attacksTo = new AttackBase[_piecesNumbers][][][];

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

            SetMoves((byte)piece);
            SetAttacks((byte)piece);

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
            SetAttackPatterns((byte)piece);
        }

        SetPawnAttackPatterns();

        _whitePawnPatterns = _attackPatterns[Pieces.WhitePawn];
        _whiteKnightPatterns = _attackPatterns[Pieces.WhiteKnight];
        _whiteBishopPatterns = _attackPatterns[Pieces.WhiteBishop];
        _whiteRookPatterns = _attackPatterns[Pieces.WhiteRook];
        _whiteQueenPatterns = _attackPatterns[Pieces.WhiteQueen];
        _whiteKingPatterns = _attackPatterns[Pieces.WhiteKing];
        _blackPawnPatterns = _attackPatterns[Pieces.BlackPawn];
        _blackKnightPatterns = _attackPatterns[Pieces.BlackKnight];
        _blackBishopPatterns = _attackPatterns[Pieces.BlackBishop];
        _blackRookPatterns = _attackPatterns[Pieces.BlackRook];
        _blackQueenPatterns = _attackPatterns[Pieces.BlackQueen];
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
        SetPromotionAttacks();
        SetPawnOver();
    }

    private void SetPawnOver()
    {
        var overAttacks = _all.OfType<PawnOverAttack>().ToList();
        var whiteOvers = _all.OfType<PawnOverWhiteMove>()
            .ToDictionary(k => k.To);
        var blackOvers = _all.OfType<PawnOverBlackMove>()
            .ToDictionary(k => k.To);
        foreach (var pawnOverAttack in overAttacks)
        {
            var to = pawnOverAttack.To;
            if (pawnOverAttack.Piece == Pieces.WhitePawn)
            {
                byte enPassantSquare = (byte)(to - 8);
                pawnOverAttack.EnPassant = blackOvers[enPassantSquare];
            }
            else if (pawnOverAttack.Piece == Pieces.BlackPawn)
            {
                byte enPassantSquare = (byte)(to + 8);
                pawnOverAttack.EnPassant = whiteOvers[enPassantSquare];
            }
            else
            {
                throw new Exception("Suka");
            }
        }

        _whitePawnOverAttacks = GeneratePawnOverAttacks(overAttacks.Where(a => a.Piece == Pieces.WhitePawn));
        _blackPawnOverAttacks = GeneratePawnOverAttacks(overAttacks.Where(a => a.Piece == Pieces.BlackPawn));
    }

    private void SetPromotionAttacks()
    {
        for (byte p = 0; p < _piecesNumbers; p++)
        {
            for (byte s = 0; s < _squaresNumber; s++)
            {
                if (_promotionsAttackTemp[p][s] == null) continue;

                var dynamicArray = new DynamicArray<PromotionAttackList>(_promotionsAttackTemp[p][s].Count);
                for (var i = 0; i < _promotionsAttackTemp[p][s].Count; i++)
                {
                    if (_promotionsAttackTemp[p][s][i] == null) continue;

                    PromotionAttackList moves = new(_promotionsAttackTemp[p][s][i].Count);
                    for (var j = 0; j < _promotionsAttackTemp[p][s][i].Count; j++)
                    {
                        moves.Add(_promotionsAttackTemp[p][s][i][j]);
                    }
                    dynamicArray.Add(moves);
                    _promotionsAttackTemp[p][s][i].Clear();
                    _promotionsAttackTemp[p][s][i] = null;
                }

                _promotionsAttackTemp[p][s].Clear();
                _promotionsAttackTemp[p][s] = null;
                _promotionAttacks[p][s] = dynamicArray;
            }

            Array.Clear(_promotionsAttackTemp[p], 0, _promotionsAttackTemp[p].Length);
            _promotionsAttackTemp[p] = null;
        }

        Array.Clear(_promotionsAttackTemp, 0, _promotionsAttackTemp.Length);
        _promotionsAttackTemp = null;
    }

    private void SetAttacks()
    {
        for (byte p = 0; p < _piecesNumbers; p++)
        {
            for (byte s = 0; s < _squaresNumber; s++)
            {
                if (_attacksTemp[p][s] == null) continue;

                var dynamicArray = new DynamicArray<AttackList>(_attacksTemp[p][s].Count);
                for (var i = 0; i < _attacksTemp[p][s].Count; i++)
                {
                    if (_attacksTemp[p][s][i] == null) continue;

                    AttackList moves = new(_attacksTemp[p][s][i].Count);
                    for (var j = 0; j < _attacksTemp[p][s][i].Count; j++)
                    {
                        moves.Add(_attacksTemp[p][s][i][j]);
                    }
                    dynamicArray.Add(moves);
                    _attacksTemp[p][s][i].Clear();
                    _attacksTemp[p][s][i] = null;
                }

                _attacksTemp[p][s].Clear();
                _attacksTemp[p][s] = null;
                _attacks[p][s] = dynamicArray;
            }

            Array.Clear(_attacksTemp[p], 0, _attacksTemp[p].Length);
            _attacksTemp[p] = null;
        }

        Array.Clear(_attacksTemp, 0, _attacksTemp.Length);
        _attacksTemp = null;


        Dictionary<byte, List<AttackBase>> AttacksMap = _all.OfType<AttackBase>().Where(m => m.IsAttack && !m.IsPromotion && !(m is PawnOverAttack))
            .GroupBy(m => m.Piece)
            .ToDictionary(k => k.Key, v => v.ToList());

        _whitePawnAttacks = GenerateAttacks(AttacksMap[Pieces.WhitePawn]);
        _whiteKnightAttacks = GenerateAttacks(AttacksMap[Pieces.WhiteKnight]);
        _whiteBishopAttacks = GenerateAttacks(AttacksMap[Pieces.WhiteBishop]);
        _whiteRookAttacks = GenerateAttacks(AttacksMap[Pieces.WhiteRook]);
        _whiteQueenAttacks = GenerateAttacks(AttacksMap[Pieces.WhiteQueen]);
        _whiteKingAttacks = GenerateAttacks(AttacksMap[Pieces.WhiteKing]);

        _blackPawnAttacks = GenerateAttacks(AttacksMap[Pieces.BlackPawn]);
        _blackKnightAttacks = GenerateAttacks(AttacksMap[Pieces.BlackKnight]);
        _blackBishopAttacks = GenerateAttacks(AttacksMap[Pieces.BlackBishop]);
        _blackRookAttacks = GenerateAttacks(AttacksMap[Pieces.BlackRook]);
        _blackQueenAttacks = GenerateAttacks(AttacksMap[Pieces.BlackQueen]);
        _blackKingAttacks = GenerateAttacks(AttacksMap[Pieces.BlackKing]);
    }

    private void SetPromotions()
    {
        for (byte p = 0; p < _piecesNumbers; p++)
        {
            for (byte s = 0; s < _squaresNumber; s++)
            {
                if (_promotionsTemp[p][s] == null) continue;

                var dynamicArray = new DynamicArray<PromotionList>(_promotionsTemp[p][s].Count);
                for (var i = 0; i < _promotionsTemp[p][s].Count; i++)
                {
                    if (_promotionsTemp[p][s][i] == null) continue;

                    PromotionList moves = new(_promotionsTemp[p][s][i].Count);
                    for (var j = 0; j < _promotionsTemp[p][s][i].Count; j++)
                    {
                        moves.Add(_promotionsTemp[p][s][i][j]);
                    }

                    dynamicArray.Add(moves);
                    _promotionsTemp[p][s][i].Clear();
                    _promotionsTemp[p][s][i] = null;
                }

                _promotionsTemp[p][s].Clear();
                _promotionsTemp[p][s] = null;
                _promotions[p][s] = dynamicArray;
            }

            Array.Clear(_promotionsTemp[p], 0, _promotionsTemp[p].Length);
            _promotionsTemp[p] = null;
        }

        Array.Clear(_promotionsTemp, 0, _promotionsTemp.Length);
        _promotionsTemp = null;

        var promotions = _all.Where(m => m.IsPromotion).ToList();

        _whitePromotions = GeneratePromotions(promotions.OfType<PromotionMove>().Where(p => p.Piece == Pieces.WhitePawn));
        _blackPromotions = GeneratePromotions(promotions.OfType<PromotionMove>().Where(p => p.Piece == Pieces.BlackPawn));

        _whitePromotionAttacks = GeneratePromotions(promotions.OfType<WhitePromotionAttack>());
        _blackPromotionAttacks = GeneratePromotions(promotions.OfType<BlackPromotionAttack>());
    }

    private void SetMoves()
    {
        for (byte p = 0; p < _piecesNumbers; p++)
        {
            for (byte s = 0; s < _squaresNumber; s++)
            {
                if (_movesTemp[p][s] == null) continue;

                var dynamicArray = new DynamicArray<MoveList>(_movesTemp[p][s].Count);
                for (var i = 0; i < _movesTemp[p][s].Count; i++)
                {
                    if (_movesTemp[p][s][i] == null) continue;

                    MoveList moves = new(_movesTemp[p][s][i].Count);
                    for (var j = 0; j < _movesTemp[p][s][i].Count; j++)
                    {
                        moves.Add(_movesTemp[p][s][i][j]);
                    }

                    dynamicArray.Add(moves);
                    _movesTemp[p][s][i].Clear();
                    _movesTemp[p][s][i] = null;
                }

                _movesTemp[p][s].Clear();
                _movesTemp[p][s] = null;
                _moves[p][s] = dynamicArray;
            }

            Array.Clear(_movesTemp[p], 0, _movesTemp[p].Length);
            _movesTemp[p] = null;
        }

        Array.Clear(_movesTemp, 0, _movesTemp.Length);
        _movesTemp = null;

        Dictionary<byte, List<MoveBase>> movesMap = _all.Where(m => !m.IsAttack && !m.IsPromotion)
            .GroupBy(m => m.Piece)
            .ToDictionary(k => k.Key, v => v.ToList());

        _whitePawnMoves = GenerateMoves(movesMap[Pieces.WhitePawn]);
        _whiteKnightMoves = GenerateMoves(movesMap[Pieces.WhiteKnight]);
        _whiteBishopMoves = GenerateMoves(movesMap[Pieces.WhiteBishop]);
        _whiteRookMoves = GenerateMoves(movesMap[Pieces.WhiteRook]);
        _whiteQueenMoves = GenerateMoves(movesMap[Pieces.WhiteQueen]);
        _whiteKingMoves = GenerateMoves(movesMap[Pieces.WhiteKing]);

        _blackPawnMoves = GenerateMoves(movesMap[Pieces.BlackPawn]);
        _blackKnightMoves = GenerateMoves(movesMap[Pieces.BlackKnight]);
        _blackBishopMoves = GenerateMoves(movesMap[Pieces.BlackBishop]);
        _blackRookMoves = GenerateMoves(movesMap[Pieces.BlackRook]);
        _blackQueenMoves = GenerateMoves(movesMap[Pieces.BlackQueen]);
        _blackKingMoves = GenerateMoves(movesMap[Pieces.BlackKing]);
    }

    private AttackList[] GeneratePawnOverAttacks(IEnumerable<PawnOverAttack> moveBases)
    {
        AttackList[] moves = new AttackList[64];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new AttackList(2);
        }

        var overAttacks = moveBases.GroupBy(m => m.From).ToDictionary(k => k.Key, v => v.ToArray());

        foreach (var over in overAttacks)
        {
            foreach (var attack in over.Value)
            {
                moves[over.Key].Add(attack);
            }
        }

        return moves;
    }

    private AttackBase[][] GenerateAttacks(List<AttackBase> moveBases)
    {
        AttackBase[][] moves = new AttackBase[64][];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new AttackBase[64];
        }

        moveBases.ForEach(m => moves[m.From][m.To] = m);

        return moves;
    }

    private MoveBase[][] GenerateMoves(List<MoveBase> moveBases)
    {
        MoveBase[][] moves = new MoveBase[64][];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new MoveBase[64];
        }

        moveBases.ForEach(m => moves[m.From][m.To] = m);

        return moves;
    }

    private PromotionAttackList[][] GeneratePromotions(IEnumerable<BlackPromotionAttack> enumerable)
    {
        PromotionAttackList[][] moves = new PromotionAttackList[64][];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new PromotionAttackList[64];
        }

        foreach (var m in enumerable)
        {
            if (moves[m.From][m.To] == null)
            {
                moves[m.From][m.To] = new PromotionAttackList(4);
            }
            moves[m.From][m.To].Add(m);
        }

        return moves;
    }

    private PromotionAttackList[][] GeneratePromotions(IEnumerable<WhitePromotionAttack> enumerable)
    {
        PromotionAttackList[][] moves = new PromotionAttackList[64][];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new PromotionAttackList[64];
        }

        foreach (var m in enumerable)
        {
            if (moves[m.From][m.To] == null)
            {
                moves[m.From][m.To] = new PromotionAttackList(4);
            }
            moves[m.From][m.To].Add(m);
        }

        return moves;
    }

    private PromotionList[][] GeneratePromotions(IEnumerable<PromotionMove> enumerable)
    {
        PromotionList[][] moves = new PromotionList[64][];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new PromotionList[64];
        }

        foreach (var m in enumerable)
        {
            if (moves[m.From][m.To] == null)
            {
                moves[m.From][m.To] = new PromotionList(4);
            }
            moves[m.From][m.To].Add(m);
        }

        return moves;
    }

    private void SetPawnAttackPatterns()
    {
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
    }

    #region Private

    private void SetAttackPatterns(byte piece)
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

    private void SetAttacks(byte piece)
    {
        switch (piece)
        {
            case Pieces.WhitePawn:
                SetWhitePawnAttacks();
                SetWhitePromotionAttacks();
                break;
            case Pieces.WhiteKnight:
                SetWhiteKnightAttacks();
                break;
            case Pieces.WhiteBishop:
                SetWhiteBishopAttacks();
                break;
            case Pieces.WhiteRook:
                SetWhiteRookAttacks();
                break;
            case Pieces.WhiteKing:
                SetWhiteKingAttacks();
                break;
            case Pieces.WhiteQueen:
                SetWhiteQueenAttacks();
                break;
            case Pieces.BlackPawn:
                SetBlackPawnAttacks();
                SetBlackPromotionAttacks();
                break;
            case Pieces.BlackKnight:
                SetBlackKnightAttacks();
                break;
            case Pieces.BlackBishop:
                SetBlackBishopAttacks();
                break;
            case Pieces.BlackRook:
                SetBlackRookAttacks();
                break;
            case Pieces.BlackKing:
                SetBlackKingAttacks();
                break;
            case Pieces.BlackQueen:
                SetBlackQueenAttacks();
                break;
        }
    }

    private void SetMoves(byte piece)
    {
        switch (piece)
        {
            case Pieces.WhitePawn:
                SetWhitePawnMoves();
                SetWhitePromotionMoves();
                break;
            case Pieces.WhiteKnight:
                SetWhiteKnightMoves();
                break;
            case Pieces.WhiteBishop:
                SetMovesWhiteBishop();
                break;
            case Pieces.WhiteRook:
                SetWhiteRookMoves();
                break;
            case Pieces.WhiteKing:
                SetWhiteKingMoves();
                break;
            case Pieces.WhiteQueen:
                SetWhiteQueenMoves();
                break;
            case Pieces.BlackPawn:
                SetBlackPawnMoves();
                SetBlackPromotionMoves();
                break;
            case Pieces.BlackKnight:
                SetBlackKnightMoves();
                break;
            case Pieces.BlackBishop:
                SetBlackBishopMoves();
                break;
            case Pieces.BlackRook:
                SetBlackRookMoves();
                break;
            case Pieces.BlackKing:
                SetBlackKingMoves();
                break;
            case Pieces.BlackQueen:
                SetBlackQueenMoves();
                break;
        }
    }

    #region Queens

    private void SetBlackQueenAttacks()
    {
        var piece = Pieces.BlackQueen;
        var moves = _attacksTemp[piece];
        SetBlackStrightAttacks(piece, moves);
        SetBlackDiagonalAttacks(piece, moves);
    }

    private void SetWhiteQueenAttacks()
    {
        var piece = Pieces.WhiteQueen;
        var moves = _attacksTemp[piece];
        SetWhiteStrightAttacks(piece, moves);
        SetWhiteDiagonalAttacks(piece, moves);
    }

    private void SetBlackQueenMoves()
    {
        var piece = Pieces.BlackQueen;
        var moves = _movesTemp[piece];
        SetBlackDiagonalMoves(piece, moves);
        SetBlackStrightMoves(piece, moves);
    }

    private void SetWhiteQueenMoves()
    {
        var piece = Pieces.WhiteQueen;
        var moves = _movesTemp[piece];
        SetWhiteDiagonalMoves(piece, moves);
        SetWhiteStrightMoves(piece, moves);
    }

    #endregion

    #region Rooks

    private void SetBlackRookAttacks()
    {
        var piece = Pieces.BlackRook;
        var moves = _attacksTemp[piece];
        SetBlackStrightAttacks(piece, moves);
    }

    private void SetWhiteRookAttacks()
    {
        var piece = Pieces.WhiteRook;
        var moves = _attacksTemp[piece];
        SetWhiteStrightAttacks(piece, moves);
    }

    private void SetBlackRookMoves()
    {
        var piece = Pieces.BlackRook;
        var moves = _movesTemp[piece];
        SetBlackStrightMoves(piece, moves);
    }

    private void SetWhiteRookMoves()
    {
        var piece = Pieces.WhiteRook;
        var moves = _movesTemp[piece];
        SetWhiteStrightMoves(piece, moves);
    }

    #endregion

    #region Bishops

    private void SetBlackBishopAttacks()
    {
        var piece = Pieces.BlackBishop;
        var moves = _attacksTemp[piece];
        SetBlackDiagonalAttacks(piece, moves);
    }

    private void SetWhiteBishopAttacks()
    {
        var piece = Pieces.WhiteBishop;
        var moves = _attacksTemp[piece];
        SetWhiteDiagonalAttacks(piece, moves);
    }

    private void SetBlackBishopMoves()
    {
        var piece = Pieces.BlackBishop;
        var moves = _movesTemp[piece];
        SetBlackDiagonalMoves(piece, moves);
    }

    private void SetMovesWhiteBishop()
    {
        var piece = Pieces.WhiteBishop;
        var moves = _movesTemp[piece];
        SetWhiteDiagonalMoves(piece, moves);
    }

    #endregion

    #region Kings

    private void SetBlackKingAttacks()
    {
        var figure = Pieces.BlackKing;
        var moves = _attacksTemp[figure];

        for (int from = 0; from < _squaresNumber; from++)
        {
            foreach (int to in KingMoves(from).Where(IsIn))
            {
                var move = new BlackSimpleAttack
                { From = (byte)from, To = (byte)to, Piece = figure };
                moves[from].Add([move]);
            }
        }
    }

    private void SetWhiteKingAttacks()
    {
        var figure = Pieces.WhiteKing;
        var moves = _attacksTemp[figure];

        for (int from = 0; from < _squaresNumber; from++)
        {
            foreach (int to in KingMoves(from).Where(IsIn))
            {
                var move = new WhiteSimpleAttack
                { From = (byte)from, To = (byte)to, Piece = figure };
                moves[from].Add([move]);
            }
        }
    }

    private void SetBlackKingMoves()
    {
        var figure = Pieces.BlackKing;
        var moves = _movesTemp[figure];

        var small = new BlackSmallCastle
        { From = 60, To = 62, Piece = figure };
        small.Set(61, 62);
        moves[60].Add([small]);

        var big = new BlackBigCastle
        { From = 60, To = 58, Piece = figure };
        big.Set(58, 59);
        moves[60].Add([big]);

        for (byte from = 0; from < _squaresNumber; from++)
        {
            foreach (byte to in KingMoves(from).Where(IsIn))
            {
                var move = new BlackMove
                { From = from, To = to, Piece = figure };
                move.Set(to);
                moves[from].Add([move]);
            }
        }
    }

    private void SetWhiteKingMoves()
    {
        var figure = Pieces.WhiteKing;
        var moves = _movesTemp[figure];

        var small = new WhiteSmallCastle
        { From = 4, To = 6, Piece = figure };
        small.Set(5, 6);
        moves[4].Add([small]);

        var big = new WhiteBigCastle
        { From = 4, To = 2, Piece = figure };
        big.Set(2, 3);
        moves[4].Add([big]);

        for (byte from = 0; from < _squaresNumber; from++)
        {
            foreach (byte to in KingMoves(from).Where(IsIn))
            {
                var move = new WhiteMove
                { From = from, To = to, Piece = figure };
                move.Set(to);
                moves[from].Add([move]);
            }
        }
    }

    private IEnumerable<int> KingMoves(int f)
    {
        if (f == 0)
        {
            return new[] { 1, 9, 8 };
        }

        if (f == 7)
        {
            return new[] { 6, 14, 15 };
        }
        if (f == 56)
        {
            return new[] { 48, 49, 57 };
        }
        if (f == 63)
        {
            return new[] { 62, 54, 55 };
        }

        if (f % 8 == 0) //B1 => Squares.A1,Squares.C1,Squares.B2,Squares.A2,Squares.C2
        {
            return new[] { f + 8, f + 9, f + 1, f - 7, f - 8 };
        }
        if (f % 8 == 7)//B8 => Squares.A8,Squares.C8,Squares.B7,Squares.A7,Squares.C7
        {
            return new[] { f + 8, f + 7, f - 1, f - 9, f - 8 };
        }

        if (f / 8 == 0)
        {
            return new[] { f + 1, f - 1, f + 7, f + 9, f + 8 };
        }
        if (f / 8 == 7)
        {
            return new[] { f + 1, f - 7, f - 1, f - 9, f - 8 };
        }

        return new[] { f + 8, f + 7, f - 1, f + 9, f + 1, f - 9, f - 7, f - 8 };
    }

    #endregion

    #region Knights

    private void SetBlackKnightAttacks()
    {
        var figure = Pieces.BlackKnight;
        var moves = _attacksTemp[figure];

        for (int from = 0; from < _squaresNumber; from++)
        {
            foreach (var to in KnightMoves(from).Where(IsIn))
            {
                var move = new BlackSimpleAttack
                { From = (byte)from, To = (byte)to, Piece = figure };
                moves[from].Add([move]);
            }
        }
    }

    private void SetWhiteKnightAttacks()
    {
        var figure = Pieces.WhiteKnight;
        var moves = _attacksTemp[figure];

        for (int from = 0; from < _squaresNumber; from++)
        {
            foreach (var to in KnightMoves(from).Where(IsIn))
            {
                var move = new WhiteSimpleAttack
                { From = (byte)from, To = (byte)to, Piece = figure };
                moves[from].Add([move]);
            }
        }
    }

    private void SetBlackKnightMoves()
    {
        var figure = Pieces.BlackKnight;
        var moves = _movesTemp[figure];

        for (byte from = 0; from < _squaresNumber; from++)
        {
            foreach (byte to in KnightMoves(from).Where(IsIn))
            {
                var move = new BlackMove
                { From = from, To = to, Piece = figure };
                move.Set(to);
                moves[from].Add([move]);
            }
        }
    }

    private void SetWhiteKnightMoves()
    {
        var figure = Pieces.WhiteKnight;
        var moves = _movesTemp[figure];

        for (byte from = 0; from < _squaresNumber; from++)
        {
            foreach (byte to in KnightMoves(from).Where(IsIn))
            {
                var move = new WhiteMove
                { From = from, To = to, Piece = figure };
                move.Set(to);
                moves[from].Add([move]);
            }
        }
    }

    private static IEnumerable<int> KnightMoves(int i)
    {
        if (i / 8 - 1 == (i - 10) / 8)
        {
            yield return i - 10;
        }
        if (i / 8 - 2 == (i - 17) / 8)
        {
            yield return i - 17;
        }
        if (i / 8 + 1 == (i + 6) / 8)
        {
            yield return i + 6;
        }
        if (i / 8 + 1 == (i + 10) / 8)
        {
            yield return i + 10;
        }
        if (i / 8 - 1 == (i - 6) / 8)
        {
            yield return i - 6;
        }
        if (i / 8 - 2 == (i - 15) / 8)
        {
            yield return i - 15;
        }
        if (i / 8 + 2 == (i + 15) / 8)
        {
            yield return i + 15;
        }
        if (i / 8 + 2 == (i + 17) / 8)
        {
            yield return i + 17;
        }
    }

    #endregion

    #region Pawns

    private void SetBlackPromotionAttacks()
    {
        var figure = Pieces.BlackPawn;
        var moves = _promotionsAttackTemp[figure];

        for (int i = 8; i < 16; i++)
        {
            var listLeft = new List<PromotionAttack>(4);
            var listRight = new List<PromotionAttack>(4);
            List<byte> types =
            [
                Pieces.BlackQueen,Pieces.BlackRook,Pieces.BlackBishop,Pieces.BlackKnight
            ];
            for (int j = 0; j < types.Count; j++)
            {
                if (i < 15)
                {
                    var a1 = new BlackPromotionAttack
                    {
                        From = (byte)i,
                        To = (byte)(i - 7),
                        Piece = figure,
                        PromotionPiece = types[j],
                        PromotionSee = see[j]
                    };
                    listLeft.Add(a1);
                }

                if (i > 8)
                {
                    var a2 = new BlackPromotionAttack
                    {
                        From = (byte)i,
                        To = (byte)(i - 9),
                        Piece = figure,
                        PromotionPiece = types[j],
                        PromotionSee = see[j]
                    };
                    listRight.Add(a2);
                }
            }

            moves[i].Add(listLeft);
            moves[i].Add(listRight);
        }
    }

    private void SetBlackPawnAttacks()
    {
        var figure = Pieces.BlackPawn;
        var moves = _attacksTemp[figure];

        for (int i = 16; i < 56; i++)
        {
            int x = i % 8;

            if (x < 7)
            {
                var a1 = new BlackSimpleAttack
                {
                    From = (byte)i,
                    To = (byte)(i - 7),
                    Piece = figure
                };
                moves[i].Add([a1]);
            }

            if (x > 0)
            {
                var a2 = new BlackSimpleAttack
                {
                    From = (byte)i,
                    To = (byte)(i - 9),
                    Piece = figure
                };
                moves[i].Add([a2]);
            }
        }

        for (int i = 24; i < 32; i++)
        {
            if (i < 31)
            {
                var a1 = new PawnOverBlackAttack
                {
                    From = (byte)i,
                    To = (byte)(i - 7),
                    Piece = figure
                };
                moves[i].Add([a1]);
            }

            if (i > 24)
            {
                var a2 = new PawnOverBlackAttack
                {
                    From = (byte)i,
                    To = (byte)(i - 9),
                    Piece = figure
                };
                moves[i].Add([a2]);
            }
        }
    }

    private void SetWhitePromotionAttacks()
    {
        var figure = Pieces.WhitePawn;
        var moves = _promotionsAttackTemp[figure];
        for (int i = 48; i < 56; i++)
        {
            var listLeft = new List<PromotionAttack>(4);
            var listRight = new List<PromotionAttack>(4);
            List<byte> types =
            [
                Pieces.WhiteQueen,Pieces.WhiteRook,Pieces.WhiteBishop,Pieces.WhiteKnight
            ];
            for (int j = 0; j < types.Count; j++)
            {
                if (i > 48)
                {
                    var a1 = new WhitePromotionAttack
                    {
                        From = (byte)i,
                        To = (byte)(i + 7),
                        Piece = figure,
                        PromotionPiece = types[j],
                        PromotionSee = see[j]
                    };
                    listLeft.Add(a1);
                }

                if (i < 55)
                {
                    var a2 = new WhitePromotionAttack
                    {
                        From = (byte)i,
                        To = (byte)(i + 9),
                        Piece = figure,
                        PromotionPiece = types[j],
                        PromotionSee = see[j]
                    };
                    listRight.Add(a2);
                }
            }

            moves[i].Add(listLeft);
            moves[i].Add(listRight);
        }
    }

    private void SetWhitePawnAttacks()
    {
        var figure = Pieces.WhitePawn;
        var moves = _attacksTemp[figure];

        for (int i = 8; i < 48; i++)
        {
            int x = i % 8;

            if (x > 0)
            {
                var a1 = new WhiteSimpleAttack
                {
                    From = (byte)i,
                    To = (byte)(i + 7),
                    Piece = figure
                };
                moves[i].Add([a1]);
            }

            if (x < 7)
            {
                var a2 = new WhiteSimpleAttack
                {
                    From = (byte)i,
                    To = (byte)(i + 9),
                    Piece = figure
                };
                moves[i].Add([a2]);
            }
        }

        for (int i = 32; i < 40; i++)
        {
            if (i > 32)
            {
                var b = i - 1;
                var a1 = new PawnOverWhiteAttack
                {
                    From = (byte)i,
                    To = (byte)(i + 7),
                    Piece = figure
                };
                moves[i].Add([a1]);
            }

            if (i < 39)
            {
                var b = i + 1;
                var a2 = new PawnOverWhiteAttack
                {
                    From = (byte)i,
                    To = (byte)(i + 9),
                    Piece = figure
                };
                moves[i].Add([a2]);
            }
        }
    }

    private void SetBlackPromotionMoves()
    {
        var moves = _promotionsTemp[6];
        for (byte i = 8; i < 16; i++)
        {
            var list = new List<PromotionMove>(4);
            List<byte> types =
            [
                Pieces.BlackQueen,Pieces.BlackRook,Pieces.BlackBishop,Pieces.BlackKnight
            ];

            for (int j = 0; j < types.Count; j++)
            {
                var move = new PromotionBlackMove
                {
                    From = i,
                    To = (byte)(i - 8),
                    Piece = Pieces.BlackPawn,
                    PromotionPiece = types[j],
                    PromotionSee = see[j]
                };


                move.Set((byte)(i - 8));
                list.Add(move);
            }
            moves[i].Add(list);
        }
    }

    private void SetBlackPawnMoves()
    {
        var figure = Pieces.BlackPawn;
        var moves = _movesTemp[figure];
        for (byte i = 48; i < 56; i++)
        {
            var to = i - 16;
            var move = new PawnOverBlackMove()
            { From = i, To = (byte)to, Piece = figure };

            if (i == 48)
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to + 1);
            }
            else if (i == 55)
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to - 1);
            }
            else
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to - 1);
                move.OpponentPawns |= move.OpponentPawns.Add(to + 1);
            }

            move.Set((byte)(i - 8), (byte)to);
            moves[i].Add([move]);
        }

        for (byte i = 16; i < 56; i++)
        {
            var move = new BlackMove
            { From = i, To = (byte)(i - 8), Piece = figure };
            move.Set((byte)(i - 8));
            moves[i].Add([move]);
        }
    }

    private void SetWhitePromotionMoves()
    {
        var moves = _promotionsTemp[0];
        for (byte i = 48; i < 56; i++)
        {
            var list = new List<PromotionMove>(4);
            List<byte> types =
            [
                Pieces.WhiteQueen,Pieces.WhiteRook,Pieces.WhiteBishop,Pieces.WhiteKnight
            ];
            for (int j = 0; j < types.Count; j++)
            {
                var move = new PromotionWhiteMove
                {
                    From = i,
                    To = (byte)(i + 8),
                    Piece = Pieces.WhitePawn,
                    PromotionPiece = types[j],
                    PromotionSee = see[j]
                };
                move.Set((byte)(i + 8));
                list.Add(move);
            }
            moves[i].Add(list);
        }
    }

    private void SetWhitePawnMoves()
    {
        var figure = Pieces.WhitePawn;
        var moves = _movesTemp[figure];
        for (byte i = 8; i < 16; i++)
        {
            var to = i + 16;
            var move = new PawnOverWhiteMove
            { From = i, To = (byte)to, Piece = figure };
            if (i == 8)
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to + 1);
            }
            else if (i == 15)
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to - 1);
            }
            else
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to - 1);
                move.OpponentPawns |= move.OpponentPawns.Add(to + 1);
            }

            move.Set((byte)(i + 8), (byte)to);
            moves[i].Add([move]);
        }

        for (byte i = 8; i < 48; i++)
        {
            var move = new WhiteMove
            { From = i, To = (byte)(i + 8), Piece = figure };
            move.Set((byte)(i + 8));
            moves[i].Add([move]);
        }
    }

    #endregion

    private static void SetBlackStrightMoves(byte piece, List<List<MoveBase>>[] moves)
    {
        for (byte y = 0; y < 8; y++)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte cF = (byte)(y * 8 + x);

                var l = new List<MoveBase>();
                int offset = 1;
                var a = x - 1;
                while (a > -1)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new BlackMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)(y * 8 + x - i));
                    }

                    l.Add(move);
                    a--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                a = x + 1;
                while (a < 8)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new BlackMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)(y * 8 + x + i));
                    }

                    l.Add(move);
                    a++;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                var b = y - 1;
                while (b > -1)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new BlackMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)((y - i) * 8 + x));
                    }

                    l.Add(move);
                    b--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                b = y + 1;
                while (b < 8)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new BlackMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)((y + i) * 8 + x));
                    }

                    l.Add(move);
                    b++;
                    offset++;
                }
                moves[cF].Add(l);
            }
        }
    }

    private static void SetWhiteStrightMoves(byte piece, List<List<MoveBase>>[] moves)
    {
        for (byte y = 0; y < 8; y++)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte cF = (byte)(y * 8 + x);

                var l = new List<MoveBase>();
                int offset = 1;
                var a = x - 1;
                while (a > -1)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new WhiteMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)(y * 8 + x - i));
                    }

                    l.Add(move);
                    a--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                a = x + 1;
                while (a < 8)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new WhiteMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)(y * 8 + x + i));
                    }

                    l.Add(move);
                    a++;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                var b = y - 1;
                while (b > -1)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new WhiteMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)((y - i) * 8 + x));
                    }

                    l.Add(move);
                    b--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                b = y + 1;
                while (b < 8)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new WhiteMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)((y + i) * 8 + x));
                    }

                    l.Add(move);
                    b++;
                    offset++;
                }
                moves[cF].Add(l);
            }
        }
    }

    private static void SetBlackStrightAttacks(byte piece, List<List<AttackBase>>[] moves)
    {
        for (byte y = 0; y < 8; y++)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte cF = (byte)(y * 8 + x);

                var l = new List<AttackBase>();
                int offset = 1;
                var a = x - 1;
                while (a > -1)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new BlackAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)((y * 8) + x - i));
                    }

                    l.Add(move);
                    a--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                a = x + 1;
                while (a < 8)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new BlackAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)(y * 8 + x + i));
                    }

                    l.Add(move);
                    a++;
                    offset++;
                }
                moves[cF].Add(l);


                l = [];
                offset = 1;
                var b = y - 1;
                while (b > -1)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new BlackAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)((y - i) * 8 + x));
                    }

                    l.Add(move);
                    b--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                b = y + 1;
                while (b < 8)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new BlackAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)((y + i) * 8 + x));
                    }

                    l.Add(move);
                    b++;
                    offset++;
                }
                moves[cF].Add(l);
            }
        }
    }

    private static void SetWhiteStrightAttacks(byte piece, List<List<AttackBase>>[] moves)
    {
        for (byte y = 0; y < 8; y++)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte cF = (byte)(y * 8 + x);

                var l = new List<AttackBase>();
                int offset = 1;
                var a = x - 1;
                while (a > -1)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new WhiteAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)(y * 8 + x - i));
                    }

                    l.Add(move);
                    a--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                a = x + 1;
                while (a < 8)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new WhiteAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)(y * 8 + x + i));
                    }

                    l.Add(move);
                    a++;
                    offset++;
                }
                moves[cF].Add(l);


                l = [];
                offset = 1;
                var b = y - 1;
                while (b > -1)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new WhiteAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)((y - i) * 8 + x));
                    }

                    l.Add(move);
                    b--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                b = y + 1;
                while (b < 8)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new WhiteAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)((y + i) * 8 + x));
                    }

                    l.Add(move);
                    b++;
                    offset++;
                }
                moves[cF].Add(l);
            }
        }
    }

    private static void SetWhiteDiagonalMoves(byte piece, List<List<MoveBase>>[] moves)
    {
        for (byte i = 0; i < _squaresNumber; i++)
        {
            int x = i % 8;
            int y = i / 8;

            int a = x + 1;
            int b = y + 1;

            var l = new List<MoveBase>();
            int to = i + 9;
            while (to < _squaresNumber && a < 8 && b < 8)
            {
                var m = new WhiteMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 9; j <= to; j += 9)
                {
                    m.Set(j);
                }
                l.Add(m);
                to += 9;
                a++;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y + 1;
            to = i + 7;
            while (to < _squaresNumber && a > -1 && b < 8)
            {
                var m = new WhiteMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 7; j <= to; j += 7)
                {
                    m.Set(j);
                }
                l.Add(m);

                to += 7;
                a--;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x + 1;
            b = y - 1;
            to = i - 7;
            while (to > -1 && a < 8 && b > -1)
            {
                var m = new WhiteMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 7; j >= to; j -= 7)
                {
                    m.Set(j);
                }
                l.Add(m);

                to -= 7;
                a++;
                b--;
            }
            moves[i].Add(l);


            l = [];
            a = x - 1;
            b = y - 1;
            to = i - 9;
            while (to > -1 && a > -1 && b > -1)
            {
                var m = new WhiteMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 9; j >= to; j -= 9)
                {
                    m.Set(j);
                }
                l.Add(m);

                to -= 9;
                a--;
                b--;
            }
            moves[i].Add(l);
        }
    }

    private static void SetBlackDiagonalMoves(byte piece, List<List<MoveBase>>[] moves)
    {
        for (byte i = 0; i < _squaresNumber; i++)
        {
            int x = i % 8;
            int y = i / 8;

            int a = x + 1;
            int b = y + 1;

            var l = new List<MoveBase>();
            int to = i + 9;
            while (to < _squaresNumber && a < 8 && b < 8)
            {
                var m = new BlackMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 9; j <= to; j += 9)
                {
                    m.Set(j);
                }
                l.Add(m);
                to += 9;
                a++;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y + 1;
            to = i + 7;
            while (to < _squaresNumber && a > -1 && b < 8)
            {
                var m = new BlackMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 7; j <= to; j += 7)
                {
                    m.Set(j);
                }
                l.Add(m);

                to += 7;
                a--;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x + 1;
            b = y - 1;
            to = i - 7;
            while (to > -1 && a < 8 && b > -1)
            {
                var m = new BlackMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 7; j >= to; j -= 7)
                {
                    m.Set(j);
                }
                l.Add(m);

                to -= 7;
                a++;
                b--;
            }
            moves[i].Add(l);


            l = [];
            a = x - 1;
            b = y - 1;
            to = i - 9;
            while (to > -1 && a > -1 && b > -1)
            {
                var m = new BlackMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 9; j >= to; j -= 9)
                {
                    m.Set(j);
                }
                l.Add(m);

                to -= 9;
                a--;
                b--;
            }
            moves[i].Add(l);
        }
    }

    private static void SetBlackDiagonalAttacks(byte piece, List<List<AttackBase>>[] moves)
    {
        for (byte i = 0; i < _squaresNumber; i++)
        {
            int x = i % 8;
            int y = i / 8;

            int a = x + 1;
            int b = y + 1;

            var l = new List<AttackBase>();
            int to = i + 9;
            while (to < _squaresNumber && a < 8 && b < 8)
            {
                var m = new BlackAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 9; j < to; j += 9)
                {
                    m.Set(j);
                }

                l.Add(m);
                to += 9;
                a++;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y + 1;
            to = i + 7;
            while (to < _squaresNumber && a > -1 && b < 8)
            {
                var m = new BlackAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 7; j < to; j += 7)
                {
                    m.Set(j);
                }

                l.Add(m);

                to += 7;
                a--;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x + 1;
            b = y - 1;
            to = i - 7;
            while (to > -1 && a < 8 && b > -1)
            {
                var m = new BlackAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 7; j > to; j -= 7)
                {
                    m.Set(j);
                }

                l.Add(m);

                to -= 7;
                a++;
                b--;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y - 1;
            to = i - 9;
            while (to > -1 && a > -1 && b > -1)
            {
                var m = new BlackAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 9; j > to; j -= 9)
                {
                    m.Set(j);
                }

                l.Add(m);

                to -= 9;
                a--;
                b--;
            }
            moves[i].Add(l);
        }
    }

    private static void SetWhiteDiagonalAttacks(byte piece, List<List<AttackBase>>[] moves)
    {
        for (byte i = 0; i < _squaresNumber; i++)
        {
            int x = i % 8;
            int y = i / 8;

            int a = x + 1;
            int b = y + 1;

            var l = new List<AttackBase>();
            int to = i + 9;
            while (to < _squaresNumber && a < 8 && b < 8)
            {
                var m = new WhiteAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 9; j < to; j += 9)
                {
                    m.Set(j);
                }

                l.Add(m);
                to += 9;
                a++;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y + 1;
            to = i + 7;
            while (to < _squaresNumber && a > -1 && b < 8)
            {
                var m = new WhiteAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 7; j < to; j += 7)
                {
                    m.Set(j);
                }

                l.Add(m);

                to += 7;
                a--;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x + 1;
            b = y - 1;
            to = i - 7;
            while (to > -1 && a < 8 && b > -1)
            {
                var m = new WhiteAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 7; j > to; j -= 7)
                {
                    m.Set(j);
                }

                l.Add(m);

                to -= 7;
                a++;
                b--;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y - 1;
            to = i - 9;
            while (to > -1 && a > -1 && b > -1)
            {
                var m = new WhiteAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 9; j > to; j -= 9)
                {
                    m.Set(j);
                }

                l.Add(m);

                to -= 9;
                a--;
                b--;
            }
            moves[i].Add(l);
        }
    }

    private bool IsIn(int i) => i > -1 && i < 64;

    #endregion

    #region Implementation of MoveProvider

    public int MovesCount => _all.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase Get(short key) => _all[key];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsAttack(short key) => _all[key].IsAttack;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<MoveBase> GetAll() => _all;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<AttackBase> GetAttacks(byte piece, byte cell)
    {
        var lists = _attacks[piece][cell];
        for (byte i = 0; i < lists.Count; i++)
        {
            var list = lists[i];
            for (byte j = 0; j < list.Count; j++)
            {
                var m = list[j];
                if (m.IsLegal())
                    yield return m;

                else if (!_board.IsEmpty(m.EmptyBoard))
                {
                    break;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetPromotions(byte piece, byte cell, PromotionList promotions)
    {
        promotions.Clear();
        var lists = _promotions[piece][cell];
        for (byte i = 0; i < lists.Count; i++)
        {
            var list = lists[i];
            if (list[0].IsLegal())
                promotions.Add(list);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetPromotions(byte piece, byte cell, List<PromotionAttackList> promotions)
    {
        promotions[0].Clear();
        promotions[1].Clear();
        var lists = _promotionAttacks[piece][cell];
        for (byte i = 0; i < lists.Count; i++)
        {
            var list = lists[i];
            if (list.Count > 0 && list[0].IsLegal())
                promotions[i].Add(list);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAttacks(byte piece, byte cell, AttackList attackList)
    {
        attackList.Clear();
        var lists = _attacks[piece][cell];
        for (byte i = 0; i < lists.Count; i++)
        {
            var list = lists[i];
            for (byte j = 0; j < list.Count; j++)
            {
                var m = list[j];
                if (m.IsLegal())
                    attackList.Add(m);

                else if (!_board.IsEmpty(m.EmptyBoard))
                {
                    break;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<MoveBase> GetMoves(byte piece, byte cell)
    {
        var lists = _moves[piece][cell];
        for (byte i = 0; i < lists.Count; i++)
        {
            var list = lists[i];
            for (byte j = 0; j < list.Count; j++)
            {
                var m = list[j];
                if (m.IsLegal())
                    yield return m;
                else
                {
                    break;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetMoves(byte piece, byte cell, MoveList moveList)
    {
        moveList.Clear();
        var lists = _moves[piece][cell];
        for (byte i = 0; i < lists.Count; i++)
        {
            var list = lists[i];
            for (byte j = 0; j < list.Count; j++)
            {
                var m = list[j];
                if (m.IsLegal())
                    moveList.Add(m);
                else
                {
                    break;
                }
            }
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

    #region Move generation

    #region Moves

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhitePawnMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = (f.AsBitBoard() << 8) & _board.GetEmpty();

            if (board.Any())
            {
                move = _whitePawnMoves[f][board.BitScanForward()];
                if (_board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
            }

            if (_whitePawnRank2.IsSet(f))
            {
                move = _whitePawnMoves[f][f + 16];
                if (move.IsLegal() && _board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKnightMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whiteKnightPatterns[f] & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _whiteKnightMoves[f][position];
                if (_board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteBishopMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _whiteBishopMoves[f][position];
                if (_board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteRookMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _whiteRookMoves[f][position];
                if (_board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteQueenMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _whiteQueenMoves[f][position];
                if (_board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKingMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;
        var f = squares.BitScanForward();
        BitBoard board = _whiteKingPatterns[f] & _board.GetEmpty();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            move = _whiteKingMoves[f][position];
            if (_board.IsWhiteMoveLigal(move))
            {
                moveList.Add(move);
            }
            board = board.Remove(position);
        }

        if (f == Squares.E1)
        {
            move = _whiteKingMoves[Squares.E1][Squares.G1];
            if (move.IsLegal() && _board.IsWhiteCastleLigal(move, Squares.F1))
            {
                moveList.Add(move);
            }
            move = _whiteKingMoves[Squares.E1][Squares.C1];
            if (move.IsLegal() && _board.IsWhiteCastleLigal(move, Squares.D1))
            {
                moveList.Add(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackPawnMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = (f.AsBitBoard() >> 8) & _board.GetEmpty();

            if (board.Any())
            {
                move = _blackPawnMoves[f][board.BitScanForward()];
                if (_board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
            }

            if (_blackPawnRank7.IsSet(f))
            {
                move = _blackPawnMoves[f][f - 16];
                if (move.IsLegal() && _board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKnightMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _blackKnightPatterns[f] & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _blackKnightMoves[f][position];
                if (_board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackBishopMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _blackBishopMoves[f][position];
                if (_board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackRookMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _blackRookMoves[f][position];
                if (_board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackQueenMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _blackQueenMoves[f][position];
                if (_board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKingMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;
        var f = squares.BitScanForward();
        BitBoard board = _blackKingPatterns[f] & _board.GetEmpty();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            move = _blackKingMoves[f][position];
            if (_board.IsBlackMoveLigal(move))
            {
                moveList.Add(move);
            }
            board = board.Remove(position);
        }

        if (f == Squares.E8)
        {
            move = _blackKingMoves[Squares.E8][Squares.G8];
            if (move.IsLegal() && _board.IsBlackCastleLigal(move, Squares.F8))
            {
                moveList.Add(move);
            }
            move = _blackKingMoves[Squares.E8][Squares.C8];
            if (move.IsLegal() && _board.IsBlackCastleLigal(move, Squares.D8))
            {
                moveList.Add(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhitePawnMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = (f.AsBitBoard() << 8) & _board.GetEmpty();

            if (board.Any())
            {
                if (_board.IsWhiteMoveLigal(_whitePawnMoves[f][board.BitScanForward()]))
                    return true;
            }

            if (_whitePawnRank2.IsSet(f))
            {
                var move = _whitePawnMoves[f][f + 16];
                if (move.IsLegal() && _board.IsWhiteMoveLigal(move))
                    return true;
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteKnightMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whiteKnightPatterns[f] & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteKnightMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteBishopMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteBishopMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteRookMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteRookMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteQueenMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteQueenMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteKingMoves(BitBoard squares)
    {
        var f = squares.BitScanForward();
        BitBoard board = _whiteKingPatterns[f] & _board.GetEmpty();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (_board.IsWhiteMoveLigal(_whiteKingMoves[f][position]))
                return true;
            board = board.Remove(position);
        }

        if (f == Squares.E1)
        {
            MoveBase move;
            move = _whiteKingMoves[Squares.E1][Squares.G1];
            if (move.IsLegal() && _board.IsWhiteCastleLigal(move, Squares.F1))
                return true;
            move = _whiteKingMoves[Squares.E1][Squares.C1];
            if (move.IsLegal() && _board.IsWhiteCastleLigal(move, Squares.D1))
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackPawnMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = (f.AsBitBoard() >> 8) & _board.GetEmpty();

            if (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackPawnMoves[f][position]))
                    return true;
            }

            if (_blackPawnRank7.IsSet(f))
            {
                MoveBase move;
                move = _blackPawnMoves[f][f - 16];
                if (move.IsLegal() && _board.IsBlackMoveLigal(move))
                    return true;
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackKnightMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _blackKnightPatterns[f] & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackKnightMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackBishopMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackBishopMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackRookMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackRookMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackQueenMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackQueenMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackKingMoves(BitBoard squares)
    {
        var f = squares.BitScanForward();
        BitBoard board = _blackKingPatterns[f] & _board.GetEmpty();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (_board.IsBlackMoveLigal(_blackKingMoves[f][position]))
                return true;
            board = board.Remove(position);
        }

        if (f == Squares.E8)
        {
            MoveBase move;
            move = _blackKingMoves[Squares.E8][Squares.G8];
            if (move.IsLegal() && _board.IsBlackCastleLigal(move, Squares.F8))
                return true;
            move = _blackKingMoves[Squares.E8][Squares.C8];
            if (move.IsLegal() && _board.IsBlackCastleLigal(move, Squares.D8))
                return true;
        }

        return false;
    }

    #endregion

    #region Attacks

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhitePawnSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whitePawnPatterns[f] & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _whitePawnAttacks[f][position];
                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            if (_blackPawnRank5.IsSet(f))
            {
                for (byte i = 0; i < _whitePawnOverAttacks[f].Count; i++)
                {
                    attack = _whitePawnOverAttacks[f][i];
                    if (to.IsOff(attack.To) && attack.IsLegal() && _board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= attack.To.AsBitBoard();
                    }
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKnightSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whiteKnightPatterns[f] & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _whiteKnightAttacks[f][position];
                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteBishopSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _whiteBishopAttacks[f][position];
                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteRookSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _whiteRookAttacks[f][position];
                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteQueenSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _whiteQueenAttacks[f][position];
                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKingSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        var f = squares.BitScanForward();

        BitBoard board = _whiteKingPatterns[f] & _board.GetBlacks();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (to.IsOff(position))
            {
                attack = _whiteKingAttacks[f][position];
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                    to |= position.AsBitBoard();
                }
            }
            board = board.Remove(position);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackPawnSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _blackPawnPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _blackPawnAttacks[f][position];

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            if (_whitePawnRank4.IsSet(f))
            {
                for (byte i = 0; i < _blackPawnOverAttacks[f].Count; i++)
                {
                    attack = _blackPawnOverAttacks[f][i];
                    if (to.IsOff(attack.To) && attack.IsLegal() && _board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= attack.To.AsBitBoard();
                    }
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKnightSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _blackKnightPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _blackKnightAttacks[f][position];

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackBishopSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _blackBishopAttacks[f][position];

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackRookSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _blackRookAttacks[f][position];

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackQueenSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _blackQueenAttacks[f][position];

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKingSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        var f = squares.BitScanForward();

        BitBoard board = _blackKingPatterns[f] & _board.GetWhites();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (to.IsOff(position))
            {
                attack = _blackKingAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                    to |= position.AsBitBoard();
                }
            }
            board = board.Remove(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhitePawnAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whitePawnPatterns[f] & _board.GetBlacks();
            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _whitePawnAttacks[f][position];
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            if (_blackPawnRank5.IsSet(f))
            {
                for (byte i = 0; i < _whitePawnOverAttacks[f].Count; i++)
                {
                    attack = _whitePawnOverAttacks[f][i];
                    if (attack.IsLegal() && _board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                    }
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKnightAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whiteKnightPatterns[f] & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _whiteKnightAttacks[f][position];
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteBishopAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _whiteBishopAttacks[f][position];
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteRookAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _whiteRookAttacks[f][position];
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteQueenAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _whiteQueenAttacks[f][position];
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKingAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;

        var f = squares.BitScanForward();
        BitBoard board = _whiteKingPatterns[f] & _board.GetBlacks();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            attack = _whiteKingAttacks[f][position];
            if (_board.IsWhiteMoveLigal(attack))
            {
                AttackList.Add(attack);
            }
            board = board.Remove(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackPawnAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = _blackPawnPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _blackPawnAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            if (_whitePawnRank4.IsSet(f))
            {
                for (byte i = 0; i < _blackPawnOverAttacks[f].Count; i++)
                {
                    attack = _blackPawnOverAttacks[f][i];
                    if (attack.IsLegal() && _board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                    }
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKnightAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = _blackKnightPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _blackKnightAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackBishopAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _blackBishopAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackRookAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _blackRookAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackQueenAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _blackQueenAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKingAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;

        var f = squares.BitScanForward();
        BitBoard board = _blackKingPatterns[f] & _board.GetWhites();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            attack = _blackKingAttacks[f][position];

            if (_board.IsBlackMoveLigal(attack))
            {
                AttackList.Add(attack);
            }
            board = board.Remove(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhitePawnAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whitePawnPatterns[f] & _board.GetBlacks();
            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whitePawnAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            if (_blackPawnRank5.IsSet(f))
            {
                AttackBase attack;
                for (byte i = 0; i < _whitePawnOverAttacks[f].Count; i++)
                {
                    attack = _whitePawnOverAttacks[f][i];
                    if (attack.IsLegal() && _board.IsWhiteMoveLigal(attack))
                        return true;
                }
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteKnightAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whiteKnightPatterns[f] & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteKnightAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteBishopAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteBishopAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteRookAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteRookAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteQueenAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteQueenAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteKingAttacks(BitBoard squares)
    {
        var f = squares.BitScanForward();
        BitBoard board = _whiteKingPatterns[f] & _board.GetBlacks();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (_board.IsWhiteMoveLigal(_whiteKingAttacks[f][position]))
                return true;
            board = board.Remove(position);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackPawnAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = _blackPawnPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackPawnAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            if (_whitePawnRank4.IsSet(f))
            {
                AttackBase attack;
                for (byte i = 0; i < _blackPawnOverAttacks[f].Count; i++)
                {
                    attack = _blackPawnOverAttacks[f][i];
                    if (attack.IsLegal() && _board.IsBlackMoveLigal(attack))
                        return true;
                }
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackKnightAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = _blackKnightPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();

                if (_board.IsBlackMoveLigal(_blackKnightAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackBishopAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackBishopAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackRookAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackRookAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackQueenAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackQueenAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackKingAttacks(BitBoard squares)
    {
        var f = squares.BitScanForward();
        BitBoard board = _blackKingPatterns[f] & _board.GetWhites();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (_board.IsBlackMoveLigal(_blackKingAttacks[f][position]))
                return true;

            board = board.Remove(position);
        }

        return false;
    }

    #endregion

    #region Promotions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionList GetWhitePromotions(byte from)
    {
        BitBoard board = (from.AsBitBoard() << 8) & _board.GetEmpty();

        return board.Any() ? _whitePromotions[from][board.BitScanForward()] : _emptyPromotions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionList GetBlackPromotions(byte from)
    {
        BitBoard board = (from.AsBitBoard() >> 8) & _board.GetEmpty();

        return board.Any() ? _blackPromotions[from][board.BitScanForward()] : _emptyPromotions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionAttackList[] GetWhitePromotionAttacks(byte from)
    {
        PromotionAttackList[] promotions = new PromotionAttackList[] { _emptyPromotionAttacks, _emptyPromotionAttacks };

        BitBoard board = _whitePawnPatterns[from] & _board.GetBlacks();

        if (board.Any())
        {
            byte position = board.BitScanForward();
            promotions[0] = _whitePromotionAttacks[from][position];
            board = board.Remove(position);

            if (board.Any())
            {
                promotions[1] = _whitePromotionAttacks[from][board.BitScanForward()];
            }
        }

        return promotions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionAttackList[] GetBlackPromotionAttacks(byte from)
    {
        PromotionAttackList[] promotions = new PromotionAttackList[] { _emptyPromotionAttacks, _emptyPromotionAttacks };

        BitBoard board = _blackPawnPatterns[from] & _board.GetWhites();

        if (board.Any())
        {
            byte position = board.BitScanForward();
            promotions[0] = _blackPromotionAttacks[from][position];
            board = board.Remove(position);

            if (board.Any())
            {
                promotions[1] = _blackPromotionAttacks[from][board.BitScanForward()];
            }
        }

        return promotions;
    }

    #endregion


    #endregion

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhitePawnAttacks(byte from, byte to) => _whitePawnAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteKnightAttacks(byte from, byte to) => _whiteKnightAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteBishopAttacks(byte from, byte to) => _whiteBishopAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteRookAttacks(byte from, byte to) => _whiteRookAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteQueenAttacks(byte from, byte to) => _whiteQueenAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteKingAttacks(byte from, byte to) => _whiteKingAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackPawnAttacks(byte from, byte to) => _blackPawnAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackKnightAttacks(byte from, byte to) => _blackKnightAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackBishopAttacks(byte from, byte to) => _blackBishopAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackRookAttacks(byte from, byte to) => _blackRookAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackQueenAttacks(byte from, byte to) => _blackQueenAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackKingAttacks(byte from, byte to) => _blackKingAttacks[from][to];
}

