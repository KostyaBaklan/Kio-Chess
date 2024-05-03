using StockFishCore;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        DateTime start= DateTime.Now;
        StockFishClient.StartServer();

        var timer = Stopwatch.StartNew();

        StockFishParameters.Initialize();
        List<StockFishParameters> stockFishParameters = new List<StockFishParameters>();
        var colorSize = 50;
        string[] strategies = new string[] { "lmrd" };
        string[] colors = Enumerable.Repeat("w", colorSize).Concat(Enumerable.Repeat("b", colorSize)).ToArray();


        for (int skill = 10; skill < 16; skill++)
        {
            for (int d = 7; d < 11; d++)
            {
                for (int sd = d - 1; sd < d + 2; sd++)
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

        //foreach (var parameters in stockFishParameters.Take(10))
        //{
        //    parameters.Execute();
        //}

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

        DateTime end = DateTime.Now;

        Save(start,end);

        timer.Stop();

        Console.WriteLine(timer.Elapsed);
        Console.WriteLine("Yalla");
        Console.WriteLine("^C");
        //Console.ReadLine();
    }

    private static void Save(DateTime start, DateTime end)
    {
        using (var db = new ResultContext())
        {
            var entities = db.ResultEntities.Where(r=>r.Time >= start && r.Time <= end).ToList();

            using (var writter = new StreamWriter("StockFishResults.csv"))
            {
                IEnumerable<string> headers = StockFishResult.GetHeaders();

                writter.WriteLine(string.Join(",", headers));

                var groups = entities.Select(e => new StockFishResult
                {
                    StockFishResultItem = new StockFishResultItem
                    {
                        Depth = e.Depth,
                        StockFishDepth = e.StockFishDepth,
                        Skill = e.Skill,
                        Strategy = e.Strategy
                    },
                    Color = e.Color,
                    Result = e.Result

                }).GroupBy(r => r.StockFishResultItem);

                foreach (var group in groups)
                {
                    var games = group.ToList();

                    List<string> values = new List<string>()
                        {
                            group.Key.Depth.ToString(),group.Key.StockFishDepth.ToString(),group.Key.Skill.ToString()
                        };

                    double kio = 0.0;
                    double st = 0.0;
                    foreach (var game in games)
                    {
                        kio += game.GetKioValue();
                        st += game.GetStockFishValue();
                    }

                    values.Add($"{Math.Round(kio, 1)} - {Math.Round(st, 1)}");

                    writter.WriteLine(string.Join(",", values));
                }
            } 
        }
    }
}
