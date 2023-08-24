using Engine.Book.Interfaces;
using Engine.Book.Models;
using Newtonsoft.Json;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        var das = Boot.GetService<IDataAccessService>();
        var bookService = Boot.GetService<IBookService>();

        try
        {
            das.Connect();

            das.LoadAsync(bookService);

            das.WaitToData();

            Dictionary<string, HistoryValue> data = bookService.GetData();

            Dictionary<string, Dictionary<string, HistoryValue>> groupedData = data
                //.Where(k =>
                //{
                //    if (!string.IsNullOrWhiteSpace(k.Key))
                //    {
                //        var parts = k.Key.Split('-');
                //        return parts.Length < 5;
                //    }
                //    return true;
                //})
                .GroupBy(k =>
            {
                if(string.IsNullOrWhiteSpace(k.Key)) return string.Empty;

                var parts = k.Key.Split('-');
                var keys = parts.Select(i=>short.Parse(i)).OrderBy(c=>c);
                return string.Join('-', keys);
            }).ToDictionary(k=>k.Key, v=>v.ToDictionary(x=>x.Key, x=>x.Value));

            Dictionary<string, HistoryValue> updatedHistory = new Dictionary<string, HistoryValue>();

            foreach (KeyValuePair<string, Dictionary<string, HistoryValue>> gData in groupedData)
            {
                Dictionary<string, HistoryValue> historyMap = gData.Value;

                Dictionary<string, HistoryValue>.ValueCollection history = historyMap.Values;

                HistoryValue historyValue = new HistoryValue();

                foreach (var item in history)
                {
                    historyValue.Merge(item);
                }

                updatedHistory.Add(gData.Key, historyValue);
            }

            using (var stream = new StreamWriter(@"C:\Dev\Temp\Export_Data.csv"))
            {
                foreach (KeyValuePair<string, HistoryValue> uHistory in updatedHistory.OrderBy(h =>
                {
                    if (string.IsNullOrWhiteSpace(h.Key)) return 0;
                    return h.Key.Split('-').Length;
                }))
                {
                    foreach (KeyValuePair<short, BookValue> h in uHistory.Value)
                    {
                        stream.WriteLine($"{uHistory.Key},{h.Key},{h.Value.White},{h.Value.Draw},{h.Value.Black}");
                    }
                } 
            }

            //Dictionary<string, Dictionary<string, HistoryValue>> gData = groupedData.Where(pair => pair.Value.Count > 1).ToDictionary(k => k.Key, v=>v.Value);

            //File.WriteAllText("GroupedData.json",JsonConvert.SerializeObject(gData, Formatting.Indented));

            Console.WriteLine($"Data = {data.Count}, Grouped = {groupedData.Count}, Updated = {updatedHistory.Count}");
        }
        finally
        {
            das.Disconnect();
        }

        timer.Stop();

        Console.WriteLine(timer.Elapsed);

        Console.ReadLine();
    }
}