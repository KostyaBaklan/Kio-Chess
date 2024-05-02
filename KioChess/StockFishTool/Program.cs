using StockFishCore;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
#if DEBUG

        var process = Process.Start(@$"..\..\..\StockFishServer\bin\Debug\net7.0\StockFishServer.exe");
        process.WaitForExit(100);
#else
        var process = Process.Start(@$"..\..\..\StockFishServer\bin\Release\net7.0\StockFishServer.exe");
        process.WaitForExit(100);
# endif

        StockFishClient client = new StockFishClient();
        var service = client.GetService();
        var timer = Stopwatch.StartNew();

        StockFishParameters.Initialize();
        List<StockFishParameters> stockFishParameters = new List<StockFishParameters>();

        string[] strategies = new string[] { "lmrd" };
        string[] colors = new[] { "w", "w", "w", "w", "w", "w", "w", "w", "w", "w", "b", "b", "b", "b", "b", "b", "b", "b", "b", "b" };

        for (int skill = 15; skill < 16; skill++)
        {
            for (int d = 7; d < 8; d++)
            {
                //for (int sd = 7; sd < 8; sd++)
                {
                    for(int c = 0; c < colors.Length; c++)
                    {
                        for (int s = 0; s < strategies.Length; s++)
                        {
                            StockFishParameters parameters = new()
                            {
                                SkillLevel = skill,
                                Depth = d,
                                StockFishDepth = d,
                                Color = colors[c],
                                Strategy = strategies[s]
                            };

                            stockFishParameters.Add(parameters);
                        }
                    }
                }
            }
        }

        //foreach (var parameters in stockFishParameters.Take(10))
        //{
        //    parameters.Execute();
        //}

        int count = 0;
        object sync = new object();

        int size = stockFishParameters.Count;

        Parallel.For(0, size, new ParallelOptions { MaxDegreeOfParallelism = 2*Environment.ProcessorCount }, i =>
        {
            StockFishParameters parameters = stockFishParameters[i];

            lock (sync)
            {
                var p = Math.Round(100.0 * (++count) / size, 2);
                parameters.Log(i, timer, p);
            }

            parameters.Execute();
        });

        service.Save();

        timer.Stop();

        Console.WriteLine(timer.Elapsed);
        Console.WriteLine("Yalla");
        Console.WriteLine("^C");
        //Console.ReadLine();
    }
}
