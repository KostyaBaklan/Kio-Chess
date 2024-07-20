namespace Engine.Models.Helpers;

public static class RandomHelpers
{
    public static Random Random { get; } = new Random();

    public static ulong NextLong()
    {
        byte[] bytes = new byte[8];
        Random.NextBytes(bytes);
        return (ulong)BitConverter.ToInt64(bytes, 0);
    }

    public static void Shuffle<T>(this T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = Random.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public static void Shuffle<T>(this List<T> array)
    {
        int n = array.Count;
        while (n > 1)
        {
            int k = Random.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}
