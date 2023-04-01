using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Newtonsoft.Json;
using System.Diagnostics;

public class SortItem
{
    public int Size { get; set; }
    public Dictionary<string, TimeSpan> Counts { get; set; }
}

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

    private static void TestHistory()
    {
        IPosition position = new Position();

        List<MoveBase> moves = new List<MoveBase>();

        foreach (var p in new List<Piece> { Piece.WhiteKnight })
        {
            foreach (var s in new List<Square>
        {
            Squares.B1,Squares.G1
        })
            {
                var all = position.GetAllMoves(s, p);
                moves.AddRange(all);
            }
        }
        foreach (var p in new List<Piece> { Piece.WhitePawn })
        {
            foreach (var s in new List<Square>
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
        List<SortItem> sortItems = new List<SortItem>();

        for (int size = 10; size < 60; size += 2)
        {
            MoveList sort = new MoveList();
            MoveList insertion = new MoveList();
            MoveList heapFull = new MoveList();
            MoveList heap = new MoveList();
            MoveList heapSort = new MoveList();

            Dictionary<string, TimeSpan> counts = new Dictionary<string, TimeSpan>
            {
                {nameof(sort), TimeSpan.Zero },
                {nameof(insertion), TimeSpan.Zero },
                {nameof(heapFull), TimeSpan.Zero },
                {nameof(heap), TimeSpan.Zero },
                {nameof(heapSort), TimeSpan.Zero }
            };


            for (int i = 0; i < 100000; i++)
            {
                var moves = Enumerable.Range(0, size).Select(x => new Move()).ToArray();

                var arr = Enumerable.Range(0, size).Select(x => Rand.Next(10000)).ToArray();

                for (int j = 0; j < arr.Length; j++)
                {
                    moves[j].History = arr[j];
                }

                for (int j = 0; j < moves.Length; j++)
                {
                    var tm = Stopwatch.StartNew();
                    sort.Add(moves[j]);
                    counts[nameof(sort)] += tm.Elapsed;

                    tm = Stopwatch.StartNew();
                    insertion.Add(moves[j]);
                    counts[nameof(insertion)] += tm.Elapsed;

                    tm = Stopwatch.StartNew();
                    heap.Insert(moves[j]);
                    counts[nameof(heap)] += tm.Elapsed;

                    tm = Stopwatch.StartNew();
                    heapFull.Insert(moves[j]);
                    counts[nameof(heapFull)] += tm.Elapsed;

                    tm = Stopwatch.StartNew();
                    heapSort.Insert(moves[j]);
                    counts[nameof(heapSort)] += tm.Elapsed;

                    tm.Stop();
                }

                var t = Stopwatch.StartNew();
                sort.Sort();
                counts[nameof(sort)] += t.Elapsed;

                t = Stopwatch.StartNew();
                insertion.FullSort();
                counts[nameof(insertion)] += t.Elapsed;

                t = Stopwatch.StartNew();
                heap.HeapSort();
                counts[nameof(heap)] += t.Elapsed;

                t = Stopwatch.StartNew();
                heapFull.FullSort();
                counts[nameof(heapFull)] += t.Elapsed;

                t = Stopwatch.StartNew();
                heapSort.Sort();
                counts[nameof(heapSort)] += t.Elapsed;

                t.Stop();

                sort.Clear();
                insertion.Clear();
                heap.Clear();
                heapFull.Clear();
                heapSort.Clear();
            }

            Console.WriteLine($"Size = {size}");
            foreach (var x in counts)
            {
                Console.WriteLine($"{x.Key} = {x.Value}");
            }

            SortItem item = new SortItem
            {
                Size = size,
                Counts = new Dictionary<string, TimeSpan>(counts)
            };

            sortItems.Add(item);

            Console.WriteLine();
        }

        File.WriteAllText("SortResult.json", JsonConvert.SerializeObject(sortItems, Formatting.Indented));
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