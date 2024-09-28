using System.Runtime.CompilerServices;
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
    private readonly Dictionary<ulong, TranspositionEntry> WhiteTable;
    private readonly Dictionary<ulong, TranspositionEntry> BlackTable;

    private static Board _board;

    public TranspositionTable(int capacity)
    {
        _nextLevel = 0;
        WhiteTable = new Dictionary<ulong, TranspositionEntry>(capacity);
        BlackTable = new Dictionary<ulong, TranspositionEntry>(capacity);

        var configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var depth = configurationProvider
            .GeneralConfiguration.GameDepth;
        _moveHistory = ContainerLocator.Current.Resolve<MoveHistoryService>();

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
    public void SetWhite(TranspositionEntry item)
    {
        WhiteTable[_board.GetKey()] = item;
        _depthTable[_moveHistory.GetPly()].Add(_board.GetKey());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlack(TranspositionEntry item)
    {
        BlackTable[_board.GetKey()] = item;
        _depthTable[_moveHistory.GetPly()].Add(_board.GetKey());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        WhiteTable.Clear();
        BlackTable.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update()
    {
        _isBlocked = true;
        Task.Factory.StartNew(() =>
        {
            try
            {
                if (_nextLevel%2 != 0)
                {
                    _depthTable[_nextLevel++].GetAndClear(WhiteTable); 
                }
                else
                {
                    _depthTable[_nextLevel++].GetAndClear(BlackTable);
                }
            }
            finally
            {
                _isBlocked = false;
            }
        });
    }
}