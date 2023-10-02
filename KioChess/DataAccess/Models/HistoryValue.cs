using Newtonsoft.Json;
using System.Collections;
using System.Runtime.CompilerServices;

namespace DataAccess.Models
{
    public class HistoryValue : IEnumerable<KeyValuePair<short, BookValue>>
    {
        private Dictionary<short, BookValue> _values;
        private readonly BookValue _default;

        public HistoryValue()
        {
            _default = new BookValue();
            _values = new Dictionary<short, BookValue>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short v1, int v2, int v3, int v4)
        {
            _values.Add(v1, new BookValue { White = v2, Draw = v3, Black = v4 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BookValue GetBookValue(short move)
        {
            if (_values.TryGetValue(move, out BookValue bookValue))
            {
                return bookValue;
            }

            return _default;
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
