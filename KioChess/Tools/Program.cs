using Engine.Models.Boards;
using Engine.Models.Helpers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Numerics;

class CountResult
{
    public int Count { get; set; }
    public int Bits { get; set; }
    public double Average { get; set; }

    public override string ToString()
    {
        return $"Size = {Count},Bits = {Bits},Relation = {Average}";
    }
}

internal class Program
{
    private static readonly Random Rand = new Random();
    private static void Main(string[] args)
    {
        List<CountResult> results = new List<CountResult>();
        for (int size = 1000000; size <= 10000000; size += 1000000)
        {
            for (int b = 0; b <= 8; b ++)
            {
                List<double> list = new List<double>();
                for (int i = 0; i < 3; i++)
                {
                    list.Add(TestCount(size, b));
                }

                CountResult item = new CountResult { Count = size, Bits = b, Average = list.Average() };
                results.Add(item);
                Console.WriteLine(item);
            }
        }

        File.WriteAllText("CountTest.txt", JsonConvert.SerializeObject(results, Formatting.Indented));

        Console.WriteLine($"Yalla !!!");
        Console.ReadLine();
    }

    private static double TestCount(int size, int b)
    {
        int[][] bits = new int[size][];
        for (int i = 0; i < bits.Length; i++)
        {
            bits[i] = GenerateBits(b);
        }

        var t1 = Stopwatch.StartNew();
        int s1 = Count(bits);
        t1.Stop();
        //Console.WriteLine($"Sum = {s1}, Time = {t1.Elapsed}");

        var t2 = Stopwatch.StartNew();
        int s2 = PopCount(bits);
        t2.Stop();
        //Console.WriteLine($"Sum = {s2}, Time = {t2.Elapsed}");

        var d = 1.0 * t1.ElapsedTicks / t2.ElapsedTicks;
        Console.WriteLine($"Size = {size},Bits = {b},Relation = {d}");

        return d;
    }

    private static int[] GenerateBits(int count)
    {
        HashSet<int> bits = new HashSet<int>();

        while(bits.Count < count) 
        { 
            bits.Add(Rand.Next(64));
        }

        return bits.ToArray();
    }

    private static int Count(int[][] bits)
    {
        int sum = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            BitBoard bitBoard = new BitBoard();
            bitBoard = bitBoard.Set(bits[i]);
            sum += bitBoard.Count();
        }

        return sum;
    }

    private static int PopCount(int[][] bits)
    {
        int sum = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            BitBoard bitBoard = new BitBoard();
            bitBoard = bitBoard.Set(bits[i]);
            sum += Bits.PopCount(bitBoard.AsValue());
        }

        return sum;
    }
}