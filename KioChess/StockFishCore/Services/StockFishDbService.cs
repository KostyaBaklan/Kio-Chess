
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StockFishCore.Data;
using DataAccess.Helpers;

namespace StockFishCore.Services
{
    public class StockFishDbService: IDbService
    {
        private readonly List<string> _headers = new List<string> { "Kio", "StockFish", "Result", "Counts", "MoveTime", "Duration", "Wins %" };
        private readonly List<string> _compareHeaders = new List<string> { "Kio", "StockFish", "Result", "Counts", "Wins %", "Non Loose %", "Left","Right",  "MoveTime", "Duration" };
        private ResultContext _db;

        public void Connect()
        {
            _db = new ResultContext();
        }

        public void Disconnect()
        {
            _db?.Dispose();
        }

        public int Execute(string sql, List<SqliteParameter> parameters = null, int timeout = 30)
        {
            using var connction = new SqliteConnection(_db.Database.GetConnectionString());
            return connction.Execute(sql, parameters, timeout);
        }

        public IEnumerable<T> Execute<T>(string sql, Func<SqliteDataReader, T> factory, List<SqliteParameter> parameters = null, int timeout = 60)
        {
            using var connction = new SqliteConnection(_db.Database.GetConnectionString());
            return connction.Execute(sql, factory, parameters, timeout);
        }

        public void GenerateLatestReport()
        {
            var maxID = _db.RunTimeInformation.Max(r => r.Id);
            SaveReportForRunTime(maxID);
        }

        public void GenerateReports()
        {
            var runTime = _db.RunTimeInformation.Select(rti=>rti.Id).ToList();

            foreach (var report in runTime)
            {
                SaveReportForRunTime(report);
            }
        }

        public void SaveReportForRunTime(int maxID)
        {
            RunTimeInformation runTimeInformation = _db.RunTimeInformation.Find(maxID);

            using (var writter = new StreamWriter($"StockFishResults_{runTimeInformation.Branch}_{runTimeInformation.RunTime.ToString("yyyy_MM_dd_hh_mm_ss")}.csv"))
            {
                writter.WriteLine(string.Join(",", _headers));
                IEnumerable<StockFishMatchItem> matchItems = _db.GetMatchItems(runTimeInformation.Id);

                foreach (var item in matchItems)
                {
                    List<string> values = new List<string>
                    {
                        $"   {item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]   ",
                        $"   SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]   ",
                        $"   {Math.Round(item.Result.Kio, 1)} x {Math.Round(item.Result.SF, 1)}   ",
                        $"   {item.Result.Wins} x {item.Result.Draws} x {item.Result.Looses}   ",
                        $"   {TimeSpan.FromMilliseconds(item.Result.MoveTime)}   ",
                        $"   {TimeSpan.FromMilliseconds(item.Result.Duration)}   "
                    };

                    writter.WriteLine(string.Join(",", values));
                }
            }
        }

        public string Compare(int left, int right)
        {
            RunTimeInformation rtLeft = _db.RunTimeInformation.Find(left);
            RunTimeInformation rtRight = _db.RunTimeInformation.Find(right);

            string fileName = $"StockFishCompare_{rtLeft.Branch}_{rtRight.Branch}.csv";
            using (var writter = new StreamWriter(fileName))
            {
                ProcessMatchItems(rtLeft, rtRight, writter);
                writter.WriteLine($" , , , , , , , , , ");
                writter.WriteLine($" , , , , , , , , , ");
                ProcessDepthMatchItems(rtLeft, rtRight, writter);
            }

            return fileName;
        }

        private void ProcessDepthMatchItems(RunTimeInformation rtLeft, RunTimeInformation rtRight, StreamWriter writter)
        {
            var leftItems = _db.GetMatchDepthItems(rtLeft.Id).ToDictionary(k => k.StockFishDepthEloItem, v => v.Result);
            var rightItems = _db.GetMatchDepthItems(rtRight.Id);

            List<StockFishDepthCompareItem> items = new List<StockFishDepthCompareItem>();

            foreach (var item in rightItems)
            {
                var leftItem = leftItems[item.StockFishDepthEloItem];

                StockFishDepthCompareItem stockFishCompareItem = new StockFishDepthCompareItem
                {
                    StockFishDepthEloItem = item.StockFishDepthEloItem,
                    Left = leftItem,
                    Right = item.Result
                };

                items.Add(stockFishCompareItem);
             }
            int leftSum = 0;
            int rightSum = 0;
            foreach (var item in items)
            {
                List<string> values = new List<string>
                    {
                        $"   {item.StockFishDepthEloItem.Depth}   ",
                        $"   {item.StockFishDepthEloItem.Elo}   ",
                        $"   {Math.Round(item.Left.Kio, 1)} x {Math.Round(item.Left.SF, 1)}={Math.Round(item.Right.Kio, 1)} x {Math.Round(item.Right.SF, 1)}   ",
                        $"   {item.Left.Wins} x {item.Left.Draws} x {item.Left.Looses}={item.Right.Wins} x {item.Right.Draws} x {item.Right.Looses}   ",
                        $"   {item.Left.WinPercentage}={item.Right.WinPercentage}   ",
                        $"   {item.Left.NonLoosePercentage}={item.Right.NonLoosePercentage}   "
                    };

                if (item.Left.Kio < item.Right.Kio)
                {
                    values.AddRange(new[] { "0", "1" });
                    rightSum++;
                }
                else if (item.Left.Kio > item.Right.Kio)
                {
                    values.AddRange(new[] { "1", "0" });
                    leftSum++;
                }
                else
                {
                    values.AddRange(new[] { "0", "0" });
                }

                values.Add($"   {TimeSpan.FromMilliseconds(item.Left.MoveTime)}={TimeSpan.FromMilliseconds(item.Right.MoveTime)}   ");
                values.Add($"   {TimeSpan.FromMilliseconds(item.Left.Duration)}={TimeSpan.FromMilliseconds(item.Right.Duration)}   ");

                writter.WriteLine(string.Join(",", values));
            }
            writter.WriteLine($" , , , , , ,{leftSum},{rightSum}, , ");
        }

        private void ProcessMatchItems(RunTimeInformation rtLeft, RunTimeInformation rtRight, StreamWriter writter)
        {
            writter.WriteLine(string.Join(",", _compareHeaders));
            var leftItems = _db.GetMatchItems(rtLeft.Id).ToDictionary(k => k.StockFishResultItem, v => v.Result);
            IEnumerable<StockFishMatchItem> rightItems = _db.GetMatchItems(rtRight.Id);

            List<StockFishCompareItem> items = new List<StockFishCompareItem>();

            foreach (var item in rightItems)
            {
                var leftItem = leftItems[item.StockFishResultItem];

                StockFishCompareItem stockFishCompareItem = new StockFishCompareItem
                {
                    StockFishResultItem = item.StockFishResultItem,
                    Left = leftItem,
                    Right = item.Result
                };

                items.Add(stockFishCompareItem);
            }
            int leftSum = 0;
            int rightSum = 0;
            foreach (var item in items)
            {
                List<string> values = new List<string>
                    {
                        $"   {item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]   ",
                        $"   SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]   ",
                        $"   {Math.Round(item.Left.Kio, 1)} x {Math.Round(item.Left.SF, 1)}={Math.Round(item.Right.Kio, 1)} x {Math.Round(item.Right.SF, 1)}   ",
                        $"   {item.Left.Wins} x {item.Left.Draws} x {item.Left.Looses}={item.Right.Wins} x {item.Right.Draws} x {item.Right.Looses}   ",
                        $"   {item.Left.WinPercentage}={item.Right.WinPercentage}   ",
                        $"   {item.Left.NonLoosePercentage}={item.Right.NonLoosePercentage}   "
                    };

                if (item.Left.Kio < item.Right.Kio)
                {
                    values.AddRange(new[] { "0", "1" });
                    rightSum++;
                }
                else if (item.Left.Kio > item.Right.Kio)
                {
                    values.AddRange(new[] { "1", "0" });
                    leftSum++;
                }
                else
                {
                    values.AddRange(new[] { "0", "0" });
                }

                values.Add($"   {TimeSpan.FromMilliseconds(item.Left.MoveTime)}={TimeSpan.FromMilliseconds(item.Right.MoveTime)}   ");
                values.Add($"   {TimeSpan.FromMilliseconds(item.Left.Duration)}={TimeSpan.FromMilliseconds(item.Right.Duration)}   ");

                writter.WriteLine(string.Join(",", values));
            }
            writter.WriteLine($" , , , , , ,{leftSum},{rightSum}, , ");
        }

        public string Compare(int id)
        {
            var last = _db.RunTimeInformation.Where(d => d.Id >= id).Max(d=>d.Id);

            var file = CompareRange(id, last);

            return file;
        }

        private string CompareRange(int id, int last)
        {
            var query = _db.RunTimeInformation.Where(rt => rt.Id >= id && rt.Id <= last);

            var rtInfo = query.ToList();

            List<List<string>> rows = new List<List<string>>();

            var branches = rtInfo.ToDictionary(k => k.Id, k => k.Branch);

            var total = branches.Values.ToDictionary(k => k, v => 0);

            string fileName = $"StockFishCompare_Range_{id}_{last}.csv";

            Dictionary<int, List<StockFishMatchItem>> matchItems = ProcessMatchItems(rtInfo, rows, branches);

            Dictionary<int, List<StockFishDepthMatchItem>> depthMatchItems = ProcessDepthMatchItems(rtInfo, rows, branches);

            ProcessMatchItemsResults(rows, branches, total, matchItems);

            ProcessMatchDepthItemsResults(rows, branches, total, depthMatchItems);

            rows.Add(new List<string> { "Branch", "Value" });

            rows.AddRange(total.OrderBy(x => x.Value).Select(p => new List<string> { p.Key, p.Value.ToString() }));

            using (var writter = new StreamWriter(fileName))
            {
                foreach (var values in rows)
                {
                    writter.WriteLine(string.Join(",", values));
                }
            }

            return fileName;
        }

        public string Compare(string[] args)
        {
            var query = _db.RunTimeInformation.Where(rt => args.Contains(rt.Branch));

            var rtInfo = query.ToList();

            List<List<string>> rows = new List<List<string>>();

            var branches = rtInfo.ToDictionary(k => k.Id, k => k.Branch);

            var total = branches.Values.ToDictionary(k => k, v => 0);

            string fileName = $"StockFishCompare_{string.Join('_', rtInfo.Select(r => r.Id))}.csv";

            Dictionary<int, List<StockFishMatchItem>> matchItems = ProcessMatchItems(rtInfo, rows, branches);

            Dictionary<int, List<StockFishDepthMatchItem>> depthMatchItems = ProcessDepthMatchItems(rtInfo, rows, branches);

            ProcessMatchItemsResults(rows, branches, total, matchItems);

            ProcessMatchDepthItemsResults(rows, branches, total, depthMatchItems);

            rows.Add(new List<string> { "Branch", "Value" });

            rows.AddRange(total.OrderBy(x => x.Value).Select(p => new List<string> { p.Key, p.Value.ToString() }));

            using (var writter = new StreamWriter(fileName))
            {
                foreach (var values in rows)
                {
                    writter.WriteLine(string.Join(",", values));
                }
            }

            return fileName;
        }

        private void ProcessMatchDepthItemsResults(List<List<string>> rows, Dictionary<int, string> branches, Dictionary<string, int> total, Dictionary<int, List<StockFishDepthMatchItem>> matchItems)
        {
            Dictionary<string, int> localTotal = branches.Values.ToDictionary(k => k, v => 0);
            var h = new List<string> { "", "" };
            foreach (var item in matchItems.Keys)
            {
                h.Add(branches[item]);
            }

            rows.Add(h);
            var row = rows.Count;

            foreach (var item in matchItems.First().Value)
            {
                rows.Add(new List<string> { $"   {item.StockFishDepthEloItem.Depth} - {item.StockFishDepthEloItem.Elo}  ", "Result" });
                //rows.Add(new List<string> { "", "Counts" });
                rows.Add(new List<string> { "", "Win %" });
                rows.Add(new List<string> { "", "Non Loose %" });
                //rows.Add(new List<string> { "", "Move Time" });
                //rows.Add(new List<string> { $"   {item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]   ",
                //        $"   SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]   " });
            }
            for (var i = 0; i < matchItems.First().Value.Count; i++)
            {
                //result
                var result = matchItems.Select(x => new KeyValuePair<string, double>(branches[x.Key], x.Value[i].Result.Kio))
                .OrderByDescending(x => x.Value)
                .GroupBy(g => g.Value, v => v.Key);

                var branchmap = new Dictionary<string, int>();
                foreach (var res in result.Select((g, ind) => new KeyValuePair<int, List<string>>(ind, g.ToList())))
                {
                    foreach (var branch in res.Value)
                    {
                        branchmap[branch] = res.Key;
                    }
                }

                foreach (var b in total.Keys)
                {
                    rows[row].Add(branchmap[b].ToString());
                    total[b] += branchmap[b];
                    localTotal[b] += branchmap[b];
                }
                row++;

                //wins
                var win = matchItems.Select(x => new KeyValuePair<string, double>(branches[x.Key], x.Value[i].Result.WinPercentage))
                .OrderByDescending(x => x.Value)
                .GroupBy(g => g.Value, v => v.Key);

                branchmap = new Dictionary<string, int>();
                foreach (var res in win.Select((g, ind) => new KeyValuePair<int, List<string>>(ind, g.ToList())))
                {
                    foreach (var branch in res.Value)
                    {
                        branchmap[branch] = res.Key;
                    }
                }

                foreach (var b in total.Keys)
                {
                    rows[row].Add(branchmap[b].ToString());
                    total[b] += branchmap[b];
                    localTotal[b] += branchmap[b];
                }
                row++;

                //non loose
                var nonloose = matchItems.Select(x => new KeyValuePair<string, double>(branches[x.Key], x.Value[i].Result.NonLoosePercentage))
                .OrderByDescending(x => x.Value)
                .GroupBy(g => g.Value, v => v.Key);

                branchmap = new Dictionary<string, int>();
                foreach (var res in nonloose.Select((g, ind) => new KeyValuePair<int, List<string>>(ind, g.ToList())))
                {
                    foreach (var branch in res.Value)
                    {
                        branchmap[branch] = res.Key;
                    }
                }

                foreach (var b in total.Keys)
                {
                    rows[row].Add(branchmap[b].ToString());
                    total[b] += branchmap[b];
                    localTotal[b] += branchmap[b];
                }
                row++;

                //move time
                //var moveTime = matchItems.Select(x => new KeyValuePair<string, double>(branches[x.Key], x.Value[i].Result.MoveTime))
                //.OrderByDescending(x => x.Value)
                //.GroupBy(g => g.Value, v => v.Key);

                //branchmap = new Dictionary<string, int>();
                //foreach (var res in moveTime.Select((g, ind) => new KeyValuePair<int, List<string>>(ind, g.ToList())))
                //{
                //    foreach (var branch in res.Value)
                //    {
                //        branchmap[branch] = res.Key;
                //    }
                //}

                //foreach (var b in h.Skip(2))
                //{
                //    rows[row].Add(branchmap[b].ToString());
                //    total[b] += branchmap[b];
                //}
                //row++;
            }

            var t = new List<string> { "Total", "" };

            t.AddRange(localTotal.Values.Select(x => x.ToString()));

            rows.Add(t);

            rows.Add(Enumerable.Repeat("", 20).ToList());
            rows.Add(Enumerable.Repeat("", 20).ToList());
        }

        private static void ProcessMatchItemsResults(List<List<string>> rows, Dictionary<int, string> branches, Dictionary<string, int> total, Dictionary<int, List<StockFishMatchItem>> matchItems)
        {
            Dictionary<string, int> localTotal = branches.Values.ToDictionary(k => k, v => 0);
            var h = new List<string> { "", "" };
            foreach (var item in matchItems.Keys)
            {
                h.Add(branches[item]);
            }

            rows.Add(h);
            var row = rows.Count;

            foreach (var item in matchItems.First().Value)
            {
                rows.Add(new List<string> { $"   {item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]-SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]   ", "Result" });
                //rows.Add(new List<string> { "", "Counts" });
                rows.Add(new List<string> { "", "Win %" });
                rows.Add(new List<string> { "", "Non Loose %" });
                //rows.Add(new List<string> { "", "Move Time" });
                //rows.Add(new List<string> { $"   {item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]   ",
                //        $"   SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]   " });
            }
            for (var i = 0; i < matchItems.First().Value.Count; i++)
            {
                //result
                var result = matchItems.Select(x => new KeyValuePair<string, double>(branches[x.Key], x.Value[i].Result.Kio))
                .OrderByDescending(x => x.Value)
                .GroupBy(g => g.Value, v => v.Key);

                var branchmap = new Dictionary<string, int>();
                foreach (var res in result.Select((g, ind) => new KeyValuePair<int, List<string>>(ind, g.ToList())))
                {
                    foreach (var branch in res.Value)
                    {
                        branchmap[branch] = res.Key;
                    }
                }

                foreach (var b in total.Keys)
                {
                    rows[row].Add(branchmap[b].ToString());
                    total[b] += branchmap[b];
                    localTotal[b] += branchmap[b];
                }
                row++;

                //wins
                var win = matchItems.Select(x => new KeyValuePair<string, double>(branches[x.Key], x.Value[i].Result.WinPercentage))
                .OrderByDescending(x => x.Value)
                .GroupBy(g => g.Value, v => v.Key);

                branchmap = new Dictionary<string, int>();
                foreach (var res in win.Select((g, ind) => new KeyValuePair<int, List<string>>(ind, g.ToList())))
                {
                    foreach (var branch in res.Value)
                    {
                        branchmap[branch] = res.Key;
                    }
                }

                foreach (var b in total.Keys)
                {
                    rows[row].Add(branchmap[b].ToString());
                    total[b] += branchmap[b];
                    localTotal[b] += branchmap[b];
                }
                row++;

                //non loose
                var nonloose = matchItems.Select(x => new KeyValuePair<string, double>(branches[x.Key], x.Value[i].Result.NonLoosePercentage))
                .OrderByDescending(x => x.Value)
                .GroupBy(g => g.Value, v => v.Key);

                branchmap = new Dictionary<string, int>();
                foreach (var res in nonloose.Select((g, ind) => new KeyValuePair<int, List<string>>(ind, g.ToList())))
                {
                    foreach (var branch in res.Value)
                    {
                        branchmap[branch] = res.Key;
                    }
                }

                foreach (var b in total.Keys)
                {
                    rows[row].Add(branchmap[b].ToString());
                    total[b] += branchmap[b];
                    localTotal[b] += branchmap[b];
                }
                row++;

                //move time
                //var moveTime = matchItems.Select(x => new KeyValuePair<string, double>(branches[x.Key], x.Value[i].Result.MoveTime))
                //.OrderByDescending(x => x.Value)
                //.GroupBy(g => g.Value, v => v.Key);

                //branchmap = new Dictionary<string, int>();
                //foreach (var res in moveTime.Select((g, ind) => new KeyValuePair<int, List<string>>(ind, g.ToList())))
                //{
                //    foreach (var branch in res.Value)
                //    {
                //        branchmap[branch] = res.Key;
                //    }
                //}

                //foreach (var b in h.Skip(2))
                //{
                //    rows[row].Add(branchmap[b].ToString());
                //    total[b] += branchmap[b];
                // localTotal[b] += branchmap[b];
                //}
                //row++;
            }

            var t = new List<string> { "Total", "" };

            t.AddRange(localTotal.Values.Select(x => x.ToString()));

            rows.Add(t);

            rows.Add(Enumerable.Repeat("", 20).ToList());
            rows.Add(Enumerable.Repeat("", 20).ToList());
        }

        private Dictionary<int, List<StockFishDepthMatchItem>> ProcessDepthMatchItems(List<RunTimeInformation> rtInfo, List<List<string>> rows, Dictionary<int, string> branches)
        {
            Dictionary<int, List<StockFishDepthMatchItem>> matchItems = new Dictionary<int, List<StockFishDepthMatchItem>>();
            
            foreach (var item in rtInfo)
            {
                matchItems.Add(item.Id, _db.GetMatchDepthItems(item.Id).ToList());
            }
            var h = new List<string> { "", "" };
            foreach (var item in matchItems.First().Value)
            {
                h.Add($"  {item.StockFishDepthEloItem.Depth} - {item.StockFishDepthEloItem.Elo}  ");
            }
            rows.Add(h);
            int row = rows.Count;

            foreach (var item in matchItems.Keys)
            {
                rows.Add(new List<string> { branches[item], "Result" });
                rows.Add(new List<string> { "", "Counts" });
                rows.Add(new List<string> { "", "Win %" });
                rows.Add(new List<string> { "", "Non Loose %" });
                rows.Add(new List<string> { "", "Move Time" });
                //rows.Add(new List<string> { $"   {item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]   ",
                //        $"   SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]   " });
            }

            foreach (var match in matchItems)
            {
                foreach (var item in match.Value)
                {
                    rows[row].Add($"  {Math.Round(item.Result.Kio, 1)} x {Math.Round(item.Result.SF, 1)}  ");
                    rows[row + 1].Add($"  {item.Result.Wins} x {item.Result.Draws} x {item.Result.Looses}  ");
                    rows[row + 2].Add($"  {item.Result.WinPercentage}  ");
                    rows[row + 3].Add($"  {item.Result.NonLoosePercentage}  ");
                    rows[row + 4].Add($"  {TimeSpan.FromMilliseconds(item.Result.MoveTime)}  ");
                }
                row += 5;
            }

            rows.Add(Enumerable.Repeat("", 20).ToList());
            rows.Add(Enumerable.Repeat("", 20).ToList());

            return matchItems;
        }

        private Dictionary<int, List<StockFishMatchItem>> ProcessMatchItems(List<RunTimeInformation> rtInfo, List<List<string>> rows, Dictionary<int, string> branches)
        {
            Dictionary<int, List<StockFishMatchItem>> matchItems = new Dictionary<int, List<StockFishMatchItem>>();
            foreach (var item in rtInfo)
            {
                matchItems.Add(item.Id, _db.GetMatchItems(item.Id).ToList());
            }
            List<string> headers = new List<string> { "", "" };
            foreach (var item in matchItems.First().Value)
            {
                headers.Add($"   {item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]-SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]   ");
            }

            rows.Add(headers);
            int row = rows.Count;

            foreach (var item in matchItems.Keys)
            {
                rows.Add(new List<string> { branches[item], "Result" });
                rows.Add(new List<string> { "", "Counts" });
                rows.Add(new List<string> { "", "Win %" });
                rows.Add(new List<string> { "", "Non Loose %" });
                rows.Add(new List<string> { "", "Move Time" });
                //rows.Add(new List<string> { $"   {item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]   ",
                //        $"   SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]   " });
            }

            foreach (var match in matchItems)
            {
                foreach (var item in match.Value)
                {
                    rows[row].Add($"  {Math.Round(item.Result.Kio, 1)} x {Math.Round(item.Result.SF, 1)}  ");
                    rows[row + 1].Add($"  {item.Result.Wins} x {item.Result.Draws} x {item.Result.Looses}  ");
                    rows[row + 2].Add($"  {item.Result.WinPercentage}  ");
                    rows[row + 3].Add($"  {item.Result.NonLoosePercentage}  ");
                    rows[row + 4].Add($"  {TimeSpan.FromMilliseconds(item.Result.MoveTime)}  ");
                }
                row += 5;
            }

            rows.Add(Enumerable.Repeat("", 20).ToList());
            rows.Add(Enumerable.Repeat("", 20).ToList());

            return matchItems;
        }
    }
}
