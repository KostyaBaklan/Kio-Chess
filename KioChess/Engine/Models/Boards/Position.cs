﻿using System.Runtime.CompilerServices;
using System.Text;
using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Models.Contexts;

namespace Engine.Models.Boards;

public class Position
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

    private Turn _turn;
    private SortContext _sortContext;

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

    private readonly SquareList[] _squaresCheck;
    private readonly SquareList _promotionSquaresCheck;

    private readonly AttackList _attacksCheck;
    private readonly MoveList _movesCheck;

    private readonly Board _board;
    private readonly MoveProvider _moveProvider;
    private readonly MoveHistoryService _moveHistoryService;

    public Position()
    {
        _turn = Turn.White;

        IConfigurationProvider configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var bookConfiguration = configurationProvider.BookConfiguration;

        _white = Enumerable.Range(0, 3).Select(pair => Enumerable.Range(0, 6).Select(x => (byte)x).ToArray()).ToArray();
        _black = Enumerable.Range(0, 3).Select(pair => Enumerable.Range(6, 6).Select(x => (byte)x).ToArray()).ToArray();
        _whiteAttacks = Enumerable.Range(0, 3).Select(pair => Enumerable.Range(0, 6).Select(x => (byte)x).ToArray()).ToArray();
        _blackAttacks = Enumerable.Range(0, 3).Select(pair => Enumerable.Range(6, 6).Select(x => (byte)x).ToArray()).ToArray();

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


        _squaresCheck = new SquareList[6];
        for (int i = 0; i < _squares.Length; i++)
        {
            _squaresCheck[i] = new SquareList();
        }
        _promotionSquaresCheck = new SquareList();

        _attacksCheck = new AttackList();
        _movesCheck = new MoveList();

        _board = new Board();
        _moveProvider = ContainerLocator.Current.Resolve<MoveProvider>();
        _moveHistoryService = ContainerLocator.Current.Resolve<MoveHistoryService>();
    }

    #region Implementation of Position

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetPiece(byte cell, out byte? piece) => _board.GetPiece(cell, out piece);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetValue()
    {
        if (_turn == Turn.White)
            return _board.Evaluate();
        return _board.EvaluateOpposite();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetStaticValue()
    {
        if (_turn == Turn.White)
            return _board.GetStaticValue();
        return -_board.GetStaticValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Turn GetTurn() => _turn;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<MoveBase> GetMoves(byte piece, byte to)
    {
        List<MoveBase> result = new List<MoveBase>();

        var positions = _board.GetPiecePositions(piece);
        for (byte s = 0; s < positions.Count; s++)
        {
            List<MoveBase> enumerable = GetAllMoves(positions[s], piece).ToList();
            result.AddRange(enumerable.Where(m => m.To == to));
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<MoveBase> GetAllMoves()
    {
        if (GetHistory().Count() == 0)
        {
            return GetFirstMoves().ToList();
        }
        if (_turn == Turn.White)
        {
            return GetAllWhiteMoves();
        }
        else
        {
            return GetAllBlackMoves();
        }
    }

    private List<MoveBase> GetAllBlackMoves()
    {
        List<MoveBase> result = new List<MoveBase>();

        for (byte p = 6; p < 12; p++)
        {
            var positions = _board.GetPiecePositions(p);
            for (byte s = 0; s < positions.Count; s++)
            {
                result.AddRange(GetAllMoves(positions[s], p));
            }
        }

        return result;
    }

    private List<MoveBase> GetAllWhiteMoves()
    {
        List<MoveBase> result = new List<MoveBase>();

        for (byte p = 0; p < 6; p++)
        {
            var positions = _board.GetPiecePositions(p);
            for (byte s = 0; s < positions.Count; s++)
            {
                result.AddRange(GetAllMoves(positions[s], p));
            }
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<MoveBase> GetAllMoves(byte cell, byte piece)
    {
        _moveProvider.GetMoves(piece, cell, _moves);
        _moveProvider.GetAttacks(piece, cell, _attacks);
        _moveProvider.GetPromotions(piece, cell, _promotions);
        _moveProvider.GetPromotions(piece, cell, _promotionsAttack);

        IEnumerable<MoveBase> moves = _moves.Concat(_attacks).Concat(_promotions).Concat(_promotionsAttack.SelectMany(p => p));

        return _turn == Turn.White
            ? moves.Where(_board.IsWhiteLigal)
            : moves.Where(_board.IsBlackLigal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhitePromotionAttacks(AttackList attacks)
    {
        _board.GetWhitePromotionSquares(_promotionSquares);

        BitBoard to = new BitBoard();

        for (byte i = 0; i < _promotionSquares.Length; i++)
        {
            var promotions = _moveProvider.GetWhitePromotionAttacks(_promotionSquares[i]);

            for (byte j = 0; j < promotions.Length; j++)
            {
                if (promotions[j].Count > 0)
                {
                    var attack = promotions[j][0];
                    if (to.IsSet(attack.To)) continue;

                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        attacks.Add(attack);
                        to |= attack.To.AsBitBoard();
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackPromotionAttacks(AttackList attacks)
    {
        _board.GetBlackPromotionSquares(_promotionSquares);
        BitBoard to = new BitBoard();

        for (byte i = 0; i < _promotionSquares.Length; i++)
        {
            var promotions = _moveProvider.GetBlackPromotionAttacks(_promotionSquares[i]);

            for (byte j = 0; j < promotions.Length; j++)
            {
                if (promotions[j].Count > 0)
                {
                    var attack = promotions[j][0];
                    if (to.IsSet(attack.To)) continue;

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        attacks.Add(attack);
                        to |= attack.To.AsBitBoard();
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteAttacks(AttackList attacks)
    {
        GetWhiteSquares(_whiteAttacks[_moveHistoryService.GetPhase()]);
        PossibleSingleWhiteAttacks(_squares, attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackAttacks(AttackList attacks)
    {
        GetBlackSquares(_blackAttacks[_moveHistoryService.GetPhase()]);
        PossibleSingleBlackAttacks(_squares, attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList GetAllWhiteForEvaluation(SortContext sortContext)
    {
        _sortContext = sortContext;

        _sortContext.Pieces = _whiteAttacks[sortContext.Phase];
        GetWhiteSquares(_sortContext.Pieces, _sortContext.Squares);

        ProcessWhiteCapuresWithoutPv();
        if (_board.CanWhitePromote())
        {
            _board.GetWhitePromotionSquares(sortContext.PromotionSquares);
            ProcessWhitePromotionCapuresWithoutPv();

            ProcessWhitePromotionsWithoutPv();
        }

        return _sortContext.GetMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList GetAllBlackForEvaluation(SortContext sortContext)
    {
        _sortContext = sortContext;

        _sortContext.Pieces = _blackAttacks[sortContext.Phase];
        GetBlackSquares(_sortContext.Pieces, _sortContext.Squares);

        ProcessBlackCapuresWithoutPv();
        if (_board.CanBlackPromote())
        {
            _board.GetBlackPromotionSquares(sortContext.PromotionSquares);
            ProcessBlackPromotionCapuresWithoutPv();

            ProcessBlackPromotionsWithoutPv();
        }

        return _sortContext.GetMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetAllWhiteAttacks(SortContext sortContext)
    {
        _sortContext = sortContext;

        _sortContext.Pieces = _whiteAttacks[sortContext.Phase];
        GetWhiteSquares(_sortContext.Pieces, _sortContext.Squares);

        ProcessWhiteCapuresWithoutPv();
        if (_board.CanWhitePromote())
        {
            _board.GetWhitePromotionSquares(sortContext.PromotionSquares);
            ProcessWhitePromotionCapuresWithoutPv();

            ProcessWhitePromotionsWithoutPv();
        }

        return sortContext.GetAttacks();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetAllWhiteBookMoves(SortContext sc)
    {
        _sortContext = sc;

        if (sc.IsRegular)
        {
            ProcessRegularWhiteMoves();
        }
        else
        {
            ProcessBookWhiteMoves();
        }
        return _sortContext.GetMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetAllWhiteMoves(SortContext sc)
    {
        _sortContext = sc;
        ProcessRegularWhiteMoves();
        return _sortContext.GetMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetAllBlackAttacks(SortContext sortContext)
    {
        _sortContext = sortContext;

        _sortContext.Pieces = _blackAttacks[sortContext.Phase];
        GetBlackSquares(_sortContext.Pieces, _sortContext.Squares);

        ProcessBlackCapuresWithoutPv();
        if (_board.CanBlackPromote())
        {
            _board.GetBlackPromotionSquares(sortContext.PromotionSquares);
            ProcessBlackPromotionCapuresWithoutPv();

            ProcessBlackPromotionsWithoutPv();
        }

        return sortContext.GetAttacks();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetAllBlackBookMoves(SortContext sc)
    {
        _sortContext = sc;

        if (sc.IsRegular)
        {
            ProcessRegularBlackMoves();
        }
        else
        {
            ProcessBookBlackMoves();
        }
        return _sortContext.GetMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetAllBlackMoves(SortContext sc)
    {
        _sortContext = sc;
        ProcessRegularBlackMoves();
        return _sortContext.GetMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBookWhiteMoves()
    {
        _sortContext.Pieces = _white[_sortContext.Phase];
        GetWhiteSquares(_sortContext.Pieces, _sortContext.Squares);

        if (_sortContext.HasPv)
        {
            if (_sortContext.IsPvCapture)
            {
                ProcessWhiteBookCapuresWithPv();
                if (_board.CanWhitePromote())
                {
                    _board.GetWhitePromotionSquares(_sortContext.PromotionSquares);
                    ProcessWhitePromotionCapuresWithPv();

                    ProcessWhitePromotionsWithoutPv();
                }
                ProcessWhiteBookMovesWithoutPv();
            }
            else
            {
                ProcessWhiteBookCapuresWithoutPv();
                if (_board.CanWhitePromote())
                {
                    _board.GetWhitePromotionSquares(_sortContext.PromotionSquares);
                    ProcessWhitePromotionCapuresWithoutPv();

                    ProcessWhitePromotionsWithPv();
                }
                ProcessWhiteBookMovesWithPv();
            }
        }
        else
        {
            ProcessWhiteBookCapuresWithoutPv();
            if (_board.CanWhitePromote())
            {
                _board.GetWhitePromotionSquares(_sortContext.PromotionSquares);
                ProcessWhitePromotionCapuresWithoutPv();

                ProcessWhitePromotionsWithoutPv();
            }
            ProcessWhiteBookMovesWithoutPv();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBookBlackMoves()
    {
        _sortContext.Pieces = _black[_sortContext.Phase];
        GetBlackSquares(_sortContext.Pieces, _sortContext.Squares);

        if (_sortContext.HasPv)
        {
            if (_sortContext.IsPvCapture)
            {
                ProcessBlackBookCapuresWithPv();
                if (_board.CanBlackPromote())
                {
                    _board.GetBlackPromotionSquares(_sortContext.PromotionSquares);
                    ProcessBlackPromotionCapuresWithPv();

                    ProcessBlackPromotionsWithoutPv();
                }
                ProcessBlackBookMovesWithoutPv();
            }
            else
            {
                ProcessBlackBookCapuresWithoutPv();
                if (_board.CanBlackPromote())
                {
                    _board.GetBlackPromotionSquares(_sortContext.PromotionSquares);
                    ProcessBlackPromotionCapuresWithoutPv();

                    ProcessBlackPromotionsWithPv();
                }
                ProcessBlackBookMovesWithPv();
            }
        }
        else
        {
            ProcessBlackBookCapuresWithoutPv();
            if (_board.CanBlackPromote())
            {
                _board.GetBlackPromotionSquares(_sortContext.PromotionSquares);
                ProcessBlackPromotionCapuresWithoutPv();

                ProcessBlackPromotionsWithoutPv();
            }
            ProcessBlackBookMovesWithoutPv();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessRegularWhiteMoves()
    {
        _sortContext.Pieces = _white[_sortContext.Phase];
        GetWhiteSquares(_sortContext.Pieces, _sortContext.Squares);

        if (_sortContext.HasPv)
        {
            if (_sortContext.IsPvCapture)
            {
                ProcessWhiteCapuresWithPv();
                if (_board.CanWhitePromote())
                {
                    _board.GetWhitePromotionSquares(_sortContext.PromotionSquares);
                    ProcessWhitePromotionCapuresWithPv();

                    ProcessWhitePromotionsWithoutPv();
                }
                ProcessWhiteMovesWithoutPv();
            }
            else
            {
                ProcessWhiteCapuresWithoutPv();
                if (_board.CanWhitePromote())
                {
                    _board.GetWhitePromotionSquares(_sortContext.PromotionSquares);
                    ProcessWhitePromotionCapuresWithoutPv();

                    ProcessWhitePromotionsWithPv();
                }
                ProcessWhiteMovesWithPv();
            }
        }
        else
        {
            ProcessWhiteCapuresWithoutPv();
            if (_board.CanWhitePromote())
            {
                _board.GetWhitePromotionSquares(_sortContext.PromotionSquares);
                ProcessWhitePromotionCapuresWithoutPv();

                ProcessWhitePromotionsWithoutPv();
            }
            ProcessWhiteMovesWithoutPv();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessRegularBlackMoves()
    {
        _sortContext.Pieces = _black[_sortContext.Phase];
        GetBlackSquares(_sortContext.Pieces, _sortContext.Squares);

        if (_sortContext.HasPv)
        {
            if (_sortContext.IsPvCapture)
            {
                ProcessBlackCapuresWithPv();
                if (_board.CanBlackPromote())
                {
                    _board.GetBlackPromotionSquares(_sortContext.PromotionSquares);
                    ProcessBlackPromotionCapuresWithPv();

                    ProcessBlackPromotionsWithoutPv();
                }
                ProcessBlackMovesWithoutPv();
            }
            else
            {
                ProcessBlackCapuresWithoutPv();
                if (_board.CanBlackPromote())
                {
                    _board.GetBlackPromotionSquares(_sortContext.PromotionSquares);
                    ProcessBlackPromotionCapuresWithoutPv();

                    ProcessBlackPromotionsWithPv();
                }
                ProcessBlackMovesWithPv();
            }
        }
        else
        {
            ProcessBlackCapuresWithoutPv();
            if (_board.CanBlackPromote())
            {
                _board.GetBlackPromotionSquares(_sortContext.PromotionSquares);
                ProcessBlackPromotionCapuresWithoutPv();

                ProcessBlackPromotionsWithoutPv();
            }
            ProcessBlackMovesWithoutPv();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhitePromotionCapuresWithPv()
    {
        for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
        {
            var promotions = _moveProvider.GetWhitePromotionAttacks(_sortContext.PromotionSquares[f]);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count == 0 || !_board.IsWhiteMoveLigal(promotions[i][0]))
                    continue;


                if (promotions[i].HasPv(_sortContext.Pv))
                {
                    _sortContext.ProcessHashMoves(promotions[i]);
                }
                else
                {
                    _sortContext.ProcessPromotionCaptures(promotions[i]);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhitePromotionCapuresWithoutPv()
    {
        for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
        {
            var promotions = _moveProvider.GetWhitePromotionAttacks(_sortContext.PromotionSquares[f]);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count != 0 && _board.IsWhiteMoveLigal(promotions[i][0]))
                    _sortContext.ProcessPromotionCaptures(promotions[i]);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackPromotionCapuresWithPv()
    {
        for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
        {
            var promotions = _moveProvider.GetBlackPromotionAttacks(_sortContext.PromotionSquares[f]);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count == 0 || !_board.IsBlackMoveLigal(promotions[i][0]))
                    continue;

                if (promotions[i].HasPv(_sortContext.Pv))
                {
                    _sortContext.ProcessHashMoves(promotions[i]);
                }
                else
                {
                    _sortContext.ProcessPromotionCaptures(promotions[i]);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackPromotionCapuresWithoutPv()
    {
        for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
        {
            var promotions = _moveProvider.GetBlackPromotionAttacks(_sortContext.PromotionSquares[f]);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count != 0 && _board.IsBlackMoveLigal(promotions[i][0]))
                    _sortContext.ProcessPromotionCaptures(promotions[i]);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhitePromotionsWithPv()
    {
        for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
        {
            var promotions = _moveProvider.GetWhitePromotions(_sortContext.PromotionSquares[f]);

            if (promotions.Count == 0 || !_board.IsWhiteMoveLigal(promotions[0]))
                continue;

            if (promotions.HasPv(_sortContext.Pv))
            {
                _sortContext.ProcessHashMoves(promotions);
            }
            else
            {
                _sortContext.ProcessPromotionMoves(promotions);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhitePromotionsWithoutPv()
    {
        for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
        {
            var promotions = _moveProvider.GetWhitePromotions(_sortContext.PromotionSquares[f]);

            if (promotions.Count > 0 && _board.IsWhiteMoveLigal(promotions[0]))
            {
                _sortContext.ProcessPromotionMoves(promotions);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackPromotionsWithPv()
    {
        for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
        {
            var promotions = _moveProvider.GetBlackPromotions(_sortContext.PromotionSquares[f]);

            if (promotions.Count <= 0 || !_board.IsBlackMoveLigal(promotions[0]))
                continue;

            if (promotions.HasPv(_sortContext.Pv))
            {
                _sortContext.ProcessHashMoves(promotions);
            }
            else
            {
                _sortContext.ProcessPromotionMoves(promotions);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackPromotionsWithoutPv()
    {
        for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
        {
            var promotions = _moveProvider.GetBlackPromotions(_sortContext.PromotionSquares[f]);

            if (promotions.Count > 0 && _board.IsBlackMoveLigal(promotions[0]))
            {
                _sortContext.ProcessPromotionMoves(promotions);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteBookCapuresWithPv()
    {
        _attacks.Clear();

        GenerateWhiteAttacks(_sortContext.Squares);

        for (byte i = 0; i < _attacks.Count; i++)
        {
            var capture = _attacks[i];
            if (_sortContext.Pv != capture.Key)
            {
                ProcessCaptureMove(capture);
            }
            else
            {
                _sortContext.ProcessHashMove(capture);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteCapuresWithPv()
    {
        _attacks.Clear();

        GenerateWhiteAttacks(_sortContext.Squares);

        for (byte i = 0; i < _attacks.Count; i++)
        {
            var capture = _attacks[i];
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteBookMovesWithPv()
    {
        _moves.Clear();

        GenerateWhiteMoves(_sortContext.Squares);

        for (byte i = 0; i < _moves.Count; i++)
        {
            var move = _moves[i];
            if (_sortContext.Pv == move.Key)
            {
                _sortContext.ProcessHashMove(move);
            }
            else if (_sortContext.IsRegularMove(move))
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteMovesWithPv()
    {
        _moves.Clear();

        GenerateWhiteMoves(_sortContext.Squares);

        for (byte i = 0; i < _moves.Count; i++)
        {
            var move = _moves[i];
            if (_sortContext.Pv != move.Key)
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
            else
            {
                _sortContext.ProcessHashMove(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteBookCapuresWithoutPv()
    {
        _attacks.Clear();

        GenerateWhiteAttacks(_sortContext.Squares);

        for (byte i = 0; i < _attacks.Count; i++)
        {
            ProcessCaptureMove(_attacks[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteCapuresWithoutPv()
    {
        _attacks.Clear();

        GenerateWhiteAttacks(_sortContext.Squares);

        for (byte i = 0; i < _attacks.Count; i++)
        {
            _sortContext.ProcessCaptureMove(_attacks[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessCaptureMove(AttackBase attack)
    {
        if (_sortContext.IsRegularMove(attack))
        {
            _sortContext.ProcessCaptureMove(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteBookMovesWithoutPv()
    {
        _moves.Clear();

        GenerateWhiteMoves(_sortContext.Squares);

        for (byte i = 0; i < _moves.Count; i++)
        {
            var move = _moves[i];
            if (_sortContext.IsRegularMove(move))
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteMovesWithoutPv()
    {
        _moves.Clear();

        GenerateWhiteMoves(_sortContext.Squares);

        for (byte i = 0; i < _moves.Count; i++)
        {
            var move = _moves[i];
            move.SetRelativeHistory();
            ProcessMove(move);
        }
    }

    private void ProcessMove(MoveBase move)
    {
        short key = move.Key;
        if (_sortContext.IsKiller(key))
        {
            _sortContext.ProcessKillerMove(move);
        }
        else if (_sortContext.CounterMove == key)
        {
            _sortContext.ProcessCounterMove(move);
        }
        else
        {
            _sortContext.ProcessMove(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackBookCapuresWithPv()
    {
        _attacks.Clear();

        GenerateBlackAttacks(_sortContext.Squares);

        for (byte i = 0; i < _attacks.Count; i++)
        {
            var capture = _attacks[i];
            if (_sortContext.Pv != capture.Key)
            {
                ProcessCaptureMove(capture);
            }
            else
            {
                _sortContext.ProcessHashMove(capture);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCapuresWithPv()
    {
        _attacks.Clear();

        GenerateBlackAttacks(_sortContext.Squares);

        for (byte i = 0; i < _attacks.Count; i++)
        {
            var capture = _attacks[i];
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackBookMovesWithPv()
    {
        _moves.Clear();

        GenerateBlackMoves(_sortContext.Squares);

        for (byte i = 0; i < _moves.Count; i++)
        {
            var move = _moves[i];
            if (_sortContext.Pv == move.Key)
            {
                _sortContext.ProcessHashMove(move);
            }
            else if (_sortContext.IsRegularMove(move))
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackMovesWithPv()
    {
        _moves.Clear();

        GenerateBlackMoves(_sortContext.Squares);

        for (byte i = 0; i < _moves.Count; i++)
        {
            var move = _moves[i];
            if (_sortContext.Pv != move.Key)
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
            else
            {
                _sortContext.ProcessHashMove(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackBookCapuresWithoutPv()
    {
        _attacks.Clear();

        GenerateBlackAttacks(_sortContext.Squares);

        for (byte i = 0; i < _attacks.Count; i++)
        {
            ProcessCaptureMove(_attacks[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCapuresWithoutPv()
    {
        _attacks.Clear();

        GenerateBlackAttacks(_sortContext.Squares);

        for (byte i = 0; i < _attacks.Count; i++)
        {
            _sortContext.ProcessCaptureMove(_attacks[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackBookMovesWithoutPv()
    {
        _moves.Clear();

        GenerateBlackMoves(_sortContext.Squares);

        for (byte i = 0; i < _moves.Count; i++)
        {
            var move = _moves[i];
            if (_sortContext.IsRegularMove(move))
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackMovesWithoutPv()
    {
        _moves.Clear();

        GenerateBlackMoves(_sortContext.Squares);

        for (byte i = 0; i < _moves.Count; i++)
        {
            var move = _moves[i];
            move.SetRelativeHistory();
            ProcessMove(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateWhiteAttacks(SquareList[] Squares)
    {
        _moveProvider.GetWhitePawnAttacks(Squares[WhitePawn], _attacks);
        _moveProvider.GetWhiteKnightAttacks(Squares[WhiteKnight], _attacks);
        _moveProvider.GetWhiteBishopAttacks(Squares[WhiteBishop], _attacks);
        _moveProvider.GetWhiteRookAttacks(Squares[WhiteRook], _attacks);
        _moveProvider.GetWhiteQueenAttacks(Squares[WhiteQueen], _attacks);
        _moveProvider.GetWhiteKingAttacks(Squares[WhiteKing], _attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateBlackAttacks(SquareList[] Squares)
    {
        _moveProvider.GetBlackPawnAttacks(Squares[WhitePawn], _attacks);
        _moveProvider.GetBlackKnightAttacks(Squares[WhiteKnight], _attacks);
        _moveProvider.GetBlackBishopAttacks(Squares[WhiteBishop], _attacks);
        _moveProvider.GetBlackRookAttacks(Squares[WhiteRook], _attacks);
        _moveProvider.GetBlackQueenAttacks(Squares[WhiteQueen], _attacks);
        _moveProvider.GetBlackKingAttacks(Squares[WhiteKing], _attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateWhiteMoves(SquareList[] Squares)
    {
        _moveProvider.GetWhitePawnMoves(Squares[WhitePawn], _moves);
        _moveProvider.GetWhiteKnightMoves(Squares[WhiteKnight], _moves);
        _moveProvider.GetWhiteBishopMoves(Squares[WhiteBishop], _moves);
        _moveProvider.GetWhiteRookMoves(Squares[WhiteRook], _moves);
        _moveProvider.GetWhiteQueenMoves(Squares[WhiteQueen], _moves);
        _moveProvider.GetWhiteKingMoves(Squares[WhiteKing], _moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateBlackMoves(SquareList[] Squares)
    {
        _moveProvider.GetBlackPawnMoves(Squares[WhitePawn], _moves);
        _moveProvider.GetBlackKnightMoves(Squares[WhiteKnight], _moves);
        _moveProvider.GetBlackBishopMoves(Squares[WhiteBishop], _moves);
        _moveProvider.GetBlackRookMoves(Squares[WhiteRook], _moves);
        _moveProvider.GetBlackQueenMoves(Squares[WhiteQueen], _moves);
        _moveProvider.GetBlackKingMoves(Squares[WhiteKing], _moves);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PossibleSingleWhiteAttacks(SquareList[] squares, AttackList attacks)
    {
        BitBoard to = new BitBoard();
        _moveProvider.GetWhitePawnSingleAttacks(squares[WhitePawn], attacks, ref to);
        _moveProvider.GetWhiteKnightSingleAttacks(squares[WhiteKnight], attacks, ref to);
        _moveProvider.GetWhiteBishopSingleAttacks(squares[WhiteBishop], attacks, ref to);
        _moveProvider.GetWhiteRookSingleAttacks(squares[WhiteRook], attacks, ref to);
        _moveProvider.GetWhiteQueenSingleAttacks(squares[WhiteQueen], attacks, ref to);
        _moveProvider.GetWhiteKingSingleAttacks(squares[WhiteKing], attacks, ref to);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PossibleSingleBlackAttacks(SquareList[] squares, AttackList attacks)
    {
        BitBoard to = new BitBoard();
        _moveProvider.GetBlackPawnSingleAttacks(squares[WhitePawn], attacks, ref to);
        _moveProvider.GetBlackKnightSingleAttacks(squares[WhiteKnight], attacks, ref to);
        _moveProvider.GetBlackBishopSingleAttacks(squares[WhiteBishop], attacks, ref to);
        _moveProvider.GetBlackRookSingleAttacks(squares[WhiteRook], attacks, ref to);
        _moveProvider.GetBlackQueenSingleAttacks(squares[WhiteQueen], attacks, ref to);
        _moveProvider.GetBlackKingSingleAttacks(squares[WhiteKing], attacks, ref to);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Board GetBoard() => _board;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<MoveBase> GetHistory() => _moveHistoryService.GetHistory();

    public void SaveHistory()
    {
        var moveFormatter = ContainerLocator.Current.Resolve<IMoveFormatter>();
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

        for (byte i = 1; i < 6; i++)
        {
            _board.GetSquares(pieces[i], squares[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetBlackSquares(byte[] pieces, SquareList[] squares)
    {
        _board.GetBlackPawnSquares(squares[0]);

        for (byte i = 1; i < 6; i++)
        {
            _board.GetSquares(pieces[i], squares[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetWhiteSquares(byte[] pieces)
    {
        _board.GetWhitePawnSquares(_squares[0]);

        for (byte i = 1; i < 6; i++)
        {
            _board.GetSquares(pieces[i], _squares[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetBlackSquares(byte[] pieces)
    {
        _board.GetBlackPawnSquares(_squares[0]);
        for (byte i = 1; i < 6; i++)
        {
            _board.GetSquares(pieces[i], _squares[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MakeFirst(MoveBase move)
    {
        _moveHistoryService.AddFirst(move);

        move.Make();

        move.IsCheck = false;

        _moveHistoryService.AddBoardHistory();
        _moveHistoryService.SetCheck(move.IsCheck);

        _turn = Turn.Black;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Make(MoveBase move)
    {
        if (_turn == Turn.White)
        {
            MakeWhite(move);
        }
        else
        {
            MakeBlack(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MakeWhite(MoveBase move)
    {
        _moveHistoryService.AddWhite(move);

        move.Make();

        move.IsCheck = _board.IsCheckToBlack();

        _moveHistoryService.AddBoardHistory();
        _moveHistoryService.SetCheck(move.IsCheck);

        _turn = Turn.Black;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MakeBlack(MoveBase move)
    {
        _moveHistoryService.AddBlack(move);

        move.Make();

        move.IsCheck = _board.IsCheckToToWhite();

        _moveHistoryService.AddBoardHistory();
        _moveHistoryService.SetCheck(move.IsCheck);

        _turn = Turn.White;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnMake()
    {
        _moveHistoryService.Remove();

        SwapTurn();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnMakeWhite()
    {
        _moveHistoryService.Remove();

        _turn = Turn.White;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnMakeBlack()
    {
        _moveHistoryService.Remove();

        _turn = Turn.Black;
    }

    #endregion


    #region Any Moves/Captures

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteMoves()
    {
        GetWhiteSquares(_white[_moveHistoryService.GetPhase()], _squaresCheck);

        return AnyWhiteMove() || AnyWhiteCapture() || AnyWhitePromotion();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyLigalCapture() => _attacksCheck.Count > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyLigalMoves() => _movesCheck.Count > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteCapture()
    {
        _attacksCheck.Clear();

        return AnyWhitePawnCapture() || AnyWhiteKnightCapture() || AnyWhiteBishopCapture()
            || AnyWhiteRookCapture() || AnyWhiteQueenCapture() || AnyWhiteKingCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackCapture()
    {
        _attacksCheck.Clear();

        return AnyBlackPawnCapture() || AnyBlackKnightCapture() || AnyBlackBishopCapture()
            || AnyBlackRookCapture() || AnyBlackQueenCapture() || AnyBlackKingCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteKingCapture()
    {
        _moveProvider.GetWhiteKingAttacks(_squaresCheck[WhiteKing], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteQueenCapture()
    {
        _moveProvider.GetWhiteQueenAttacks(_squaresCheck[WhiteQueen], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteRookCapture()
    {
        _moveProvider.GetWhiteRookAttacks(_squaresCheck[WhiteRook], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteBishopCapture()
    {
        _moveProvider.GetWhiteBishopAttacks(_squaresCheck[WhiteBishop], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteKnightCapture()
    {
        _moveProvider.GetWhiteKnightAttacks(_squaresCheck[WhiteKnight], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhitePawnCapture()
    {
        _moveProvider.GetWhitePawnAttacks(_squaresCheck[WhitePawn], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackKingCapture()
    {
        _moveProvider.GetBlackKingAttacks(_squaresCheck[WhiteKing], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackQueenCapture()
    {
        _moveProvider.GetBlackQueenAttacks(_squaresCheck[WhiteQueen], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackRookCapture()
    {
        _moveProvider.GetBlackRookAttacks(_squaresCheck[WhiteRook], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackBishopCapture()
    {
        _moveProvider.GetBlackBishopAttacks(_squaresCheck[WhiteBishop], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackKnightCapture()
    {
        _moveProvider.GetBlackKnightAttacks(_squaresCheck[WhiteKnight], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackPawnCapture()
    {
        _moveProvider.GetBlackPawnAttacks(_squaresCheck[WhitePawn], _attacksCheck);
        return AnyLigalCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteMove()
    {
        _movesCheck.Clear();

        return AnyWhiteKingMove() || AnyWhitePawnMove() || AnyWhiteKnightMove() || AnyWhiteBishopMove() ||
            AnyWhiteRookMove() || AnyWhiteQueenMove();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteKingMove()
    {
        _moveProvider.GetWhiteKingMoves(_squaresCheck[WhiteKing], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteQueenMove()
    {
        _moveProvider.GetWhiteQueenMoves(_squaresCheck[WhiteQueen], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteRookMove()
    {
        _moveProvider.GetWhiteRookMoves(_squaresCheck[WhiteRook], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteBishopMove()
    {
        _moveProvider.GetWhiteBishopMoves(_squaresCheck[WhiteBishop], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteKnightMove()
    {
        _moveProvider.GetWhiteKnightMoves(_squaresCheck[WhiteKnight], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhitePawnMove()
    {
        _moveProvider.GetWhitePawnMoves(_squaresCheck[WhitePawn], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackMove()
    {
        _movesCheck.Clear();

        return AnyBlackKingMove() || AnyBlackPawnMove() || AnyBlackKnightMove() || AnyBlackBishopMove() ||
            AnyBlackRookMove() || AnyBlackQueenMove();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackKingMove()
    {
        _moveProvider.GetBlackKingMoves(_squaresCheck[WhiteKing], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackQueenMove()
    {
        _moveProvider.GetBlackQueenMoves(_squaresCheck[WhiteQueen], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackRookMove()
    {
        _moveProvider.GetBlackRookMoves(_squaresCheck[WhiteRook], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackBishopMove()
    {
        _moveProvider.GetBlackBishopMoves(_squaresCheck[WhiteBishop], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackKnightMove()
    {
        _moveProvider.GetBlackKnightMoves(_squaresCheck[WhiteKnight], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackPawnMove()
    {
        _moveProvider.GetBlackPawnMoves(_squaresCheck[WhitePawn], _movesCheck);
        return AnyLigalMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhitePromotion()
    {
        if (_board.CanWhitePromote())
        {
            _board.GetWhitePromotionSquares(_promotionSquaresCheck);

            for (byte f = 0; f < _promotionSquaresCheck.Length; f++)
            {
                var promotions = _moveProvider.GetWhitePromotionAttacks(_promotionSquaresCheck[f]);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count != 0 && _board.IsWhiteMoveLigal(promotions[i][0]))
                        return true;
                }

                var p = _moveProvider.GetWhitePromotions(_promotionSquaresCheck[f]);

                if (p.Count > 0 && _board.IsWhiteMoveLigal(p[0]))
                    return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackMoves()
    {
        GetBlackSquares(_black[_moveHistoryService.GetPhase()], _squaresCheck);

        return AnyBlackMove() || AnyBlackCapture() || AnyBlackPromotion();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackPromotion()
    {
        if (_board.CanBlackPromote())
        {
            _board.GetBlackPromotionSquares(_promotionSquaresCheck);

            for (byte f = 0; f < _promotionSquaresCheck.Length; f++)
            {
                var promotions = _moveProvider.GetBlackPromotionAttacks(_promotionSquaresCheck[f]);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count != 0 && _board.IsBlackMoveLigal(promotions[i][0]))
                        return true;
                }

                var p = _moveProvider.GetBlackPromotions(_promotionSquaresCheck[f]);

                if (p.Count > 0 && _board.IsBlackMoveLigal(p[0]))
                    return true;
            }
        }
        return false;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SwapTurn() => _turn = _turn == Turn.White ? Turn.Black : Turn.White;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetWhiteTurn() => _turn = Turn.White;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlackTurn() => _turn = Turn.Black;

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Turn = {_turn}, Key = {_board.GetKey()}, Value = {GetValue()}, Static = {GetStaticValue()}");
        builder.AppendLine(_board.ToString());
        return builder.ToString();
    }

    public MoveList GetFirstMoves()
    {
        MoveList moves = new MoveList(20);

        foreach (var p in new List<byte> { Pieces.WhiteKnight })
        {
            foreach (var s in new List<byte> { Squares.B1, Squares.G1 })
            {
                var all = GetAllMoves(s, p);
                foreach (var m in all)
                {
                    moves.Add(m);
                }
            }
        }

        foreach (var p in new List<byte> { Pieces.WhitePawn })
        {
            foreach (var s in new List<byte> { Squares.A2, Squares.B2, Squares.C2, Squares.D2, Squares.E2, Squares.F2, Squares.G2, Squares.H2 })
            {
                var all = GetAllMoves(s, p);
                foreach (var m in all)
                {
                    moves.Add(m);
                }
            }
        }

        return moves;
    }
}