using Newtonsoft.Json;
using StockFishCore;
using StockFishCore.Data;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        DateTime start = DateTime.Now;
        StockFishClient.StartServer();

        var timer = Stopwatch.StartNew();

        StockFishParameters.Initialize();
        List<StockFishParameters> stockFishParameters = new List<StockFishParameters>();
        var colorSize = 10;
        string[] strategies = new string[] { "lmrd" };
        string[] colors = Enumerable.Repeat("w", colorSize).Concat(Enumerable.Repeat("b", colorSize)).ToArray();

        for (int skill = 10; skill < 11; skill++)
        {
            for (int d = 6; d < 8; d++)
            {
                for (int sd = d; sd > d - 2; sd--)
                {
                    for (int c = 0; c < colors.Length; c++)
                    {
                        for (int s = 0; s < strategies.Length; s++)
                        {
                            StockFishParameters parameters = new()
                            {
                                SkillLevel = skill,
                                Depth = d,
                                StockFishDepth = sd,
                                Color = colors[c],
                                Strategy = strategies[s]
                            };

                            stockFishParameters.Add(parameters);
                        }
                    }
                }
            }
        }

        int count = 0;
        object sync = new object();

        int size = stockFishParameters.Count;

        Parallel.For(0, size, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
        {
            StockFishParameters parameters = stockFishParameters[i];

            lock (sync)
            {
                var p = Math.Round(100.0 * (++count) / size, 4);
                parameters.Log(i, timer, p);
            }

            parameters.Execute();
        });

        Save(start, DateTime.Now);

        timer.Stop();

        Console.WriteLine();
        Console.WriteLine($"Time = {timer.Elapsed}, Total = {size}, Average = {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds / size)}");
        Console.WriteLine("Yalla");
        Console.WriteLine("^C");
        //Console.ReadLine();
    }

    private static void Save(DateTime start, DateTime end)
    {
        using (var writter = new StreamWriter("StockFishResults.csv"))
        {
            IEnumerable<string> headers = StockFishResult.GetHeaders();

            writter.WriteLine(string.Join(",", headers));

            using (var db = new ResultContext())
            {
                var matchItems = db.GetMatchItems(start, end);

                foreach (var item in matchItems)
                {
                    List<string> values = new List<string>
                    {
                        item.StockFishResultItem.Depth.ToString(),
                        item.StockFishResultItem.StockFishDepth.ToString(),
                        item.StockFishResultItem.Skill.ToString(),
                        $"{Math.Round(item.Kio, 1)}:{Math.Round(item.SF, 1)}"
                    };

                    writter.WriteLine(string.Join(",", values));
                }
            }
        }
    }
}
