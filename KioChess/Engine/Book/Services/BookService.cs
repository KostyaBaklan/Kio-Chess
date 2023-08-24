using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.Models.Moves;
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
        public Dictionary<short, int> GetBlackBookValues(IEnumerable<MoveBase> history)
        {
            return Get(history).GetBlackBookValues();
        }

        public Dictionary<string, HistoryValue> GetData()
        {
            return _history;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<short, int> GetWhiteBookValues(IEnumerable<MoveBase> history)
        {
            return Get(history).GetWhiteBookValues();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HistoryValue Get(IEnumerable<MoveBase> history)
        {
            var key = _dataKeyService.Get(history);

            if (_history.TryGetValue(key, out var historyValue)) return historyValue;

            return _defaultValue;
        }
    }
}
