using System.Runtime.CompilerServices;

namespace Engine.Models.Helpers;

public static class EnumerableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Slice<T>(this IEnumerable<T> source, int start, int length) => [.. source.Skip(start).Take(length)];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int[] Factor(this int[] array, int factor) => [.. array.Select(a => a * factor)];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Resize<T>(ref T[] array, int offset)
    {
        Array.Resize(ref array, array.Length + offset);
    }
}
