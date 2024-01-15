using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Transposition;

namespace Engine.DataStructures.Hash;

public class TranspositionTable 
{
    private int _nextLevel;
    private bool _isBlocked;

    private readonly ZoobristKeyList[] _depthTable;
    private readonly IMoveHistoryService _moveHistory; 
    private readonly Dictionary<ulong, TranspositionEntry> Table;

    public TranspositionTable(int capacity)
    {
        _nextLevel = 0; 
        Table = new Dictionary<ulong, TranspositionEntry>(capacity);

        var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
        var depth = configurationProvider
            .GeneralConfiguration.GameDepth;
        _moveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();

        _depthTable = new ZoobristKeyList[depth];
        for (var i = 0; i < _depthTable.Length; i++)
        {
            _depthTable[i] = new ZoobristKeyList();
        }
    }
    public int Count => Table.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(ulong key, out TranspositionEntry item) => Table.TryGetValue(key, out item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlocked() => _isBlocked;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(ulong key, TranspositionEntry item)
    {
        Table[key] = item;
        _depthTable[_moveHistory.GetPly()].Add(key);
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