using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using System.Runtime.CompilerServices;

namespace Engine.Book.Services
{
    public class BookService : IBookService
    {
        private readonly HistoryValue _defaultValue;
        private readonly Dictionary<string, HistoryValue> _history;
        private readonly IDataKeyService _dataKeyService;

        public BookService(IDataKeyService dataKeyService)
        {
            _defaultValue = new HistoryValue();
            _history = new Dictionary<string, HistoryValue>();
            _dataKeyService = dataKeyService;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string history, HistoryValue historyValue)
        {
            _history.Add(history, historyValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<short, int> GetBlackBookValues(ref MoveKeyList history)
        {
            return Get(ref history).GetBlackBookValues();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<short, int> GetWhiteBookValues(ref MoveKeyList history)
        {
            return Get(ref history).GetWhiteBookValues();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, HistoryValue> GetData()
        {
            return _history;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HistoryValue Get(ref MoveKeyList history)
        {
            var key = _dataKeyService.Get(ref history);

            return _history.TryGetValue(key, out var historyValue) ? historyValue : _defaultValue;
        }
    }
}
