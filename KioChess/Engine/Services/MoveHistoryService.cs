using System.Runtime.CompilerServices;
using System.Text;
using Engine.Dal.Models;
using Engine.DataStructures;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Services;

public class MoveHistoryService
{
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

    private short _ply = -1;
    private readonly int _popularDepth;
    private readonly bool[] _whiteSmallCastleHistory;
    private readonly bool[] _whiteBigCastleHistory;
    private readonly bool[] _blackSmallCastleHistory;
    private readonly bool[] _blackBigCastleHistory;
    private readonly byte[] _phases;
    private readonly bool[] _nullMoves;
    private readonly bool[] _checks;
    private readonly MoveBase[] _history;
    private readonly ulong[] _boardHistory;
    private readonly int[] _reversibleMovesHistory;
    private short[] _counterMoves;
    private readonly short[] _sequence;
    private readonly short _depth;
    private readonly short _search;
    private Dictionary<string, PopularMoves> _popularMoves;
    private Dictionary<string, MoveBase[]> _veryPopularMoves;
    private Board _board;

    public MoveHistoryService()
    {
        IConfigurationProvider configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var historyDepth = configurationProvider
            .GeneralConfiguration.GameDepth;

        _popularDepth = configurationProvider.BookConfiguration.PopularDepth;

        _whiteSmallCastleHistory = new bool[historyDepth];
        _whiteBigCastleHistory = new bool[historyDepth];
        _blackSmallCastleHistory = new bool[historyDepth];
        _blackBigCastleHistory = new bool[historyDepth];
        _history = new MoveBase[historyDepth];
        _boardHistory = new ulong[historyDepth];
        _phases = new byte[historyDepth];
        _nullMoves = new bool[historyDepth];
        _checks = new bool[historyDepth];
        _reversibleMovesHistory = new int[historyDepth];
        _depth = configurationProvider.BookConfiguration.SaveDepth;
        _search = configurationProvider.BookConfiguration.SearchDepth;
        _sequence = new short[_depth];

        var history = ContainerLocator.Current.Resolve<MoveProvider>();
        SetCounterMoves(history.MovesCount);
    }

    #region Implementation of MoveHistoryService

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetBoard(Board board) => _board = board;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetPly() => _ply;

    public void CreateSequenceCache(Dictionary<string, PopularMoves> map) => _popularMoves = map;

    public void CreatePopularCache(Dictionary<string, MoveBase[]> popular) => _veryPopularMoves = popular;

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
    public short[] GetKeys()
    {
        MoveKeyList keys = stackalloc short[_search];

        keys.Add(new Span<short>(_sequence, 0, Math.Min(keys._items.Length, _ply + 1)));

        keys.Order();

        return keys.AsKeys();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase[] GetFirstMoves() => _veryPopularMoves[string.Empty];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase[] GetCachedMoves() => _veryPopularMoves.TryGetValue(GetSequenceKey(), out var moves) ? moves : null;

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
    public void SetCheck(bool isCheck)=> _checks[_ply] = isCheck;

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

        _reversibleMovesHistory[_ply] = move.IsIrreversible ? 0 : _reversibleMovesHistory[_ply - 1] + 1;

        _blackSmallCastleHistory[_ply] = _blackSmallCastleHistory[ply];
        _blackBigCastleHistory[_ply] = _blackBigCastleHistory[ply];

        switch (move.Piece)
        {
            case WhiteKing:
                _whiteSmallCastleHistory[_ply] = false;
                _whiteBigCastleHistory[_ply] = false;
                break;
            case WhiteRook:
                _whiteSmallCastleHistory[_ply] = _whiteSmallCastleHistory[ply] && move.From != H1;
                _whiteBigCastleHistory[_ply] = _whiteBigCastleHistory[ply] && move.From != A1;
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

        _reversibleMovesHistory[_ply] = move.IsIrreversible ? 0 : _reversibleMovesHistory[_ply - 1] + 1;

        _whiteSmallCastleHistory[_ply] = _whiteSmallCastleHistory[ply];
        _whiteBigCastleHistory[_ply] = _whiteBigCastleHistory[ply];

        switch (move.Piece)
        {
            case BlackKing:
                _blackSmallCastleHistory[_ply] = false;
                _blackBigCastleHistory[_ply] = false;
                break;
            case BlackRook:
                _blackSmallCastleHistory[_ply] = _blackSmallCastleHistory[ply] && move.From != H8;
                _blackBigCastleHistory[_ply] = _blackBigCastleHistory[ply] && move.From != A8;
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

    #region Overrides of Object

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        foreach (var item in GetHistory())
        {
            builder.Append(item);
        }

        return builder.ToString();
    }

    #endregion
}