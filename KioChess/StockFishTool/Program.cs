using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        StockFishParameters.Initialize();
        List<StockFishParameters> stockFishParameters = new List<StockFishParameters>();

        string[] strategies = new string[] { "lmrd" };
        string[] colors = new[] { "w", "w", "w", "w", "w", "b", "b", "b", "b", "b" };

        for (int skill = 10; skill < 13; skill++)
        {
            for (int d = 6; d < 10; d++)
            {
                for (int sd = 6; sd < 10; sd++)
                {
                    for(int c = 0; c < colors.Length; c++)
                    {
                        for (int s = 0; s < strategies.Length; s++)
                        {
                            StockFishParameters parameters = new StockFishParameters
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

        //foreach (var parameters in stockFishParameters)
        //{
        //    parameters.Execute();
        //    break;
        //}

        int count = 0;
        object sync = new object();

        Parallel.For(0, stockFishParameters.Count, new ParallelOptions { MaxDegreeOfParallelism = colors.Length }, i =>
        {
            StockFishParameters parameters = stockFishParameters[i];

            lock (sync)
            {
                var p = Math.Round(100.0 * (++count) / stockFishParameters.Count, 2);
                parameters.Log(i, timer, p); 
            }

            parameters.Execute();
        });

        timer.Stop();

        Console.WriteLine(timer.Elapsed);
        Console.ReadLine();
    }
}

internal class StockFishParameters
{
    private static string Exe;
    public int SkillLevel { get; internal set; }
    public int Depth { get; internal set; }
    public int StockFishDepth { get; internal set; }
    public string Color { get; internal set; }
    public string Strategy { get; internal set; }

    internal static void Initialize()
    {
        Exe = @"StockfishApp.exe";
    }

    internal void Execute()
    {
        Process process = Process.Start(Exe, $"{Depth} {StockFishDepth} {Strategy} {Color} {SkillLevel}");

        process.WaitForExit();
    }

    internal void Log(int i, Stopwatch timer, double v)
    {
        string message = $"I = {i}, T = {timer.Elapsed}, P = {v}%, D = {Depth}, SD = {StockFishDepth}, S = {Strategy}, C = {Color}, L={SkillLevel}";

        Console.WriteLine(message);
    }
}