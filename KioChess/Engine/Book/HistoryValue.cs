using System.Runtime.CompilerServices;

namespace Engine.Book
{
    public class HistoryValue
    {
        private string _history;
        private Dictionary<short, BookValue> _values;

        public HistoryValue() { }

        public HistoryValue(string history)
        {
            _history = history;
            _values = new Dictionary<short, BookValue>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short v1, int v2, int v3, int v4)
        {
            _values.Add(v1, new BookValue { White = v2, Draw = v3, Black = v4 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<short, int> GetBlackBookValues()
        {
            return _values.ToDictionary(k => k.Key, v => v.Value.GetBlack());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<short, int> GetWhiteBookValues()
        {
            return _values.ToDictionary(k => k.Key, v => v.Value.GetWhite());
        }
    }
}
