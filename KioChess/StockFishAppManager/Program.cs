using StockFishCore;
using StockFishCore.Execution;
using System.Diagnostics;

internal class Program
{
    private static string _pathToConfig;
    private static string _text;
    private static int _executionSize;
    private static int _totalItems;
    private static List<BranchItem> _items;

    static Program()
    {
        _totalItems = 0;
        _pathToConfig = Path.Combine("Config", "Configuration.json");

        _text = File.ReadAllText(_pathToConfig);

        _executionSize = 145;

        _items = new List<BranchItem>();
    }

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

        //ProcessAttackMarginBulk();

        ProcessDataBulk();

        ProcessBranchItems();

        timer.Stop();

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * 45.0)}");
        Console.WriteLine($"Time = {timer.Elapsed}, Total = {_totalItems}, Average = {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds / _totalItems)}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
        Console.WriteLine("Yalla");
        Console.WriteLine("^C");

        Console.WriteLine("GAME OVER !");
    }

    private static void ProcessDataBulk()
    {
        int b = 1;

        string branchPattern = "155-Data-{0}";
        string[] descriptionP = { "\"GamesThreshold\": {0},", "\"SearchDepth\": {0},", "\"MinimumPopular\": {0},", "\"PopularDepth\": {0}," };
        string descriptionPattern = "GT-{0}-SD-{1}-MP-{2}-PD-{3}";

        for (int pd = 7; pd < 9; pd++)
        {
            if (_items.Count >= _executionSize) break;
            for (int gt = 20; gt < 23; gt++)
            {
                if (_items.Count >= _executionSize) break;
                for (int sd = 28; sd < 29; sd++)
                {
                    if (_items.Count >= _executionSize) break;
                    for (int mp = 750; mp < 850; mp += 25)
                    {
                        if (_items.Count >= _executionSize) break;

                        var branch = string.Format(branchPattern, b++);

                        var description = string.Format(descriptionPattern, gt, sd, mp, pd);

                        BranchItem item = BranchFactory.Create(branch, description);
                        if (item == null) continue;

                        var config = _text.Replace("\"GamesThreshold\": 20,", $"\"GamesThreshold\": {gt},")
                           //.Replace("\"SearchDepth\": 28,", $"\"SearchDepth\": {sd},")
                           .Replace("\"MinimumPopular\": 750,", $"\"MinimumPopular\": {mp},")
                           .Replace("\"PopularDepth\": 7,", $"\"PopularDepth\": {pd},");

                        item.Config = config;

                        _items.Add(item);

                        Console.WriteLine(item);

                        Console.WriteLine();
                        Console.WriteLine(" ----- ");
                        Console.WriteLine();
                    }
                }
            } 
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * 45.0)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * 45.0).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void ProcessBranchItems()
    {
        HashSet<string> names = new HashSet<string>();

        foreach (var item in _items.Take(_executionSize))
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(item);

            File.WriteAllText(_pathToConfig, item.Config);

            BranchExecutor branchExecutor = new BranchExecutor(item);

            _totalItems += branchExecutor.Execute();

            names.Add(item.Name);
        }

        Console.ForegroundColor = ConsoleColor.White;

        File.WriteAllText(_pathToConfig, _text);

        Process process = Process.Start("StockFishComparer.exe", $"-t {string.Join(' ', names)}");

        process.WaitForExit();

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void ProcessAttackMarginBulk()
    {
        int b = 1;

        string branchPattern = "154-AM-{0}";
        string descriptionPattern = "[ {0}, {1}, {2} ]";

        for (int open = 120; open < 160; open += 10)
        {
            if (_items.Count >= _executionSize) break;
            for (int middle = 150; middle < 210; middle += 10)
            {
                if (_items.Count >= _executionSize) break;
                for (int end = 150; end < 210; end += 10)
                {
                    if (_items.Count >= _executionSize) break;

                    var branch = string.Format(branchPattern, b++);

                    var description = string.Format(descriptionPattern, open, middle, end);

                    BranchItem item = BranchFactory.Create(branch, description);
                    if (item == null) continue;

                    var config = _text.Replace(": [ 120, 150, 170 ],", $": [ {open}, {middle}, {end} ],");

                    item.Config = config;

                    _items.Add(item);

                    Console.WriteLine(item);

                    Console.WriteLine();
                    Console.WriteLine(" ----- ");
                    Console.WriteLine();
                }
            }
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * 45.0)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * 45.0).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }
}