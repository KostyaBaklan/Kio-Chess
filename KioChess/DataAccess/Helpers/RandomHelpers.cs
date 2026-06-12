using System.Security.Cryptography;

namespace DataAccess.Helpers;

public static class RandomHelpers
{
    public static Random Random { get; } = new Random();

    public static ulong NextLong()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(8);
        return BitConverter.ToUInt64(bytes);
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
