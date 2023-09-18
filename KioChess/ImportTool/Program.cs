using Engine.Book.Interfaces;
using Engine.Book.Services;
using System.Diagnostics;
using System.Text;
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

            int count = 0;

            using (var stream = new StreamReader(@"C:\Dev\Temp\Export_Data.csv"))
            {
                double step = 1000000.0 / stream.BaseStream.Length;
                double next = step;

                string line;
                while((line = stream.ReadLine()) != null)
                {
                    count++;
                    var parts = line.Split(",", StringSplitOptions.None);

                    var shorts = string.IsNullOrWhiteSpace(parts[0])
                        ?new short[0]
                        :parts[0].Split("-").Select(short.Parse).ToArray();

                    byte[] bytes = new byte[2 * shorts.Length];

                    Buffer.BlockCopy(shorts, 0, bytes, 0, bytes.Length);

                    string insert = @$"INSERT INTO [dbo].[Books] ([History] ,[NextMove] ,[White] ,[Draw] ,[Black]) VALUES (@History,{parts[1]},{parts[2]},{parts[3]},{parts[4]})";

                    try
                    {
                        //Console.WriteLine(insert);
                        dataAccessService.Execute(insert, new string[] { "@History" }, new object[] { bytes });
                    }
                    catch (Exception e)
                    {
                        failures.Add(insert);

                        Console.WriteLine(e.ToFormattedString());
                    }

                    var percent = 100.0 * stream.BaseStream.Position / stream.BaseStream.Length;

                    if (percent <= next) continue;

                    Console.WriteLine($"{Math.Round(percent, 2)}%   ---   {count}");
                    Console.WriteLine($"\t{insert}");

                    next += step;
                }
            }
        }
        finally
        {
            dataAccessService.Disconnect();

            if (failures.Count > 0)
            {
                File.WriteAllLines(@$"..\..\..\..\Engine\Data\Failure_{DateTime.Now.ToFileName()}.txt", failures);
            }
        }

        timer.Stop();

        Console.WriteLine();

        Console.WriteLine(timer.Elapsed);

        Console.WriteLine();

        Console.WriteLine("Finish!");
        Console.ReadLine();
    }

    private static void ImportByChunks(List<string> failures, IDataAccessService dataAccessService)
    {
        dataAccessService.Clear();

        Console.WriteLine("Clear");

        var lines = File.ReadAllLines(@"C:\Dev\Temp\Export_Data.csv");

        var chunks = lines.Chunk(100);

        int count = 1;
        double step = 500000.0 / lines.Length;
        double next = step;

        foreach (var batch in chunks)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("INSERT INTO [dbo].[Books] ([History] ,[NextMove] ,[White] ,[Draw] ,[Black]) VALUES");

            for (int i = 0; i < batch.Length - 1; i++)
            {
                string line = batch[i];
                var parts = line.Split(",", StringSplitOptions.None);
                builder.AppendLine($"('{parts[0]}',{parts[1]},{parts[2]},{parts[3]},{parts[4]}),");
            }

            var part = batch.Last().Split(",", StringSplitOptions.None);
            builder.AppendLine($"('{part[0]}',{part[1]},{part[2]},{part[3]},{part[4]});");

            var sql = builder.ToString();

            try
            {
                dataAccessService.Execute(sql);
            }
            catch (Exception e)
            {
                failures.Add(sql);

                Console.WriteLine(e.ToFormattedString());
            }
            count += batch.Length;

            var percent = 100.0 * count / lines.Length;

            if (percent <= next) continue;

            Console.WriteLine($"{Math.Round(percent, 2)}%");

            next += step;
        }
    }
}