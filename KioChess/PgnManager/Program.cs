using Engine.Interfaces.Config;
using OpeningMentor.Chess.Model;
using System.Diagnostics;
using System.Text;
using Tools.Common;

internal class Program
{
    private static int _elo;
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        _elo = Boot.GetService<IConfigurationProvider>().BookConfiguration.Elo;

        object sync = new object();

        int count = 0;
        int f = 0;

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            foreach (var file in files)
            {
                f++;

                int white = 0;
                int black = 0;

                var tasks = new List<Task>();

                StringBuilder stringBuilder = new StringBuilder();

                using(var reader = new StreamReader(file))
                {
                    var size = 100.0/reader.BaseStream.Length;

                    string line;

                    while((line = reader.ReadLine()) != null)
                    {
                        if (line.ToLower().StartsWith("[event"))
                        {
                            if (Math.Min(white, black) > _elo)
                            {
                                var gameAsString = stringBuilder.ToString();

                                if (!string.IsNullOrWhiteSpace(gameAsString))
                                {
                                    var progress = Math.Round(reader.BaseStream.Position * size, 6);

                                    var task = Task.Factory.StartNew(() =>
                                    {
                                        var buffer = Encoding.UTF8.GetBytes(gameAsString);

                                        var text = Convert.ToBase64String(buffer);

                                        var t = Stopwatch.StartNew();

                                        var process = Process.Start("PgnTool.exe", text);
                                        process.WaitForExit();

                                        t.Stop();

                                        lock (sync)
                                        {
                                            Console.WriteLine($"{f}/{files.Length}   {++count}   {progress}%   {t.Elapsed}   {timer.Elapsed}");
                                        }
                                    });

                                    tasks.Add(task);
                                }
                            }

                            white = 0;
                            black = 0;

                            stringBuilder = new StringBuilder(line);
                        }
                        else
                        {
                            if (line.ToLower().StartsWith("[whiteelo"))
                            {
                                var parts = line.Split('"');
                                if (int.TryParse(parts[1], out var w))
                                {
                                    white = w;
                                }
                                else
                                {
                                    white = 0;
                                }
                            }
                            else if (line.ToLower().StartsWith("[blackelo"))
                            {
                                var parts = line.Split('"');
                                if (int.TryParse(parts[1], out var b))
                                {
                                    black = b;
                                }
                                else
                                {
                                    black = 0;
                                }
                            }

                            stringBuilder.Append(line);
                        }
                    }
                }

                Task.WaitAll(tasks.ToArray());

                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to delete '{file}'");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());

            Console.WriteLine("Pizdets !!!");
        }

        timer.Stop();

        Console.WriteLine(timer.Elapsed);

        Console.WriteLine("PGN DONE !!!");

        Console.ReadLine();
    }
}