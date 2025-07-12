using Engine.DataStructures.Moves;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Models.Helpers;

public static class SpanExtensions
{
    public static byte One = 1;
    public static byte Zero = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Order(this Span<short> items)
    {
        for (byte i = One; i < items.Length; i++)
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
        for (byte i = One; i < items.Length; i++)
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
    public static void BinaryInsertionSort(this Span<MoveHistory> items)
    {
        for (int i = 1; i < items.Length; i++)
        {
            MoveHistory key = items[i];
            int lo = 0;
            int hi = i;

            // Binary search to find insertion point
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (key.IsGreater(items[mid]))
                    hi = mid;
                else
                    lo = mid + 1;
            }

            // Move elements to make space for key
            for (int j = i; j > lo; j--)
                items[j] = items[j - 1];

            items[lo] = key;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InsertionSort(this Span<AttackBase> items)
    {
        for (int i = One; i < items.Length; i++)
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
    public static int FindIndex(this Span<MoveBase> span, short key)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].Key == key) return i;
        }
        return -1;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(this Span<short> span, char separator)
    {
        StringBuilder builder = new();

        for (byte i = Zero; i < span.Length - 1; i++)
        {
            builder.Append($"{span[i]}{separator}");
        }

        builder.Append(span[span.Length - 1]);

        return builder.ToString();
    }
}
