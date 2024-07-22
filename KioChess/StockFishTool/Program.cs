using Engine.Models.Helpers;
using StockFishCore;
using StockFishCore.Services;
using StockFishTool;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        Boot.SetUp();
        StockFishClient.StartServer();

        if (!Directory.Exists("Log"))
        {
            Directory.CreateDirectory("Log");
        }

        Thread.Sleep(1000);

        var timer = Stopwatch.StartNew();

        int threads = 3 * Environment.ProcessorCount / 4;

        List<StockFishParameters> stockFishParameters = CreateStockFishParameters(threads);

        ParallelExecutor parallelExecutor = new ParallelExecutor(threads, stockFishParameters);

        parallelExecutor.Execute();

        Save();

        timer.Stop();

        Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine();
        Console.WriteLine($"Time = {timer.Elapsed}, Total = {stockFishParameters.Count}, Average = {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds / stockFishParameters.Count)}");
        

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();

        Console.WriteLine(" ----- Please write left branch ID:");
        var left = Console.ReadLine();
        Console.WriteLine(" ----- Please write right branch ID:");
        var right = Console.ReadLine();

        Process process = Process.Start("StockFishComparer.exe", $"{left} {right}");

        process.WaitForExit();

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
        Console.WriteLine("Yalla");
        Console.WriteLine("^C");
        //Console.ReadLine();
    }

    private static List<StockFishParameters> CreateStockFishParameters(int threads)
    {
        StockFishParameters.Initialize();
        List<StockFishParameters> stockFishParameters = new List<StockFishParameters>();
        string[] strategies = new string[] { "lmrd","id","asp" };
        string[] colors = { "w", "b" };
        string[] moves = new string[] { "7686-11778", "7688-11434", "7686-11436", "7688-11438", "7688-11435", "7688-11439", "7750-11778", "7750-11436", "7688-11436", "7688-11443", "7686-11434", "7688-11437", "7686-11443", "7686-11439", "7688-11778", "7750-11443", "7686-11437", "7750-11434", "7684-11778", "7686-11435", "7684-11438" };

        var depthSkillMap = new Dictionary<int, List<int>>
        {
            //{5, new List<Tuple<int, int>> { Tuple.Create(1800, 4)}},
            {5, new List<int> { 1800}},
            {6, new List<int> { 1900}},
            {7, new List<int>{ 2000}},
            {8, new List<int> { 2100 }},
            {9, new List<int>{ 2200}},
            {10, new List<int>{2300}},
            //{11, new List<Tuple<int, int>> { Tuple.Create(2400, 9),Tuple.Create(2400, 10), Tuple.Create(2400, 11),Tuple.Create(2400, 12)}},
            //{12, new List<Tuple<int, int>> { Tuple.Create(15, 11),Tuple.Create(15, 11), Tuple.Create(15, 12),Tuple.Create(15, 13)}}
        };

        foreach (KeyValuePair<int, List<int>> dsm in depthSkillMap)
        {
            foreach (int skillMap in dsm.Value)
            {
                for (int c = 0; c < colors.Length; c++)
                {
                    for (int s = 0; s < strategies.Length; s++)
                    {
                        for (int m = 0; m < moves.Length; m++)
                        {
                            StockFishParameters parameters = new()
                            {
                                Elo = skillMap,
                                Depth = dsm.Key,
                                StockFishDepth = dsm.Key,
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
        StockFishDbService stockFishDbService = new StockFishDbService();

        try
        {
            stockFishDbService.Connect();

            stockFishDbService.GenerateLatestReport();
        }
        finally
        {
            stockFishDbService.Disconnect();
        }
    }
}
