using Engine.DataStructures.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Models.Helpers
{
    public static class SpanExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Order(this Span<short> items)
        {
            for (byte i = 1; i < items.Length; i++)
            {
                var key = items[i];
                int j = i - 1;

                while (j > -1 && key < items[j])
                {
                    items[j + 1] = items[j];
                    j--;
                }
                items[j + 1] = key;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InsertionSort(this Span<MoveHistory> items)
        {
            for (byte i = 1; i < items.Length; i++)
            {
                var key = items[i];
                int j = i - 1;

                while (j > -1 && key.IsGreater(items[j]))
                {
                    items[j + 1] = items[j];
                    j--;
                }
                items[j + 1] = key;
            }
        }
    }
}
