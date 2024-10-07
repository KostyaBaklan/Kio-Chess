using StockFishCore;
using StockFishCore.Execution;
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

        Thread.Sleep(2000);

        var timer = Stopwatch.StartNew();

        string branchPattern = "152-T1-AM-{0}";
        string descriptionPattern = "[ {0}, {1}, {2} ]";

        var pathToConfig = Path.Combine("Config", "Configuration.json");

        var text = File.ReadAllText(pathToConfig);

        int b = 1;

        int totalItems = 0;

        int executionSize = 18;

        List<BranchItem> items = new List<BranchItem>();

        for (int open = 120; open < 160; open += 10)
        {
            if (items.Count >= executionSize) break;
            for (int middle = 150; middle < 210; middle += 10)
            {
                if (items.Count >= executionSize) break;
                for (int end = 150; end < 210; end += 10)
                {
                    if (items.Count >= executionSize) break;

                    var branch = string.Format(branchPattern, b++);

                    var description = string.Format(descriptionPattern, open, middle, end);

                    BranchItem item = BranchFactory.Create(branch, description);
                    if (item == null) continue;

                    var config = text.Replace(": [ 120, 190, 170 ],", $": [ {open}, {middle}, {end} ],");

                    item.Config = config;

                    items.Add(item);

                    Console.WriteLine(item);

                    Console.WriteLine();
                    Console.WriteLine(" ----- ");
                    Console.WriteLine();
                }
            }
        }

        Console.WriteLine($"Total Branches: {items.Count}, Expected Run Time: {TimeSpan.FromMinutes(items.Count * 45.0)}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();

        HashSet<string> names = new HashSet<string>();

        foreach (var item in items.Take(executionSize))
        {
            Console.WriteLine(item);

            File.WriteAllText(pathToConfig, item.Config);

            BranchExecutor branchExecutor = new BranchExecutor(item);

            totalItems += branchExecutor.Execute();

            names.Add(item.Name);
        }

        timer.Stop();

        Console.ForegroundColor = ConsoleColor.White;

        File.WriteAllText(pathToConfig, text);

        Process process = Process.Start("StockFishComparer.exe", $"-t {string.Join(' ', names)}");

        process.WaitForExit();

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();

        Console.WriteLine($"Total Branches: {items.Count}, Expected Run Time: {TimeSpan.FromMinutes(items.Count * 45.0)}");
        Console.WriteLine($"Time = {timer.Elapsed}, Total = {totalItems}, Average = {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds / totalItems)}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
        Console.WriteLine("Yalla");
        Console.WriteLine("^C");

        Console.WriteLine("GAME OVER !");
    }
}