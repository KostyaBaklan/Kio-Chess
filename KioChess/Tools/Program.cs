using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;

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

        TestSort();

        Console.WriteLine($"Yalla !!!");
        Console.ReadLine();
    }

    private static void TranspositionTableServiceTest()
    {
        string FormatNumber(int i)
        {
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
            return i.ToString("N0", nfi);
        }

        TranspositionTableService transpositionTableService = new TranspositionTableService();

        for (short i = 0; i < 20; i++)
        {
            var f = transpositionTableService.GetFactor(i);
            var p = transpositionTableService.NextPrime(f);

            Console.WriteLine($"D = {i}, F = {FormatNumber(f)}, P = {FormatNumber(p)}");
        }
    }

    private static void TestHistory()
    {
        IPosition position = new Position();

        List<MoveBase> moves = new List<MoveBase>();

        foreach (var p in new List<byte> { Pieces.WhiteKnight })
        {
            foreach (var s in new List<byte>{Squares.B1,Squares.G1})
            {
                var all = position.GetAllMoves(s, p);
                moves.AddRange(all);
            }
        }

        foreach (var p in new List<byte> { Pieces.WhitePawn })
        {
            foreach (var s in new List<byte>
        {
            Squares.A2,Squares.B2,Squares.C2,Squares.D2,Squares.E2,Squares.F2,Squares.G2,Squares.H2
        })
            {
                var all = position.GetAllMoves(s, p);
                moves.AddRange(all);
            }
        }

        StrategyBase sb1 = new LmrStrategy(9, position);
        StrategyBase sb2 = new LmrStrategy(9, position);

        IMoveProvider moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        foreach (MoveBase move in moves)
        {
            Console.WriteLine(move.ToString());

            foreach (var m in moveProvider.GetAll())
            {
                m.History = 0;
            }

            position.MakeFirst(move);

            for (int i = 0; i < 5; i++)
            {
                var result = sb1.GetResult();

                position.Make(result.Move);

                result = sb2.GetResult();

                position.Make(result.Move);
            }

            moveProvider.SaveHistory(move);

            var history = position.GetHistory().ToList();

            for (int i = 0; i < history.Count; i++)
            {
                position.UnMake();
            }
        }
    }

    private static void ProcessHistory()
    {
        Dictionary<short, int> history = new Dictionary<short, int>();
        var files = Directory.GetFiles(@"C:\Dev\AI\Kio-Chess\KioChess\Tools\bin\Release\net6.0", "History_*.json", SearchOption.TopDirectoryOnly);
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
        var Moves = ServiceLocator.Current.GetInstance<IMoveProvider>()
                .GetAll()
                .ToArray();

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

            for (byte i = 0; i < moves.Length; i++)
            {
                moves[i].Key = i;
                sort.Add(moves[i]);
                insertion.Add(moves[i]);
                array.Add(moves[i]);
            }

            for (int i = 0; i < 1000000; i++)
            {
                var arr = Enumerable.Range(0, size).Select(i => Rand.Next(10000)).ToArray();
                for (byte j = 0; j < arr.Length; j++)
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
                array.SortAndCopy(moves);
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