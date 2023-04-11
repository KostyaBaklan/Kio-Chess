﻿using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Strategies.Models;

namespace Engine.Models.Boards
{
    public class Position : IPosition
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

        private Turn _turn;
        private byte _phase;
        private SortContext _sortContext;
        private readonly ArrayStack<byte> _figureHistory;

        private readonly byte[][] _white;
        private readonly byte[][] _black;
        private readonly byte[][] _whiteAttacks;
        private readonly byte[][] _blackAttacks;

        private readonly SquareList[] _squares;
        private readonly SquareList _promotionSquares;

        private readonly AttackList _attacks;

        private readonly MoveList _moves;

        private readonly PromotionList _promotions;

        private readonly List<PromotionAttackList> _promotionsAttack;
        private readonly List<PromotionAttackList> _promotionsSingleAttack;

        private readonly IBoard _board;
        private readonly IMoveProvider _moveProvider;
        private readonly IMoveHistoryService _moveHistoryService;

        public Position()
        {
            _turn = Turn.White;

            IPieceOrderConfiguration pieceOrderConfiguration = ServiceLocator.Current.GetInstance<IConfigurationProvider>().PieceOrderConfiguration;

            _white = pieceOrderConfiguration.Whites.Select(pair => pair.Value.Select(p => p).ToArray()).ToArray();
            _black = pieceOrderConfiguration.Blacks.Select(pair => pair.Value.Select(p => p).ToArray()).ToArray();
            _whiteAttacks = pieceOrderConfiguration.WhitesAttacks.Select(pair => pair.Value.Select(p => p).ToArray()).ToArray();
            _blackAttacks = pieceOrderConfiguration.BlacksAttacks.Select(pair => pair.Value.Select(p => p).ToArray()).ToArray();

            _squares = new SquareList[6];
            for (int i = 0; i < _squares.Length; i++)
            {
                _squares[i] = new SquareList();
            }
            _promotionSquares = new SquareList();

            _attacks = new AttackList();
            _moves = new MoveList();
            _promotions = new PromotionList();
            _promotionsAttack = new List<PromotionAttackList> { new PromotionAttackList(), new PromotionAttackList() };
            _promotionsSingleAttack = new List<PromotionAttackList> { new PromotionAttackList(), new PromotionAttackList() };

            _board = new Board();
            _figureHistory = new ArrayStack<byte>();
            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            _moveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        #region Implementation of IPosition

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetPiece(byte cell, out byte? piece)
        {
            return _board.GetPiece(cell, out piece);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetKey()
        {
            return _board.GetKey();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetValue()
        {
            if (_turn == Turn.White)
                return _board.GetValue();
            return (short) -_board.GetValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetStaticValue()
        {
            if (_turn == Turn.White)
                return _board.GetStaticValue();
            return (short)-_board.GetStaticValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKingSafetyValue()
        {
            if (_turn == Turn.White)
                return _board.GetKingSafetyValue();
            return (short)-_board.GetKingSafetyValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPawnValue()
        {
            if (_turn == Turn.White)
                return _board.GetPawnValue();
            return (short)-_board.GetPawnValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOpponentMaxValue()
        {
            if (_turn == Turn.White)
                return _board.GetBlackMaxValue();
            return _board.GetWhiteMaxValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Turn GetTurn()
        {
            return _turn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetAllMoves(byte cell, byte piece)
        {
            _moveProvider.GetMoves(piece, cell, _moves);
            _moveProvider.GetAttacks(piece, cell, _attacks);
            _moveProvider.GetPromotions(piece, cell, _promotions);
            _moveProvider.GetPromotions(piece, cell, _promotionsAttack);

            IEnumerable<MoveBase> moves = _moves.Concat(_attacks).Concat(_promotions).Concat(_promotionsAttack.SelectMany(p=>p));
            
            return _turn == Turn.White
                ? moves.Where(a => IsWhiteLigal(a))
                : moves.Where(a => IsBlackLigal(a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhitePromotionAttacks(AttackList attacks)
        {
            _board.GetWhitePromotionSquares(_promotionSquares);

            BitBoard to = new BitBoard();

            for (int i = 0; i < _promotionSquares.Length; i++)
            {
                _moveProvider.GetPromotions(0, _promotionSquares[i], _promotionsSingleAttack);

                for (var j = 0; j < _promotionsSingleAttack.Count; j++)
                {
                    if (_promotionsSingleAttack[j].Count > 0)
                    {
                        var attack = _promotionsSingleAttack[j][0];
                        if (to.IsSet(attack.To.AsBitBoard())) continue;

                        if (IsWhiteLigal(attack))
                        {
                            attacks.Add(attack);
                        }
                        to |= attack.To.AsBitBoard();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackPromotionAttacks(AttackList attacks)
        {
            _board.GetBlackPromotionSquares(_promotionSquares);
            BitBoard to = new BitBoard();

            for (int i = 0; i < _promotionSquares.Length; i++)
            {
                _moveProvider.GetPromotions(6, _promotionSquares[i], _promotionsSingleAttack);

                for (var j = 0; j < _promotionsSingleAttack.Count; j++)
                {
                    if (_promotionsSingleAttack[j].Count > 0)
                    {
                        var attack = _promotionsSingleAttack[j][0];
                        if (to.IsSet(attack.To.AsBitBoard())) continue;

                        if (IsBlackLigal(attack))
                        {
                            attacks.Add(attack);
                        }
                        to |= attack.To.AsBitBoard();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteAttacks(AttackList attacks)
        {
            GetWhiteSquares(_whiteAttacks[_phase]);
            PossibleSingleWhiteAttacks(_whiteAttacks[_phase],attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackAttacks(AttackList attacks)
        {
            GetBlackSquares(_blackAttacks[_phase]);
            PossibleSingleBlackAttacks(_blackAttacks[_phase],attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList GetAllAttacks(SortContext sortContext)
        {
            _sortContext = sortContext;
            sortContext.InitializeSort();

            if(_turn == Turn.White)
            {
                _sortContext.Pieces = _whiteAttacks[_phase];
                GetWhiteSquares(_sortContext.Pieces, _sortContext.Squares);

                ProcessWhiteCapuresWithoutPv();
                if (_board.CanWhitePromote())
                {
                    _board.GetWhitePromotionSquares(sortContext.PromotionSquares);
                    ProcessWhitePromotionCapuresWithoutPv();
                    _sortContext.FinalizeSort();
                    ProcessWhitePromotionsWithoutPv();
                }
                else
                {
                    _sortContext.FinalizeSort();
                }

            }
            else
            {
                _sortContext.Pieces = _blackAttacks[_phase];
                GetBlackSquares(_sortContext.Pieces, _sortContext.Squares);

                ProcessBlackCapuresWithoutPv();
                if (_board.CanBlackPromote())
                {
                    _board.GetBlackPromotionSquares(sortContext.PromotionSquares);
                    ProcessBlackPromotionCapuresWithoutPv();
                    _sortContext.FinalizeSort();
                    ProcessBlackPromotionsWithoutPv();
                }
                else
                {
                    _sortContext.FinalizeSort();
                }
            }

            return sortContext.GetMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList GetAllMoves(SortContext sortContext)
        {
            _sortContext = sortContext;
            sortContext.InitializeSort();
            if(_turn == Turn.White)
            {
                _sortContext.Pieces = _white[_phase];
                GetWhiteSquares(_sortContext.Pieces, _sortContext.Squares);

                if (sortContext.HasPv)
                {
                    if (sortContext.IsPvCapture)
                    {
                        ProcessWhiteCapuresWithPv();
                        if (_board.CanWhitePromote())
                        {
                            _board.GetWhitePromotionSquares(sortContext.PromotionSquares);
                            ProcessWhitePromotionCapuresWithPv();
                            _sortContext.FinalizeSort();
                            ProcessWhitePromotionsWithoutPv();
                        }
                        else
                        {
                            _sortContext.FinalizeSort();
                        }
                        ProcessWhiteMovesWithoutPv();
                    }
                    else
                    {
                        ProcessWhiteCapuresWithoutPv(); 
                        if (_board.CanWhitePromote())
                        {
                            _board.GetWhitePromotionSquares(sortContext.PromotionSquares);
                            ProcessWhitePromotionCapuresWithoutPv();
                            _sortContext.FinalizeSort();
                            ProcessWhitePromotionsWithPv();
                        }
                        else
                        {
                            _sortContext.FinalizeSort();
                        }
                        ProcessWhiteMovesWithPv();
                    }
                }
                else
                {
                    ProcessWhiteCapuresWithoutPv(); 
                    if (_board.CanWhitePromote())
                    {
                        _board.GetWhitePromotionSquares(sortContext.PromotionSquares);
                        ProcessWhitePromotionCapuresWithoutPv();
                        _sortContext.FinalizeSort();
                        ProcessWhitePromotionsWithoutPv();
                    }
                    else
                    {
                        _sortContext.FinalizeSort();
                    }
                    ProcessWhiteMovesWithoutPv();
                }
            }
            else
            {
                _sortContext.Pieces = _black[_phase];
                GetBlackSquares(_sortContext.Pieces, _sortContext.Squares);

                if (sortContext.HasPv)
                {
                    if (sortContext.IsPvCapture)
                    {
                        ProcessBlackCapuresWithPv();
                        if (_board.CanBlackPromote())
                        {
                            _board.GetBlackPromotionSquares(sortContext.PromotionSquares);
                            ProcessBlackPromotionCapuresWithPv();
                            _sortContext.FinalizeSort();
                            ProcessBlackPromotionsWithoutPv();
                        }
                        else
                        {
                            _sortContext.FinalizeSort();
                        }
                        ProcessBlackMovesWithoutPv();
                    }
                    else
                    {
                        ProcessBlackCapuresWithoutPv();
                        if (_board.CanBlackPromote())
                        {
                            _board.GetBlackPromotionSquares(sortContext.PromotionSquares);
                            ProcessBlackPromotionCapuresWithoutPv();
                            _sortContext.FinalizeSort();
                            ProcessBlackPromotionsWithPv();
                        }
                        else
                        {
                            _sortContext.FinalizeSort();
                        }
                        ProcessBlackMovesWithPv();
                    }
                }
                else
                {
                    ProcessBlackCapuresWithoutPv();
                    if (_board.CanBlackPromote())
                    {
                        _board.GetBlackPromotionSquares(sortContext.PromotionSquares);
                        ProcessBlackPromotionCapuresWithoutPv();
                        _sortContext.FinalizeSort();
                        ProcessBlackPromotionsWithoutPv();
                    }
                    else
                    {
                        _sortContext.FinalizeSort();
                    }
                    ProcessBlackMovesWithoutPv();
                }
            }
            return sortContext.GetMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionCapuresWithPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(0, _sortContext.PromotionSquares[f], _promotionsAttack);

                for (int i = 0; i < _promotionsAttack.Count; i++)
                {
                    if (_promotionsAttack[i].Count == 0 || !IsWhiteLigal(_promotionsAttack[i][0]))
                        continue;


                    if (_promotionsAttack[i].HasPv(_sortContext.Pv))
                    {
                        _sortContext.ProcessHashMoves(_promotionsAttack[i]);
                    }
                    else
                    {
                        _sortContext.ProcessPromotionCaptures(_promotionsAttack[i]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionCapuresWithoutPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(0, _sortContext.PromotionSquares[f], _promotionsAttack);

                for (int i = 0; i < _promotionsAttack.Count; i++)
                {
                    if (_promotionsAttack[i].Count != 0 && IsWhiteLigal(_promotionsAttack[i][0]))
                        _sortContext.ProcessPromotionCaptures(_promotionsAttack[i]); 
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionCapuresWithPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(6, _sortContext.PromotionSquares[f], _promotionsAttack);

                for (int i = 0; i < _promotionsAttack.Count; i++)
                {
                    if (_promotionsAttack[i].Count == 0 || !IsBlackLigal(_promotionsAttack[i][0]))
                        continue;

                    if (_promotionsAttack[i].HasPv(_sortContext.Pv))
                    {
                        _sortContext.ProcessHashMoves(_promotionsAttack[i]);
                    }
                    else
                    {
                        _sortContext.ProcessPromotionCaptures(_promotionsAttack[i]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionCapuresWithoutPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(6, _sortContext.PromotionSquares[f], _promotionsAttack);

                for (int i = 0; i < _promotionsAttack.Count; i++)
                {
                    if (_promotionsAttack[i].Count != 0 && IsBlackLigal(_promotionsAttack[i][0]))
                        _sortContext.ProcessPromotionCaptures(_promotionsAttack[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionsWithPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(0, _sortContext.PromotionSquares[f], _promotions);

                if (_promotions.Count == 0 || !IsWhiteLigal(_promotions[0]))
                    continue;

                if (_promotions.HasPv(_sortContext.Pv))
                {
                    _sortContext.ProcessHashMoves(_promotions);
                }
                else
                {
                    _sortContext.ProcessPromotionMoves(_promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionsWithoutPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(0, _sortContext.PromotionSquares[f], _promotions);

                if (_promotions.Count > 0 && IsWhiteLigal(_promotions[0]))
                {
                    _sortContext.ProcessPromotionMoves(_promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionsWithPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(6, _sortContext.PromotionSquares[f], _promotions);

                if (_promotions.Count <= 0 || !IsBlackLigal(_promotions[0]))
                    continue;

                if (_promotions.HasPv(_sortContext.Pv))
                {
                    _sortContext.ProcessHashMoves(_promotions);
                }
                else
                {
                    _sortContext.ProcessPromotionMoves(_promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionsWithoutPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(6, _sortContext.PromotionSquares[f], _promotions);

                if (_promotions.Count > 0 && IsBlackLigal(_promotions[0]))
                {
                    _sortContext.ProcessPromotionMoves(_promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteCapuresWithPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];

                var square = _sortContext.Squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacks);

                    for (var i = 0; i < _attacks.Count; i++)
                    {
                        var capture = _attacks[i];
                        if (!IsWhiteLigal(capture))
                            continue;

                        if (_sortContext.Pv != capture.Key)
                        {
                            _sortContext.ProcessCaptureMove(capture);
                        }
                        else
                        {
                            _sortContext.ProcessHashMove(capture);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMovesWithPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];
                var from = _sortContext.Squares[p % 6];

                for (var f = 0; f < from.Length; f++)
                {
                    _moveProvider.GetMoves(p, @from[f], _moves);
                    for (var i = 0; i < _moves.Count; i++)
                    {
                        var move = _moves[i];
                        if (!IsWhiteLigal(move))
                            continue;

                        if (_sortContext.Pv == move.Key)
                        {
                            _sortContext.ProcessHashMove(move);
                        }
                        else if (_sortContext.IsKiller(move.Key))
                        {
                            _sortContext.ProcessKillerMove(move);
                        }
                        else
                        {
                            _sortContext.ProcessMove(move);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteCapuresWithoutPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];

                var square = _sortContext.Squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacks);

                    for (var i = 0; i < _attacks.Count; i++)
                    {
                        if (IsWhiteLigal(_attacks[i]))
                        {
                            _sortContext.ProcessCaptureMove(_attacks[i]);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMovesWithoutPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];
                var from = _sortContext.Squares[p % 6];

                for (var f = 0; f < from.Length; f++)
                {
                    _moveProvider.GetMoves(p, @from[f], _moves);
                    for (var i = 0; i < _moves.Count; i++)
                    {
                        var move = _moves[i];
                        if (!IsWhiteLigal(move))
                            continue;

                        if (_sortContext.IsKiller(move.Key))
                        {
                            _sortContext.ProcessKillerMove(move);
                        }
                        else
                        {
                            _sortContext.ProcessMove(move);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackCapuresWithPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];

                var square = _sortContext.Squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacks);

                    for (var i = 0; i < _attacks.Count; i++)
                    {
                        var capture = _attacks[i];
                        if (!IsBlackLigal(capture))
                            continue;

                        if (_sortContext.Pv != capture.Key)
                        {
                            _sortContext.ProcessCaptureMove(capture);
                        }
                        else
                        {
                            _sortContext.ProcessHashMove(capture);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMovesWithPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];
                var from = _sortContext.Squares[p % 6];

                for (var f = 0; f < from.Length; f++)
                {
                    _moveProvider.GetMoves(p, @from[f], _moves);
                    for (var i = 0; i < _moves.Count; i++)
                    {
                        var move = _moves[i];
                        if (!IsBlackLigal(move))
                            continue;

                        if (_sortContext.Pv == move.Key)
                        {
                            _sortContext.ProcessHashMove(move);
                        }
                        else if (_sortContext.IsKiller(move.Key))
                        {
                            _sortContext.ProcessKillerMove(move);
                        }
                        else
                        {
                            _sortContext.ProcessMove(move);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackCapuresWithoutPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];

                var square = _sortContext.Squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacks);

                    for (var i = 0; i < _attacks.Count; i++)
                    {
                        if (IsBlackLigal(_attacks[i]))
                        {
                            _sortContext.ProcessCaptureMove(_attacks[i]);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMovesWithoutPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];
                var from = _sortContext.Squares[p % 6];

                for (var f = 0; f < from.Length; f++)
                {
                    _moveProvider.GetMoves(p, @from[f], _moves);
                    for (var i = 0; i < _moves.Count; i++)
                    {
                        var move = _moves[i];
                        if (!IsBlackLigal(move))
                            continue;

                        if (_sortContext.IsKiller(move.Key))
                        {
                            _sortContext.ProcessKillerMove(move);
                        }
                        else
                        {
                            _sortContext.ProcessMove(move);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PossibleSingleWhiteAttacks(byte[] pieces, AttackList attacks)
        {
            BitBoard to = new BitBoard();

            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];

                var square = _squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacks);

                    for (var i = 0; i < _attacks.Count; i++)
                    {
                        var attack = _attacks[i];
                        if (to.IsSet(attack.To.AsBitBoard())) continue;

                        if (IsWhiteLigal(attack))
                        {
                            attacks.Add(attack);
                        }
                        to |= attack.To.AsBitBoard();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteAttacksTo(byte to, AttackList attackList)
        {
            attackList.Clear();

            _moveProvider.GetWhiteAttacksToForPromotion(to, _attacks);

            for (var i = 0; i < _attacks.Count; i++)
            {
                var attack = _attacks[i];

                if (IsWhiteLigal(attack))
                {
                    attackList.Add(attack);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackAttacksTo(byte to, AttackList attackList)
        {
            attackList.Clear();

            _moveProvider.GetBlackAttacksToForPromotion(to, _attacks);

            for (var i = 0; i < _attacks.Count; i++)
            {
                var attack = _attacks[i];

                if (IsBlackLigal(attack))
                {
                    attackList.Add(attack);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PossibleSingleBlackAttacks(byte[] pieces, AttackList attacks)
        {
            BitBoard to = new BitBoard();

            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];

                var square = _squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacks);

                    for (var i = 0; i < _attacks.Count; i++)
                    {
                        var attack = _attacks[i];
                        if (to.IsSet(attack.To.AsBitBoard())) continue;

                        if (IsBlackLigal(attack))
                        {
                            attacks.Add(attack);
                        }
                        to |= attack.To.AsBitBoard();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPieceValue(byte square)
        {
            return _board.GetPiece(square).AsValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBoard GetBoard()
        {
            return _board;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetHistory()
        {
            return _moveHistoryService.GetHistory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhiteNotLegal(MoveBase move)
        {
            return _board.IsBlackAttacksTo(_board.GetWhiteKingPosition()) || 
                (move.IsCastle && _board.IsBlackAttacksTo(move.To == C1 ? D1 : F1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackNotLegal(MoveBase move)
        {
           return _board.IsWhiteAttacksTo(_board.GetBlackKingPosition()) || 
                (move.IsCastle && _board.IsWhiteAttacksTo(move.To == C8 ? D8 : F8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetPhase()
        {
            return _phase;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanWhitePromote()
        {
            return _board.CanWhitePromote();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanBlackPromote()
        {
            return _board.CanBlackPromote();
        }

        public void SaveHistory()
        {
            var moveFormatter = ServiceLocator.Current.GetInstance<IMoveFormatter>();
            IEnumerable<MoveBase> history = GetHistory();
            List<string> moves = new List<string>();
            bool isWhite = true;
            StringBuilder builder = new StringBuilder();
            foreach (var move in history)
            {
                if (isWhite)
                {
                    builder = new StringBuilder();
                    builder.Append($"W={moveFormatter.Format(move)} ");
                }
                else
                {
                    builder.Append($"B={moveFormatter.Format(move)} ");
                    moves.Add(builder.ToString());
                }
                isWhite = !isWhite;
            }
            var path = "History";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllLines($@"{path}\\{DateTime.Now:yyyy_MM_dd_hh_mm_ss}.txt", moves);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetWhiteSquares(byte[] pieces, SquareList[] squares)
        {
            _board.GetWhitePawnSquares(squares[0]);

            for (var i = 1; i < 6; i++)
            {
                _board.GetSquares(pieces[i], squares[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBlackSquares(byte[] pieces, SquareList[] squares)
        {
            _board.GetBlackPawnSquares(squares[0]);

            for (var i = 1; i < 6; i++)
            {
                _board.GetSquares(pieces[i], squares[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetWhiteSquares(byte[] pieces)
        {
            _board.GetWhitePawnSquares(_squares[0]);

            for (var i = 1; i < 6; i++)
            {
                _board.GetSquares(pieces[i], _squares[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBlackSquares(byte[] pieces)
        {
            _board.GetBlackPawnSquares(_squares[0]);
            for (var i = 1; i < 6; i++)
            {
                _board.GetSquares(pieces[i], _squares[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MakeFirst(MoveBase move)
        {
            _moveHistoryService.AddFirst(move);

            move.Make(_board, _figureHistory);

            move.IsCheck = _turn != Turn.White 
                ? _board.IsBlackAttacksTo(_board.GetWhiteKingPosition()) 
                : _board.IsWhiteAttacksTo(_board.GetBlackKingPosition());

            _phase = _board.UpdatePhase();

            _moveHistoryService.Add(_board.GetKey());

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Make(MoveBase move)
        {
            _moveHistoryService.Add(move);

            move.Make(_board, _figureHistory);

            move.IsCheck = _turn != Turn.White 
                ? _board.IsBlackAttacksTo(_board.GetWhiteKingPosition()) 
                : _board.IsWhiteAttacksTo(_board.GetBlackKingPosition());

            _phase = _board.UpdatePhase();

            _moveHistoryService.Add(_board.GetKey());

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnMake()
        {
            _moveHistoryService.Remove(_board.GetKey());
            MoveBase move = _moveHistoryService.Remove();

            move.UnMake(_board, _figureHistory);

            move.IsCheck = false;

            _phase = _board.UpdatePhase();

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Do(MoveBase move)
        {
            move.Make(_board, _figureHistory);

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnDo(MoveBase move)
        {
            move.UnMake(_board, _figureHistory);

            SwapTurn();
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsWhiteLigal(MoveBase move)
        {
            Do(move);

            bool isLegal = !IsWhiteNotLegal(move);

            UnDo(move);

            return isLegal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBlackLigal(MoveBase move)
        {
            Do(move);

            bool isLegal = !IsBlackNotLegal(move);

            UnDo(move);

            return isLegal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapTurn()
        {
            _turn = _turn == Turn.White ? Turn.Black : Turn.White;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Turn = {_turn}, Key = {GetKey()}, Value = {GetValue()}, Static = {GetStaticValue()}");
            builder.AppendLine(_board.ToString());
            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDraw()
        {
            return _board.IsDraw();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlockedByBlack(byte position)
        {
            return _board.IsBlockedByBlack(position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlockedByWhite(byte position)
        {
            return _board.IsBlockedByWhite(position);
        }
    }
}