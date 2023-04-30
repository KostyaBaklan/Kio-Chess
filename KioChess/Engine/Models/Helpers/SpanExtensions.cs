using Engine.DataStructures.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Models.Helpers
{
    public static class SpanExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InsertionSort(this Span<MoveHistory> _items)
        {
            for (int i = 1; i < _items.Length; i++)
            {
                var key = _items[i];
                int j = i - 1;

                while (j > -1 && key.IsGreater(_items[j]))
                {
                    _items[j + 1] = _items[j];
                    j--;
                }
                _items[j + 1] = key;
            }
        }
    }
}
