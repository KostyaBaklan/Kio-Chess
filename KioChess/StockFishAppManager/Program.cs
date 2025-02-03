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

        _executionSize = 24;

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

        ProcessCheckExtesions();

        //ProcessAttackMarginBulk();

        //ProcessDataBulk();

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

    private static void ProcessCheckExtesions()
    {
        int b = 1;

        string branchPattern = "3-Ext-{0}";
        string descriptionPattern = "E={0}-D={1}-End={2}";

        for (int ed = 3; ed < 5; ed++)
        {
            if (_items.Count >= _executionSize) break;
            for (int dd = 3; dd < 8; dd++)
            {
                if (_items.Count >= _executionSize) break;
                for (int edd = 3; edd < 8; edd++)
                {
                    if (_items.Count >= _executionSize) break;
                    var branch = string.Format(branchPattern, b++);

                    var description = string.Format(descriptionPattern, ed, dd, edd);

                    BranchItem item = BranchFactory.Create(branch, description);
                    if (item == null) continue;

                    var config = _text.Replace("\"ExtensionDepth\": 3,", $"\"ExtensionDepth\": {ed},")
                       .Replace("\"DepthDifference\": 6,", $"\"DepthDifference\": {dd},")
                       .Replace("\"EndDepthDifference\": 4,", $"\"EndDepthDifference\": {edd},");

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

    private static void ProcessDataBulk()
    {
        int b = 1;

        string branchPattern = "165-Data-{0}";
        string[] descriptionP = { "\"GamesThreshold\": {0},", "\"SearchDepth\": {0},", "\"MinimumPopular\": {0},", "\"PopularDepth\": {0}," };
        string descriptionPattern = "GT-{0}-SD-{1}-MP-{2}-PD-{3}";

        for (int pd = 8; pd < 10; pd++)
        {
            if (_items.Count >= _executionSize) break;
            for (int gt = 22; gt < 24; gt++)
            {
                if (_items.Count >= _executionSize) break;
                for (int sd = 28; sd < 30; sd++)
                {
                    if (_items.Count >= _executionSize) break;
                    for (int mp = 800; mp < 900; mp += 25)
                    {
                        if (_items.Count >= _executionSize) break;

                        var branch = string.Format(branchPattern, b++);

                        var description = string.Format(descriptionPattern, gt, sd, mp, pd);

                        BranchItem item = BranchFactory.Create(branch, description);
                        if (item == null) continue;

                        var config = _text.Replace("\"GamesThreshold\": 22,", $"\"GamesThreshold\": {gt},")
                           .Replace("\"SearchDepth\": 29,", $"\"SearchDepth\": {sd},")
                           .Replace("\"MinimumPopular\": 825,", $"\"MinimumPopular\": {mp},")
                           .Replace("\"PopularDepth\": 8,", $"\"PopularDepth\": {pd},");

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
        foreach (var item in _items.Take(_executionSize))
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(item);

            File.WriteAllText(_pathToConfig, item.Config);

            BranchExecutor branchExecutor = new BranchExecutor(item);

            _totalItems += branchExecutor.Execute();
        }

        Console.ForegroundColor = ConsoleColor.White;

        File.WriteAllText(_pathToConfig, _text);

        Process process = Process.Start("StockFishComparer.exe", $"-c {_items.Min(i=>i.Id)}");

        process.WaitForExit();

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void ProcessAttackMarginBulk()
    {
        int b = 1;

        string branchPattern = "164-AM-1-{0}";
        string descriptionPattern = "[ {0}, {1}, {2} ]";

        for (int open = 140; open < 150; open += 10)
        {
            if (_items.Count >= _executionSize) break;
            for (int middle = 150; middle < 210; middle += 10)
            {
                if (_items.Count >= _executionSize) break;
                for (int end = middle; end < 210; end += 10)
                {
                    if (_items.Count >= _executionSize) break;

                    var branch = string.Format(branchPattern, b++);

                    var description = string.Format(descriptionPattern, open, middle, end);

                    BranchItem item = BranchFactory.Create(branch, description);
                    if (item == null) continue;

                    var config = _text.Replace(": [ 120, 170, 190 ],", $": [ {open}, {middle}, {end} ],");

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