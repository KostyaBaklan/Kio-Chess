using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Transposition;
using Engine.Services;

namespace Engine.DataStructures.Hash;

public class TranspositionTable 
{
    private int _nextLevel;
    private bool _isBlocked;

    private readonly ZoobristKeyList[] _depthTable;
    private readonly MoveHistoryService _moveHistory; 
    private readonly Dictionary<ulong, TranspositionEntry> Table;

    private static Board _board;

    public TranspositionTable(int capacity)
    {
        _nextLevel = 0; 
        Table = new Dictionary<ulong, TranspositionEntry>(capacity);

        var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
        var depth = configurationProvider
            .GeneralConfiguration.GameDepth;
        _moveHistory = ServiceLocator.Current.GetInstance<MoveHistoryService>();

        _depthTable = new ZoobristKeyList[depth];
        for (var i = 0; i < _depthTable.Length; i++)
        {
            _depthTable[i] = new ZoobristKeyList();
        }
    }

    public static void SetBoard(Board board) => _board = board;
    public int Count => WhiteTable.Count + BlackTable.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetWhite(out TranspositionEntry item) => WhiteTable.TryGetValue(_board.GetKey(), out item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetBlack(out TranspositionEntry item) => BlackTable.TryGetValue(_board.GetKey(), out item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlocked() => _isBlocked;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(TranspositionEntry item)
    {
        Table[_board.GetKey()] = item;
        _depthTable[_moveHistory.GetPly()].Add(_board.GetKey());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Table.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update()
    {
        _isBlocked = true;
        Task.Factory.StartNew(() =>
        {
            try
            {
                _depthTable[_nextLevel++].GetAndClear(Table);
            }
            finally
            {
                _isBlocked = false;
            }
        });
    }
}