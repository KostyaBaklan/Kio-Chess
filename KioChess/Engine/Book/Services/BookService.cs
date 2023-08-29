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
        public BookMoves GetBlackBookValues(ref MoveKeyList history)
        {
            return Get(ref history).GetBlackBookValues();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BookMoves GetWhiteBookValues(ref MoveKeyList history)
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
            history.Order();

            return _history.TryGetValue(history.AsKey(), out var historyValue) ? historyValue : _defaultValue;
        }

        public Dictionary<short, int> GetBookValues()
        {
            return _history[string.Empty].ToDictionary(k => k.Key, v => v.Value.GetWhite());
        }
    }
}
