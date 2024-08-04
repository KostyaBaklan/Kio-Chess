
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
    }
}
