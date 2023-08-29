using Newtonsoft.Json;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Engine.Book.Models
{
    public class HistoryValue:IEnumerable<KeyValuePair<short, BookValue>>
    {
        private List<BookMove> _suggestedWhiteBookMoves;
        private List<BookMove> _nonSuggestedWhiteBookMoves;
        private List<BookMove> _suggestedBlackBookMoves;
        private List<BookMove> _nonSuggestedBlackBookMoves;
        private Dictionary<short, BookValue> _values;
        private BookMoves _whiteBookValues;
        private BookMoves _blackBookValues;

        public HistoryValue()
        {
            _values = new Dictionary<short, BookValue>();
            _suggestedWhiteBookMoves = new List<BookMove>(2);
            _nonSuggestedWhiteBookMoves= new List<BookMove>(2);
            _suggestedBlackBookMoves = new List<BookMove>(2);
            _nonSuggestedBlackBookMoves = new List<BookMove>(2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short v1, int v2, int v3, int v4)
        {
            _values.Add(v1, new BookValue { White = v2, Draw = v3, Black = v4 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BookMoves GetBlackBookValues()
        {
            if (_blackBookValues == null)
            {
                _blackBookValues = new BookMoves(_suggestedBlackBookMoves, _nonSuggestedBlackBookMoves);
            }
            return _blackBookValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BookMoves GetWhiteBookValues()
        {
            if (_whiteBookValues == null)
            {
                _whiteBookValues = new BookMoves(_suggestedWhiteBookMoves, _nonSuggestedWhiteBookMoves);
            }
            return _whiteBookValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
        {
            var moves = _values
                .OrderByDescending(b=>b.Value)
                .Select(x => new BookMove { Id = x.Key, Value = x.Value.GetWhite() })
                .ToList();

            for (int i = 0; i < moves.Count; i++)
            {
                if(i < 2)
                {
                    if (moves[i].Value > 0)
                    {
                        _suggestedWhiteBookMoves.Add(moves[i]);
                        _nonSuggestedBlackBookMoves.Insert(0,new BookMove { Id = moves[i].Id, Value = -moves[i].Value });
                    }
                }
                else if(i > moves.Count - 3)
                {
                    if (moves[i].Value < 0)
                    {
                        _nonSuggestedWhiteBookMoves.Add(moves[i]);
                        _suggestedBlackBookMoves.Insert(0, new BookMove { Id = moves[i].Id, Value = -moves[i].Value });
                    }
                }
            }
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
