using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Evaluation;
using Engine.Models.Boards;
using Engine.Models.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Engine.Tools;
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

public class SortingItem
{
    public SortingItem(string key, PerformanceItem performanceItem)
    {
        var split = key.Split('_');
        Name= split[0];
        BeforeKiller = int.Parse(split[1]);
        AfterKiller= int.Parse(split[2]);

        PerformanceItem = performanceItem;
    }
    public string Name { get;  }
    public int BeforeKiller { get;  }
    public int AfterKiller { get; }
    public PerformanceItem PerformanceItem { get; }

    public override string ToString()
    { 
        return $"B={BeforeKiller} A={AfterKiller}";
    }
}

public class SortingStatisticItem
{
    public Dictionary<int, Dictionary<int, int>> BeforeKiller3 { get; set; }
    public Dictionary<int, Dictionary<int, int>> BeforeKiller4 { get; set; }
}

public class PieceAttacksItem
{
    public int Piece { get; internal set; }
    public int AttacksCount { get; internal set; }
    public double PieceAttackWeight { get; internal set; }
    public byte PieceAttackValue { get; internal set; }
    public double Exact { get; internal set; }
    public int Value { get; internal set; }
    //public int Round { get; internal set; }
    public int Total { get; internal set; }
    //public int TotalRound { get; internal set; }
}

public class PieceAttacks
{
    public List<PieceAttacksItem> PieceAttacksItem { get; set; }

    public PieceAttacks()
    {
        PieceAttacksItem = new List<PieceAttacksItem>();
    }
}

internal class Program
{
    private static readonly Random Rand = new Random();
    private static void Main(string[] args)
    {
        Boot.SetUp();

        var pieceAttackValue = new byte[] { 10, 20, 20, 40, 80};

        //for (int i = 0; i < pieceAttackValue.Length; i++)
        //{
        //    pieceAttackValue[i] /= 10;
        //}
        var pieceAttackWeight = new double[] { 0.005, 0.025, 0.105, 0.155, 0.165, 0.175, 0.185, 0.195, 0.2052, 0.215, 0.225, 0.235, 0.245, 0.255, 0.265 };

        //for (int i = 0; i < pieceAttackWeight.Length; i++)
        //{
        //    pieceAttackWeight[i] += 0.005;
        //}

       // var x = JsonConvert.SerializeObject(pieceAttackWeight);

        PieceAttacks pieceAttacks = new PieceAttacks();

        for (int i = 0; i < pieceAttackValue.Length; i++)
        {
            for (int j = 1; j < 3; j++)
            {
                double paw = pieceAttackWeight[j];
                byte pav = pieceAttackValue[i];
                PieceAttacksItem item = new PieceAttacksItem
                {
                    Piece = i,
                    AttacksCount = j,
                    PieceAttackWeight = paw,
                    PieceAttackValue = pav,
                    Exact = pav * paw,
                    Value = (int)(pav * paw),
                    //Round = (int)Math.Round(pav * paw),
                    Total = 5 * (int)(pav * paw),
                    //TotalRound = 5 * (int)Math.Round(pav * paw)
                };

                pieceAttacks.PieceAttacksItem.Add(item);
            }
        }

        File.WriteAllText("PieceAttacks.json", JsonConvert.SerializeObject(pieceAttacks, Formatting.Indented));

        //MoveGenerationPerformanceTest();

        //TestSort();

        Console.WriteLine($"Yalla !!!");
        Console.ReadLine();
    }

    private static void MoveGenerationPerformanceTest()
    {
        var json = File.ReadAllText(@"C:\Projects\AI\Kio-Chess\KioChess\Application\bin\Release\net6.0-windows\MoveGenerationPerformance.json");

        var map = JsonConvert.DeserializeObject<Dictionary<string, PerformanceItem>>(json);

        var items = map.Select(p => new SortingItem(p.Key, p.Value)).ToList();

        var groupByName = items.GroupBy(i => i.Name).ToDictionary(k => k.Key, v => v.ToList());

        Dictionary<int, Dictionary<int, int>> asGroupByBeforeKiller3 = items
            .Where(i => i.Name == "AS" && i.BeforeKiller < 4)
                    .GroupBy(i => i.BeforeKiller)
                    .OrderBy(d => d.Key)
                    .ToDictionary(k => k.Key, v => v.GroupBy(a => a.AfterKiller).OrderBy(f => f.Key).ToDictionary(q => q.Key, w => w.Sum(e => e.PerformanceItem.Count)));

        Dictionary<int, Dictionary<int, int>> asGroupByBeforeKiller4 = items
            .Where(i => i.Name == "AS" && i.BeforeKiller > 3)
                    .GroupBy(i => i.BeforeKiller)
                    .OrderBy(d => d.Key)
                    .ToDictionary(k => k.Key, v => v.GroupBy(a => a.AfterKiller).OrderBy(f => f.Key).ToDictionary(q => q.Key, w => w.Sum(e => e.PerformanceItem.Count)));



        var groupByNameAndBefore = items.GroupBy(i => new { Name = i.Name, i.BeforeKiller }).ToDictionary(k => k.Key, v => v.ToList());

        SortingStatisticItem sortingStatisticItem = new SortingStatisticItem();
        sortingStatisticItem.BeforeKiller3 = asGroupByBeforeKiller3;
        sortingStatisticItem.BeforeKiller4 = asGroupByBeforeKiller4;

        File.WriteAllText("Killers.json", JsonConvert.SerializeObject(sortingStatisticItem, Formatting.Indented));
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