using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
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
        Boot.SetUp();
        //ProcessHistory();

        Console.WriteLine($"Yalla !!!");
        Console.ReadLine();
    }

    private static void ProcessHistory()
    {
        Dictionary<short, int> history = new Dictionary<short, int>();
        var files = Directory.GetFiles(@"C:\Dev\AI\Kio-Chess\KioChess\Application\bin\Release\net6.0-windows", "History_*.json", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < files.Length; i++)
        {
            var j = File.ReadAllText(files[i]);
            var h = JsonConvert.DeserializeObject<Dictionary<short, int>>(j);

            foreach (var p in h)
            {
                if (history.ContainsKey(p.Key))
                {
                    history[p.Key] += p.Value;
                }
                else { history[p.Key] = p.Value; }
            }
        }
        foreach (var p in history)
        {
            history[p.Key] = p.Value / files.Length;
        }

        history = history.Where(h => h.Value > 0).ToDictionary(k => k.Key, v => v.Value);

        var json = JsonConvert.SerializeObject(history, Formatting.Indented);
        File.WriteAllText($"History.json", json);
    }

    private static void TestSort()
    {
        for (int size = 10; size < 60; size += 10)
        {
            var moves = Enumerable.Range(0, size).Select(i => new Move()).ToArray();

            MoveList sort = new MoveList();
            MoveList insertion = new MoveList();
            MoveList array = new MoveList();

            Dictionary<string, TimeSpan> counts = new Dictionary<string, TimeSpan>
        {
            {nameof(sort), TimeSpan.Zero },
            {nameof(insertion), TimeSpan.Zero },
            //{typeof(BubbleSorter).Name, TimeSpan.Zero },
            //{typeof(QuickSorter).Name, TimeSpan.Zero },
            {nameof(array), TimeSpan.Zero }
        };

            for (int i = 0; i < moves.Length; i++)
            {
                sort.Add(moves[i]);
                insertion.Add(moves[i]);
                array.Add(moves[i]);
            }

            for (int i = 0; i < 10000000; i++)
            {
                var arr = Enumerable.Range(0, size).Select(i => Rand.Next(10000)).ToArray();
                for (int j = 0; j < arr.Length; j++)
                {
                    sort[j].History = arr[j];
                    insertion[j].History = arr[j];
                    array[j].History = arr[j];
                }

                //Sorter[] sorters = new Sorter[]
                //{
                //    new InsertionSorter(arr),
                //    new SelectionSorter(arr),
                //    //new BubbleSorter(arr),
                //    //new QuickSorter(arr),
                //    new ArraySorter(arr)
                //};

                var t = Stopwatch.StartNew();
                sort.Sort();
                counts[nameof(sort)] += t.Elapsed;

                t = Stopwatch.StartNew();
                insertion.FullSort();
                counts[nameof(insertion)] += t.Elapsed;

                t = Stopwatch.StartNew();
                array.ArraySort();
                counts[nameof(array)] += t.Elapsed;
            }

            Console.WriteLine($"Size = {size}");
            foreach (var item in counts)
            {
                Console.WriteLine($"{item.Key} = {item.Value}");
            }

            Console.WriteLine();
        }
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
}