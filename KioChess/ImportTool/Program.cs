using Engine.Book.Interfaces;
using System.Diagnostics;
using Tools.Common;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        List<string> failures = new List<string>();

        IDataAccessService dataAccessService = Boot.GetService<IDataAccessService>();

        try
        {
            dataAccessService.Connect();

            dataAccessService.Clear();

            Console.WriteLine("Clear");

            var lines = File.ReadAllLines(@"C:\Dev\Temp\Export_Data.csv");

            int count = 1;
            double step = 50000.0 / lines.Length;
            double next = step;

            foreach(var line in lines)
            {
                var parts = line.Split(",", StringSplitOptions.None);
                var sql = $@"INSERT INTO [dbo].[Books] ([History] ,[NextMove] ,[White] ,[Draw] ,[Black]) VALUES ('{parts[0]}',{parts[1]},{parts[2]},{parts[3]},{parts[4]})";

                try
                {
                    dataAccessService.Execute(sql);
                }
                catch (Exception e)
                {
                    failures.Add(sql);

                    Console.WriteLine(e.ToFormattedString());
                }

                var percent = 100.0 * count++ / lines.Length;

                if (percent <= next) continue;

                Console.WriteLine($"{Math.Round(percent,2)}%");

                next += step;
            }
        }
        finally
        {
            dataAccessService.Disconnect();

            if (failures.Count > 0) File.WriteAllLines(@$"..\..\..\..\Engine\Data\Failure_{DateTime.Now.ToFileName()}.txt", failures);
        }

        timer.Stop();

        Console.WriteLine();

        Console.WriteLine(timer.Elapsed);

        Console.WriteLine();

        Console.WriteLine("Finish!");
        Console.ReadLine();
    }
}