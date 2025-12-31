using Engine.Dal.Models;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Boards.Buffers;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Services;

public class MoveHistoryService
{
    private short _ply = -1;
    private readonly int _popularDepth;
    private bool[] _whiteSmallCastleHistory;
    private bool[] _whiteBigCastleHistory;
    private bool[] _blackSmallCastleHistory;
    private bool[] _blackBigCastleHistory;
    private byte[] _phases;
    private bool[] _nullMoves;
    private bool[] _checks;
    private MoveBase[] _history;
    private GameBuffer<ulong> _boardHistory;
    private GameBuffer<int> _reversibleMovesHistory;
    private short[] _counterMoves;

    // Countermove History (CMH) - tracks move sequences 2-ply deep
    // Key: (prevMove2, prevMove1) → Value: refutation move
    private readonly Dictionary<int, short> _countermoveHistory;

    private readonly short[] _sequence;
    private readonly short _depth;
    private readonly short _search;
    private FrozenDictionary<string, PopularMoves> _popularMoves;
    private FrozenDictionary<string, MoveHistory[]> _veryPopularMoves;
    private Board _board;

    public MoveHistoryService()
    {
        IConfigurationProvider configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var historyDepth = configurationProvider
            .GeneralConfiguration.DynamicGameDepth;

        _popularDepth = configurationProvider.BookConfiguration.PopularDepth;

        _whiteSmallCastleHistory = new bool[historyDepth];
        _whiteBigCastleHistory = new bool[historyDepth];
        _blackSmallCastleHistory = new bool[historyDepth];
        _blackBigCastleHistory = new bool[historyDepth];
        _history = new MoveBase[historyDepth];
        _boardHistory = new();
        _phases = new byte[historyDepth];
        _nullMoves = new bool[historyDepth];
        _checks = new bool[historyDepth];
        _reversibleMovesHistory = new();
        _depth = configurationProvider.BookConfiguration.SaveDepth;
        _search = configurationProvider.BookConfiguration.SearchDepth;
        _sequence = new short[_depth];

        var history = ContainerLocator.Current.Resolve<MoveProvider>();
        SetCounterMoves(history.MovesCount);
        _countermoveHistory = new Dictionary<int, short>(capacity: short.MaxValue);
    }

    #region Implementation of MoveHistoryService

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetBoard(Board board) => _board = board;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetPly() => _ply;

    public void CreateSequenceCache(Dictionary<string, PopularMoves> map) => _popularMoves = map.ToFrozenDictionary();

    public void CreatePopularCache(Dictionary<string, MoveHistory[]> popular) => _veryPopularMoves = popular.ToFrozenDictionary();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetSequence(ref MoveKeyList keys) => keys.Add(new Span<short>(_sequence, 0, Math.Min(keys._items.Length, _ply + 1)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetSequenceKey()
    {
        MoveKeyList keys = stackalloc short[_search];

        keys.Add(new Span<short>(_sequence, 0, Math.Min(keys._items.Length, _ply + 1)));

        keys.Order();

        return keys.AsStringKey();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetSequenceKey(int length)
    {
        MoveKeyList keys = stackalloc short[length];

        keys.Add(new Span<short>(_sequence, 0, Math.Min(length, _ply + 1)));

        keys.Order();

        return keys.AsStringKey();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] GetSequence()
    {
        MoveKeyList keys = stackalloc short[_search];

        keys.Add(new Span<short>(_sequence, 0, Math.Min(keys._items.Length, _ply + 1)));

        keys.Order();

        return keys.AsByteKey();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] GetSequence(int length)
    {
        MoveKeyList keys = stackalloc short[length];

        keys.Add(new Span<short>(_sequence, 0, Math.Min(length, _ply + 1)));

        keys.Order();

        return keys.AsByteKey();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short[] GetKeys()
    {
        MoveKeyList keys = stackalloc short[_search];

        keys.Add(new Span<short>(_sequence, 0, Math.Min(keys._items.Length, _ply + 1)));

        keys.Order();

        return keys.AsKeys();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveHistory[] GetFirstMoves() => _veryPopularMoves[string.Empty];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveHistory[] GetCachedMoves() => _veryPopularMoves.TryGetValue(GetSequenceKey(), out var moves) ? moves : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PopularMoves GetBook() => _popularMoves.TryGetValue(GetSequenceKey(), out var moves) ? moves : PopularMoves.Default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase GetLastMove() => _history[_ply];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Any() => _ply > -1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLateMiddleGame() => _phases[_ply] == Phase.Middle && _board.IsLateMiddleGame();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPhase() => _phases[_ply];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEndPhase() => _phases[_ply] == Phase.End;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCheck(bool isCheck) => _checks[_ply] = isCheck;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFirst(MoveBase move)
    {
        _history[++_ply] = move;
        _sequence[_ply] = move.Key;
        _phases[_ply] = Phase.Opening;

        _reversibleMovesHistory[_ply] = move.IsIrreversible ? 0 : 1;

        _whiteSmallCastleHistory[0] = true;
        _whiteBigCastleHistory[0] = true;
        _blackSmallCastleHistory[0] = true;
        _blackBigCastleHistory[0] = true;
        _nullMoves[_ply] = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool CanUseNull() => _nullMoves[_ply];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetNull() => _nullMoves[_ply] = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWhite(MoveBase move)
    {
        var ply = _ply;

        _history[++_ply] = move;

        _nullMoves[_ply] = _nullMoves[ply];

        _phases[_ply] = _ply < 16 ? Phase.Opening : _ply > 35 && _board.IsEndGame() ? Phase.End : Phase.Middle;

        if (_ply < _depth)
        {
            _sequence[_ply] = move.Key;
        }

        _reversibleMovesHistory[_ply] = move.IsIrreversible ? 0 : _reversibleMovesHistory[ply] + 1;

        _blackSmallCastleHistory[_ply] = _blackSmallCastleHistory[ply];
        _blackBigCastleHistory[_ply] = _blackBigCastleHistory[ply];

        switch (move.Piece)
        {
            case Pieces.WhiteKing:
                _whiteSmallCastleHistory[_ply] = false;
                _whiteBigCastleHistory[_ply] = false;
                break;
            case Pieces.WhiteRook:
                _whiteSmallCastleHistory[_ply] = _whiteSmallCastleHistory[ply] && move.From != Squares.H1;
                _whiteBigCastleHistory[_ply] = _whiteBigCastleHistory[ply] && move.From != Squares.A1;
                break;
            default:
                _whiteSmallCastleHistory[_ply] = _whiteSmallCastleHistory[ply];
                _whiteBigCastleHistory[_ply] = _whiteBigCastleHistory[ply];
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBlack(MoveBase move)
    {
        var ply = _ply;

        _history[++_ply] = move;

        _nullMoves[_ply] = _nullMoves[ply];

        if (_ply < _depth)
        {
            _sequence[_ply] = move.Key;
        }

        _phases[_ply] = _ply < 16 ? Phase.Opening : _ply > 35 && _board.IsEndGame() ? Phase.End : Phase.Middle;

        _reversibleMovesHistory[_ply] = move.IsIrreversible ? 0 : _reversibleMovesHistory[ply] + 1;

        _whiteSmallCastleHistory[_ply] = _whiteSmallCastleHistory[ply];
        _whiteBigCastleHistory[_ply] = _whiteBigCastleHistory[ply];

        switch (move.Piece)
        {
            case Pieces.BlackKing:
                _blackSmallCastleHistory[_ply] = false;
                _blackBigCastleHistory[_ply] = false;
                break;
            case Pieces.BlackRook:
                _blackSmallCastleHistory[_ply] = _blackSmallCastleHistory[ply] && move.From != Squares.H8;
                _blackBigCastleHistory[_ply] = _blackBigCastleHistory[ply] && move.From != Squares.A8;
                break;
            default:
                _blackSmallCastleHistory[_ply] = _blackSmallCastleHistory[ply];
                _blackBigCastleHistory[_ply] = _blackBigCastleHistory[ply];
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove() => _history[_ply--].UnMake();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackCastle() => _blackSmallCastleHistory[_ply] || _blackBigCastleHistory[_ply];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteCastle() => _whiteSmallCastleHistory[_ply] || _whiteBigCastleHistory[_ply];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteSmallCastle() => _whiteSmallCastleHistory[_ply];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteBigCastle() => _whiteBigCastleHistory[_ply];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackSmallCastle() => _blackSmallCastleHistory[_ply];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackBigCastle() => _blackBigCastleHistory[_ply];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<MoveBase> GetHistory() => _history.Take(_ply + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsThreefoldRepetition()
    {
        if (_reversibleMovesHistory[_ply] < 8)
            return false;

        int count = 1;
        int offset = _ply - _reversibleMovesHistory[_ply];
        ulong board = _board.GetKey();

        for (var i = _ply - 4; i > offset; i -= 2)
        {
            if (_boardHistory[i] == board && ++count > 2)
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFiftyMoves() => _reversibleMovesHistory[_ply] > 99;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBoardHistory() => _boardHistory[_ply] = _board.GetKey();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLastMoveWasCheck() => _checks[_ply];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLastMoveNotReducible() => _checks[_ply] || _history[_ply].CanNotReduceNext;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLast(short key) => _history[_ply].Key == key;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldExtend() => _checks[_ply] || _history[_ply].IsPromotionExtension;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRecapture() => _history[_ply].IsAttack && _history[_ply - 1].IsAttack && (_history[_ply].To == _history[_ply - 1].To || _history[_ply - 2].IsAttack);

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCounterMoves(int size)
    {
        _counterMoves = new short[size];
        for (int i = 0; i < _counterMoves.Length; i++)
        {
            _counterMoves[i] = -1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCounterMove(short move) => _counterMoves[_history[_ply].Key] = move;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetCounterMove() => _counterMoves[_history[_ply].Key];

    #region Countermove History (CMH)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCountermoveHistory(short move)
    {
        if (_ply > 0)
        {
            _countermoveHistory[_history[_ply - 1].Key << 16 | (int)_history[_ply].Key] = move;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetCountermoveHistory()
    {
        if (_ply > 1 && _countermoveHistory.TryGetValue(_history[_ply - 1].Key << 16 | (int)_history[_ply].Key, out var refutation))
            return refutation;

        return -1;
    }

    #endregion

    #region Overrides of Object

    public override string ToString()
    {
        StringBuilder builder = new();

        foreach (var item in GetHistory())
        {
            builder.Append(item);
        }

        return builder.ToString();
    }

    internal void Resize(int offset)
    {
        int previousCapacity = _history.Length;

        EnumerableExtensions.Resize(ref _whiteSmallCastleHistory, offset);
        EnumerableExtensions.Resize(ref _whiteBigCastleHistory, offset);
        EnumerableExtensions.Resize(ref _blackSmallCastleHistory, offset);
        EnumerableExtensions.Resize(ref _blackBigCastleHistory, offset);
        EnumerableExtensions.Resize(ref _history, offset);
        EnumerableExtensions.Resize(ref _phases, offset);
        EnumerableExtensions.Resize(ref _nullMoves, offset);
        EnumerableExtensions.Resize(ref _checks, offset);
    }

    #endregion
}