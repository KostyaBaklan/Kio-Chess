using Engine.DataStructures.Moves;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Engine.Models.Helpers;

public static class SpanExtensions
{
    public static byte One = 1;
    public static byte Zero = 0; 
    
    public static int FindIndex(this Span<MoveHistory> span, short key)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].Key == key) return i;
        }
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Order(this Span<short> items)
    {
        ref short keysRef = ref MemoryMarshal.GetReference(items);

        for (int i = One; i < items.Length; i++)
        {
            short key = Unsafe.Add(ref keysRef, i);
            int j = i - 1;

            while (j >= 0 && key < Unsafe.Add(ref keysRef, j))
            {
                Unsafe.Add(ref keysRef, j + 1) = Unsafe.Add(ref keysRef, j);
                j--;
            }
            Unsafe.Add(ref keysRef, j + 1) = key;
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
