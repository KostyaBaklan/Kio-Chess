using CommonServiceLocator;
using DataAccess.Models;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Engine.Tools;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;

internal class Program
{
    private static readonly Random Rand = new Random();
    private static void Main(string[] args)
    {
        Boot.SetUp();

        Console.WriteLine($"Yalla !!!");

        Console.ReadLine();
    }

    private static GameValue GetGameValue(GameValue value)
    {
        if (value == GameValue.WhiteWin) return GameValue.Draw;
        if (value == GameValue.Draw) return GameValue.BlackWin; 
        return GameValue.WhiteWin;
    }

    private static void PieceAttacks()
    {
        var sa = new List<double> { 0, 5, 50, 75, 88, 94, 97, 99 };
        for (int i = 0; i < 12; i++)
        {
            sa.Add(99 + (i + 1) * 2);
        }

        var we = sa.Select(x => x / 100.0).ToArray();
        var pieceAttackValue = new byte[] { 5, 20, 20, 40, 80, 5 };

        //for (int i = 0; i < pieceAttackValue.Length; i++)
        //{
        //    pieceAttackValue[i] /= 10;
        //}

        var pieceAttackWeightOr = new double[] { 0.0, 0.05, 0.5, 0.75, 0.88, 0.94, 0.97, 0.99, 1.01, 1.03, 1.05, 1.07, 1.09, 1.11, 1.13, 1.15, 1.17, 1.19, 1.21, 1.23 };
        var pieceAttackWeight = new double[] { 0.0, 0.05, 0.5, 0.75, 0.9, 0.95, 0.975, 1.0, 1.125, 1.25, 1.375, 1.5, 1.625, 1.75, 1.875, 2.0, 2.125, 2.25, 2.375, 2.5 };
        //var ds = 1.125;
        for (int i = 1; i < pieceAttackWeight.Length; i++)
        {
            pieceAttackWeight[i] -= 0.005;
        }

        var x = JsonConvert.SerializeObject(pieceAttackWeight);

        PieceAttacks pieceAttacks = new PieceAttacks();

        for (int i = 0; i < pieceAttackValue.Length; i++)
        {
            for (int j = 1; j < 2; j++)
            {
                short pav = (short)(pieceAttackValue[i] * j);
                //for (int k = Math.Max(i - j, 0); k < i; k++)
                //{
                //    pav += pieceAttackValue[k];
                //}
                double paw = pieceAttackWeight[j];
                PieceAttacksItem item = new PieceAttacksItem
                {
                    Piece = i,
                    AttacksCount = j,
                    PieceAttackWeight = paw,
                    PieceAttackValue = pav,
                    Exact = pav * paw,
                    //Value = (int)(pav * paw),
                    //Double = 5.0* pav * paw,
                    Round = Round(pav * paw),
                    //Total = 5 * (int)(pav * paw)
                };

                pieceAttacks.PieceAttacksItem.Add(item);
            }
        }

        File.WriteAllText("PieceAttacks.json", JsonConvert.SerializeObject(pieceAttacks, Formatting.Indented));
    }

    private static int Round(double v)
    {
        var _round = new int[] { 0, -1, -2, 2, 1, 0, -1, -2, 2, 1 }; 
        int x = (int)Math.Round(v, 0, MidpointRounding.AwayFromZero);
        return x + _round[x % 10];
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
        var moves = position.GetFirstMoves();

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