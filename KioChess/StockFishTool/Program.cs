using Engine.Models.Helpers;
using StockFishCore;
using StockFishCore.Data;
using StockFishTool;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        Boot.SetUp();
        DateTime start = DateTime.Now;
        StockFishClient.StartServer();

        var timer = Stopwatch.StartNew();

        int threads = 4*Environment.ProcessorCount/5;

        List<StockFishParameters> stockFishParameters = CreateStockFishParameters(threads);

        ParallelExecutor parallelExecutor = new ParallelExecutor(threads, stockFishParameters);

        parallelExecutor.Execute();

        Save(start, DateTime.Now);

        timer.Stop();

        Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine();
        Console.WriteLine($"Time = {timer.Elapsed}, Total = {stockFishParameters.Count}, Average = {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds / stockFishParameters.Count)}");
        Console.WriteLine("Yalla");
        Console.WriteLine("^C");
        //Console.ReadLine();
    }

    private static List<StockFishParameters> CreateStockFishParameters(int threads)
    {
        StockFishParameters.Initialize();
        List<StockFishParameters> stockFishParameters = new List<StockFishParameters>();
        string[] strategies = new string[] { "lmrd" };
        string[] colors = { "w", "b" };
        string[] moves = new string[] { "7686-11778", "7686-11436", "7686-11434", "7686-11443", "7686-11439", "7686-11437", "7688-11434", "7688-11438", "7688-11435", "7688-11439", "7688-11436", "7688-11443", "7684-11778", "7684-11438", "7684-11439", "7684-11434", "7684-11435", "7684-11443", "7693-11436", "7693-11778", "7693-11443", "7693-11444", "7693-11435", "7693-11434", "7750-11778", "7750-11436", "7750-11443", "7750-11434", "7750-11439", "7750-11437", "7683-11778", "7683-11436", "7683-11438", "7683-11434", "7683-11443", "7683-11439" };

        var depthSkillMap = new Dictionary<int, List<Tuple<int, int>>>
        {
            //{6, new List<Tuple<int, int>> {  Tuple.Create(2000, 6),Tuple.Create(2000, 7)}},
            {7, new List<Tuple<int, int>> { Tuple.Create(2000, 6), Tuple.Create(2000, 7),Tuple.Create(2000, 8)}},
            {8, new List<Tuple<int, int>> { Tuple.Create(2100, 7), Tuple.Create(2100, 8),Tuple.Create(2100, 9) }},
            {9, new List<Tuple<int, int>> { Tuple.Create(2200, 8), Tuple.Create(2200, 9),Tuple.Create(2200, 10) }},
            {10, new List<Tuple<int, int>> { Tuple.Create(2300, 9), Tuple.Create(2300, 10),Tuple.Create(2300, 11)}},
            //{11, new List<Tuple<int, int>> { Tuple.Create(2400, 10), Tuple.Create(2400, 11),Tuple.Create(2400, 12)}},
            //{12, new List<Tuple<int, int>> { Tuple.Create(15, 11), Tuple.Create(15, 12),Tuple.Create(15, 13)}}
        };

        foreach (KeyValuePair<int, List<Tuple<int, int>>> dsm in depthSkillMap)
        {
            foreach (Tuple<int, int> skillMap in dsm.Value)
            {
                for (int c = 0; c < colors.Length; c++)
                {
                    for (int s = 0; s < strategies.Length; s++)
                    {
                        for (int m = 0; m < moves.Length; m++)
                        {
                            StockFishParameters parameters = new()
                            {
                                Elo = skillMap.Item1,
                                Depth = dsm.Key,
                                StockFishDepth = skillMap.Item2,
                                Color = colors[c],
                                Strategy = strategies[s],
                                Move = moves[m]
                            };

                            stockFishParameters.Add(parameters);
                        }
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
                        $"SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]",
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
                        $"SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]",
                        $"{Math.Round(item.Kio, 1)}x{Math.Round(item.SF, 1)}"
                    };

                    writter.WriteLine(string.Join(",", values));
                }
            }
        }
    }
}
