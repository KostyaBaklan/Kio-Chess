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
        var bits = GenerateBits(6);
        BitBoard bitBoard = new BitBoard();
        bitBoard = bitBoard.Set(bits);

        var bitScan = bitBoard.BitScan().ToArray();

        List<byte> result = new List<byte>();
        var b = new BitBoard();
        b = b.Set(bits); 
        while (b.Any())
        {
            byte position = (byte)Bits.TrailingZeroCount(b.AsValue());
            result.Add(position);
            b = b.Remove(position);
        }

        var b1 = bitBoard.BitScanForward();
        var b2 = Bits.LeadingZeroCount(bitBoard.AsValue());
        var b3 = Bits.TrailingZeroCount(bitBoard.AsValue());

        //TestCount();

        //TestBitScanForward();

        Console.WriteLine($"Yalla !!!");
        Console.ReadLine();
    }

    private static void TestBitScanForward()
    {
        List<CountResult> results = new List<CountResult>();
        for (int size = 100000; size <= 1000000; size += 100000)
        {
            for (int b = 0; b <= 8; b++)
            {
                List<double> list = new List<double>();
                for (int i = 0; i < 3; i++)
                {
                    list.Add(TestBitScanForward(size, b));
                }

                CountResult item = new CountResult { Count = size, Bits = b, Average = list.Average() };
                results.Add(item);
                Console.WriteLine(item);
            }
        }

        File.WriteAllText("BitScanForwardTest.txt", JsonConvert.SerializeObject(results, Formatting.Indented));
    }

    private static void TestCount()
    {
        List<CountResult> results = new List<CountResult>();
        for (int size = 1000000; size <= 10000000; size += 1000000)
        {
            for (int b = 0; b <= 8; b++)
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
    }

    private static double TestBitScanForward(int size, int b)
    {
        int[][] bits = new int[size][];
        for (int i = 0; i < bits.Length; i++)
        {
            bits[i] = GenerateBits(b);
        }

        var t1 = Stopwatch.StartNew();
        var s1 = BitScanForward(bits);
        t1.Stop();
        //Console.WriteLine($"Sum = {s1}, Time = {t1.Elapsed}");

        var t2 = Stopwatch.StartNew();
        var s2 = ZeroCount(bits);
        t2.Stop();
        //Console.WriteLine($"Sum = {s2}, Time = {t2.Elapsed}");

        var d = 1.0 * t1.ElapsedTicks / t2.ElapsedTicks;
        Console.WriteLine($"Size = {size},Bits = {b},Relation = {d}");

        return d;
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

    private static int BitScanForward(int[][] bits)
    {
        int sum = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            BitBoard bitBoard = new BitBoard();
            bitBoard = bitBoard.Set(bits[i]);
            for (int j = 0; j < bits[i].Length; j++)
            {
                var x =  bitBoard.BitScanForward();
                bitBoard = bitBoard.Remove(x);
            }
        }

        return sum;
    }

    private static int ZeroCount(int[][] bits)
    {
        int sum = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            BitBoard bitBoard = new BitBoard();
            bitBoard = bitBoard.Set(bits[i]);
            for (int j = 0; j < bits[i].Length; j++)
            {
                var x = Bits.TrailingZeroCount(bitBoard.AsValue());
                bitBoard = bitBoard.Remove(x);
            }
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