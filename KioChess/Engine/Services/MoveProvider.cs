﻿using System.Runtime.CompilerServices;
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

            foreach (var piece in Enum.GetValues(typeof(Piece)).OfType<Piece>())
            {
                _moves[piece.AsByte()] = new DynamicArray<MoveList>[_squaresNumber];
                _attacks[piece.AsByte()] = new DynamicArray<AttackList>[_squaresNumber];
                _promotionAttacks[piece.AsByte()] = new DynamicArray<PromotionAttackList>[_squaresNumber];
                _promotions[piece.AsByte()] = new DynamicArray<PromotionList>[_squaresNumber];
                _movesTemp[piece.AsByte()] = new List<List<MoveBase>>[_squaresNumber];
                _attacksTemp[piece.AsByte()] = new List<List<Attack>>[_squaresNumber];
                _promotionsAttackTemp[piece.AsByte()] = new List<List<PromotionAttack>>[_squaresNumber];
                _promotionsTemp[piece.AsByte()] = new List<List<PromotionMove>>[_squaresNumber];
                _attackPatterns[piece.AsByte()] = new BitBoard[_squaresNumber];
                _attacksTo[piece.AsByte()] = new Attack[_squaresNumber][][];
                for (int square = 0; square < _squaresNumber; square++)
                {
                    _movesTemp[piece.AsByte()][square] = new List<List<MoveBase>>();
                    _attacksTemp[piece.AsByte()][square] = new List<List<Attack>>();
                    _promotionsTemp[piece.AsByte()][square] = new List<List<PromotionMove>>();
                    _promotionsAttackTemp[piece.AsByte()][square] = new List<List<PromotionAttack>>();
                    _attackPatterns[piece.AsByte()][square] = new BitBoard(0);
                }

                SetMoves(piece);
                SetAttacks(piece);

                for (int i = 0; i < _squaresNumber; i++)
                {
                    Dictionary<byte, Attack[]> attacksTo = _attacksTemp[piece.AsByte()][i].SelectMany(m => m)
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

                    _attacksTo[piece.AsByte()][i] = aTo;
                }

                if (piece == Piece.WhitePawn || piece == Piece.BlackPawn)
                {
                    for (int i = 0; i < _squaresNumber; i++)
                    {
                        Dictionary<byte, PromotionAttack[]> attacksTo = _promotionsAttackTemp[piece.AsByte()][i].SelectMany(m => m)
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

                        _attacksTo[piece.AsByte()][i] = aTo;
                    }
                }
            }

            foreach (var piece in Enum.GetValues(typeof(Piece)).OfType<Piece>())
            {
                SetAttackPatterns(piece);
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

            HashSet<byte> whitePromotion= new HashSet<byte>() { A6, B6, C6, D6, E6, F6, G6, H6, };
            HashSet<byte> blackPromotion = new HashSet<byte>() { A3, B3, C3, D3, E3, F3, G3, H3, };
            _all = all.ToArray();
            for (var i = 0; i < _all.Length; i++)
            {
                var move = _all[i];
                move.Key = (short)i;
                if (move.Piece == Piece.WhitePawn && move.From > 31)
                {
                    move.IsPassed = move.From > 39;
                    move.CanReduce = false;
                }
                else if (move.Piece == Piece.BlackPawn && move.From < 32)
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

                move.IsIrreversible = move.IsAttack || move.IsCastle || move.IsPromotion || move.Piece == Piece.WhitePawn || move.Piece == Piece.BlackPawn;

                move.IsPromotionExtension = (move.Piece== Piece.BlackPawn && blackPromotion.Contains(move.From))|| (move.Piece == Piece.WhitePawn && whitePromotion.Contains(move.From));
            }

            var promotions = _all.OfType<PromotionMove>();
            foreach (var move in promotions)
            {
                move.IsPromotionToQueen = move.PromotionPiece == Piece.BlackQueen || move.PromotionPiece == Piece.WhiteQueen;
            }

            var promotionAttacks = _all.OfType<PromotionAttack>();
            foreach (var move in promotionAttacks)
            {
                move.IsPromotionToQueen = move.PromotionPiece == Piece.BlackQueen || move.PromotionPiece == Piece.WhiteQueen;
            }

            SetHistory(configurationProvider);

            SetMoves();
            SetPromotions();
            SetAttacks();
            SetPromotionAttacks();

            var overAttacks = _all.OfType<PawnOverAttack>().ToList();
            var whiteOvers = _all.OfType<PawnOverWhiteMove>()
                .ToDictionary(k => k.To);
            var blackOvers = _all.OfType<PawnOverBlackMove>()
                .ToDictionary(k => k.To);
            foreach (var pawnOverAttack in overAttacks)
            {
                var to = pawnOverAttack.To;
                if (pawnOverAttack.Piece == Piece.WhitePawn)
                {
                    byte enPassantSquare = (byte)(to - 8);
                    pawnOverAttack.EnPassant = blackOvers[enPassantSquare];
                }
                else if (pawnOverAttack.Piece == Piece.BlackPawn)
                {
                    byte enPassantSquare = (byte)(to + 8);
                    pawnOverAttack.EnPassant = whiteOvers[enPassantSquare];
                }
                else
                {
                    throw new Exception("Suka");
                }
            }
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
        }

        private void SetPawnAttackPatterns()
        {
            _attackPatterns[Piece.WhitePawn.AsByte()][A1] = B2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][B1] = A2.AsBitBoard() | C2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][C1] = B2.AsBitBoard() | D2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][D1] = C2.AsBitBoard() | E2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][E1] = D2.AsBitBoard() | F2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][F1] = E2.AsBitBoard() | G2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][G1] = F2.AsBitBoard()| H2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][H1] = G2.AsBitBoard();

            _attackPatterns[Piece.BlackPawn.AsByte()][A8] = B7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][B8] = A7.AsBitBoard() | C7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][C8] = B7.AsBitBoard() | D7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][D8] = C7.AsBitBoard() | E7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][E8] = D7.AsBitBoard() | F7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][F8] = E7.AsBitBoard() | G7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][G8] = F7.AsBitBoard() | H7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][H8] = G7.AsBitBoard();
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
            var value = _evaluationService.GetValue(move.Piece.AsByte(), move.To, Phase.Opening);
            move.Difference = value - _evaluationService.GetValue(move.Piece.AsByte(), move.From, Phase.Opening);
        }

        private void SetAttackPatterns(Piece piece)
        {
            foreach (List<List<Attack>> attacks in _attacksTemp[piece.AsByte()])
            {
                if (attacks == null) continue;

                foreach (var moves in attacks)
                {
                    foreach (var move in moves)
                    {
                        _attackPatterns[piece.AsByte()][move.From] = _attackPatterns[piece.AsByte()][move.From] | move.EmptyBoard|move.To.AsBitBoard();
                    }
                }
            }
        }

        private void SetAttacks(Piece piece)
        {
            switch (piece)
            {
                case Piece.WhitePawn:
                    SetWhitePawnAttacks();
                    SetWhitePromotionAttacks();
                    break;
                case Piece.WhiteKnight:
                    SetWhiteKnightAttacks();
                    break;
                case Piece.WhiteBishop:
                    SetWhiteBishopAttacks();
                    break;
                case Piece.WhiteRook:
                    SetWhiteRookAttacks();
                    break;
                case Piece.WhiteKing:
                    SetWhiteKingAttacks();
                    break;
                case Piece.WhiteQueen:
                    SetWhiteQueenAttacks();
                    break;
                case Piece.BlackPawn:
                    SetBlackPawnAttacks();
                    SetBlackPromotionAttacks();
                    break;
                case Piece.BlackKnight:
                    SetBlackKnightAttacks();
                    break;
                case Piece.BlackBishop:
                    SetBlackBishopAttacks();
                    break;
                case Piece.BlackRook:
                    SetBlackRookAttacks();
                    break;
                case Piece.BlackKing:
                    SetBlackKingAttacks();
                    break;
                case Piece.BlackQueen:
                    SetBlackQueenAttacks();
                    break;
            }
        }

        private void SetMoves(Piece piece)
        {
            switch (piece)
            {
                case Piece.WhitePawn:
                    SetWhitePawnMoves();
                    SetWhitePromotionMoves();
                    break;
                case Piece.WhiteKnight:
                    SetWhiteKnightMoves();
                    break;
                case Piece.WhiteBishop:
                    SetMovesWhiteBishop();
                    break;
                case Piece.WhiteRook:
                    SetWhiteRookMoves();
                    break;
                case Piece.WhiteKing:
                    SetWhiteKingMoves();
                    break;
                case Piece.WhiteQueen:
                    SetWhiteQueenMoves();
                    break;
                case Piece.BlackPawn:
                    SetBlackPawnMoves();
                    SetBlackPromotionMoves();
                    break;
                case Piece.BlackKnight:
                    SetBlackKnightMoves();
                    break;
                case Piece.BlackBishop:
                    SetBlackBishopMoves();
                    break;
                case Piece.BlackRook:
                    SetBlackRookMoves();
                    break;
                case Piece.BlackKing:
                    SetBlackKingMoves();
                    break;
                case Piece.BlackQueen:
                    SetBlackQueenMoves();
                    break;
            }
        }

        #region Queens

        private void SetBlackQueenAttacks()
        {
            var piece = Piece.BlackQueen;
            var moves = _attacksTemp[(int)piece];
            SetBlackStrightAttacks(piece, moves);
            SetBlackDiagonalAttacks(piece, moves);
        }

        private void SetWhiteQueenAttacks()
        {
            var piece = Piece.WhiteQueen;
            var moves = _attacksTemp[(int)piece];
            SetWhiteStrightAttacks(piece,  moves);
            SetWhiteDiagonalAttacks(piece, moves);
        }

        private void SetBlackQueenMoves()
        {
            var piece = Piece.BlackQueen;
            var moves = _movesTemp[(int)piece];
            SetDiagonalMoves(piece, moves);
            SetStrightMoves(piece, moves);
        }

        private void SetWhiteQueenMoves()
        {
            var piece = Piece.WhiteQueen;
            var moves = _movesTemp[(int)piece];
            SetDiagonalMoves(piece, moves);
            SetStrightMoves(piece,  moves);
        }

        #endregion

        #region Rooks

        private void SetBlackRookAttacks()
        {
            var piece = Piece.BlackRook;
            var moves = _attacksTemp[(int)piece];
            SetBlackStrightAttacks(piece, moves);
        }

        private void SetWhiteRookAttacks()
        {
            var piece = Piece.WhiteRook;
            var moves = _attacksTemp[(int)piece];
            SetWhiteStrightAttacks(piece, moves);
        }

        private void SetBlackRookMoves()
        {
            var piece = Piece.BlackRook;
            var moves = _movesTemp[(int)piece];
            SetStrightMoves(piece, moves);
        }

        private void SetWhiteRookMoves()
        {
            var piece = Piece.WhiteRook;
            var moves = _movesTemp[(int)piece];
            SetStrightMoves(piece, moves);
        }

        #endregion

        #region Bishops

        private void SetBlackBishopAttacks()
        {
            var piece = Piece.BlackBishop;
            var moves = _attacksTemp[(int)piece];
            SetBlackDiagonalAttacks(piece, moves);
        }

        private void SetWhiteBishopAttacks()
        {
            var piece = Piece.WhiteBishop;
            var moves = _attacksTemp[(int)piece];
            SetWhiteDiagonalAttacks(piece, moves);
        }

        private void SetBlackBishopMoves()
        {
            var piece = Piece.BlackBishop;
            var moves = _movesTemp[(int) piece];
            SetDiagonalMoves(piece, moves);
        }

        private void SetMovesWhiteBishop()
        {
            var piece = Piece.WhiteBishop;
            var moves = _movesTemp[(int)piece];
            SetDiagonalMoves(piece, moves);
        }

        #endregion

        #region Kings

        private void SetBlackKingAttacks()
        {
            var figure = Piece.BlackKing;
            var moves = _attacksTemp[(int)figure];

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
            var figure = Piece.WhiteKing;
            var moves = _attacksTemp[(int)figure];

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
            var figure = Piece.BlackKing;
            var moves = _movesTemp[(int)figure];

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
            var figure = Piece.WhiteKing;
            var moves = _movesTemp[(int)figure];

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
            var figure = Piece.BlackKnight;
            var moves = _attacksTemp[(int)figure];

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
            var figure = Piece.WhiteKnight;
            var moves = _attacksTemp[(int)figure];

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
            var figure = Piece.BlackKnight;
            var moves = _movesTemp[(int)figure];

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
            var figure = Piece.WhiteKnight;
            var moves = _movesTemp[(int) figure];

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
            var figure = Piece.BlackPawn;
            var moves = _promotionsAttackTemp[(int)figure];

            for (int i = 8; i < 16; i++)
            {
                var listLeft = new List<PromotionAttack>(4);
                var listRight = new List<PromotionAttack>(4);
                List<Piece> types = new List<Piece>
                {
                    Piece.BlackQueen,Piece.BlackRook,Piece.BlackBishop,Piece.BlackKnight
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
            var figure = Piece.BlackPawn;
            var moves = _attacksTemp[(int)figure];

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
            var figure = Piece.WhitePawn;
            var moves = _promotionsAttackTemp[(int)figure];
            for (int i = 48; i < 56; i++)
            {
                var listLeft = new List<PromotionAttack>(4);
                var listRight = new List<PromotionAttack>(4);
                List<Piece> types = new List<Piece>
                {
                    Piece.WhiteQueen,Piece.WhiteRook,Piece.WhiteBishop,Piece.WhiteKnight
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
            var figure = Piece.WhitePawn;
            var moves = _attacksTemp[(int)figure];

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
                List<Piece> types = new List<Piece>
                {
                    Piece.BlackQueen,Piece.BlackRook,Piece.BlackBishop,Piece.BlackKnight
                };
                foreach (var type in types)
                {
                    var move = new PromotionMove
                    {
                        From = i,
                        To = (byte)(i - 8),
                        Piece = Piece.BlackPawn,
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
            var figure = Piece.BlackPawn;
            var moves = _movesTemp[(int)figure];
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
                List<Piece> types = new List<Piece>
                {
                    Piece.WhiteQueen,Piece.WhiteRook,Piece.WhiteBishop,Piece.WhiteKnight
                };
                foreach (var type in types)
                {
                    var move = new PromotionMove
                    {
                        From = i,
                        To = (byte)(i + 8),
                        Piece = Piece.WhitePawn,
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
            var figure = Piece.WhitePawn;
            var moves = _movesTemp[(int)figure];
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

        private static void SetStrightMoves(Piece piece, List<List<MoveBase>>[] moves)
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

        private static void SetBlackStrightAttacks(Piece piece, List<List<Attack>>[] moves)
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

        private static void SetWhiteStrightAttacks(Piece piece, List<List<Attack>>[] moves)
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

        private static void SetDiagonalMoves(Piece piece, List<List<MoveBase>>[] moves)
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

        private static void SetBlackDiagonalAttacks(Piece piece, List<List<Attack>>[] moves)
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

        private static void SetWhiteDiagonalAttacks(Piece piece, List<List<Attack>>[] moves)
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
        public IEnumerable<AttackBase> GetAttacks(Piece piece, byte cell)
        {
            var lists = _attacks[piece.AsByte()][cell];
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
        public void GetAttacks(Piece piece, byte @from, AttackList attackList)
        {
            attackList.Clear();
            var lists = _attacks[piece.AsByte()][from];
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
        public bool AnyLegalAttacksTo(Piece piece, byte @from, byte to)
        {
            var attacks = _attacksTo[piece.AsByte()][@from][to];
            if (attacks == null) return false;

            for (var i = 0; i < attacks.Length; i++)
            {
                if (attacks[i].IsLegalAttack(_board))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetMoves(Piece piece, byte cell)
        {
            var lists = _moves[piece.AsByte()][cell];
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
            GetAttacksTo(Piece.WhiteKnight.AsByte(), to, attackList);
            GetAttacksTo(Piece.WhiteQueen.AsByte(), to, attackList);
            GetAttacksTo(Piece.WhiteBishop.AsByte(), to, attackList);
            GetAttacksTo(Piece.WhiteRook.AsByte(), to, attackList);
            GetAttacksTo(Piece.WhiteKing.AsByte(), to, attackList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackAttacksToForPromotion(byte to, AttackList attackList)
        {
            attackList.Clear();
            GetAttacksTo(Piece.BlackKnight.AsByte(), to, attackList);
            GetAttacksTo(Piece.BlackQueen.AsByte(), to, attackList);
            GetAttacksTo(Piece.BlackBishop.AsByte(), to, attackList);
            GetAttacksTo(Piece.BlackRook.AsByte(), to, attackList);
            GetAttacksTo(Piece.BlackKing.AsByte(), to, attackList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetAttacksTo(byte piece, byte to, AttackList attackList)
        {
            var positions = _board.GetPiecePositions(piece);
            for (var p = 0; p < positions.Count; p++)
            {
                var moveWrappers = _attacksTo[piece][positions[p]][to];
                if (moveWrappers == null) continue;

                for (var i = 0; i < moveWrappers.Length; i++)
                {
                    if (moveWrappers[i].IsLegalAttack(_board))
                    {
                        attackList.Add(moveWrappers[i]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetAttackPattern(byte piece, byte position)
        {
            return _attackPatterns[piece][position];
        }

        #endregion
    }
}
