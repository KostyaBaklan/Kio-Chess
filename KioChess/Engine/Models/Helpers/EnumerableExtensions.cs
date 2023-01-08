namespace Engine.Models.Helpers
{
    public static class EnumerableExtensions
    {
        public static T[] Slice<T>(this IEnumerable<T> source, int start, int length)
        {
            return source.Skip(start).Take(length).ToArray();
        }

        public static int[] Factor(this int[] array, int factor)
        {
            return array.Select(a => a * factor).ToArray();
        }
    }
}
