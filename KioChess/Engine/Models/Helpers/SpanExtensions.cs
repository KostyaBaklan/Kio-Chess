using Engine.DataStructures.Moves;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Models.Helpers;

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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(this Span<short> span, char separator)
    {
        StringBuilder builder = new StringBuilder();

        for (byte i = 0; i < span.Length - 1; i++)
        {
            builder.Append($"{span[i]}{separator}");
        }

        builder.Append(span[span.Length - 1]);

        return builder.ToString();
    }
}
