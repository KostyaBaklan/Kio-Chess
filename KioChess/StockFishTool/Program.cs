using Engine.Models.Helpers;
using StockFishCore;
using StockFishCore.Data;
using StockFishTool;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        DateTime start = DateTime.Now;
        StockFishClient.StartServer();

        var timer = Stopwatch.StartNew();

        int threads = Environment.ProcessorCount;

        List<StockFishParameters> stockFishParameters = CreateStockFishParameters(threads);

        ParallelExecutor parallelExecutor = new ParallelExecutor(threads, stockFishParameters);

        parallelExecutor.Execute();

        Save(start, DateTime.Now);

        timer.Stop();

        Console.WriteLine();
        Console.WriteLine($"Time = {timer.Elapsed}, Total = {stockFishParameters.Count}, Average = {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds / stockFishParameters.Count)}");
        Console.WriteLine("Yalla");
        Console.WriteLine("^C");
        //Console.ReadLine();
    }

    private static int ExecuteInParallel(Stopwatch timer, int threads, List<StockFishParameters> stockFishParameters)
    {
        int count = 0;
        object sync = new object();

        int size = stockFishParameters.Count;

        Console.WriteLine($"Total games: {size}");

        Parallel.For(0, size, new ParallelOptions { MaxDegreeOfParallelism = threads }, i =>
        {
            StockFishParameters parameters = stockFishParameters[i];

            lock (sync)
            {
                var p = Math.Round(100.0 * (++count) / size, 3);
                parameters.Log(i, timer, p);
            }

            parameters.Execute();
        });
        return size;
    }

    private static List<StockFishParameters> CreateStockFishParameters(int threads)
    {
        StockFishParameters.Initialize();
        List<StockFishParameters> stockFishParameters = new List<StockFishParameters>();
        var colorSize = 50;
        string[] strategies = new string[] { "lmrd" };
        string[] colors = Enumerable.Repeat("w", colorSize).Concat(Enumerable.Repeat("b", colorSize)).ToArray();

        var depthSkillMap = new Dictionary<int, List<Tuple<int, int>>>
        {
            //{6, new List<Tuple<int, int>> {  Tuple.Create(9, 6),Tuple.Create(9, 7)}},
            {7, new List<Tuple<int, int>> {  Tuple.Create(10, 7),Tuple.Create(10, 8)}},
            {8, new List<Tuple<int, int>> {  Tuple.Create(11, 8),Tuple.Create(11, 9) }},
            {9, new List<Tuple<int, int>> { Tuple.Create(12, 9),Tuple.Create(12, 10) }},
            {10, new List<Tuple<int, int>> { Tuple.Create(13, 9), Tuple.Create(13, 10),Tuple.Create(13, 11)}},
            {11, new List<Tuple<int, int>> { Tuple.Create(14, 10), Tuple.Create(14, 11),Tuple.Create(14, 12)}},
            {12, new List<Tuple<int, int>> { Tuple.Create(15, 11), Tuple.Create(15, 12),Tuple.Create(15, 13)}}
        };

        foreach (KeyValuePair<int, List<Tuple<int, int>>> dsm in depthSkillMap)
        {
            foreach (Tuple<int, int> skillMap in dsm.Value)
            {
                for (int c = 0; c < colors.Length; c++)
                {
                    for (int s = 0; s < strategies.Length; s++)
                    {
                        StockFishParameters parameters = new()
                        {
                            SkillLevel = skillMap.Item1,
                            Depth = dsm.Key,
                            StockFishDepth = skillMap.Item2,
                            Color = colors[c],
                            Strategy = strategies[s]
                        };

                        stockFishParameters.Add(parameters);
                    }
                }
            }
        }

        stockFishParameters.Shuffle();

        stockFishParameters.Sort();

        List<List<StockFishParameters>> parametersSet = new List<List<StockFishParameters>>();

        for (int i = 0; i < threads; i++)
        {
            parametersSet.Add(new List<StockFishParameters>());
        }

        for (int i = 0; i < stockFishParameters.Count; i++)
        {
            parametersSet[i % threads].Add(stockFishParameters[i]);
        }

        stockFishParameters.Clear();

        foreach (var set in parametersSet)
        {
            set.Shuffle();
            stockFishParameters.AddRange(set);
        }

        return stockFishParameters;
    }

    private static void Save()
    {
        using (var writter = new StreamWriter("StockFishResults.csv"))
        {
            IEnumerable<string> headers = new List<string> { "Kio", "StockFish", "Result" };

            writter.WriteLine(string.Join(",", headers));

            using (var db = new ResultContext())
            {
                var matchItems = db.GetMatchItems();

                foreach (var item in matchItems)
                {
                    List<string> values = new List<string>
                    {
                        $"{item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]",
                        $"SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Skill}]",
                        $"{Math.Round(item.Kio, 1)}x{Math.Round(item.SF, 1)}"
                    };

                    writter.WriteLine(string.Join(",", values));
                }
            }
        }
    }

    private static void Save(DateTime start, DateTime end)
    {
        using (var writter = new StreamWriter("StockFishResults.csv"))
        {
            IEnumerable<string> headers = new List<string> { "Kio", "StockFish", "Result" };

            writter.WriteLine(string.Join(",", headers));

            using (var db = new ResultContext())
            {
                var matchItems = db.GetMatchItems(start, end);

                foreach (var item in matchItems)
                {
                    List<string> values = new List<string>
                    {
                        $"{item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]",
                        $"SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Skill}]",
                        $"{Math.Round(item.Kio, 1)}x{Math.Round(item.SF, 1)}"
                    };

                    writter.WriteLine(string.Join(",", values));
                }
            }
        }
    }
}
