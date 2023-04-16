using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Newtonsoft.Json;

namespace Engine.Services
{
    public class MoveProvider : IMoveProvider
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

        private readonly PromotionList _emptyPromotions = new PromotionList(0);
        private readonly PromotionAttackList _emptyPromotionAttacks = new PromotionAttackList(0);

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
        private List<List<Attack>>[][] _attacksTemp;
        private List<List<PromotionMove>>[][] _promotionsTemp;
        private List<List<PromotionAttack>>[][] _promotionsAttackTemp;
        private readonly Attack[][][][] _attacksTo;
        private readonly BitBoard[][] _attackPatterns;
        private static readonly int _squaresNumber = 64;
        private readonly int _piecesNumbers = 12;

        private readonly IEvaluationService _evaluationService;
        private IBoard _board;

        public MoveProvider(IEvaluationService evaluationService, IConfigurationProvider configurationProvider)
        {
            _evaluationService = evaluationService;
            _moves = new DynamicArray<MoveList>[_piecesNumbers][];
            _attacks = new DynamicArray<AttackList>[_piecesNumbers][];
            _promotionAttacks = new DynamicArray<PromotionAttackList>[_piecesNumbers][];
            _promotions = new DynamicArray<PromotionList>[_piecesNumbers][];
            _movesTemp = new List<List<MoveBase>>[_piecesNumbers][];
            _attacksTemp = new List<List<Attack>>[_piecesNumbers][];
            _promotionsAttackTemp = new List<List<PromotionAttack>>[_piecesNumbers][];
            _promotionsTemp = new List<List<PromotionMove>>[_piecesNumbers][];
            _attackPatterns = new BitBoard[_piecesNumbers][];
            _attacksTo = new Attack[_piecesNumbers][][][];

            foreach (var piece in Enumerable.Range(0, 12))
            {
                _moves[piece] = new DynamicArray<MoveList>[_squaresNumber];
                _attacks[piece] = new DynamicArray<AttackList>[_squaresNumber];
                _promotionAttacks[piece] = new DynamicArray<PromotionAttackList>[_squaresNumber];
                _promotions[piece] = new DynamicArray<PromotionList>[_squaresNumber];
                _movesTemp[piece] = new List<List<MoveBase>>[_squaresNumber];
                _attacksTemp[piece] = new List<List<Attack>>[_squaresNumber];
                _promotionsAttackTemp[piece] = new List<List<PromotionAttack>>[_squaresNumber];
                _promotionsTemp[piece] = new List<List<PromotionMove>>[_squaresNumber];
                _attackPatterns[piece] = new BitBoard[_squaresNumber];
                _attacksTo[piece] = new Attack[_squaresNumber][][];
                for (int square = 0; square < _squaresNumber; square++)
                {
                    _movesTemp[piece][square] = new List<List<MoveBase>>();
                    _attacksTemp[piece][square] = new List<List<Attack>>();
                    _promotionsTemp[piece][square] = new List<List<PromotionMove>>();
                    _promotionsAttackTemp[piece][square] = new List<List<PromotionAttack>>();
                    _attackPatterns[piece][square] = new BitBoard(0);
                }

                SetMoves((byte)piece);
                SetAttacks((byte)piece);

                for (int i = 0; i < _squaresNumber; i++)
                {
                    Dictionary<byte, Attack[]> attacksTo = _attacksTemp[piece][i].SelectMany(m => m)
                        .GroupBy(g => g.To)
                        .ToDictionary(key => key.Key, v => v.ToArray());
                    Attack[][] aTo = new Attack[_squaresNumber][];
                    for (byte q = 0; q < aTo.Length; q++)
                    {
                        if (attacksTo.TryGetValue(q, out var list))
                        {
                            aTo[q] = list;
                        }
                    }

                    _attacksTo[piece][i] = aTo;
                }

                if (piece == WhitePawn || piece == BlackPawn)
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

            SetValues();

            List<MoveBase> all = new List<MoveBase>();
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

            HashSet<byte> whitePromotion = new HashSet<byte>() { A6, B6, C6, D6, E6, F6, G6, H6, };
            HashSet<byte> blackPromotion = new HashSet<byte>() { A3, B3, C3, D3, E3, F3, G3, H3, };
            _all = all.ToArray();
            for (var i = 0; i < _all.Length; i++)
            {
                var move = _all[i];
                move.Key = (short)i;
                if (move.Piece == WhitePawn && move.From > 31)
                {
                    move.IsPassed = move.From > 39;
                    move.CanReduce = false;
                }
                else if (move.Piece == BlackPawn && move.From < 32)
                {

                    move.IsPassed = move.From < 24;
                    move.CanReduce = false;
                }
                else
                {
                    move.IsPassed = false;
                    move.CanReduce = !move.IsAttack && !move.IsPromotion;
                }

                if (move.Piece.IsWhite())
                {
                    move.IsWhite = true;
                    move.IsBlack = false;
                }
                else
                {
                    move.IsWhite = false;
                    move.IsBlack = true;
                }

                move.IsFutile = !move.IsAttack && !move.IsPromotion;

                move.IsIrreversible = move.IsAttack || move.IsCastle || move.IsPromotion || move.Piece == WhitePawn || move.Piece == BlackPawn;

                move.IsPromotionExtension = (move.Piece == BlackPawn && blackPromotion.Contains(move.From)) || (move.Piece == WhitePawn && whitePromotion.Contains(move.From));
            }

            var promotions = _all.OfType<PromotionMove>();
            foreach (var move in promotions)
            {
                move.IsPromotionToQueen = move.PromotionPiece == BlackQueen || move.PromotionPiece == WhiteQueen;
            }

            var promotionAttacks = _all.OfType<PromotionAttack>();
            foreach (var move in promotionAttacks)
            {
                move.IsPromotionToQueen = move.PromotionPiece == BlackQueen || move.PromotionPiece == WhiteQueen;
            }

            SetHistory(configurationProvider);

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
                if (pawnOverAttack.Piece == WhitePawn)
                {
                    byte enPassantSquare = (byte)(to - 8);
                    pawnOverAttack.EnPassant = blackOvers[enPassantSquare];
                }
                else if (pawnOverAttack.Piece == BlackPawn)
                {
                    byte enPassantSquare = (byte)(to + 8);
                    pawnOverAttack.EnPassant = whiteOvers[enPassantSquare];
                }
                else
                {
                    throw new Exception("Suka");
                }
            }

            _whitePawnOverAttacks = GeneratePawnOverAttacks(overAttacks.Where(a=>a.Piece == WhitePawn));
            _blackPawnOverAttacks = GeneratePawnOverAttacks(overAttacks.Where(a => a.Piece == BlackPawn));
        }

        public void SaveHistory()
        {
            var history = _all.Where(m => !m.IsAttack && m.History > 0).ToDictionary(k => k.Key, v => v.History);
            var json = JsonConvert.SerializeObject(history, Formatting.Indented);
            File.WriteAllText($"History.json", json);
        }

        public void SaveHistory(MoveBase move)
        {
            var history = _all.Where(m =>!m.IsAttack && m.History > 0).ToDictionary(k => k.Key, v => v.History);
            var json = JsonConvert.SerializeObject(history, Formatting.Indented);
            File.WriteAllText($"History_{move.From}_{move.To}.json", json);
        }

        private void SetHistory(IConfigurationProvider configurationProvider)
        {
            if (configurationProvider.GeneralConfiguration.UseHistory)
            {
                var text = File.ReadAllText(@"Config/History.json");
                var moveHistory = JsonConvert.DeserializeObject<Dictionary<short, int>>(text);

                for (var i = 0; i < _all.Length; i++)
                {
                    if (moveHistory.TryGetValue(_all[i].Key, out var history))
                    {
                        _all[i].History = history;
                    }
                }
            }
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

                        PromotionAttackList moves = new PromotionAttackList(_promotionsAttackTemp[p][s][i].Count);
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

                        AttackList moves = new AttackList(_attacksTemp[p][s][i].Count);
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

            _whitePawnAttacks = GenerateAttacks(AttacksMap[WhitePawn]);
            _whiteKnightAttacks = GenerateAttacks(AttacksMap[WhiteKnight]);
            _whiteBishopAttacks = GenerateAttacks(AttacksMap[WhiteBishop]);
            _whiteRookAttacks = GenerateAttacks(AttacksMap[WhiteRook]);
            _whiteQueenAttacks = GenerateAttacks(AttacksMap[WhiteQueen]);
            _whiteKingAttacks = GenerateAttacks(AttacksMap[WhiteKing]);

            _blackPawnAttacks = GenerateAttacks(AttacksMap[BlackPawn]);
            _blackKnightAttacks = GenerateAttacks(AttacksMap[BlackKnight]);
            _blackBishopAttacks = GenerateAttacks(AttacksMap[BlackBishop]);
            _blackRookAttacks = GenerateAttacks(AttacksMap[BlackRook]);
            _blackQueenAttacks = GenerateAttacks(AttacksMap[BlackQueen]);
            _blackKingAttacks = GenerateAttacks(AttacksMap[BlackKing]);
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

                        PromotionList moves = new PromotionList(_promotionsTemp[p][s][i].Count);
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

            var promotions = _all.Where(m=>m.IsPromotion).ToList();

            _whitePromotions = GeneratePromotions(promotions.OfType<PromotionMove>().Where(p => p.Piece == WhitePawn));
            _blackPromotions = GeneratePromotions(promotions.OfType<PromotionMove>().Where(p => p.Piece == BlackPawn));

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

                        MoveList moves = new MoveList(_movesTemp[p][s][i].Count);
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

            _whitePawnMoves = GenerateMoves(movesMap[WhitePawn]);
            _whiteKnightMoves = GenerateMoves(movesMap[WhiteKnight]);
            _whiteBishopMoves = GenerateMoves(movesMap[WhiteBishop]);
            _whiteRookMoves = GenerateMoves(movesMap[WhiteRook]);
            _whiteQueenMoves = GenerateMoves(movesMap[WhiteQueen]);
            _whiteKingMoves = GenerateMoves(movesMap[WhiteKing]);

            _blackPawnMoves = GenerateMoves(movesMap[BlackPawn]);
            _blackKnightMoves = GenerateMoves(movesMap[BlackKnight]);
            _blackBishopMoves = GenerateMoves(movesMap[BlackBishop]);
            _blackRookMoves = GenerateMoves(movesMap[BlackRook]);
            _blackQueenMoves = GenerateMoves(movesMap[BlackQueen]);
            _blackKingMoves = GenerateMoves(movesMap[BlackKing]);
        }

        private AttackList[] GeneratePawnOverAttacks(IEnumerable<PawnOverAttack> moveBases)
        {
            AttackList[] moves = new AttackList[64];
            for (int f = 0; f < 64; f++)
            {
                moves[f] = new AttackList(2);
            }

            var overAttacks = moveBases.GroupBy(m=>m.From).ToDictionary(k=>k.Key, v=>v.ToArray());

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
            for (int f = 0;f < 64; f++)
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
            _attackPatterns[WhitePawn][A1] = B2.AsBitBoard();
            _attackPatterns[WhitePawn][B1] = A2.AsBitBoard() | C2.AsBitBoard();
            _attackPatterns[WhitePawn][C1] = B2.AsBitBoard() | D2.AsBitBoard();
            _attackPatterns[WhitePawn][D1] = C2.AsBitBoard() | E2.AsBitBoard();
            _attackPatterns[WhitePawn][E1] = D2.AsBitBoard() | F2.AsBitBoard();
            _attackPatterns[WhitePawn][F1] = E2.AsBitBoard() | G2.AsBitBoard();
            _attackPatterns[WhitePawn][G1] = F2.AsBitBoard() | H2.AsBitBoard();
            _attackPatterns[WhitePawn][H1] = G2.AsBitBoard();

            _attackPatterns[BlackPawn][A8] = B7.AsBitBoard();
            _attackPatterns[BlackPawn][B8] = A7.AsBitBoard() | C7.AsBitBoard();
            _attackPatterns[BlackPawn][C8] = B7.AsBitBoard() | D7.AsBitBoard();
            _attackPatterns[BlackPawn][D8] = C7.AsBitBoard() | E7.AsBitBoard();
            _attackPatterns[BlackPawn][E8] = D7.AsBitBoard() | F7.AsBitBoard();
            _attackPatterns[BlackPawn][F8] = E7.AsBitBoard() | G7.AsBitBoard();
            _attackPatterns[BlackPawn][G8] = F7.AsBitBoard() | H7.AsBitBoard();
            _attackPatterns[BlackPawn][H8] = G7.AsBitBoard();

            _attackPatterns[WhitePawn][A7] = B8.AsBitBoard();
            _attackPatterns[WhitePawn][B7] = A8.AsBitBoard() | C8.AsBitBoard();
            _attackPatterns[WhitePawn][C7] = B8.AsBitBoard() | D8.AsBitBoard();
            _attackPatterns[WhitePawn][D7] = C8.AsBitBoard() | E8.AsBitBoard();
            _attackPatterns[WhitePawn][E7] = D8.AsBitBoard() | F8.AsBitBoard();
            _attackPatterns[WhitePawn][F7] = E8.AsBitBoard() | G8.AsBitBoard();
            _attackPatterns[WhitePawn][G7] = F8.AsBitBoard()| H8.AsBitBoard();
            _attackPatterns[WhitePawn][H7] = G8.AsBitBoard();

            _attackPatterns[BlackPawn][A2] = B1.AsBitBoard();
            _attackPatterns[BlackPawn][B2] = A1.AsBitBoard() | C1.AsBitBoard();
            _attackPatterns[BlackPawn][C2] = B1.AsBitBoard() | D1.AsBitBoard();
            _attackPatterns[BlackPawn][D2] = C1.AsBitBoard() | E1.AsBitBoard();
            _attackPatterns[BlackPawn][E2] = D1.AsBitBoard() | F1.AsBitBoard();
            _attackPatterns[BlackPawn][F2] = E1.AsBitBoard() | G1.AsBitBoard();
            _attackPatterns[BlackPawn][G2] = F1.AsBitBoard() | H1.AsBitBoard();
            _attackPatterns[BlackPawn][H2] = G1.AsBitBoard();
        }

        #region Private

        private void SetValues()
        {
            foreach (var move in from lists in _movesTemp from list in lists from moves in list from move in moves select move)
            {
                SetValueForMove(move);
            }
        }

        private void SetValueForMove(MoveBase move)
        {
            var value = _evaluationService.GetValue(move.Piece, move.To, Phase.Opening);
            move.Difference = value - _evaluationService.GetValue(move.Piece, move.From, Phase.Opening);
        }

        private void SetAttackPatterns(byte piece)
        {
            foreach (List<List<Attack>> attacks in _attacksTemp[piece])
            {
                if (attacks == null) continue;

                foreach (var moves in attacks)
                {
                    foreach (var move in moves)
                    {
                        _attackPatterns[piece][move.From] = _attackPatterns[piece][move.From] | move.EmptyBoard|move.To.AsBitBoard();
                    }
                }
            }
        }

        private void SetAttacks(byte piece)
        {
            switch (piece)
            {
                case WhitePawn:
                    SetWhitePawnAttacks();
                    SetWhitePromotionAttacks();
                    break;
                case WhiteKnight:
                    SetWhiteKnightAttacks();
                    break;
                case WhiteBishop:
                    SetWhiteBishopAttacks();
                    break;
                case WhiteRook:
                    SetWhiteRookAttacks();
                    break;
                case WhiteKing:
                    SetWhiteKingAttacks();
                    break;
                case WhiteQueen:
                    SetWhiteQueenAttacks();
                    break;
                case BlackPawn:
                    SetBlackPawnAttacks();
                    SetBlackPromotionAttacks();
                    break;
                case BlackKnight:
                    SetBlackKnightAttacks();
                    break;
                case BlackBishop:
                    SetBlackBishopAttacks();
                    break;
                case BlackRook:
                    SetBlackRookAttacks();
                    break;
                case BlackKing:
                    SetBlackKingAttacks();
                    break;
                case BlackQueen:
                    SetBlackQueenAttacks();
                    break;
            }
        }

        private void SetMoves(byte piece)
        {
            switch (piece)
            {
                case WhitePawn:
                    SetWhitePawnMoves();
                    SetWhitePromotionMoves();
                    break;
                case WhiteKnight:
                    SetWhiteKnightMoves();
                    break;
                case WhiteBishop:
                    SetMovesWhiteBishop();
                    break;
                case WhiteRook:
                    SetWhiteRookMoves();
                    break;
                case WhiteKing:
                    SetWhiteKingMoves();
                    break;
                case WhiteQueen:
                    SetWhiteQueenMoves();
                    break;
                case BlackPawn:
                    SetBlackPawnMoves();
                    SetBlackPromotionMoves();
                    break;
                case BlackKnight:
                    SetBlackKnightMoves();
                    break;
                case BlackBishop:
                    SetBlackBishopMoves();
                    break;
                case BlackRook:
                    SetBlackRookMoves();
                    break;
                case BlackKing:
                    SetBlackKingMoves();
                    break;
                case BlackQueen:
                    SetBlackQueenMoves();
                    break;
            }
        }

        #region Queens

        private void SetBlackQueenAttacks()
        {
            var piece = BlackQueen;
            var moves = _attacksTemp[piece];
            SetBlackStrightAttacks(piece, moves);
            SetBlackDiagonalAttacks(piece, moves);
        }

        private void SetWhiteQueenAttacks()
        {
            var piece = WhiteQueen;
            var moves = _attacksTemp[piece];
            SetWhiteStrightAttacks(piece,  moves);
            SetWhiteDiagonalAttacks(piece, moves);
        }

        private void SetBlackQueenMoves()
        {
            var piece = BlackQueen;
            var moves = _movesTemp[piece];
            SetDiagonalMoves(piece, moves);
            SetStrightMoves(piece, moves);
        }

        private void SetWhiteQueenMoves()
        {
            var piece = WhiteQueen;
            var moves = _movesTemp[piece];
            SetDiagonalMoves(piece, moves);
            SetStrightMoves(piece,  moves);
        }

        #endregion

        #region Rooks

        private void SetBlackRookAttacks()
        {
            var piece = BlackRook;
            var moves = _attacksTemp[piece];
            SetBlackStrightAttacks(piece, moves);
        }

        private void SetWhiteRookAttacks()
        {
            var piece = WhiteRook;
            var moves = _attacksTemp[piece];
            SetWhiteStrightAttacks(piece, moves);
        }

        private void SetBlackRookMoves()
        {
            var piece = BlackRook;
            var moves = _movesTemp[piece];
            SetStrightMoves(piece, moves);
        }

        private void SetWhiteRookMoves()
        {
            var piece = WhiteRook;
            var moves = _movesTemp[piece];
            SetStrightMoves(piece, moves);
        }

        #endregion

        #region Bishops

        private void SetBlackBishopAttacks()
        {
            var piece = BlackBishop;
            var moves = _attacksTemp[piece];
            SetBlackDiagonalAttacks(piece, moves);
        }

        private void SetWhiteBishopAttacks()
        {
            var piece = WhiteBishop;
            var moves = _attacksTemp[piece];
            SetWhiteDiagonalAttacks(piece, moves);
        }

        private void SetBlackBishopMoves()
        {
            var piece = BlackBishop;
            var moves = _movesTemp[piece];
            SetDiagonalMoves(piece, moves);
        }

        private void SetMovesWhiteBishop()
        {
            var piece = WhiteBishop;
            var moves = _movesTemp[piece];
            SetDiagonalMoves(piece, moves);
        }

        #endregion

        #region Kings

        private void SetBlackKingAttacks()
        {
            var figure = BlackKing;
            var moves = _attacksTemp[figure];

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (int to in KingMoves(from).Where(IsIn))
                {
                    var move = new BlackSimpleAttack
                    { From = (byte)from, To = (byte)to, Piece = figure };
                    moves[from].Add(new List<Attack> {move});
                }
            }
        }

        private void SetWhiteKingAttacks()
        {
            var figure = WhiteKing;
            var moves = _attacksTemp[figure];

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (int to in KingMoves(from).Where(IsIn))
                {
                    var move = new WhiteSimpleAttack
                    { From = (byte)from, To = (byte)to, Piece = figure };
                    moves[from].Add(new List<Attack> { move});
                }
            }
        }

        private void SetBlackKingMoves()
        {
            var figure = BlackKing;
            var moves = _movesTemp[figure];

            var small = new BlackSmallCastle
            { From = 60, To = 62, Piece = figure };
            small.Set(61, 62);
            moves[60].Add(new List<MoveBase>{small});

            var big = new BlackBigCastle
            { From = 60, To = 58, Piece = figure };
            big.Set(58, 59);
            moves[60].Add(new List<MoveBase> { big});

            for (byte from = 0; from < _squaresNumber; from++)
            {
                foreach (byte to in KingMoves(from).Where(IsIn))
                {
                    var move = new Move
                    { From = from, To = to, Piece = figure };
                    move.Set(to);
                    moves[from].Add(new List<MoveBase> { move});
                }
            }
        }

        private void SetWhiteKingMoves()
        {
            var figure = WhiteKing;
            var moves = _movesTemp[figure];

            var small = new WhiteSmallCastle
            { From = 4, To = 6, Piece = figure };
            small.Set(5, 6);
            moves[4].Add(new List<MoveBase> { small});

            var big = new WhiteBigCastle
            { From = 4, To = 2, Piece = figure };
            big.Set(2, 3);
            moves[4].Add(new List<MoveBase> { big});

            for (byte from = 0; from < _squaresNumber; from++)
            {
                foreach (byte to in KingMoves(from).Where(IsIn))
                {
                    var move = new Move
                    { From = from, To = to, Piece = figure };
                    move.Set(to);
                    moves[from].Add(new List<MoveBase> { move});
                }
            }
        }

        private IEnumerable<int> KingMoves(int f)
        {
            if (f == 0)
            {
                return new[] {1, 9, 8};
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

            if (f % 8 == 0) //B1 => A1,C1,B2,A2,C2
            {
                return new[] { f+8, f+9, f+1, f-7,f-8 };
            }
            if (f % 8 == 7)//B8 => A8,C8,B7,A7,C7
            {
                return new[] { f + 8, f + 7, f - 1, f - 9, f - 8 };
            }

            if (f / 8 == 0)
            {
                return new[] { f + 1, f -1, f + 7, f + 9, f + 8 };
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
            var figure = BlackKnight;
            var moves = _attacksTemp[figure];

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (var to in KnightMoves(from).Where(IsIn))
                {
                    var move = new BlackSimpleAttack
                    { From = (byte)from, To = (byte)to, Piece = figure };
                    moves[from].Add(new List<Attack> { move});
                }
            }
        }

        private void SetWhiteKnightAttacks()
        {
            var figure = WhiteKnight;
            var moves = _attacksTemp[figure];

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (var to in KnightMoves(from).Where(IsIn))
                {
                    var move = new WhiteSimpleAttack
                    { From = (byte)from, To = (byte)to, Piece = figure };
                    moves[from].Add(new List<Attack> { move});
                }
            }
        }

        private void SetBlackKnightMoves()
        {
            var figure = BlackKnight;
            var moves = _movesTemp[figure];

            for (byte from = 0; from < _squaresNumber; from++)
            {
                foreach (byte to in KnightMoves(from).Where(IsIn))
                {
                    var move = new Move
                    { From = from, To = to, Piece = figure };
                    move.Set(to);
                    moves[from].Add(new List<MoveBase> { move});
                }
            }
        }

        private void SetWhiteKnightMoves()
        {
            var figure = WhiteKnight;
            var moves = _movesTemp[figure];

            for (byte from = 0; from < _squaresNumber; from++)
            {
                foreach (byte to in KnightMoves(from).Where(IsIn))
                {
                    var move = new Move
                        { From = from, To = to, Piece = figure};
                    move.Set(to);
                    moves[from].Add(new List<MoveBase> { move});
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
            var figure = BlackPawn;
            var moves = _promotionsAttackTemp[figure];

            for (int i = 8; i < 16; i++)
            {
                var listLeft = new List<PromotionAttack>(4);
                var listRight = new List<PromotionAttack>(4);
                List<byte> types = new List<byte>
                {
                    BlackQueen,BlackRook,BlackBishop,BlackKnight
                };
                foreach (var type in types)
                {
                    if (i < 15)
                    {
                        var a1 = new BlackPromotionAttack
                        {
                            From = (byte)i,
                            To = (byte)(i - 7),
                            Piece = figure,
                            PromotionPiece = type
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
                            PromotionPiece = type
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
            var figure = BlackPawn;
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
                    moves[i].Add(new List<Attack> { a1});
                }

                if (x > 0)
                {
                    var a2 = new BlackSimpleAttack
                    {
                        From = (byte)i,
                        To = (byte)(i - 9),
                        Piece = figure
                    };
                    moves[i].Add(new List<Attack> { a2});
                }
            }

            for (int i = 24; i < 32; i++)
            {
                if (i < 31)
                {
                    var a1 = new PawnOverAttack
                    {
                        From = (byte)i,
                        To = (byte)(i - 7),
                        Piece = figure
                    };
                    moves[i].Add(new List<Attack> { a1});
                }

                if (i > 24)
                {
                    var a2 = new PawnOverAttack
                    {
                        From = (byte)i,
                        To = (byte)(i - 9),
                        Piece = figure
                    };
                    moves[i].Add(new List<Attack> { a2});
                }
            }
        }

        private void SetWhitePromotionAttacks()
        {
            var figure = WhitePawn;
            var moves = _promotionsAttackTemp[figure];
            for (int i = 48; i < 56; i++)
            {
                var listLeft = new List<PromotionAttack>(4);
                var listRight = new List<PromotionAttack>(4);
                List<byte> types = new List<byte>
                {
                    WhiteQueen,WhiteRook,WhiteBishop,WhiteKnight
                };
                foreach (var type in types)
                {
                    if (i > 48)
                    {
                        var a1 = new WhitePromotionAttack
                        {
                            From = (byte)i,
                            To = (byte)(i + 7),
                            Piece = figure,
                            PromotionPiece = type
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
                            PromotionPiece = type
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
            var figure = WhitePawn;
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
                    moves[i].Add(new List<Attack> { a1});
                }

                if (x < 7)
                {
                    var a2 = new WhiteSimpleAttack
                    {
                        From = (byte)i,
                        To = (byte)(i + 9),
                        Piece = figure
                    };
                    moves[i].Add(new List<Attack> { a2});
                }
            }            

            for (int i = 32; i < 40; i++)
            {
                if (i > 32)
                {
                    var b = i - 1;
                    var a1 = new PawnOverAttack
                    {
                        From = (byte)i,
                        To = (byte)(i + 7),
                        Piece = figure
                    };
                    moves[i].Add(new List<Attack> { a1});
                }

                if (i < 39)
                {
                    var b = i + 1;
                    var a2 = new PawnOverAttack
                    {
                        From = (byte)i,
                        To = (byte)(i + 9),
                        Piece = figure
                    };
                    moves[i].Add(new List<Attack> { a2});
                }
            }
        }

        private void SetBlackPromotionMoves()
        {
            var moves = _promotionsTemp[6];
            for (byte i = 8; i < 16; i++)
            {
                var list = new List<PromotionMove>(4);
                List<byte> types = new List<byte>
                {
                    BlackQueen,BlackRook,BlackBishop,BlackKnight
                };
                foreach (var type in types)
                {
                    var move = new PromotionMove
                    {
                        From = i,
                        To = (byte)(i - 8),
                        Piece = BlackPawn,
                        PromotionPiece = type
                    };


                    move.Set((byte)(i - 8));
                    list.Add(move);
                }
                moves[i].Add(list);
            }
        }

        private void SetBlackPawnMoves()
        {
            var figure = BlackPawn;
            var moves = _movesTemp[figure];
            for (byte i = 48; i < 56; i++)
            {
                var to = i - 16;
                var move = new PawnOverBlackMove()
                    { From = i, To = (byte)to, Piece = figure};

                if (i == 48)
                {
                    move.OpponentPawns|= move.OpponentPawns.Add(to+1);
                }
                else if (i == 55)
                {
                    move.OpponentPawns |= move.OpponentPawns.Add(to-1);
                }
                else
                {
                    move.OpponentPawns |= move.OpponentPawns.Add(to-1);
                    move.OpponentPawns |= move.OpponentPawns.Add(to+1);
                }

                move.Set((byte)(i - 8), (byte)to);
                moves[i].Add(new List<MoveBase> { move});
            }

            for (byte i = 16; i < 56; i++)
            {
                var move = new Move
                    { From = i, To = (byte)(i - 8), Piece = figure };
                move.Set((byte)(i - 8));
                moves[i].Add(new List<MoveBase> { move});
            }
        }

        private void SetWhitePromotionMoves()
        {
            var moves = _promotionsTemp[0];
            for (byte i = 48; i < 56; i++)
            {
                var list = new List<PromotionMove>(4);
                List<byte> types = new List<byte>
                {
                    WhiteQueen,WhiteRook,WhiteBishop,WhiteKnight
                };
                foreach (var type in types)
                {
                    var move = new PromotionMove
                    {
                        From = i,
                        To = (byte)(i + 8),
                        Piece = WhitePawn,
                        PromotionPiece = type
                    };
                    move.Set((byte)(i + 8));
                    list.Add(move);
                }
                moves[i].Add(list);
            }
        }

        private void SetWhitePawnMoves()
        {
            var figure = WhitePawn;
            var moves = _movesTemp[figure];
            for (byte i = 8; i < 16; i++)
            {
                var to = i + 16;
                var move = new PawnOverWhiteMove
                { From = i, To = (byte)to, Piece = figure };
                if (i == 8)
                {
                    move.OpponentPawns |= move.OpponentPawns.Add(to+1);
                }
                else if (i == 15)
                {
                    move.OpponentPawns |= move.OpponentPawns.Add(to-1);
                }
                else
                {
                    move.OpponentPawns |= move.OpponentPawns.Add(to - 1);
                    move.OpponentPawns |= move.OpponentPawns.Add(to + 1);
                }

                move.Set((byte)(i + 8), (byte)to);
                moves[i].Add(new List<MoveBase> { move});
            }

            for (byte i = 8; i < 48; i++)
            {
                var move = new Move
                { From = i, To = (byte)(i + 8), Piece = figure };
                move.Set((byte)(i + 8));
                moves[i].Add(new List<MoveBase> { move});
            }
        }

        #endregion

        private static void SetStrightMoves(byte piece, List<List<MoveBase>>[] moves)
        {
            for (byte y = 0; y < 8; y++)
            {
                for (byte x = 0; x < 8; x++)
                {
                    byte cF = (byte)(y *8+x);

                    var l = new List<MoveBase>();
                    int offset = 1;
                    var a = x - 1;
                    while (a > -1)
                    {
                        byte cT = (byte)(y *8+a);
                        var move = new Move { From = cF, To = cT, Piece = piece };
                        for (byte i = 1; i <= offset; i++)
                        {
                            move.Set((byte)(y *8+x-i));
                        }

                        l.Add(move);
                        a--;
                        offset++;
                    }
                    moves[cF].Add(l);

                    l = new List<MoveBase>();
                    offset = 1;
                    a = x + 1;
                    while (a < 8)
                    {
                        byte cT = (byte)(y * 8 + a);
                        var move = new Move { From = cF, To = cT, Piece = piece};
                        for (byte i = 1; i <= offset; i++)
                        {
                            move.Set((byte)(y * 8 + x + i));
                        }

                        l.Add(move);
                        a++;
                        offset++;
                    }
                    moves[cF].Add(l);

                    l = new List<MoveBase>();
                    offset = 1;
                    var b = y - 1;
                    while (b > -1)
                    {
                        byte cT = (byte)(b * 8 + x);
                        var move = new Move { From = cF, To = cT, Piece = piece };
                        for (byte i = 1; i <= offset; i++)
                        {
                            move.Set((byte)((y-i) * 8 + x));
                        }

                        l.Add(move);
                        b--;
                        offset++;
                    }
                    moves[cF].Add(l);

                    l = new List<MoveBase>();
                    offset = 1;
                    b = y + 1;
                    while (b < 8)
                    {
                        byte cT = (byte)(b * 8 + x);
                        var move = new Move { From = cF, To = cT, Piece = piece };
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

        private static void SetBlackStrightAttacks(byte piece, List<List<Attack>>[] moves)
        {
            for (byte y = 0; y < 8; y++)
            {
                for (byte x = 0; x < 8; x++)
                {
                    byte cF = (byte)(y * 8 + x);

                    var l = new List<Attack>();
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

                    l = new List<Attack>();
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


                    l = new List<Attack>();
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

                    l = new List<Attack>();
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

        private static void SetWhiteStrightAttacks(byte piece, List<List<Attack>>[] moves)
        {
            for (byte y = 0; y < 8; y++)
            {
                for (byte x = 0; x < 8; x++)
                {
                    byte cF = (byte)(y * 8 + x);

                    var l = new List<Attack>();
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

                    l = new List<Attack>();
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


                    l = new List<Attack>();
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

                    l = new List<Attack>();
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

        private static void SetDiagonalMoves(byte piece, List<List<MoveBase>>[] moves)
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
                    var m = new Move { From = i, To = (byte)to, Piece = piece };
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

                l = new List<MoveBase>();
                a = x - 1;
                b = y + 1;
                to = i + 7;
                while (to < _squaresNumber&& a > -1 && b < 8)
                {
                    var m = new Move { From = i, To = (byte)to, Piece = piece };
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

                l = new List<MoveBase>();
                a = x + 1;
                b = y - 1;
                to = i - 7;
                while (to > -1 && a < 8 && b > -1)
                {
                    var m = new Move { From = i, To = (byte)to, Piece = piece };
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


                l = new List<MoveBase>();
                a = x - 1;
                b = y - 1;
                to = i - 9;
                while (to > -1 && a > -1 && b > -1)
                {
                    var m = new Move { From = i, To = (byte)to, Piece = piece };
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

        private static void SetBlackDiagonalAttacks(byte piece, List<List<Attack>>[] moves)
        {
            for (byte i = 0; i < _squaresNumber; i++)
            {
                int x = i % 8;
                int y = i / 8;

                int a = x + 1;
                int b = y + 1;

                var l = new List<Attack>();
                int to = i + 9;
                while (to < _squaresNumber && a < 8 && b < 8)
                {
                    var m = new BlackAttack { From = i, To = (byte)to, Piece = piece};
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

                l = new List<Attack>();
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

                l = new List<Attack>();
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

                l = new List<Attack>();
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

        private static void SetWhiteDiagonalAttacks(byte piece, List<List<Attack>>[] moves)
        {
            for (byte i = 0; i < _squaresNumber; i++)
            {
                int x = i % 8;
                int y = i / 8;

                int a = x + 1;
                int b = y + 1;

                var l = new List<Attack>();
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

                l = new List<Attack>();
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

                l = new List<Attack>();
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

                l = new List<Attack>();
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

        private bool IsIn(int i)
        {
            return i > -1 && i < 64;
        }

        #endregion

        #region Implementation of IMoveProvider

        public int MovesCount => _all.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveBase Get(short key)
        {
            return _all[key];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetAll()
        {
            return _all;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<AttackBase> GetAttacks(byte piece, byte cell)
        {
            var lists = _attacks[piece][cell];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(_board))
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
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                if (list[0].IsLegal(_board))
                    promotions.Add(list);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetPromotions(byte piece, byte cell, List<PromotionAttackList> promotions)
        {
            promotions[0].Clear();
            promotions[1].Clear();
            var lists = _promotionAttacks[piece][cell];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                if (list.Count > 0 && list[0].IsLegal(_board))
                    promotions[i].Add(list);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAttacks(byte piece, byte cell, AttackList attackList)
        {
            attackList.Clear();
            var lists = _attacks[piece][cell];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(_board))
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
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(_board))
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
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(_board))
                        moveList.Add(m);
                    else
                    {
                        break;
                    }
                }
            }
        }

        public void SetBoard(IBoard b)
        {
            _board = b;
            _whitePawnRank2 = b.GetRank(1);
            _whitePawnRank4 = b.GetRank(3);
            _blackPawnRank5 = b.GetRank(4);
            _blackPawnRank7 = b.GetRank(6);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AgeHistory()
        {
            for (var i = 0; i < _all.Length; i++)
            {
                if (_all[i].History > 0)
                {
                    _all[i].History /= 2;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteAttacksToForPromotion(byte to, AttackList attackList)
        {
            attackList.Clear();
            GetWhiteKnightAttacksTo(to, attackList);
            GetWhiteQueenAttacksTo(to, attackList);
            GetWhiteBishopAttacksTo(to, attackList);
            GetWhiteRookAttacksTo(to, attackList);
            GetWhiteKingAttacksTo(to, attackList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackAttacksToForPromotion(byte to, AttackList attackList)
        {
            attackList.Clear();
            GetBlackKnightAttacksTo(to, attackList);
            GetBlackQueenAttacksTo(to, attackList);
            GetBlackBishopAttacksTo(to, attackList);
            GetBlackRookAttacksTo(to, attackList);
            GetBlackKingAttacksTo(to, attackList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetAttackPattern(byte piece, byte position)
        {
            return _attackPatterns[piece][position];
        }

        #region Move generation

        #region Moves

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhitePawnMoves(SquareList squares, MoveList moveList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = (squares[f].AsBitBoard() << 8) & _board.GetEmpty();

                if (board.Any())
                {
                    byte position = board.BitScanForward();
                    moveList.Add(_whitePawnMoves[squares[f]][position]);
                }

                if (_whitePawnRank2.IsSet(squares[f]))
                {
                    var move = _whitePawnMoves[squares[f]][squares[f] + 16];
                    if (move.IsLegal(_board))
                    {
                        moveList.Add(move);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteKnightMoves(SquareList squares, MoveList moveList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = _attackPatterns[WhiteKnight][squares[f]] & _board.GetEmpty();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    moveList.Add(_whiteKnightMoves[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteBishopMoves(SquareList squares, MoveList moveList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].BishopAttacks(_board.GetOccupied()) & _board.GetEmpty();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    moveList.Add(_whiteBishopMoves[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteRookMoves(SquareList squares, MoveList moveList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].RookAttacks(_board.GetOccupied()) & _board.GetEmpty();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    moveList.Add(_whiteRookMoves[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteQueenMoves(SquareList squares, MoveList moveList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].QueenAttacks(_board.GetOccupied()) & _board.GetEmpty();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    moveList.Add(_whiteQueenMoves[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteKingMoves(SquareList squares, MoveList moveList)
        {
            BitBoard board = _attackPatterns[WhiteKing][squares[0]] & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                moveList.Add(_whiteKingMoves[squares[0]][position]);
                board = board.Remove(position);
            }

            if (squares[0] == E1)
            {
                var small = _whiteKingMoves[E1][G1];
                if (small.IsLegal(_board))
                {
                    moveList.Add(small);
                }
                var big = _whiteKingMoves[E1][C1];
                if (big.IsLegal(_board))
                {
                    moveList.Add(big);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackPawnMoves(SquareList squares, MoveList moveList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = (squares[f].AsBitBoard() >> 8) & _board.GetEmpty();

                if (board.Any())
                {
                    byte position = board.BitScanForward();
                    moveList.Add(_blackPawnMoves[squares[f]][position]);
                }

                if (_blackPawnRank7.IsSet(squares[f]))
                {
                    var move = _blackPawnMoves[squares[f]][squares[f] - 16];
                    if (move.IsLegal(_board))
                    {
                        moveList.Add(move);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackKnightMoves(SquareList squares, MoveList moveList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = _attackPatterns[BlackKnight][squares[f]] & _board.GetEmpty();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    moveList.Add(_blackKnightMoves[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackBishopMoves(SquareList squares, MoveList moveList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].BishopAttacks(_board.GetOccupied()) & _board.GetEmpty();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    moveList.Add(_blackBishopMoves[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackRookMoves(SquareList squares, MoveList moveList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].RookAttacks(_board.GetOccupied()) & _board.GetEmpty();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    moveList.Add(_blackRookMoves[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackQueenMoves(SquareList squares, MoveList moveList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].QueenAttacks(_board.GetOccupied()) & _board.GetEmpty();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    moveList.Add(_blackQueenMoves[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackKingMoves(SquareList squares, MoveList moveList)
        {
            BitBoard board = _attackPatterns[BlackKing][squares[0]] & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                moveList.Add(_blackKingMoves[squares[0]][position]);
                board = board.Remove(position);
            }

            if (squares[0] == E8)
            {
                var small = _blackKingMoves[E8][G8];
                if (small.IsLegal(_board))
                {
                    moveList.Add(small);
                }
                var big = _blackKingMoves[E8][C8];
                if (big.IsLegal(_board))
                {
                    moveList.Add(big);
                }
            }
        }

        #endregion

        #region Attacks

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhitePawnAttacks(SquareList squares, AttackList AttackList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = _attackPatterns[WhitePawn][squares[f]] & _board.GetBlacks();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    AttackList.Add(_whitePawnAttacks[squares[f]][position]);
                    board = board.Remove(position);
                }

                if (_blackPawnRank5.IsSet(squares[f]))
                {
                    for (int i = 0; i < _whitePawnOverAttacks[squares[f]].Count; i++)
                    {
                        var Attack = _whitePawnOverAttacks[squares[f]][i];
                        if (Attack.IsLegal(_board))
                        {
                            AttackList.Add(Attack);
                        } 
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteKnightAttacks(SquareList squares, AttackList AttackList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = _attackPatterns[WhiteKnight][squares[f]] & _board.GetBlacks();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    AttackList.Add(_whiteKnightAttacks[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteBishopAttacks(SquareList squares, AttackList AttackList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].BishopAttacks(_board.GetOccupied()) & _board.GetBlacks();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    AttackList.Add(_whiteBishopAttacks[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteRookAttacks(SquareList squares, AttackList AttackList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].RookAttacks(_board.GetOccupied()) & _board.GetBlacks();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    AttackList.Add(_whiteRookAttacks[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteQueenAttacks(SquareList squares, AttackList AttackList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].QueenAttacks(_board.GetOccupied()) & _board.GetBlacks();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    AttackList.Add(_whiteQueenAttacks[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteKingAttacks(SquareList squares, AttackList AttackList)
        {
            BitBoard board = _attackPatterns[WhiteKing][squares[0]] & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                AttackList.Add(_whiteKingAttacks[squares[0]][position]);
                board = board.Remove(position);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackPawnAttacks(SquareList squares, AttackList AttackList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = _attackPatterns[BlackPawn][squares[f]] & _board.GetWhites();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    AttackList.Add(_blackPawnAttacks[squares[f]][position]);
                    board = board.Remove(position);
                }

                if (_whitePawnRank4.IsSet(squares[f]))
                {
                    for (int i = 0; i < _blackPawnOverAttacks[squares[f]].Count; i++)
                    {
                        var Attack = _blackPawnOverAttacks[squares[f]][i];
                        if (Attack.IsLegal(_board))
                        {
                            AttackList.Add(Attack);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackKnightAttacks(SquareList squares, AttackList AttackList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = _attackPatterns[BlackKnight][squares[f]] & _board.GetWhites();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    AttackList.Add(_blackKnightAttacks[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackBishopAttacks(SquareList squares, AttackList AttackList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].BishopAttacks(_board.GetOccupied()) & _board.GetWhites();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    AttackList.Add(_blackBishopAttacks[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackRookAttacks(SquareList squares, AttackList AttackList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].RookAttacks(_board.GetOccupied()) & _board.GetWhites();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    AttackList.Add(_blackRookAttacks[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackQueenAttacks(SquareList squares, AttackList AttackList)
        {
            for (byte f = 0; f < squares.Length; f++)
            {
                BitBoard board = squares[f].QueenAttacks(_board.GetOccupied()) & _board.GetWhites();

                while (board.Any())
                {
                    byte position = board.BitScanForward();
                    AttackList.Add(_blackQueenAttacks[squares[f]][position]);
                    board = board.Remove(position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackKingAttacks(SquareList squares, AttackList AttackList)
        {
            BitBoard board = _attackPatterns[BlackKing][squares[0]] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                AttackList.Add(_blackKingAttacks[squares[0]][position]);
                board = board.Remove(position);
            }
        }

        #endregion

        #region Attacks To

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetWhiteKingAttacksTo(byte to, AttackList attackList)
        {
            byte from = _board.GetPieceBits(WhiteKing).BitScanForward();
            if (_attackPatterns[WhiteKing][from].IsSet(to))
            {
                attackList.Add(_whiteKingAttacks[from][to]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetWhiteRookAttacksTo(byte to, AttackList attackList)
        {
            var fromBoard = _board.GetPieceBits(WhiteRook);

            while (fromBoard.Any())
            {
                byte from = fromBoard.BitScanForward();
                if (from.RookAttacks(_board.GetOccupied()).IsSet(to))
                {
                    attackList.Add(_whiteRookAttacks[from][to]);
                }
                fromBoard = fromBoard.Remove(from);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetWhiteBishopAttacksTo(byte to, AttackList attackList)
        {
            var fromBoard = _board.GetPieceBits(WhiteBishop);

            while (fromBoard.Any())
            {
                byte from = fromBoard.BitScanForward();
                if (from.BishopAttacks(_board.GetOccupied()).IsSet(to))
                {
                    attackList.Add(_whiteBishopAttacks[from][to]);
                }
                fromBoard = fromBoard.Remove(from);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetWhiteQueenAttacksTo(byte to, AttackList attackList)
        {
            var fromBoard = _board.GetPieceBits(WhiteQueen);

            while (fromBoard.Any())
            {
                byte from = fromBoard.BitScanForward();
                if (from.QueenAttacks(_board.GetOccupied()).IsSet(to))
                {
                    attackList.Add(_whiteQueenAttacks[from][to]);
                }
                fromBoard = fromBoard.Remove(from);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetWhiteKnightAttacksTo(byte to, AttackList attackList)
        {
            var fromBoard = _board.GetPieceBits(WhiteKnight);

            while (fromBoard.Any())
            {
                byte from = fromBoard.BitScanForward();
                if (_attackPatterns[WhiteKnight][from].IsSet(to))
                {
                    attackList.Add(_whiteKnightAttacks[from][to]);
                }
                fromBoard = fromBoard.Remove(from);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBlackKingAttacksTo(byte to, AttackList attackList)
        {
            byte from = _board.GetPieceBits(BlackKing).BitScanForward();
            if (_attackPatterns[BlackKing][from].IsSet(to))
            {
                attackList.Add(_blackKingAttacks[from][to]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBlackRookAttacksTo(byte to, AttackList attackList)
        {
            var fromBoard = _board.GetPieceBits(BlackRook);

            while (fromBoard.Any())
            {
                byte from = fromBoard.BitScanForward();
                if (from.RookAttacks(_board.GetOccupied()).IsSet(to))
                {
                    attackList.Add(_blackRookAttacks[from][to]);
                }
                fromBoard = fromBoard.Remove(from);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBlackBishopAttacksTo(byte to, AttackList attackList)
        {
            var fromBoard = _board.GetPieceBits(BlackBishop);

            while (fromBoard.Any())
            {
                byte from = fromBoard.BitScanForward();
                if (from.BishopAttacks(_board.GetOccupied()).IsSet(to))
                {
                    attackList.Add(_blackBishopAttacks[from][to]);
                }
                fromBoard = fromBoard.Remove(from);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBlackQueenAttacksTo(byte to, AttackList attackList)
        {
            var fromBoard = _board.GetPieceBits(BlackQueen);

            while (fromBoard.Any())
            {
                byte from = fromBoard.BitScanForward();
                if (from.QueenAttacks(_board.GetOccupied()).IsSet(to))
                {
                    attackList.Add(_blackQueenAttacks[from][to]);
                }
                fromBoard = fromBoard.Remove(from);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBlackKnightAttacksTo(byte to, AttackList attackList)
        {
            var fromBoard = _board.GetPieceBits(BlackKnight);

            while (fromBoard.Any())
            {
                byte from = fromBoard.BitScanForward();
                if (_attackPatterns[BlackKnight][from].IsSet(to))
                {
                    attackList.Add(_blackKnightAttacks[from][to]);
                }
                fromBoard = fromBoard.Remove(from);
            }
        }

        #endregion

        #region Promotions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PromotionList GetWhitePromotions(byte from)
        {
            BitBoard board = (from.AsBitBoard() << 8) & _board.GetEmpty();

            if (board.Any())
            {
                return _whitePromotions[from][board.BitScanForward()];
            }

            return _emptyPromotions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PromotionList GetBlackPromotions(byte from)
        {
            BitBoard board = (from.AsBitBoard() >> 8) & _board.GetEmpty();

            if (board.Any())
            {
                return _blackPromotions[from][board.BitScanForward()];
            }

            return _emptyPromotions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PromotionAttackList[] GetWhitePromotionAttacks(byte from)
        {
            PromotionAttackList[] promotions = new PromotionAttackList[] { _emptyPromotionAttacks, _emptyPromotionAttacks };

            BitBoard board = _attackPatterns[WhitePawn][from] & _board.GetBlacks();

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

            BitBoard board = _attackPatterns[BlackPawn][from] & _board.GetWhites();

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
    }
}
