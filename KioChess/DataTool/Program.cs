using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures.Moves.Lists;
using System.Diagnostics;
using System.Text;
using Tools.Common;


internal class Program
{
    private static TimeSpan _upsertTime = TimeSpan.Zero;
    private static TimeSpan _upsertBulkTime = TimeSpan.Zero;

    private static void Main(string[] args)
    {
        
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        var das = Boot.GetService<IDataAccessService>();
        var bookService = Boot.GetService<IBookService>();

        try
        {
            das.Connect();

            int size = 1;
            int sequence = 24;

            for (int i = 0; i < size; i++)
            {
                Upsert(das, GenerateMoveKeys(sequence));

                UpsertBulk(das, GenerateMoveKeys(sequence));
            }
        }
        finally
        {

            das.Disconnect();
        }
        timer.Stop();

        Console.WriteLine();

        Console.WriteLine($"Update = {_upsertTime}, Bulk = {_upsertBulkTime}");

        Console.WriteLine();

        Console.WriteLine(timer.Elapsed);
        Console.WriteLine();

        Console.WriteLine($"Finished !!!");

        Console.ReadLine();
    }

    private static List<short> GenerateMoveKeys(int sequence)
    {
        HashSet<short> list = new HashSet<short>();

        while (list.Count < sequence)
        {
            short shortik = (short)RandomExtensions.Random.Next(1000);
            list.Add(shortik);
        }

        return list.ToList();
    }

    private static void UpsertBulk(IDataAccessService das, List<short> list)
    {
        var time = Stopwatch.StartNew();

        List<HitoryStructure> hitoryStructures = new List<HitoryStructure>(list.Count);

        MoveKeyCollection collection = new MoveKeyCollection(list.Count);

        hitoryStructures.Add(new HitoryStructure { History = string.Empty, NextMove = list[0], Draw = 1 });

        collection.Add(list[0]);

        for (int i = 1; i < list.Count; i++)
        {
            collection.Sort();

            hitoryStructures.Add(new HitoryStructure { History = collection.AsKey(), NextMove = list[i], Draw = 1 });

            collection.Add(list[i]);
        }

        das.UpsertBulk(hitoryStructures);

        time.Stop();

        _upsertBulkTime += time.Elapsed;
    }

    private static void Upsert(IDataAccessService das, List<short> list)
    {
        var time = Stopwatch.StartNew();

        MoveKeyCollection collection = new MoveKeyCollection(list.Count);

        das.Upsert(string.Empty, list[0], GameValue.Draw);

        collection.Add(list[0]);

        for (int i = 1; i < list.Count; i++)
        {
            collection.Sort();

            das.Upsert(collection.AsKey(), list[i], GameValue.Draw);

            collection.Add(list[i]);
        }

        time.Stop();

        _upsertTime += time.Elapsed;
    }

    private static void ProcessKeys(IDataAccessService das, IBookService bookService)
    {
        das.LoadAsync(bookService);

        das.WaitToData();

        Dictionary<string, HistoryValue> data = bookService.GetData();

        Dictionary<string, Dictionary<string, HistoryValue>> groupedData = data
            .GroupBy(k =>
            {
                if (string.IsNullOrWhiteSpace(k.Key)) return string.Empty;

                var parts = k.Key.Split('-');
                var keys = parts.Select(i => short.Parse(i)).OrderBy(c => c);
                return string.Join('-', keys);
            }).ToDictionary(k => k.Key, v => v.ToDictionary(x => x.Key, x => x.Value));

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
    }
}