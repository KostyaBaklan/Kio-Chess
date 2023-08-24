using Newtonsoft.Json;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Engine.Book.Models
{
    public class HistoryValue:IEnumerable<KeyValuePair<short, BookValue>>
    {
        private Dictionary<short, BookValue> _values;
        private Dictionary<short, int> _bookValues;

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
            _bookValues ??= _values.ToDictionary(k => k.Key, v => v.Value.GetBlack());
            return _bookValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<short, int> GetWhiteBookValues()
        {
            _bookValues ??= _values.ToDictionary(k => k.Key, v => v.Value.GetWhite());
            return _bookValues;
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

        public void Merge(HistoryValue item)
        {
            foreach (var values in item._values)
            {
                if (_values.TryGetValue(values.Key, out BookValue bookValue))
                {
                    _values[values.Key] = bookValue.Merge(values.Value);
                }
                else
                {
                    _values[values.Key] = values.Value;
                }
            }
        }

        public IEnumerator<KeyValuePair<short, BookValue>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(_values);
        }
    }
}
