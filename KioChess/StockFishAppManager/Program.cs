using StockFishCore;
using StockFishCore.Execution;
using System.Diagnostics;

internal class Program
{
    private static string _pathToConfig;
    private static string _text;
    private static int _executionSize;
    private static double _executionTime;
    private static int _totalItems;
    private static List<BranchItem> _items;

    static Program()
    {
        _totalItems = 0;
        _pathToConfig = Path.Combine("Config", "Configuration.json");

        _text = File.ReadAllText(_pathToConfig);

        _executionSize = 27;
        _executionTime = 42.0;

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

        //ProcessCheckExtesions();

        ProcessAttackMarginBulk();

        //ProcessDataBulk();

        //ProcessLmr();

        //ProcessSortDepth();

        //ProcessKingZone();

        ProcessBranchItems();

        timer.Stop();

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}");
        Console.WriteLine($"Time = {timer.Elapsed}, Total = {_totalItems}, Average = {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds / _totalItems)}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
        Console.WriteLine("Yalla");
        Console.WriteLine("^C");

        Console.WriteLine("GAME OVER !");
    }

    private static void ProcessKingZone()
    {
        int b = 1;

        string branchPattern = "29-KZA-{0}";
        string[] pavMap = { "[ 0, 1, 1, 2, 4, 0 ]", "[ 0, 1, 1, 3, 5, 0 ]" };
        string descriptionPattern = "PAV-{0}-AW-[ 0, 0, {1}, {2}, {3}, {4}, 20, 20, 20, 20, 20, 25, 25, 25, 25, 25, 25, 25, 25, 25 ]";

        for (int pav = 0; pav < 2; pav++)
        {
            if (_items.Count >= _executionSize) break;
            for (int a5 = 4; a5 < 6; a5++)
            {
                if (_items.Count >= _executionSize) break;
                for (int a10 = 8; a10 < 10; a10++)
                {
                    if (_items.Count >= _executionSize) break;
                    for (int a15 = 12; a15 < 15; a15 ++)
                    {
                        if(a15-a10 > 5) continue;
                        if (_items.Count >= _executionSize) break;
                        for (int a20 = 16; a20 < 20; a20 ++)
                        {
                            if (a20 - a15 > 5) continue;
                            if (_items.Count >= _executionSize) break;

                            var branch = string.Format(branchPattern, b++);

                            var description = string.Format(descriptionPattern, pav, a5, a10, a15, a20);

                            BranchItem item = BranchFactory.Create(branch, description);
                            if (item == null) continue;

                            var config = _text.Replace("\"PieceAttackValue\": [ 0, 1, 1, 2, 4, 0 ],", $"\"PieceAttackValue\": {pavMap[pav]},")
                               .Replace("\"AttackWeight\": [ 0, 0, 5, 10, 15, 20, 20, 20, 20, 20, 20, 25, 25, 25, 25, 25, 25, 25, 25, 25 ]", $"\"AttackWeight\": [ 0, 0, {a5}, {a10}, {a15}, {a20}, 20, 20, 20, 20, 20, 25, 25, 25, 25, 25, 25, 25, 25, 25 ]");

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
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void ProcessSortDepth()
    {
        int b = 1;

        string branchPattern = "10-SD-{0}";
        string descriptionPattern = "[ 1, 1, 1, 1, 1, {0}, {1}, {2}, {3}, {4}, {5}, {6}, 3, 4, 4, 4, 4, 4, 4, 4 ]";

        for (int five = 1; five < 3; five++)
        {
            if (_items.Count >= _executionSize) break;
            for (int six = five; six < 3; six++)
            {
                if (_items.Count >= _executionSize) break;
                for (int seven = six; seven < 3; seven++)
                {
                    if (_items.Count >= _executionSize) break;
                    for (int eight = seven; eight < 3; eight++)
                    {
                        if (_items.Count >= _executionSize) break;
                        for (int nine = eight; nine < 4; nine++)
                        {
                            if (_items.Count >= _executionSize) break;
                            for (int ten = nine; ten < 4; ten++)
                            {
                                for (int eleven = ten; eleven < 4; eleven++)
                                {
                                    if (_items.Count >= _executionSize) break;

                                    var branch = string.Format(branchPattern, b++);

                                    var description = string.Format(descriptionPattern, five, six, seven, eight, nine, ten, eleven);

                                    BranchItem item = BranchFactory.Create(branch, description);
                                    if (item == null) continue;

                                    var config = _text.Replace("\"SortDepth\": [ 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 4, 4, 4, 4, 4, 4, 4 ],",
                                        $"\"SortDepth\": [ 1, 1, 1, 1, 1, {five}, {six}, {seven}, {eight}, {nine}, {ten}, {eleven}, 3, 4, 4, 4, 4, 4, 4, 4 ],");

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
                }
            }
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * 45.0).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void ProcessLmr()
    {
        int b = 1;

        string branchPattern = "9-Lmr-O-{0}";
        string descriptionPattern = "Lmr=[{0},{1},{2}] - End=[{3},{4},{5}]";

        for (int rd = 2; rd < 3; rd++)
        {
            if (_items.Count >= _executionSize) break;
            for (int erd = 2; erd < 3; erd++)
            {
                if (_items.Count >= _executionSize) break;
                for (int nonLmr = 4; nonLmr < 5; nonLmr++)
                {
                    if (_items.Count >= _executionSize) break;
                    for (int nonLmrEnd = 5; nonLmrEnd < 6; nonLmrEnd++)
                    {
                        if (_items.Count >= _executionSize) break;
                        for (int lmr = 7; lmr < 12; lmr++)
                        {
                            if (_items.Count >= _executionSize) break;
                            for (int lmrEnd = 7; lmrEnd < 12; lmrEnd++)
                            {
                                if (lmr == lmrEnd) continue;
                                if (_items.Count >= _executionSize) break;

                                var branch = string.Format(branchPattern, b++);

                                var description = string.Format(descriptionPattern, rd, nonLmr, lmr, erd, nonLmrEnd, lmrEnd);

                                BranchItem item = BranchFactory.Create(branch, description);
                                if (item == null) continue;

                                var config = _text.Replace("\"Lmrd\": [ 2, 4, 9 ],", $"\"Lmrd\": [ {rd}, {nonLmr}, {lmr} ],")
                                   .Replace("\"LmrEnd\": [ 2, 5, 9 ]", $"\"LmrEnd\": [ {erd}, {nonLmrEnd}, {lmrEnd} ]");

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
            }
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
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

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void ProcessDataBulk()
    {
        int b = 1;

        string branchPattern = "30-Data-{0}";
        string descriptionPattern = "GT-{0}-SD-{1}-MP-{2}-PD-{3}-MPT-{4}";

        for (int pd = 8; pd < 10; pd++)
        {
            if (_items.Count >= _executionSize) break;
            for (int gt = 24; gt < 26; gt++)
            {
                if (_items.Count >= _executionSize) break;
                for (int sd = 29; sd < 30; sd++)
                {
                    if (_items.Count >= _executionSize) break;
                    for (int mpt = 8; mpt < 10; mpt++)
                    {
                        if (_items.Count >= _executionSize) break;
                        for (int mp = 825; mp < 925; mp += 25)
                        {
                            if (_items.Count >= _executionSize) break;

                            var branch = string.Format(branchPattern, b++);

                            var description = string.Format(descriptionPattern, gt, sd, mp, pd, mpt);

                            BranchItem item = BranchFactory.Create(branch, description);
                            if (item == null) continue;

                            var config = _text.Replace("\"GamesThreshold\": 24,", $"\"GamesThreshold\": {gt},")
                               .Replace("\"SearchDepth\": 29,", $"\"SearchDepth\": {sd},")
                               .Replace("\"MinimumPopular\": 825,", $"\"MinimumPopular\": {mp},")
                               .Replace("\"MaximumPopularThreshold\": 9,", $"\"MaximumPopularThreshold\": {mpt},")
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
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * 45.0).ToString("dd/MM/yyyy HH:mm")}");

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

        string branchPattern = "31-AM-{0}";
        string descriptionPattern = "[ {0}, {1}, {2} ]";

        for (int open = 120; open < 140; open += 10)
        {
            if (_items.Count >= _executionSize) break;
            for (int middle = 170; middle < 210; middle += 10)
            {
                if (_items.Count >= _executionSize) break;
                for (int end = middle; end < 230; end += 10)
                {
                    if (_items.Count >= _executionSize) break;

                    var branch = string.Format(branchPattern, b++);

                    var description = string.Format(descriptionPattern, open, middle, end);

                    BranchItem item = BranchFactory.Create(branch, description);
                    if (item == null) continue;

                    var config = _text.Replace(": [ 130, 170, 190 ],", $": [ {open}, {middle}, {end} ],");

                    item.Config = config;

                    _items.Add(item);

                    Console.WriteLine(item);

                    Console.WriteLine();
                    Console.WriteLine(" ----- ");
                    Console.WriteLine();
                }
            }
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }
}