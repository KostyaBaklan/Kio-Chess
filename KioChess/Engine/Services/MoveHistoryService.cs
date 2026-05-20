using Engine.Dal.Models;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Boards.Buffers;
using Engine.Models.Enums;
using Engine.Models.Hash;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Services;

[SkipLocalsInit]
public class MoveHistoryService
{
    private short _ply = -1;
    private readonly int _popularDepth;

    // Castle rights bitpacking: 4 bits per ply (75% memory reduction vs 4 bool arrays)
    // Bit layout: [W-Small][W-Big][B-Small][B-Big][unused][unused][unused][unused]
    //             bit 7     bit 6  bit 5     bit 4   bits 3-0 (reserved)
    // Example: 0xF0 = 11110000 = all castle rights available
    private byte[] _castleHistory;

    // Castle right bit masks
    private const byte WHITE_SMALL_CASTLE_MASK = 0x80;  // 10000000 - bit 7
    private const byte WHITE_BIG_CASTLE_MASK = 0x40;  // 01000000 - bit 6
    private const byte BLACK_SMALL_CASTLE_MASK = 0x20;  // 00100000 - bit 5
    private const byte BLACK_BIG_CASTLE_MASK = 0x10;  // 00010000 - bit 4
    private const byte WHITE_CASTLE_MASK = 0xC0;  // 11000000 - bits 7-6
    private const byte BLACK_CASTLE_MASK = 0x30;  // 00110000 - bits 5-4
    private const byte ALL_CASTLE_MASK = 0xF0;  // 11110000 - bits 7-4

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
    private readonly Dictionary<long, short> _continiousMoveHistory;

    private readonly short[] _sequence;
    private readonly short _depth;
    private readonly short _search;
    private FrozenDictionary<ulong, PopularMoves> _popularMoves;
    private FrozenDictionary<ulong, MoveHistory[]> _veryPopularMoves;
    private Board _board;

    public MoveHistoryService()
    {
        IConfigurationProvider configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var historyDepth = configurationProvider
            .GeneralConfiguration.DynamicGameDepth;

        _popularDepth = configurationProvider.BookConfiguration.PopularDepth;

        _castleHistory = new byte[historyDepth];
        _history = new MoveBase[historyDepth];
        _boardHistory = new();
        _phases = new byte[historyDepth];
        _nullMoves = new bool[historyDepth];
        _checks = new bool[historyDepth];
        _reversibleMovesHistory = new();
        _depth = configurationProvider.BookConfiguration.SaveDepth;
        _search = configurationProvider.BookConfiguration.SearchDepth;
        _sequence = new short[_depth]; 
        
        for (int i = 0; i < Math.Min(16, historyDepth); i++)
            _phases[i] = Phase.Opening;
        for (int i = 16; i < historyDepth; i++)
            _phases[i] = Phase.Middle;

        var history = ContainerLocator.Current.Resolve<MoveProvider>();
        SetCounterMoves(history.MovesCount);
        _countermoveHistory = new Dictionary<int, short>(capacity: short.MaxValue);
        _continiousMoveHistory = new Dictionary<long, short>(capacity: ushort.MaxValue);
    }

    #region Implementation of MoveHistoryService

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetBoard(Board board) => _board = board;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetPly() => _ply;

    /// <summary>
    /// Create hash-based sequence cache (39% memory reduction, 3-5x faster)
    /// </summary>
    public void CreateSequenceCache(Dictionary<ulong, PopularMoves> map) => _popularMoves = map.ToFrozenDictionary();

    /// <summary>
    /// Create hash-based popular cache (39% memory reduction, 3-5x faster)
    /// </summary>
    public void CreatePopularCache(Dictionary<ulong, MoveHistory[]> popular) => _veryPopularMoves = popular.ToFrozenDictionary();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetSequence(ref MoveKeyList keys) => keys.Add(new Span<short>(_sequence, 0, Math.Min(keys._items.Length, _ply + 1)));

    /// <summary>
    /// Get hash-based sequence key for current position (order-independent, no sorting needed)
    /// Used during game play where moves may be in any order
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetSequenceHash()
    {
        // Get current sequence from live game (order doesn't matter)
        ReadOnlySpan<short> sequence = new(_sequence, 0, Math.Min(_search, _ply + 1));

        // Use order-independent hash - no sorting needed!
        return OrderIndependentSequenceHasher.ComputeOrderIndependentHash(sequence);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] GetSequence()
    {
        MoveKeyList keys = stackalloc short[_search];

        keys.Add(new Span<short>(_sequence, 0, Math.Min(keys._items.Length, _ply + 1)));

        // No sorting needed with order-independent hash

        return keys.AsByteKey();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] GetSequence(int length)
    {
        MoveKeyList keys = stackalloc short[length];

        keys.Add(new Span<short>(_sequence, 0, Math.Min(length, _ply + 1)));

        // No sorting needed with order-independent hash

        return keys.AsByteKey();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short[] GetKeys()
    {
        MoveKeyList keys = stackalloc short[_search];

        keys.Add(new Span<short>(_sequence, 0, Math.Min(keys._items.Length, _ply + 1)));

        // No sorting needed with order-independent hash

        return keys.AsKeys();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveHistory[] GetFirstMoves() => _veryPopularMoves.GetValueOrDefault(0UL);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveHistory[] GetCachedMoves() => _veryPopularMoves.TryGetValue(GetSequenceHash(), out var moves) ? moves : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PopularMoves GetBook() => _popularMoves.TryGetValue(GetSequenceHash(), out var moves) ? moves : PopularMoves.Default;

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

        _reversibleMovesHistory[_ply] = move.IsIrreversible ? 0 : 1;

        _castleHistory[0] = ALL_CASTLE_MASK;  // Set all castle rights (0xF0)
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

        SetPhase();

        if (_ply < _depth)
        {
            _sequence[_ply] = move.Key;
        }

        _reversibleMovesHistory[_ply] = move.IsIrreversible ? 0 : _reversibleMovesHistory[ply] + 1;

        // Copy previous castle state and update white castle rights
        _castleHistory[_ply] = _castleHistory[ply];

        switch (move.Piece)
        {
            case Pieces.WhiteKing:
                // Clear both white castle bits (0xC0)
                _castleHistory[_ply] &= unchecked((byte)~WHITE_CASTLE_MASK);
                break;
            case Pieces.WhiteRook:
                // Clear specific white castle bit based on rook position
                if (move.From == Squares.H1)
                    _castleHistory[_ply] &= unchecked((byte)~WHITE_SMALL_CASTLE_MASK);
                else if (move.From == Squares.A1)
                    _castleHistory[_ply] &= unchecked((byte)~WHITE_BIG_CASTLE_MASK);
                break;
                // Default: no change (already copied from previous ply)
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

        SetPhase();

        _reversibleMovesHistory[_ply] = move.IsIrreversible ? 0 : _reversibleMovesHistory[ply] + 1;

        // Copy previous castle state and update black castle rights
        _castleHistory[_ply] = _castleHistory[ply];

        switch (move.Piece)
        {
            case Pieces.BlackKing:
                // Clear both black castle bits (0x30)
                _castleHistory[_ply] &= unchecked((byte)~BLACK_CASTLE_MASK);
                break;
            case Pieces.BlackRook:
                // Clear specific black castle bit based on rook position
                if (move.From == Squares.H8)
                    _castleHistory[_ply] &= unchecked((byte)~BLACK_SMALL_CASTLE_MASK);
                else if (move.From == Squares.A8)
                    _castleHistory[_ply] &= unchecked((byte)~BLACK_BIG_CASTLE_MASK);
                break;
                // Default: no change (already copied from previous ply)
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetPhase()
    {
        if (_ply < 36)
            return;

        _phases[_ply] = _board.IsEndGame() ? Phase.End : Phase.Middle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove() => _history[_ply--].UnMake();

    /// <summary>
    /// Check if black can castle (either side). Single memory load, branchless operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackCastle() => (_castleHistory[_ply] & BLACK_CASTLE_MASK) != 0;

    /// <summary>
    /// Check if white can castle (either side). Single memory load, branchless operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteCastle() => (_castleHistory[_ply] & WHITE_CASTLE_MASK) != 0;

    /// <summary>
    /// Check if black can castle both sides. Single memory load, branchless operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBothBlackCastle() => (_castleHistory[_ply] & BLACK_CASTLE_MASK) == BLACK_CASTLE_MASK;

    /// <summary>
    /// Check if white can castle both sides. Single memory load, branchless operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBothWhiteCastle() => (_castleHistory[_ply] & WHITE_CASTLE_MASK) == WHITE_CASTLE_MASK;

    /// <summary>
    /// Check if white can castle kingside (O-O). Single memory load, branchless operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteSmallCastle() => (_castleHistory[_ply] & WHITE_SMALL_CASTLE_MASK) != 0;

    /// <summary>
    /// Check if white can castle queenside (O-O-O). Single memory load, branchless operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteBigCastle() => (_castleHistory[_ply] & WHITE_BIG_CASTLE_MASK) != 0;

    /// <summary>
    /// Check if black can castle kingside (O-O). Single memory load, branchless operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackSmallCastle() => (_castleHistory[_ply] & BLACK_SMALL_CASTLE_MASK) != 0;

    /// <summary>
    /// Check if black can castle queenside (O-O-O). Single memory load, branchless operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackBigCastle() => (_castleHistory[_ply] & BLACK_BIG_CASTLE_MASK) != 0;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (short CounterMove, short CountermoveHistory, short ContiniousMoveHistory) GetHeuristicMoves()
    {
        short currentKey = _history[_ply].Key;

        short counterMove = _counterMoves[currentKey];

        if (_ply < 2)
            return (counterMove, -1, -1);

        short prevKey = _history[_ply - 1].Key;
        short countermoveHistory = _countermoveHistory.TryGetValue(prevKey << 16 | (int)currentKey, out var cmh) ? cmh : (short)-1;

        if (_ply < 3)
            return (counterMove, countermoveHistory, -1);

        short continiousMoveHistory = _continiousMoveHistory.TryGetValue(((long)_history[_ply - 2].Key << 32) | ((long)prevKey << 16) | (long)currentKey, out var cmhCont) ? cmhCont : (short)-1;

        return (counterMove, countermoveHistory, continiousMoveHistory);
    }

    #region Countermove History (CMH)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCountermoveHistory(short move)
    {
        if (_ply > 0)
        {
            _countermoveHistory[_history[_ply - 1].Key << 16 | (int)_history[_ply].Key] = move;
            if (_ply > 1)
            {
                _continiousMoveHistory[((long)_history[_ply - 2].Key << 32) | ((long)_history[_ply - 1].Key << 16) | (long)_history[_ply].Key] = move;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetCountiniousMoveHistory()
    {
        if (_ply > 2 && _continiousMoveHistory.TryGetValue(((long)_history[_ply - 2].Key << 32) | ((long)_history[_ply - 1].Key << 16) | (long)_history[_ply].Key, out var refutation))
            return refutation;

        return -1;
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

        EnumerableExtensions.Resize(ref _castleHistory, offset);
        EnumerableExtensions.Resize(ref _history, offset);
        EnumerableExtensions.Resize(ref _phases, offset);
        EnumerableExtensions.Resize(ref _nullMoves, offset);
        EnumerableExtensions.Resize(ref _checks, offset);


        for (int i = previousCapacity; i < _phases.Length; i++)
            _phases[i] = Phase.Middle;
    }

    #endregion
}