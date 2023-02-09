using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Transposition;

namespace Engine.DataStructures.Hash
{
    public class TranspositionTable : ZobristDictionary<TranspositionEntry>
    {
        private int _nextLevel;
        private bool _isBlocked;

        private readonly ZoobristKeyList[] _depthTable;
        private readonly IMoveHistoryService _moveHistory;

        public TranspositionTable(int capacity) : base(capacity)
        {
            _nextLevel = 0;

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

        #region Overrides of ZobristDictionary<TranspositionEntry>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Replace(ulong key, TranspositionEntry oldItem, TranspositionEntry newItem)
        {
            if (oldItem.Depth < newItem.Depth)
            {
                Table[key] = newItem;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Add(ulong key, TranspositionEntry item)
        {
            Table.Add(key, item);
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlocked()
        {
            return _isBlocked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong key, TranspositionEntry item)
        {
            Table[key] = item;
            _depthTable[_moveHistory.GetPly()].Add(key);
        }

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
}