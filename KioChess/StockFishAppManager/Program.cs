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

        _executionSize = 30;
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

        //TTPriority();

        //DeltaPruning();

        //Mobility();

        //OneReplyExtension();

        //AlphaFutility();

        //CutoffDepth();

        //GameSort();

        //LmrReduction();

        //HistoryHeuristicFactor();

        //QueenValues();

        //ProcessBishopPair();

        //ProcessCheckExtesions();

        ProcessAttackMarginBulk();

        //ProcessPassedPawns();

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

    private static void TTPriority()
    {
        int b = 1;

        string branchPattern = "86-TT-P-{0}";
        string descriptionPattern = "Depth = [{0}]";

        for (int d = 4; d <= 12; d++)
        {
            var branch = string.Format(branchPattern, b++);

            var description = string.Format(descriptionPattern, d);

            BranchItem item = BranchFactory.Create(branch, description);
            if (item == null) continue;

            var config = _text.Replace("\"TranspositionTableDepthFactor\": 8", $"\"TranspositionTableDepthFactor\": {d}");

            item.Config = config;

            _items.Add(item);

            Console.WriteLine(item);

            Console.WriteLine();
            Console.WriteLine(" ----- ");
            Console.WriteLine();
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void DeltaPruning()
    {
        int b = 1;

        string branchPattern = "68-DP-02-{0}";
        string descriptionPattern = "Delta = [{0}]";

        for (int delta = 300; delta <= 400; delta+=25)
        {
            var branch = string.Format(branchPattern, b++);

            var description = string.Format(descriptionPattern, delta);

            BranchItem item = BranchFactory.Create(branch, description);
            if (item == null) continue;

            var config = _text.Replace("\"DeltaPruningMargin\": 256", $"\"DeltaPruningMargin\": {delta}");

            item.Config = config;

            _items.Add(item);

            Console.WriteLine(item);

            Console.WriteLine();
            Console.WriteLine(" ----- ");
            Console.WriteLine();
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void Mobility()
    {
        int b = 1;

        string branchPattern = "62-Mb-04-{0}";
        string descriptionPattern = "Mob = [{0}, {1}, {2}]";

        for (int open = 3; open < 6; open++)
        {
            for (int middle = 3; middle < 6; middle++)
            {
                if (_items.Count >= _executionSize) break;
                for (int end = 3; end < 4; end++)
                {
                    if (_items.Count >= _executionSize) break;

                    var branch = string.Format(branchPattern, b++);

                    var description = string.Format(descriptionPattern, open, middle, end);

                    BranchItem item = BranchFactory.Create(branch, description);
                    if (item == null) continue;

                    var config = _text.Replace("\"MobilityThreshold\": [ 5, 4, 3 ]", $"\"MobilityThreshold\": [ {open}, {middle}, {end} ]");

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

    private static void OneReplyExtension()
    {
        int b = 1;

        string branchPattern = "55-ORE-{0}";
        string descriptionPattern = "ORE = [{0}]";

        for (int ore = 4; ore < 13; ore ++)
        {
            var branch = string.Format(branchPattern, b++);

            var description = string.Format(descriptionPattern, ore);

            BranchItem item = BranchFactory.Create(branch, description);
            if (item == null) continue;

            var config = _text.Replace("\"OneReplyDepthDifference\": 10", $"\"OneReplyDepthDifference\": {ore}");

            item.Config = config;

            _items.Add(item);

            Console.WriteLine(item);

            Console.WriteLine();
            Console.WriteLine(" ----- ");
            Console.WriteLine();
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void AlphaFutility()
    {
        int b = 1;

        string branchPattern = "99-BFM-{0}";
        string descriptionPattern = "BF = [ {0}, {1}, 0 ]";

        for (int d1 = 0; d1 < 75; d1 += 25)
        {
            if (_items.Count >= _executionSize) break;
            for (int d2 = 25; d2 < 101; d2 += 25)
            {
                if (_items.Count >= _executionSize) break;

                var branch = string.Format(branchPattern, b++);

                var description = string.Format(descriptionPattern, d1, d2);

                BranchItem item = BranchFactory.Create(branch, description);
                if (item == null) continue;

                var config = _text.Replace("[ 11, 22, 33 ]", $"[ {d1}, {d2}, 0 ]");

                item.Config = config;

                _items.Add(item);

                Console.WriteLine(item);

                Console.WriteLine();
                Console.WriteLine(" ----- ");
                Console.WriteLine();
            }
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void CutoffDepth()
    {
        int b = 1;

        string branchPattern = "57-CD-{0}";
        string descriptionPattern = "CD = [ 0, 1, 2, 2, 2, 3, 3, {0}, {1}, {2}, {3}, {4}, 6, 6, 6, 6, 7, 7, 8, 8 ]";

        for (int cd7 = 3; cd7 < 6; cd7++)
        {
            for (int cd8 = cd7 + 1; cd8 < 7; cd8++)
            {
                if (_items.Count >= _executionSize) break;
                for (int cd9 = cd8 + 1; cd9 < 8; cd9++)
                {
                    if (_items.Count >= _executionSize) break;
                    for (int cd10 = cd9 + 1; cd10 < 9; cd10++)
                    {
                        if (_items.Count >= _executionSize) break;
                        for (int cd11 = cd10 + 1; cd11 < 10; cd11++)
                        {
                            if (_items.Count >= _executionSize) break;

                            var branch = string.Format(branchPattern, b++);

                            var description = string.Format(descriptionPattern, cd7, cd8, cd9, cd10, cd11);

                            BranchItem item = BranchFactory.Create(branch, description);
                            if (item == null) continue;

                            var config = _text.Replace("\"CutoffDepth\": [ 0, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 8, 8 ],",
                                $"\"CutoffDepth\": [ 0, 1, 2, 2, 2, 3, 3, {cd7}, {cd8}, {cd9}, {cd10}, {cd11}, 6, 6, 6, 6, 7, 7, 8, 8 ],");

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

    private static void GameSort()
    {
        int b = 1;

        string branchPattern = "41-S-01-{0}";
        string descriptionPattern = "GS = [{0}]";

        for (int st = 9; st < 26; st += 2)
        {
            var branch = string.Format(branchPattern, b++);

            var description = string.Format(descriptionPattern, st);

            BranchItem item = BranchFactory.Create(branch, description);
            if (item == null) continue;

            var config = _text.Replace("\"SortThreshold\": 11", $"\"SortThreshold\": {st}");

            item.Config = config;

            _items.Add(item);

            Console.WriteLine(item);

            Console.WriteLine();
            Console.WriteLine(" ----- ");
            Console.WriteLine();
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void LmrReduction()
    {
        int b = 1;

        string branchPattern = "96-Lmr-{0}";
        string descriptionPattern = "Lmr=[{0},{1}] - End=[{2},{3}]";

        for (int r = 15; r < 19; r++)
        {
            if (_items.Count >= _executionSize) break;
            for (int dr = 4; dr < 8; dr++)
            {
                if (_items.Count >= _executionSize) break;
                var branch = string.Format(branchPattern, b++);

                var description = string.Format(descriptionPattern, r, dr, r-1, dr-1);

                BranchItem item = BranchFactory.Create(branch, description);
                if (item == null) continue;

                var config = _text.Replace("\"LmrRatio\": [ 15, 4],", $"\"LmrRatio\": [ {r}, {dr} ],")
                   .Replace("\"LmrEndRatio\": [ 14, 3 ]", $"\"LmrEndRatio\": [ {r-1}, {dr-1} ]");

                item.Config = config;

                _items.Add(item);

                Console.WriteLine(item);

                Console.WriteLine();
                Console.WriteLine(" ----- ");
                Console.WriteLine();
            }
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void HistoryHeuristicFactor()
    {
        int b = 1;

        string branchPattern = "34-05-HHF-{0}";
        string descriptionPattern = "F = [{0}]";

        for (float f = 0.1f; f > 0.000009; f /=10)
        {
            var branch = string.Format(branchPattern, b++);

            f = (float)Math.Round(f, 6);

            var description = string.Format(descriptionPattern, f);

            BranchItem item = BranchFactory.Create(branch, description);
            if (item == null) continue;

            var config = _text.Replace("\"RelativeHistoryFactor\": 0.1", $"\"RelativeHistoryFactor\": {f}");

            item.Config = config;

            _items.Add(item);

            Console.WriteLine(item);

            Console.WriteLine();
            Console.WriteLine(" ----- ");
            Console.WriteLine();
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void QueenValues()
    {
        int b = 1;

        string branchPattern = "34-001-QV-{0}";
        string descriptionPattern = "Q = [{0}]";

        for (int q = 900; q < 1025; q+=10)
        {
            var branch = string.Format(branchPattern, b++);

            var description = string.Format(descriptionPattern, q);

            BranchItem item = BranchFactory.Create(branch, description);
            if (item == null) continue;

            var config = _text.Replace("\"Queen\": 990,", $"\"Queen\": {q},");

            item.Config = config;

            _items.Add(item);

            Console.WriteLine(item);

            Console.WriteLine();
            Console.WriteLine(" ----- ");
            Console.WriteLine();
        }

        Console.WriteLine($"Total Branches: {_items.Count}, Expected Run Time: {TimeSpan.FromMinutes(_items.Count * _executionTime)}, Expected finish: {DateTime.Now.AddMinutes(_items.Count * _executionTime).ToString("dd/MM/yyyy HH:mm")}");

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void ProcessBishopPair()
    {
        int b = 1;

        string branchPattern = "34-0-BP-{0}";
        string descriptionPattern = "[{0}, {1}, {2}]";

        for (int o = 30; o < 45; o+=5)
        {
            if (_items.Count >= _executionSize) break;
            for (int m = 40; m < 55; m+=5)
            {
                if (_items.Count >= _executionSize) break;
                for (int e = 50; e < 65; e+=5)
                {
                    if (_items.Count >= _executionSize) break;
                    var branch = string.Format(branchPattern, b++);

                    var description = string.Format(descriptionPattern, o, m, e);

                    BranchItem item = BranchFactory.Create(branch, description);
                    if (item == null) continue;

                    var config = _text.Replace("\"DoubleBishopValue\": 1,", $"\"DoubleBishopValue\": {o},")
                       .Replace("\"DoubleBishopValue\": 2,", $"\"DoubleBishopValue\": {m},")
                       .Replace("\"DoubleBishopValue\": 3,", $"\"DoubleBishopValue\": {e},");

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

        string branchPattern = "55-Ext-{0}";
        string descriptionPattern = "E={0}-D={1}-End={2}-O={3}";

        for (int ed = 3; ed < 5; ed++)
        {
            if (_items.Count >= _executionSize) break;
            for (int dd = 4; dd < 7; dd++)
            {
                if (_items.Count >= _executionSize) break;
                for (int one = 10; one < 13; one++)
                {
                    if (_items.Count >= _executionSize) break;
                    var branch = string.Format(branchPattern, b++);

                    var description = string.Format(descriptionPattern, ed, dd, dd, one);

                    BranchItem item = BranchFactory.Create(branch, description);
                    if (item == null) continue;

                    var config = _text.Replace("\"ExtensionDepth\": 3,", $"\"ExtensionDepth\": {ed},")
                       .Replace("\"DepthDifference\": 5,", $"\"DepthDifference\": {dd},")
                       .Replace("\"EndDepthDifference\": 5", $"\"EndDepthDifference\": {dd}")
                       .Replace("\"OneReplyDepthDifference\": 12", $"\"OneReplyDepthDifference\": {one}");

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

        string branchPattern = "100-Data-{0}";
        string descriptionPattern = "GT-{0}-SD-{1}-MP-{2}-PD-{3}-MPT-{4}";

        for (int pd = 8; pd < 10; pd++)
        {
            if (_items.Count >= _executionSize) break;
            for (int gt = 27; gt < 28; gt++)
            {
                if (_items.Count >= _executionSize) break;
                for (int sd = 31; sd < 32; sd++)
                {
                    if (_items.Count >= _executionSize) break;
                    for (int mpt = 8; mpt < 10; mpt++)
                    {
                        if (_items.Count >= _executionSize) break;
                        for (int mp = 875; mp < 950; mp += 25)
                        {
                            if (_items.Count >= _executionSize) break;

                            var branch = string.Format(branchPattern, b++);

                            var description = string.Format(descriptionPattern, gt, sd, mp, pd, mpt);

                            BranchItem item = BranchFactory.Create(branch, description);
                            if (item == null) continue;

                            var config = _text.Replace("\"GamesThreshold\": 27,", $"\"GamesThreshold\": {gt},")
                               .Replace("\"SearchDepth\": 32,", $"\"SearchDepth\": {sd},")
                               .Replace("\"MinimumPopular\": 850,", $"\"MinimumPopular\": {mp},")
                               .Replace("\"MaximumPopularThreshold\": 8,", $"\"MaximumPopularThreshold\": {mpt},")
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
        StockFishClient client = new StockFishClient();
        var service = client.GetService();

        foreach (var item in _items.Take(_executionSize))
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(item);

            File.WriteAllText(_pathToConfig, item.Config);

            BranchExecutor branchExecutor = new BranchExecutor(item);

            _totalItems += branchExecutor.Execute();

            service.Save();
        }

        Console.ForegroundColor = ConsoleColor.White;

        File.WriteAllText(_pathToConfig, _text);

        Process process = Process.Start("StockFishComparer.exe", $"-c {_items.Min(i => i.Id)}");

        process.WaitForExit();

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
    }

    private static void ProcessAttackMarginBulk()
    {
        int b = 1;

        string branchPattern = "100-AM-{0}";
        string descriptionPattern = "[ {0}, {1}, {2} ]";

        for (int open = 120; open < 140; open += 10)
        {
            if (_items.Count >= _executionSize) break;
            for (int middle = 170; middle < 210; middle += 10)
            {
                if (_items.Count >= _executionSize) break;
                for (int end = middle + 10; end < 220; end += 10)
                {
                    if (end < middle) continue;

                    if (_items.Count >= _executionSize) break;

                    var branch = string.Format(branchPattern, b++);

                    var description = string.Format(descriptionPattern, open, middle, end);

                    BranchItem item = BranchFactory.Create(branch, description);
                    if ( item == null) continue;

                    var config = _text.Replace(": [ 120, 190, 200 ],", $": [ {open}, {middle}, {end} ],");

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

    private static void ProcessPassedPawns()
    {
        int b = 1;

        string branchPattern = "70-PP-EG-01-{0}";
        string descriptionPattern = "PP=[0, 0, {0}, {1}, {2}, {3}, {4}, 0]";

        //[ 0, 0, 5, 20, 30, 40, 50, 0 ]

        for (int rank2 = 10; rank2 < 15; rank2 += 5)
        {
            if (_items.Count >= _executionSize) break;
            for (int rank3 = Math.Max(rank2 + 10, 20); rank3 < 35; rank3 += 10)
            {
                if (rank3 != 20) continue;
                if (_items.Count >= _executionSize) break;
                for (int rank4 = Math.Max(rank3 + 10, 30); rank4 < 55; rank4 += 10)
                {
                    if (rank4 != 30) continue;
                    if ((rank4 - rank3) > 20) continue;
                    if (_items.Count >= _executionSize) break;
                    for (int rank5 = Math.Max(rank4 + 10, 45); rank5 < 65; rank5 += 10)
                    {
                        if ((rank5 - rank4) > 25) continue;
                        if (_items.Count >= _executionSize) break;
                        for (int rank6 = Math.Max(rank5 + 10, 55); rank6 < 95; rank6 += 5)
                        {
                            if ((rank6 - rank5) > 35) continue;

                            if (_items.Count >= _executionSize) break;

                            var branch = string.Format(branchPattern, b++);

                            var description = string.Format(descriptionPattern, rank2, rank3, rank4, rank5, rank6);

                            //Console.WriteLine($"{branch} {description}");

                            BranchItem item = BranchFactory.Create(branch, description);
                            if (item == null) continue;

                            var config = _text
                                .Replace("\"WhiteEnd\": [ 0, 0, 5, 20, 30, 40, 50, 0 ]",
                                    $"\"WhiteEnd\": [ 0, 0, {rank2}, {rank3}, {rank4}, {rank5}, {rank6}, 0 ]")
                                .Replace("\"BlackEnd\": [ 0, 50, 40, 30, 20, 5, 0, 0 ]",
                                    $"\"BlackEnd\": [ 0, {rank6}, {rank5}, {rank4}, {rank3}, {rank2}, 0, 0 ]");

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
}