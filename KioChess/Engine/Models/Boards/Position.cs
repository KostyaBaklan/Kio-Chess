using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards.Structures;
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
    private Turn _turn;
    private SortContext _sortContext;

    private readonly Board _board;
    private readonly MoveProvider _moveProvider;
    private readonly MoveHistoryService _moveHistoryService;

    public Position()
    {
        _turn = Turn.White;

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
        List<MoveBase> result = [];

        _moveProvider.GetMoves(piece, cell, result.Add);

        _moveProvider.GetAttacks(piece, cell, result.Add);

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
    public void GetWhiteAttacks(Action<AttackBase> attacks)
    {
        BitBoard to = new();
        _moveProvider.GetWhitePawnSingleAttacks(_board.GetWhitePawnSquares(), attacks, ref to);
        _moveProvider.GetWhiteKnightSingleAttacks(_board.GetPieceBits(Pieces.WhiteKnight), attacks, ref to);
        _moveProvider.GetWhiteBishopSingleAttacks(_board.GetPieceBits(Pieces.WhiteBishop), attacks, ref to);
        _moveProvider.GetWhiteRookSingleAttacks(_board.GetPieceBits(Pieces.WhiteRook), attacks, ref to);
        _moveProvider.GetWhiteQueenSingleAttacks(_board.GetPieceBits(Pieces.WhiteQueen), attacks, ref to);
        _moveProvider.GetWhiteKingSingleAttacks(_board.GetPieceBits(Pieces.WhiteKing), attacks, ref to);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackAttacks(Action<AttackBase> attacks)
    {
        BitBoard to = new();
        _moveProvider.GetBlackPawnSingleAttacks(_board.GetBlackPawnSquares(), attacks, ref to);
        _moveProvider.GetBlackKnightSingleAttacks(_board.GetPieceBits(Pieces.BlackKnight), attacks, ref to);
        _moveProvider.GetBlackBishopSingleAttacks(_board.GetPieceBits(Pieces.BlackBishop), attacks, ref to);
        _moveProvider.GetBlackRookSingleAttacks(_board.GetPieceBits(Pieces.BlackRook), attacks, ref to);
        _moveProvider.GetBlackQueenSingleAttacks(_board.GetPieceBits(Pieces.BlackQueen), attacks, ref to);
        _moveProvider.GetBlackKingSingleAttacks(_board.GetPieceBits(Pieces.BlackKing), attacks, ref to);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void GetAllWhiteForEvaluation(SortContext sortContext, ref MoveHistoryList moves)
    {
        _sortContext = sortContext;

        ProcessWhiteCapuresWithoutPv();
        if (_board.CanWhitePromote())
        {
            var promotions = _board.GetWhitePromotionSquares();
            ProcessWhitePromotionCapuresWithoutPv(promotions);

            ProcessWhitePromotionsWithoutPv(promotions);
        }

        _sortContext.GetMoves(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void GetAllBlackForEvaluation(SortContext sortContext, ref MoveHistoryList moves)
    {
        _sortContext = sortContext;

        ProcessBlackCapuresWithoutPv();
        if (_board.CanBlackPromote())
        {
            var promotions = _board.GetBlackPromotionSquares();
            ProcessBlackPromotionCapuresWithoutPv(promotions);

            ProcessBlackPromotionsWithoutPv(promotions);
        }

        _sortContext.GetMoves(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAllWhiteBookMoves(SortContext sc, ref MoveHistoryList moves)
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

        _sortContext.GetMoves(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAllWhiteMoves(SortContext sc, ref MoveHistoryList moves)
    {
        _sortContext = sc;
        ProcessRegularWhiteMoves();

        _sortContext.GetMoves(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAllBlackBookMoves(SortContext sc, ref MoveHistoryList moves)
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

        _sortContext.GetMoves(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAllBlackMoves(SortContext sc, ref MoveHistoryList moves)
    {
        _sortContext = sc;
        ProcessRegularBlackMoves();

        _sortContext.GetMoves(ref moves);
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
        GenerateWhiteAttacks(capture =>
        {
            if (_sortContext.Pv != capture.Key)
            {
                ProcessCaptureMove(capture);
            }
            else
            {
                _sortContext.ProcessHashMove(capture);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteCapuresWithPv()
    {
        GenerateWhiteAttacks(capture =>
        {
            if (_sortContext.Pv != capture.Key)
            {
                _sortContext.ProcessCaptureMove(capture);
            }
            else
            {
                _sortContext.ProcessHashMove(capture);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteBookMovesWithPv()
    {
        GenerateWhiteMoves(move=>
        {
            if (_sortContext.Pv == move.Key)
            {
                _sortContext.ProcessHashMove(move);
            }
            else if (_sortContext.IsRegularMove(move))
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteMovesWithPv()
    {
        GenerateWhiteMoves(move =>
        {
            if (_sortContext.Pv != move.Key)
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
            else
            {
                _sortContext.ProcessHashMove(move);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteBookCapuresWithoutPv()
    {
        GenerateWhiteAttacks(ProcessCaptureMove);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteCapuresWithoutPv()
    {
        GenerateWhiteAttacks(_sortContext.ProcessCaptureMove);
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
        GenerateWhiteMoves(move => 
        {
            if (_sortContext.IsRegularMove(move))
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
        } );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteMovesWithoutPv()
    {
        GenerateWhiteMoves(move => 
        {
            move.SetRelativeHistory();
            ProcessMove(move);
        });
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
        else if (_sortContext.CountermoveHistoryMove == key)
        {
            _sortContext.ProcessCountermoveHistoryMove(move);
        }
        else
        {
            _sortContext.ProcessMove(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackBookCapuresWithPv()
    {
        GenerateBlackAttacks(capture =>
        {
            if (_sortContext.Pv != capture.Key)
            {
                ProcessCaptureMove(capture);
            }
            else
            {
                _sortContext.ProcessHashMove(capture);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCapuresWithPv()
    {
        GenerateBlackAttacks(capture =>
        {
            if (_sortContext.Pv != capture.Key)
            {
                _sortContext.ProcessCaptureMove(capture);
            }
            else
            {
                _sortContext.ProcessHashMove(capture);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackBookMovesWithPv()
    {
        GenerateBlackMoves(move => 
        {
            if (_sortContext.Pv == move.Key)
            {
                _sortContext.ProcessHashMove(move);
            }
            else if (_sortContext.IsRegularMove(move))
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackMovesWithPv()
    {
        GenerateBlackMoves(move =>
        {
            if (_sortContext.Pv != move.Key)
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
            else
            {
                _sortContext.ProcessHashMove(move);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackBookCapuresWithoutPv()
    {
        GenerateBlackAttacks(ProcessCaptureMove);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCapuresWithoutPv()
    {
        GenerateBlackAttacks(_sortContext.ProcessCaptureMove);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackBookMovesWithoutPv()
    {
        GenerateBlackMoves(move =>
        {
            if (_sortContext.IsRegularMove(move))
            {
                move.SetRelativeHistory();
                ProcessMove(move);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackMovesWithoutPv()
    {
        GenerateBlackMoves(move =>
        {
            move.SetRelativeHistory();
            ProcessMove(move);
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateWhiteAttacks(Action<AttackBase> action)
    {
        _moveProvider.GetWhitePawnAttacks(_board.GetWhitePawnSquares(), action);
        _moveProvider.GetWhiteKnightAttacks(_board.GetPieceBits(Pieces.WhiteKnight), action);
        _moveProvider.GetWhiteBishopAttacks(_board.GetPieceBits(Pieces.WhiteBishop), action);
        _moveProvider.GetWhiteRookAttacks(_board.GetPieceBits(Pieces.WhiteRook), action);
        _moveProvider.GetWhiteQueenAttacks(_board.GetPieceBits(Pieces.WhiteQueen), action);
        _moveProvider.GetWhiteKingAttacks(_board.GetPieceBits(Pieces.WhiteKing), action );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateBlackAttacks(Action<AttackBase> action)
    {
        _moveProvider.GetBlackPawnAttacks(_board.GetBlackPawnSquares(), action);
        _moveProvider.GetBlackKnightAttacks(_board.GetPieceBits(Pieces.BlackKnight), action);
        _moveProvider.GetBlackBishopAttacks(_board.GetPieceBits(Pieces.BlackBishop), action);
        _moveProvider.GetBlackRookAttacks(_board.GetPieceBits(Pieces.BlackRook), action);
        _moveProvider.GetBlackQueenAttacks(_board.GetPieceBits(Pieces.BlackQueen), action);
        _moveProvider.GetBlackKingAttacks(_board.GetPieceBits(Pieces.BlackKing), action);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateWhiteMoves(Action<MoveBase> action)
    {
        _moveProvider.GetWhitePawnMoves(_board.GetWhitePawnSquares(), action);
        _moveProvider.GetWhiteKnightMoves(_board.GetPieceBits(Pieces.WhiteKnight), action);
        _moveProvider.GetWhiteBishopMoves(_board.GetPieceBits(Pieces.WhiteBishop), action);
        _moveProvider.GetWhiteRookMoves(_board.GetPieceBits(Pieces.WhiteRook), action);
        _moveProvider.GetWhiteQueenMoves(_board.GetPieceBits(Pieces.WhiteQueen), action);
        _moveProvider.GetWhiteKingMoves(_board.GetPieceBits(Pieces.WhiteKing), action);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateBlackMoves(Action<MoveBase> action)
    {
        _moveProvider.GetBlackPawnMoves(_board.GetBlackPawnSquares(), action);
        _moveProvider.GetBlackKnightMoves(_board.GetPieceBits(Pieces.BlackKnight), action);
        _moveProvider.GetBlackBishopMoves(_board.GetPieceBits(Pieces.BlackBishop), action);
        _moveProvider.GetBlackRookMoves(_board.GetPieceBits(Pieces.BlackRook), action);
        _moveProvider.GetBlackQueenMoves(_board.GetPieceBits(Pieces.BlackQueen), action);
        _moveProvider.GetBlackKingMoves(_board.GetPieceBits(Pieces.BlackKing), action);
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
    public bool AnyWhiteMoves() => AnyWhiteMove() || AnyWhiteCapture() || AnyWhitePromotion();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteCapture() => _moveProvider.AnyWhitePawnAttacks(_board.GetWhitePawnSquares())
            || _moveProvider.AnyWhiteKnightAttacks(_board.GetPieceBits(Pieces.WhiteKnight))
            || _moveProvider.AnyWhiteBishopAttacks(_board.GetPieceBits(Pieces.WhiteBishop))
            || _moveProvider.AnyWhiteRookAttacks(_board.GetPieceBits(Pieces.WhiteRook))
            || _moveProvider.AnyWhiteQueenAttacks(_board.GetPieceBits(Pieces.WhiteQueen))
            || _moveProvider.AnyWhiteKingAttacks(_board.GetPieceBits(Pieces.WhiteKing));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackCapture() => _moveProvider.AnyBlackPawnAttacks(_board.GetBlackPawnSquares())
            || _moveProvider.AnyBlackKnightAttacks(_board.GetPieceBits(Pieces.BlackKnight))
            || _moveProvider.AnyBlackBishopAttacks(_board.GetPieceBits(Pieces.BlackBishop))
            || _moveProvider.AnyBlackRookAttacks(_board.GetPieceBits(Pieces.BlackRook))
            || _moveProvider.AnyBlackQueenAttacks(_board.GetPieceBits(Pieces.BlackQueen))
            || _moveProvider.AnyBlackKingAttacks(_board.GetPieceBits(Pieces.BlackKing));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteMove() => _moveProvider.AnyWhiteKingMoves(_board.GetPieceBits(Pieces.WhiteKing))
            || _moveProvider.AnyWhitePawnMoves(_board.GetWhitePawnSquares())
            || _moveProvider.AnyWhiteKnightMoves(_board.GetPieceBits(Pieces.WhiteKnight))
            || _moveProvider.AnyWhiteBishopMoves(_board.GetPieceBits(Pieces.WhiteBishop))
            || _moveProvider.AnyWhiteRookMoves(_board.GetPieceBits(Pieces.WhiteRook))
            || _moveProvider.AnyWhiteQueenMoves(_board.GetPieceBits(Pieces.WhiteQueen));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackMove() => _moveProvider.AnyBlackKingMoves(_board.GetPieceBits(Pieces.BlackKing))
            || _moveProvider.AnyBlackPawnMoves(_board.GetBlackPawnSquares())
            || _moveProvider.AnyBlackKnightMoves(_board.GetPieceBits(Pieces.BlackKnight))
            || _moveProvider.AnyBlackBishopMoves(_board.GetPieceBits(Pieces.BlackBishop))
            || _moveProvider.AnyBlackRookMoves(_board.GetPieceBits(Pieces.BlackRook))
            || _moveProvider.AnyBlackQueenMoves(_board.GetPieceBits(Pieces.BlackQueen));

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
    public bool AnyBlackMoves() => AnyBlackMove() || AnyBlackCapture() || AnyBlackPromotion();

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
