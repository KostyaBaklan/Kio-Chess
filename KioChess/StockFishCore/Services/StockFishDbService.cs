
using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StockFishCore.Data;
using DataAccess.Helpers;

namespace StockFishCore.Services
{
    public class StockFishDbService: IDbService
    {
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
                IEnumerable<string> headers = new List<string> { "Kio", "StockFish", "Result", "Counts", "Duration" };

                writter.WriteLine(string.Join(",", headers));
                IEnumerable<StockFishMatchItem> matchItems = _db.GetMatchItems(runTimeInformation.Id);

                foreach (var item in matchItems)
                {
                    List<string> values = new List<string>
                    {
                        $"   {item.StockFishResultItem.Strategy}[{item.StockFishResultItem.Depth}]   ",
                        $"   SF[{item.StockFishResultItem.StockFishDepth}][{item.StockFishResultItem.Elo}]   ",
                        $"   {Math.Round(item.Kio, 1)} x {Math.Round(item.SF, 1)}   ",
                        $"   {item.Wins} x {item.Draws} x {item.Looses}   ",
                        $"   {TimeSpan.FromSeconds(item.Duration)}   "
                    };

                    writter.WriteLine(string.Join(",", values));
                }
            }
        }
    }
}
