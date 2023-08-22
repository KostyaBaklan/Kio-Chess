using System.Runtime.CompilerServices;

namespace Engine.Book.Models
{
    public class HistoryValue
    {
        private Dictionary<short, BookValue> _values;

        public HistoryValue()
        {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BookValue GetBookValue(short move)
        {
            if(_values.TryGetValue(move, out BookValue bookValue))
            {
                return bookValue;
            }

            return new BookValue();
        }

        public override string ToString()
        {
            return _values.ToString();
        }
    }
}
