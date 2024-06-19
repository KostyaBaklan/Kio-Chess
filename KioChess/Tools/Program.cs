using CommonServiceLocator;
using DataAccess.Models;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Config;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Engine.Tools;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Globalization;


class Difference
{
    public int D { get; set; }
    public int T { get; set; }
    public double P { get; set; }

}
internal class Program
{
    private static readonly Random Rand = new Random();
    private static void Main(string[] args)
    {
        Boot.SetUp();

        //Difference();

        //TranspositionTableServiceTest();

        //PieceAttacks();

        //GenerateStaticTables();

        var text = File.ReadAllText(@"C:\Dev\AI\Kio-Chess\KioChess\Application\bin\Release\net7.0-windows\LmrPerformance_10.json");

        LmrParity lmrParity = JsonConvert.DeserializeObject<LmrParity>(text);
        SortedDictionary<int, List<LmrParityItem>> maxLevelItems = new SortedDictionary<int, List<LmrParityItem>>();
        SortedDictionary<int, List<LmrParityItem>> minLevelItems = new SortedDictionary<int, List<LmrParityItem>>();

        foreach (var plyMap in lmrParity.Items)
        {
            foreach (var item in plyMap.Value)
            {
                if (lmrParity.Depth % 2  == item.Depth % 2) // max
                {
                    if(!maxLevelItems.ContainsKey(plyMap.Key))
                        maxLevelItems[plyMap.Key] = new List<LmrParityItem>();
                    maxLevelItems[plyMap.Key].Add(item);
                }
                else
                {
                    if (!minLevelItems.ContainsKey(plyMap.Key))
                        minLevelItems[plyMap.Key] = new List<LmrParityItem>();
                    minLevelItems[plyMap.Key].Add(item);
                }  
            }
        }
        int maxCount = 0;
        int minCount = 0;
        foreach (var plyMap in lmrParity.Items)
        {
            int maxSize = 0;
            int minSize = 0;

            if (maxLevelItems.TryGetValue(plyMap.Key, out var maxCut))
            {
                maxSize = maxCut.Count;
            }
            if (minLevelItems.TryGetValue(plyMap.Key, out var minCut))
            {
                minSize = minCut.Count;
            }

            string maxMin;
            if (maxSize > minSize)
            {
                maxMin = "max";
                maxCount++;
            }
            else
            {
                maxMin = "min";
                minCount++;
            }

            Console.WriteLine($"{plyMap.Key} {maxSize} {minSize} {maxMin} {minSize - maxSize}");
        }

        Console.WriteLine($"Max = {maxCount}, Min = {minCount}");
        Console.WriteLine();
        Console.WriteLine();

        var countMap = lmrParity.Items.Values.SelectMany(l => l).GroupBy(x => x.Depth).ToImmutableSortedDictionary(k => k.Key, v => v.Count());
        foreach (var count in countMap)
        {
            Console.WriteLine(count);
        }
        Console.WriteLine();
        Console.WriteLine();

        var indexmap = minLevelItems.Values.SelectMany(l => l).GroupBy(x => x.Index).ToImmutableSortedDictionary(k => k.Key, v => v.Count());
        foreach (var index in indexmap)
        {
            Console.WriteLine(index);
        }
        Console.WriteLine();
        Console.WriteLine(); 
        
        Console.WriteLine($"Yalla !!!");
        Console.ReadLine();
    }

    private static void GenerateStaticTables()
    {
        var valueProvide = ServiceLocator.Current.GetInstance<IStaticValueProvider>();

        //Set Minimum
        int[][] minimumTable = new int[12][];

        for (byte piece = 0; piece < 12; piece++)
        {
            minimumTable[piece] = new int[3];
            for (byte phase = 0; phase < 3; phase++)
            {
                minimumTable[piece][phase] = short.MaxValue;
                for (byte square = 0; square < 64; square++)
                {
                    var value = valueProvide.GetValue(piece, phase, square);
                    if (value < minimumTable[piece][phase])
                    {
                        minimumTable[piece][phase] = value;
                    }

                }
            }
        }

        //Set Static Table
        StaticTableCollection staticTableCollection = new StaticTableCollection();
        for (byte piece = 0; piece < 12; piece++)
        {
            PieceStaticTable pieceStaticTable = new PieceStaticTable(piece);
            for (byte phase = 0; phase < 3; phase++)
            {
                pieceStaticTable.AddPhase(phase);
                for (byte square = 0; square < 64; square++)
                {
                    var value = valueProvide.GetValue(piece, phase, square) - minimumTable[piece][phase];
                    pieceStaticTable.AddValue(phase, square.AsString(), (short)value);
                }
            }
            staticTableCollection.Add(piece, pieceStaticTable);
        }

        var json = JsonConvert.SerializeObject(staticTableCollection, Formatting.Indented);
        File.WriteAllText(@"StaticTables.json", json);
    }

    private static void Difference()
    {
        Position position = new Position();

        var moveProvider = ServiceLocator.Current.GetInstance<MoveProvider>();

        var moves = moveProvider.GetAll().Where(m => !m.IsAttack && !m.IsPromotion).ToList();

        var ef = ServiceLocator.Current.GetInstance<IEvaluationServiceFactory>();

        var services = ef.GetEvaluationServices();

        for (int i = 0; i < services.Length; i++)
        {
            var map = moves.GroupBy(m => services[i].GetDifference(m))
                .Where(j => j.Key > 0)
                .ToDictionary(k => k.Key, v => v.Count());

            var total = map.Values.Sum();

            var set = map.OrderByDescending(g => g.Key)
                .ToDictionary(k => k.Key, v => JsonConvert.SerializeObject(new Difference { D = v.Value, T = total, P = Math.Round(100.0 * v.Value / total,4) }));

            var json = JsonConvert.SerializeObject(set, Formatting.Indented);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(json);
            Console.WriteLine();
            Console.WriteLine();
        }
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
        //var pieceAttackWeight = new double[] { 0.0, 0.075, 0.475, 0.725, 0.9, 0.95, 0.975, 1.0, 1.125, 1.25, 1.375, 1.5, 1.625, 1.75, 1.875, 2.0, 2.125, 2.25, 2.375, 2.5 };
        var pieceAttackWeight = new double[] { 0.0, 0.0, 0.5, 0.75, 0.88, 0.94, 0.97, 0.99, 1.0, 1.01, 1.02, 1.03, 1.04, 1.05, 1.06, 1.07, 1.08, 1.09, 1.1, 1.11 };
        //var ds = 1.125;
        for (int i = 1; i < pieceAttackWeight.Length; i++)
        {
            if(pieceAttackWeight[i] > 0)
            {
                pieceAttackWeight[i] -= 0.05;
            }
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

        for (int c = 200000; c < 1200001; c+=100000)
        {
            Console.WriteLine(c);
            for (short i = 0; i < 20; i++)
            {
                var f = transpositionTableService.GetFactor(i,c);
                var p = transpositionTableService.NextPrime(f);

                Console.WriteLine($"D = {i}, F = {FormatNumber(f)}, P = {FormatNumber(p)}");
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }

    private static void TestHistory()
    {
        Position position = new Position();
        var moves = position.GetFirstMoves();

        StrategyBase sb1 = new LmrStrategy(9, position);
        StrategyBase sb2 = new LmrStrategy(9, position);

        MoveProvider moveProvider = ServiceLocator.Current.GetInstance<MoveProvider>();

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