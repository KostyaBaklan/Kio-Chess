using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.Services
{
    public class CheckService: ICheckService
    {
        private readonly IMoveProvider _moveProvider;
        private readonly Dictionary<ulong, bool> _whiteTable;
        private readonly Dictionary<ulong, bool> _blackTable;
        //private readonly ZobristDictionary<bool> _whiteTable;
        //private readonly ZobristDictionary<bool> _blackTable;

        public CheckService(IMoveProvider moveProvider)
        {
            _moveProvider = moveProvider;
            _whiteTable = new Dictionary<ulong, bool>(30094277);
            _blackTable = new Dictionary<ulong, bool>(30094277);
        }

        public int Size => _whiteTable.Count + _blackTable.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _whiteTable.Clear();
            _blackTable.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackCheck(ulong key, IBoard board)
        {
            if (_blackTable.TryGetValue(key, out var isCheck)) return isCheck;

            Clear(_blackTable);

            isCheck = _moveProvider.AnyBlackCheck();
            _blackTable.Add(key, isCheck);
            return isCheck;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Clear(Dictionary<ulong, bool> table)
        {
            if (table.Count >= 30000000)
            {
                table.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhiteCheck(ulong key, IBoard board)
        {
            if (_whiteTable.TryGetValue(key, out var isCheck)) return isCheck;

            Clear(_whiteTable);

            isCheck = _moveProvider.AnyWhiteCheck();
            _whiteTable.Add(key, isCheck);
            return isCheck;
        }
    }
}