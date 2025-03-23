using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;
using System.Text;

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

    private readonly AttackList _attacks;
    private readonly MoveList _moves;
    private readonly PromotionList _promotions;
    private readonly List<PromotionAttackList> _promotionsAttack;

    private readonly Board _board;
    private readonly MoveProvider _moveProvider;
    private readonly MoveHistoryService _moveHistoryService;

    public Position()
    {
        _turn = Turn.White;

        IConfigurationProvider configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var bookConfiguration = configurationProvider.BookConfiguration;

        _attacks = [];
        _moves = [];
        _promotions = [];
        _promotionsAttack = [new PromotionAttackList(), new PromotionAttackList()];

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
    public int GetWhiteValue() => _board.Evaluate();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetBlackValue() => _board.EvaluateOpposite();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetStaticValue()
    {
        if (_turn == Turn.White)
            return _board.GetStaticValue();
        return -_board.GetStaticValue();
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
    public Turn GetTurn() => _turn;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<MoveBase> GetMoves(byte piece, byte to)
    {
        List<MoveBase> result = [];

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
        List<MoveBase> result = [];

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
        List<MoveBase> result = [];

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
        var squares = _board.GetWhitePromotionSquares();

        BitBoard to = new();

        while (squares.Any())
        {
            var f = squares.BitScanForward();

            var promotions = _moveProvider.GetWhitePromotionAttacks(f);

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

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackPromotionAttacks(AttackList attacks)
    {
        var squares = _board.GetBlackPromotionSquares();

        BitBoard to = new();

        while (squares.Any())
        {
            var f = squares.BitScanForward();

            var promotions = _moveProvider.GetBlackPromotionAttacks(f);

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

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteAttacks(AttackList attacks)
    {
        BitBoard to = new();
        _moveProvider.GetWhitePawnSingleAttacks(_board.GetWhitePawnSquares(), attacks, ref to);
        _moveProvider.GetWhiteKnightSingleAttacks(_board.GetPieceBits(WhiteKnight), attacks, ref to);
        _moveProvider.GetWhiteBishopSingleAttacks(_board.GetPieceBits(WhiteBishop), attacks, ref to);
        _moveProvider.GetWhiteRookSingleAttacks(_board.GetPieceBits(WhiteRook), attacks, ref to);
        _moveProvider.GetWhiteQueenSingleAttacks(_board.GetPieceBits(WhiteQueen), attacks, ref to);
        _moveProvider.GetWhiteKingSingleAttacks(_board.GetPieceBits(WhiteKing), attacks, ref to);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackAttacks(AttackList attacks)
    {
        BitBoard to = new();
        _moveProvider.GetBlackPawnSingleAttacks(_board.GetBlackPawnSquares(), attacks, ref to);
        _moveProvider.GetBlackKnightSingleAttacks(_board.GetPieceBits(BlackKnight), attacks, ref to);
        _moveProvider.GetBlackBishopSingleAttacks(_board.GetPieceBits(BlackBishop), attacks, ref to);
        _moveProvider.GetBlackRookSingleAttacks(_board.GetPieceBits(BlackRook), attacks, ref to);
        _moveProvider.GetBlackQueenSingleAttacks(_board.GetPieceBits(BlackQueen), attacks, ref to);
        _moveProvider.GetBlackKingSingleAttacks(_board.GetPieceBits(BlackKing), attacks, ref to);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList GetAllWhiteForEvaluation(SortContext sortContext)
    {
        _sortContext = sortContext;

        ProcessWhiteCapuresWithoutPv();
        if (_board.CanWhitePromote())
        {
            var promotions = _board.GetWhitePromotionSquares();
            ProcessWhitePromotionCapuresWithoutPv(promotions);

            ProcessWhitePromotionsWithoutPv(promotions);
        }

        return _sortContext.GetMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveList GetAllBlackForEvaluation(SortContext sortContext)
    {
        _sortContext = sortContext;

        ProcessBlackCapuresWithoutPv();
        if (_board.CanBlackPromote())
        {
            var promotions = _board.GetBlackPromotionSquares();
            ProcessBlackPromotionCapuresWithoutPv(promotions);

            ProcessBlackPromotionsWithoutPv(promotions);
        }

        return _sortContext.GetMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveList GetAllWhiteAttacks(SortContext sortContext)
    {
        _sortContext = sortContext;

        ProcessWhiteCapuresWithoutPv();
        if (_board.CanWhitePromote())
        {
            var promotions = _board.GetWhitePromotionSquares();
            ProcessWhitePromotionCapuresWithoutPv(promotions);

            ProcessWhitePromotionsWithoutPv(promotions);
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

        ProcessBlackCapuresWithoutPv();
        if (_board.CanBlackPromote())
        {
            var promotions = _board.GetBlackPromotionSquares();
            ProcessBlackPromotionCapuresWithoutPv(promotions);

            ProcessBlackPromotionsWithoutPv(promotions);
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
        if (_sortContext.HasPv)
        {
            if (_sortContext.IsPvCapture)
            {
                ProcessWhiteBookCapuresWithPv();
                if (_board.CanWhitePromote())
                {
                    var promotions = _board.GetWhitePromotionSquares();
                    ProcessWhitePromotionCapuresWithPv(promotions);

                    ProcessWhitePromotionsWithoutPv(promotions);
                }
                ProcessWhiteBookMovesWithoutPv();
            }
            else
            {
                ProcessWhiteBookCapuresWithoutPv();
                if (_board.CanWhitePromote())
                {
                    var promotions = _board.GetWhitePromotionSquares();
                    ProcessWhitePromotionCapuresWithoutPv(promotions);

                    ProcessWhitePromotionsWithPv(promotions);
                }
                ProcessWhiteBookMovesWithPv();
            }
        }
        else
        {
            ProcessWhiteBookCapuresWithoutPv();
            if (_board.CanWhitePromote())
            {
                var promotions = _board.GetWhitePromotionSquares();
                ProcessWhitePromotionCapuresWithoutPv(promotions);

                ProcessWhitePromotionsWithoutPv(promotions);
            }
            ProcessWhiteBookMovesWithoutPv();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBookBlackMoves()
    {
        if (_sortContext.HasPv)
        {
            if (_sortContext.IsPvCapture)
            {
                ProcessBlackBookCapuresWithPv();
                if (_board.CanBlackPromote())
                {
                    var promotions = _board.GetBlackPromotionSquares();
                    ProcessBlackPromotionCapuresWithPv(promotions);

                    ProcessBlackPromotionsWithoutPv(promotions);
                }
                ProcessBlackBookMovesWithoutPv();
            }
            else
            {
                ProcessBlackBookCapuresWithoutPv();
                if (_board.CanBlackPromote())
                {
                    var promotions = _board.GetBlackPromotionSquares();
                    ProcessBlackPromotionCapuresWithoutPv(promotions);

                    ProcessBlackPromotionsWithPv(promotions);
                }
                ProcessBlackBookMovesWithPv();
            }
        }
        else
        {
            ProcessBlackBookCapuresWithoutPv();
            if (_board.CanBlackPromote())
            {
                var promotions = _board.GetBlackPromotionSquares();
                ProcessBlackPromotionCapuresWithoutPv(promotions);

                ProcessBlackPromotionsWithoutPv(promotions);
            }
            ProcessBlackBookMovesWithoutPv();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessRegularWhiteMoves()
    {
        if (_sortContext.HasPv)
        {
            if (_sortContext.IsPvCapture)
            {
                ProcessWhiteCapuresWithPv();
                if (_board.CanWhitePromote())
                {
                    var promotions = _board.GetWhitePromotionSquares();
                    ProcessWhitePromotionCapuresWithPv(promotions);

                    ProcessWhitePromotionsWithoutPv(promotions);
                }
                ProcessWhiteMovesWithoutPv();
            }
            else
            {
                ProcessWhiteCapuresWithoutPv();
                if (_board.CanWhitePromote())
                {
                    var promotions = _board.GetWhitePromotionSquares();
                    ProcessWhitePromotionCapuresWithoutPv(promotions);

                    ProcessWhitePromotionsWithPv(promotions);
                }
                ProcessWhiteMovesWithPv();
            }
        }
        else
        {
            ProcessWhiteCapuresWithoutPv();
            if (_board.CanWhitePromote())
            {
                var promotions = _board.GetWhitePromotionSquares();
                ProcessWhitePromotionCapuresWithoutPv(promotions);

                ProcessWhitePromotionsWithoutPv(promotions);
            }
            ProcessWhiteMovesWithoutPv();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessRegularBlackMoves()
    {
        if (_sortContext.HasPv)
        {
            if (_sortContext.IsPvCapture)
            {
                ProcessBlackCapuresWithPv();
                if (_board.CanBlackPromote())
                {
                    var promotions = _board.GetBlackPromotionSquares();
                    ProcessBlackPromotionCapuresWithPv(promotions);

                    ProcessBlackPromotionsWithoutPv(promotions);
                }
                ProcessBlackMovesWithoutPv();
            }
            else
            {
                ProcessBlackCapuresWithoutPv();
                if (_board.CanBlackPromote())
                {
                    var promotions = _board.GetBlackPromotionSquares();
                    ProcessBlackPromotionCapuresWithoutPv(promotions);

                    ProcessBlackPromotionsWithPv(promotions);
                }
                ProcessBlackMovesWithPv();
            }
        }
        else
        {
            ProcessBlackCapuresWithoutPv();
            if (_board.CanBlackPromote())
            {
                var promotions = _board.GetBlackPromotionSquares();
                ProcessBlackPromotionCapuresWithoutPv(promotions);

                ProcessBlackPromotionsWithoutPv(promotions);
            }
            ProcessBlackMovesWithoutPv();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhitePromotionCapuresWithPv(BitBoard board)
    {
        while (board.Any())
        {
            var f = board.BitScanForward();
            var promotions = _moveProvider.GetWhitePromotionAttacks(f);

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

            board = board.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhitePromotionCapuresWithoutPv(BitBoard board)
    {
        while (board.Any())
        {
            var f = board.BitScanForward();
            var promotions = _moveProvider.GetWhitePromotionAttacks(f);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count != 0 && _board.IsWhiteMoveLigal(promotions[i][0]))
                    _sortContext.ProcessPromotionCaptures(promotions[i]);
            }

            board = board.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackPromotionCapuresWithPv(BitBoard board)
    {
        while (board.Any())
        {
            var f = board.BitScanForward();
            var promotions = _moveProvider.GetBlackPromotionAttacks(f);

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

            board = board.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackPromotionCapuresWithoutPv(BitBoard board)
    {
        while (board.Any())
        {
            var f = board.BitScanForward();
            var promotions = _moveProvider.GetBlackPromotionAttacks(f);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count != 0 && _board.IsBlackMoveLigal(promotions[i][0]))
                    _sortContext.ProcessPromotionCaptures(promotions[i]);
            }

            board = board.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhitePromotionsWithPv(BitBoard board)
    {
        while (board.Any())
        {
            var f = board.BitScanForward();
            var promotions = _moveProvider.GetWhitePromotions(f);

            if (promotions.Count != 0 && _board.IsWhiteMoveLigal(promotions[0]))
            {
                if (promotions.HasPv(_sortContext.Pv))
                {
                    _sortContext.ProcessHashMoves(promotions);
                }
                else
                {
                    _sortContext.ProcessPromotionMoves(promotions);
                }
            }

            board = board.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhitePromotionsWithoutPv(BitBoard board)
    {
        while (board.Any())
        {
            var f = board.BitScanForward();
            var promotions = _moveProvider.GetWhitePromotions(f);

            if (promotions.Count > 0 && _board.IsWhiteMoveLigal(promotions[0]))
            {
                _sortContext.ProcessPromotionMoves(promotions);
            }

            board = board.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackPromotionsWithPv(BitBoard board)
    {
        while (board.Any())
        {
            var f = board.BitScanForward();

            var promotions = _moveProvider.GetBlackPromotions(f);

            if (promotions.Count > 0 && _board.IsBlackMoveLigal(promotions[0]))
            {
                if (promotions.HasPv(_sortContext.Pv))
                {
                    _sortContext.ProcessHashMoves(promotions);
                }
                else
                {
                    _sortContext.ProcessPromotionMoves(promotions);
                }
            }

            board = board.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackPromotionsWithoutPv(BitBoard board)
    {
        while (board.Any())
        {
            var f = board.BitScanForward();
            var promotions = _moveProvider.GetBlackPromotions(f);

            if (promotions.Count > 0 && _board.IsBlackMoveLigal(promotions[0]))
            {
                _sortContext.ProcessPromotionMoves(promotions);
            }

            board = board.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteBookCapuresWithPv()
    {
        AttackBase capture;
        _attacks.Clear();

        GenerateWhiteAttacks();

        for (byte i = 0; i < _attacks.Count; i++)
        {
            capture = _attacks[i];
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
        AttackBase capture;
        _attacks.Clear();

        GenerateWhiteAttacks();

        for (byte i = 0; i < _attacks.Count; i++)
        {
            capture = _attacks[i];
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
        MoveBase move;
        _moves.Clear();

        GenerateWhiteMoves();

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
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
        MoveBase move;
        _moves.Clear();

        GenerateWhiteMoves();

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
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

        GenerateWhiteAttacks();

        for (byte i = 0; i < _attacks.Count; i++)
        {
            ProcessCaptureMove(_attacks[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteCapuresWithoutPv()
    {
        _attacks.Clear();

        GenerateWhiteAttacks();

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
        MoveBase move;
        _moves.Clear();

        GenerateWhiteMoves();

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
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
        MoveBase move;
        _moves.Clear();

        GenerateWhiteMoves();

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
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
        AttackBase capture;
        _attacks.Clear();

        GenerateBlackAttacks();

        for (byte i = 0; i < _attacks.Count; i++)
        {
            capture = _attacks[i];
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
        AttackBase capture;
        _attacks.Clear();

        GenerateBlackAttacks();

        for (byte i = 0; i < _attacks.Count; i++)
        {
            capture = _attacks[i];
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
        MoveBase move;
        _moves.Clear();

        GenerateBlackMoves();

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
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
        MoveBase move;
        _moves.Clear();

        GenerateBlackMoves();

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
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

        GenerateBlackAttacks();

        for (byte i = 0; i < _attacks.Count; i++)
        {
            ProcessCaptureMove(_attacks[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCapuresWithoutPv()
    {
        _attacks.Clear();

        GenerateBlackAttacks();

        for (byte i = 0; i < _attacks.Count; i++)
        {
            _sortContext.ProcessCaptureMove(_attacks[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackBookMovesWithoutPv()
    {
        MoveBase move;
        _moves.Clear();

        GenerateBlackMoves();

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
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
        MoveBase move;
        _moves.Clear();

        GenerateBlackMoves();

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
            move.SetRelativeHistory();
            ProcessMove(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateWhiteAttacks()
    {
        _moveProvider.GetWhitePawnAttacks(_board.GetWhitePawnSquares(), _attacks);
        _moveProvider.GetWhiteKnightAttacks(_board.GetPieceBits(WhiteKnight), _attacks);
        _moveProvider.GetWhiteBishopAttacks(_board.GetPieceBits(WhiteBishop), _attacks);
        _moveProvider.GetWhiteRookAttacks(_board.GetPieceBits(WhiteRook), _attacks);
        _moveProvider.GetWhiteQueenAttacks(_board.GetPieceBits(WhiteQueen), _attacks);
        _moveProvider.GetWhiteKingAttacks(_board.GetPieceBits(WhiteKing), _attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateBlackAttacks()
    {
        _moveProvider.GetBlackPawnAttacks(_board.GetBlackPawnSquares(), _attacks);
        _moveProvider.GetBlackKnightAttacks(_board.GetPieceBits(BlackKnight), _attacks);
        _moveProvider.GetBlackBishopAttacks(_board.GetPieceBits(BlackBishop), _attacks);
        _moveProvider.GetBlackRookAttacks(_board.GetPieceBits(BlackRook), _attacks);
        _moveProvider.GetBlackQueenAttacks(_board.GetPieceBits(BlackQueen), _attacks);
        _moveProvider.GetBlackKingAttacks(_board.GetPieceBits(BlackKing), _attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateWhiteMoves()
    {
        _moveProvider.GetWhitePawnMoves(_board.GetWhitePawnSquares(), _moves);
        _moveProvider.GetWhiteKnightMoves(_board.GetPieceBits(WhiteKnight), _moves);
        _moveProvider.GetWhiteBishopMoves(_board.GetPieceBits(WhiteBishop), _moves);
        _moveProvider.GetWhiteRookMoves(_board.GetPieceBits(WhiteRook), _moves);
        _moveProvider.GetWhiteQueenMoves(_board.GetPieceBits(WhiteQueen), _moves);
        _moveProvider.GetWhiteKingMoves(_board.GetPieceBits(WhiteKing), _moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateBlackMoves()
    {
        _moveProvider.GetBlackPawnMoves(_board.GetBlackPawnSquares(), _moves);
        _moveProvider.GetBlackKnightMoves(_board.GetPieceBits(BlackKnight), _moves);
        _moveProvider.GetBlackBishopMoves(_board.GetPieceBits(BlackBishop), _moves);
        _moveProvider.GetBlackRookMoves(_board.GetPieceBits(BlackRook), _moves);
        _moveProvider.GetBlackQueenMoves(_board.GetPieceBits(BlackQueen), _moves);
        _moveProvider.GetBlackKingMoves(_board.GetPieceBits(BlackKing), _moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        var count = GetHistory().Count();
        for (int i = 0; i < count; i++)
        {
            UnMake();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Board GetBoard() => _board;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<MoveBase> GetHistory() => _moveHistoryService.GetHistory();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteNotLegal(MoveBase move) => _board.IsBlackAttacksTo(_board.GetWhiteKingPosition()) ||
            (move.IsCastle && _board.IsBlackAttacksTo(move.To == C1 ? D1 : F1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackNotLegal(MoveBase move) => _board.IsWhiteAttacksTo(_board.GetBlackKingPosition()) ||
             (move.IsCastle && _board.IsWhiteAttacksTo(move.To == C8 ? D8 : F8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanWhitePromote() => _board.CanWhitePromote();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanBlackPromote() => _board.CanBlackPromote();

    public void SaveHistory()
    {
        var moveFormatter = ContainerLocator.Current.Resolve<IMoveFormatter>();
        IEnumerable<MoveBase> history = GetHistory();
        List<string> moves = [];
        bool isWhite = true;
        StringBuilder builder = new();
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

        move.IsCheck = _board.IsCheckToWhite();

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
        return AnyWhiteMove() || AnyWhiteCapture() || AnyWhitePromotion();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteCapture()
    {
        return _moveProvider.AnyWhitePawnAttacks(_board.GetWhitePawnSquares())
            || _moveProvider.AnyWhiteKnightAttacks(_board.GetPieceBits(WhiteKnight))
            || _moveProvider.AnyWhiteBishopAttacks(_board.GetPieceBits(WhiteBishop))
            || _moveProvider.AnyWhiteRookAttacks(_board.GetPieceBits(WhiteRook))
            || _moveProvider.AnyWhiteQueenAttacks(_board.GetPieceBits(WhiteQueen))
            || _moveProvider.AnyWhiteKingAttacks(_board.GetPieceBits(WhiteKing));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackCapture()
    {
        return _moveProvider.AnyBlackPawnAttacks(_board.GetBlackPawnSquares())
            || _moveProvider.AnyBlackKnightAttacks(_board.GetPieceBits(BlackKnight))
            || _moveProvider.AnyBlackBishopAttacks(_board.GetPieceBits(BlackBishop))
            || _moveProvider.AnyBlackRookAttacks(_board.GetPieceBits(BlackRook))
            || _moveProvider.AnyBlackQueenAttacks(_board.GetPieceBits(BlackQueen))
            || _moveProvider.AnyBlackKingAttacks(_board.GetPieceBits(BlackKing));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteMove()
    {
        return _moveProvider.AnyWhiteKingMoves(_board.GetPieceBits(WhiteKing))
            || _moveProvider.AnyWhitePawnMoves(_board.GetWhitePawnSquares())
            || _moveProvider.AnyWhiteKnightMoves(_board.GetPieceBits(WhiteKnight))
            || _moveProvider.AnyWhiteBishopMoves(_board.GetPieceBits(WhiteBishop))
            || _moveProvider.AnyWhiteRookMoves(_board.GetPieceBits(WhiteRook))
            || _moveProvider.AnyWhiteQueenMoves(_board.GetPieceBits(WhiteQueen));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackMove()
    {
        return _moveProvider.AnyBlackKingMoves(_board.GetPieceBits(BlackKing))
            || _moveProvider.AnyBlackPawnMoves(_board.GetBlackPawnSquares())
            || _moveProvider.AnyBlackKnightMoves(_board.GetPieceBits(BlackKnight))
            || _moveProvider.AnyBlackBishopMoves(_board.GetPieceBits(BlackBishop))
            || _moveProvider.AnyBlackRookMoves(_board.GetPieceBits(BlackRook))
            || _moveProvider.AnyBlackQueenMoves(_board.GetPieceBits(BlackQueen));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhitePromotion()
    {
        if (!_board.CanWhitePromote())
            return false;

        var board = _board.GetWhitePromotionSquares();

        while (board.Any())
        {
            var f = board.BitScanForward();

            var promotions = _moveProvider.GetWhitePromotionAttacks(f);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count != 0 && _board.IsWhiteMoveLigal(promotions[i][0]))
                    return true;
            }

            var p = _moveProvider.GetWhitePromotions(f);

            if (p.Count > 0 && _board.IsWhiteMoveLigal(p[0]))
                return true;

            board = board.Remove(f);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackMoves()
    {
        return AnyBlackMove() || AnyBlackCapture() || AnyBlackPromotion();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackPromotion()
    {
        if (!_board.CanBlackPromote())
            return false;

        var board = _board.GetBlackPromotionSquares();

        while (board.Any())
        {
            var f = board.BitScanForward();

            var promotions = _moveProvider.GetBlackPromotionAttacks(f);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count != 0 && _board.IsBlackMoveLigal(promotions[i][0]))
                    return true;
            }

            var p = _moveProvider.GetBlackPromotions(f);

            if (p.Count > 0 && _board.IsBlackMoveLigal(p[0]))
                return true;

            board = board.Remove(f);
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
        StringBuilder builder = new();
        builder.AppendLine($"Turn = {_turn}, Key = {_board.GetKey()}, Value = {GetValue()}, Static = {GetStaticValue()}");
        builder.AppendLine(_board.ToString());
        return builder.ToString();
    }

    public MoveList GetFirstMoves()
    {
        MoveList moves = new(20);

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