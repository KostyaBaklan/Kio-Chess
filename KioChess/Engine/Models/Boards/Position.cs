using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards.Structures;
using Engine.Models.Common;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Models.Boards;

[SkipLocalsInit]
public class Position
{
    private Turn _turn;
    private SortContext _sortContext;

    private readonly AttackList _attacks;
    private readonly MoveList _moves;

    private readonly Board _board;
    private readonly MoveProvider _moveProvider;
    private readonly MoveHistoryService _moveHistoryService;

    public Position()
    {
        _turn = Turn.White;

        _attacks = [];
        _moves = [];

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
            return GetAllMovesForColor<WhiteColor>();
        }
        else
        {
            return GetAllMovesForColor<BlackColor>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<MoveBase> GetAllMovesForColor<TColor>() where TColor : struct, IColorOperations
    {
        List<MoveBase> result = [];
        var color = default(TColor);

        for (byte p = color.PieceStartIndex; p < color.PieceEndIndex; p++)
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
        List<MoveBase> result = [];

        _moveProvider.GetMoves(piece, cell, _moves);
        result.AddRange(_moves);

        _moveProvider.GetAttacks(piece, cell, _attacks);
        result.AddRange(_attacks);

        if (_turn == Turn.White)
        {
            if (_board.CanWhitePromote() && piece == Pieces.WhitePawn && cell > Squares.H6)
            {
                var promotions = _moveProvider.GetWhitePromotions(cell);
                if (promotions.Count > 0)
                {
                    result.AddRange(promotions);
                }
                var promotionsAttack = _moveProvider.GetWhitePromotionAttacks(cell);
                foreach (var pa in promotionsAttack)
                {
                    if (pa.Count > 0)
                    {
                        result.AddRange(pa);
                    }
                }
            }
        }
        else
        {
            if (_board.CanBlackPromote() && piece == Pieces.BlackPawn && cell < Squares.A3)
            {
                var promotions = _moveProvider.GetBlackPromotions(cell);
                if (promotions.Count > 0)
                {
                    result.AddRange(promotions);
                }
                var promotionsAttack = _moveProvider.GetBlackPromotionAttacks(cell);
                foreach (var pa in promotionsAttack)
                {
                    if (pa.Count > 0)
                    {
                        result.AddRange(pa);
                    }
                }
            }
        }

        return _turn == Turn.White
            ? result.Where(_board.IsWhiteLigal)
            : result.Where(_board.IsBlackLigal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteAttacks(AttackList attacks)
    {
        default(WhiteColor).GenerateSingleAttacks(_moveProvider, _board, attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackAttacks(AttackList attacks)
    {
        default(BlackColor).GenerateSingleAttacks(_moveProvider, _board, attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void GetAllWhiteForEvaluation(SortContext sortContext, ref MoveHistoryList moves)
    {
        _sortContext = sortContext;

        ProcessCapuresWithoutPv<WhiteColor>();
        if (_board.CanWhitePromote())
        {
            var promotions = _board.GetWhitePromotionSquares();
            ProcessPromotionCapuresWithoutPv<WhiteColor>(promotions);

            ProcessPromotionsWithoutPv<WhiteColor>(promotions);
        }

        _sortContext.GetMoves(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void GetAllBlackForEvaluation(SortContext sortContext, ref MoveHistoryList moves)
    {
        _sortContext = sortContext;

        ProcessCapuresWithoutPv<BlackColor>();
        if (_board.CanBlackPromote())
        {
            var promotions = _board.GetBlackPromotionSquares();
            ProcessPromotionCapuresWithoutPv<BlackColor>(promotions);

            ProcessPromotionsWithoutPv<BlackColor>(promotions);
        }

        _sortContext.GetMoves(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAllBookMoves<TColor>(SortContext sc, ref MoveHistoryList moves) where TColor : struct, IColorOperations
    {
        _sortContext = sc;

        if (sc.IsRegular)
        {
            ProcessRegularMoves<TColor>();
        }
        else
        {
            ProcessBookMoves<TColor>();
        }

        _sortContext.GetMoves(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAllWhiteMoves(SortContext sc, ref MoveHistoryList moves)
    {
        _sortContext = sc;
        ProcessRegularMoves<WhiteColor>();
        _sortContext.GetMoves(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAllBlackMoves(SortContext sc, ref MoveHistoryList moves)
    {
        _sortContext = sc;
        ProcessRegularMoves<BlackColor>();
        _sortContext.GetMoves(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBookMoves<TColor>() where TColor : struct, IColorOperations
    {
        if (_sortContext.HasPv)
        {
            if (_sortContext.IsPvCapture)
            {
                ProcessBookCapuresWithPv<TColor>();
                if (default(TColor).CanPromote(_board))
                {
                    var promotions = default(TColor).GetPromotionSquares(_board);
                    ProcessPromotionCapuresWithPv<TColor>(promotions);

                    ProcessPromotionsWithoutPv<TColor>(promotions);
                }
                ProcessBookMovesWithoutPv<TColor>();
            }
            else
            {
                ProcessBookCapuresWithoutPv<TColor>();
                if (default(TColor).CanPromote(_board))
                {
                    var promotions = default(TColor).GetPromotionSquares(_board);
                    ProcessPromotionCapuresWithoutPv<TColor>(promotions);

                    ProcessPromotionsWithPv<TColor>(promotions);
                }
                ProcessBookMovesWithPv<TColor>();
            }
        }
        else
        {
            ProcessBookCapuresWithoutPv<TColor>();
            if (default(TColor).CanPromote(_board))
            {
                var promotions = default(TColor).GetPromotionSquares(_board);
                ProcessPromotionCapuresWithoutPv<TColor>(promotions);

                ProcessPromotionsWithoutPv<TColor>(promotions);
            }
            ProcessBookMovesWithoutPv<TColor>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessRegularMoves<TColor>() where TColor : struct, IColorOperations
    {
        if (_sortContext.HasPv)
        {
            if (_sortContext.IsPvCapture)
            {
                ProcessCapuresWithPv<TColor>();
                if (default(TColor).CanPromote(_board))
                {
                    var promotions = default(TColor).GetPromotionSquares(_board);
                    ProcessPromotionCapuresWithPv<TColor>(promotions);

                    ProcessPromotionsWithoutPv<TColor>(promotions);
                }
                ProcessMovesWithoutPv<TColor>();
            }
            else
            {
                ProcessCapuresWithoutPv<TColor>();
                if (default(TColor).CanPromote(_board))
                {
                    var promotions = default(TColor).GetPromotionSquares(_board);
                    ProcessPromotionCapuresWithoutPv<TColor>(promotions);

                    ProcessPromotionsWithPv<TColor>(promotions);
                }
                ProcessMovesWithPv<TColor>();
            }
        }
        else
        {
            ProcessCapuresWithoutPv<TColor>();
            if (default(TColor).CanPromote(_board))
            {
                var promotions = default(TColor).GetPromotionSquares(_board);
                ProcessPromotionCapuresWithoutPv<TColor>(promotions);

                ProcessPromotionsWithoutPv<TColor>(promotions);
            }
            ProcessMovesWithoutPv<TColor>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessPromotionCapuresWithPv<TColor>(BitBoard board) where TColor : struct, IColorOperations
    {
        while (board.Any())
        {
            var f = board.BitScanForward();
            var promotions = default(TColor).GetPromotionAttacks(_moveProvider, f);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count == 0 || !default(TColor).IsMoveLegal(_board, promotions[i][0]))
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
    private void ProcessPromotionCapuresWithoutPv<TColor>(BitBoard board) where TColor : struct, IColorOperations
    {
        while (board.Any())
        {
            var f = board.BitScanForward();
            var promotions = default(TColor).GetPromotionAttacks(_moveProvider, f);

            for (byte i = 0; i < promotions.Length; i++)
            {
                if (promotions[i].Count != 0 && default(TColor).IsMoveLegal(_board, promotions[i][0]))
                    _sortContext.ProcessPromotionCaptures(promotions[i]);
            }

            board = board.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessPromotionsWithPv<TColor>(BitBoard board) where TColor : struct, IColorOperations
    {
        while (board.Any())
        {
            var f = board.BitScanForward();

            var promotions = default(TColor).GetPromotions(_moveProvider, f);

            if (promotions.Count > 0 && default(TColor).IsMoveLegal(_board, promotions[0]))
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
    private void ProcessPromotionsWithoutPv<TColor>(BitBoard board) where TColor : struct, IColorOperations
    {
        while (board.Any())
        {
            var f = board.BitScanForward();
            var promotions = default(TColor).GetPromotions(_moveProvider, f);

            if (promotions.Count > 0 && default(TColor).IsMoveLegal(_board, promotions[0]))
            {
                _sortContext.ProcessPromotionMoves(promotions);
            }

            board = board.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBookCapuresWithPv<TColor>() where TColor : struct, IColorOperations
    {
        AttackBase capture;
        _attacks.Clear();

        default(TColor).GenerateAttacks(_moveProvider, _board, _attacks);

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
    private void ProcessCapuresWithPv<TColor>() where TColor : struct, IColorOperations
    {
        AttackBase capture;
        _attacks.Clear();

        default(TColor).GenerateAttacks(_moveProvider, _board, _attacks);

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
    private void ProcessBookMovesWithPv<TColor>() where TColor : struct, IColorOperations
    {
        MoveBase move;
        _moves.Clear();

        default(TColor).GenerateMoves(_moveProvider, _board, _moves);

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
            if (_sortContext.Pv == move.Key)
            {
                _sortContext.ProcessHashMove(move);
            }
            else if (_sortContext.IsRegularMove(move))
            {
                ProcessMove(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessMovesWithPv<TColor>() where TColor : struct, IColorOperations
    {
        MoveBase move;
        _moves.Clear();

        default(TColor).GenerateMoves(_moveProvider, _board, _moves);

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
            if (_sortContext.Pv != move.Key)
            {
                ProcessMove(move);
            }
            else
            {
                _sortContext.ProcessHashMove(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBookCapuresWithoutPv<TColor>() where TColor : struct, IColorOperations
    {
        _attacks.Clear();

        default(TColor).GenerateAttacks(_moveProvider, _board, _attacks);

        for (byte i = 0; i < _attacks.Count; i++)
        {
            ProcessCaptureMove(_attacks[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessCapuresWithoutPv<TColor>() where TColor : struct, IColorOperations
    {
        _attacks.Clear();

        default(TColor).GenerateAttacks(_moveProvider, _board, _attacks);

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
    private void ProcessBookMovesWithoutPv<TColor>() where TColor : struct, IColorOperations
    {
        MoveBase move;
        _moves.Clear();

        default(TColor).GenerateMoves(_moveProvider, _board, _moves);

        for (byte i = 0; i < _moves.Count; i++)
        {
            move = _moves[i];
            if (_sortContext.IsRegularMove(move))
            {
                ProcessMove(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessMovesWithoutPv<TColor>() where TColor : struct, IColorOperations
    {
        _moves.Clear();

        default(TColor).GenerateMoves(_moveProvider, _board, _moves);

        for (byte i = 0; i < _moves.Count; i++)
        {
            ProcessMove(_moves[i]);
        }
    }

    private void ProcessMove(MoveBase move)
    {
        short key = move.Key;
        if (_sortContext.IsKiller(key))
        {
            move.SetRelativeHistory();
            _sortContext.ProcessKillerMove(move);
        }
        else if (_sortContext.CounterMove == key)
        {
            _sortContext.ProcessCounterMove(move);
        }
        else if (_sortContext.CountermoveHistoryMove == key)
        {
            _sortContext.ProcessCountermoveHistoryMove(move);
        }
        else if (_sortContext.CountiniousMoveHistory == key)
        {
            _sortContext.ProcessCountiniousMoveHistoryMove(move);
        }
        else
        {
            move.SetRelativeHistory();
            _sortContext.ProcessMove(move);
        }
    }

    // Removed obsolete Generate*Attacks/Moves methods - use IColorOperations.GenerateAttacks/GenerateMoves instead

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
                builder.Append($"{move.Key} - W={moveFormatter.Format(move)} ");
            }
            else
            {
                builder.Append($"{move.Key} - B={moveFormatter.Format(move)} ");
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
    public bool AnyMoves<TColor>() where TColor : struct, IColorOperations
    {
        var color = default(TColor);
        return color.AnyMove(_moveProvider, _board) || 
               color.AnyCapture(_moveProvider, _board) || 
               color.AnyPromotion(_moveProvider, _board);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhitePromotion()
    {
        if (_board.CanWhitePromote())
        {
            var board = _board.GetWhitePromotionSquares();

            while (board.Any())
            {
                var f = board.BitScanForward();

                var promotions = _moveProvider.GetWhitePromotionAttacks(f);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count != 0 && _board.IsWhiteMoveLigal(promotions[i][0]))
                    {
                        return true;
                    }

                    var p = _moveProvider.GetWhitePromotions(f);

                    if (p.Count > 0 && _board.IsWhiteMoveLigal(p[0]))
                    {
                        PromotionMove whitePromotion = p[0];

                        MakeWhite(whitePromotion);
                        AttackBase attack = _board.GetBlackAttackToForPromotion(whitePromotion.To);
                        UnMakeWhite();
                        if (attack == null)
                        {
                            return true;
                        }
                    }

                    board = board.Remove(f);
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackPromotion()
    {
        if (_board.CanBlackPromote())
        {
            var board = _board.GetBlackPromotionSquares();

            while (board.Any())
            {
                var f = board.BitScanForward();

                var promotions = _moveProvider.GetBlackPromotionAttacks(f);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count != 0 && _board.IsBlackMoveLigal(promotions[i][0]))
                    {
                        return true;
                    }

                    var p = _moveProvider.GetBlackPromotions(f);

                    if (p.Count > 0 && _board.IsBlackMoveLigal(p[0]))
                    {
                        PromotionMove blackPromotion = p[0];

                        MakeBlack(blackPromotion);
                        AttackBase attack = _board.GetWhiteAttackToForPromotion(blackPromotion.To);
                        UnMakeBlack();
                        if (attack == null)
                        {
                            return true;
                        }
                    }

                    board = board.Remove(f);
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnySuccessfullWhiteCapture()
    {
        return _moveProvider.AnySuccessfullWhiteQueenAttacks(_board.GetPieceBits(Pieces.WhiteQueen)) ||
        _moveProvider.AnySuccessfullWhiteRookAttacks(_board.GetPieceBits(Pieces.WhiteRook)) ||
        _moveProvider.AnySuccessfullWhiteBishopAttacks(_board.GetPieceBits(Pieces.WhiteBishop)) ||
        _moveProvider.AnySuccessfullWhiteKnightAttacks(_board.GetPieceBits(Pieces.WhiteKnight)) ||
        _moveProvider.AnySuccessfullWhitePawnAttacks(_board.GetWhitePawnSquares()) ||
        _moveProvider.AnySuccessfullWhiteKingAttacks(_board.GetPieceBits(Pieces.WhiteKing));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnySuccessfullBlackCapture()
    {
        return _moveProvider.AnySuccessfullBlackQueenAttacks(_board.GetPieceBits(Pieces.BlackQueen)) ||
        _moveProvider.AnySuccessfullBlackRookAttacks(_board.GetPieceBits(Pieces.BlackRook)) ||
        _moveProvider.AnySuccessfullBlackBishopAttacks(_board.GetPieceBits(Pieces.BlackBishop)) ||
        _moveProvider.AnySuccessfullBlackKnightAttacks(_board.GetPieceBits(Pieces.BlackKnight)) ||
        _moveProvider.AnySuccessfullBlackPawnAttacks(_board.GetBlackPawnSquares()) ||
        _moveProvider.AnySuccessfullBlackKingAttacks(_board.GetPieceBits(Pieces.BlackKing));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnySuccessfullWhiteMove()
    {
        return _moveProvider.AnySuccessfullWhiteQueenMoves(_board.GetPieceBits(Pieces.WhiteQueen), IsSuccessfullWhiteCheck) ||
         _moveProvider.AnySuccessfullWhiteRookMoves(_board.GetPieceBits(Pieces.WhiteRook), IsSuccessfullWhiteCheck) ||
         _moveProvider.AnySuccessfullWhiteBishopMoves(_board.GetPieceBits(Pieces.WhiteBishop), IsSuccessfullWhiteCheck) ||
         _moveProvider.AnySuccessfullWhiteKnightMoves(_board.GetPieceBits(Pieces.WhiteKnight), IsSuccessfullWhiteCheck) ||
         _moveProvider.AnySuccessfullWhitePawnMoves(_board.GetWhitePawnSquares(), IsSuccessfullWhiteCheck) ||
         _moveProvider.AnySuccessfullWhiteKingMoves(_board.GetPieceBits(Pieces.WhiteKing), IsSuccessfullWhiteCheck);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnySuccessfullBlackMove()
    {
        return _moveProvider.AnySuccessfullBlackQueenMoves(_board.GetPieceBits(Pieces.BlackQueen), IsSuccessfullBlackCheck) ||
          _moveProvider.AnySuccessfullBlackRookMoves(_board.GetPieceBits(Pieces.BlackRook), IsSuccessfullBlackCheck) ||
          _moveProvider.AnySuccessfullBlackBishopMoves(_board.GetPieceBits(Pieces.BlackBishop), IsSuccessfullBlackCheck) ||
          _moveProvider.AnySuccessfullBlackKnightMoves(_board.GetPieceBits(Pieces.BlackKnight), IsSuccessfullBlackCheck) ||
          _moveProvider.AnySuccessfullBlackPawnMoves(_board.GetBlackPawnSquares(), IsSuccessfullBlackCheck) ||
          _moveProvider.AnySuccessfullBlackKingMoves(_board.GetPieceBits(Pieces.BlackKing), IsSuccessfullBlackCheck);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSuccessfullWhiteCheck(MoveBase move)
    {
        bool isTreat = false;
        MakeWhite(move);

        if (move.IsCheck)
        {
            var bit = _board.GetBlackKingAttackPositions();

            // Double check is always a serious threat
            if (bit.Count() > 1)
            {
                isTreat = true;
            }
            else
            {
                var counterAttack = _board.GetBlackAttackToForCheck(bit.BitScanForward());
                if (counterAttack == null || _board.StaticExchangeWithPinsWithoutTarget(counterAttack) < 0)
                {
                    isTreat = true;
                }
            }

            UnMakeWhite();

            if (isTreat) return true;
        }
        else
        {
            UnMakeWhite();
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSuccessfullBlackCheck(MoveBase move)
    {
        bool isTreat = false;
        MakeBlack(move);

        if (move.IsCheck)
        {
            var bit = _board.GetWhiteKingAttackPositions();

            // Double check is always a serious threat
            if (bit.Count() > 1)
            {
                isTreat = true;
            }
            else
            {
                var counterAttack = _board.GetWhiteAttackToForCheck(bit.BitScanForward());
                if (counterAttack == null || _board.StaticExchangeWithPinsWithoutTarget(counterAttack) < 0)
                {
                    isTreat = true;
                }
            }

            UnMakeBlack();

            if (isTreat) return true;
        }
        else
        {
            UnMakeBlack();
        }

        return false;
    }
}
