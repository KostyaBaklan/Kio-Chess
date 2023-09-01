using OpeningMentor.Chess.Model;
using OpeningMentor.Chess.Pgn;
using System.Diagnostics;
using Tools.Common;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        object sync = new object();

        int count = 0;
        int progress = 0;

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            foreach (var file in files)
            {
                PgnReader pgnReader = new PgnReader();
                IEnumerable<Game> games = pgnReader.ReadGamesFromFile(file);

                Parallel.ForEach(games.Where(IsGoodElo), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, game =>
                {
                    Database database = new Database();
                    database.Games.Add(game);

                    byte[] buffer;
                    using (var stream = new MemoryStream())
                    {
                        PgnWriter pgnWriter = new PgnWriter(stream);
                        pgnWriter.Write(database);

                        buffer = stream.ToArray();
                    }

                    if (buffer != null)
                    {
                        var text = Convert.ToBase64String(buffer);

                        var t = Stopwatch.StartNew();

                        var process = Process.Start("PgnTool.exe", text);
                        process.WaitForExit();

                        t.Stop();

                        lock (sync)
                        {
                            Console.WriteLine($"\t{++count}   {text.Length}   {t.Elapsed}");
                        }
                    }
                });

                //foreach (Game game in games.Where(IsGoodElo))
                //{
                //    Database database = new Database();
                //    database.Games.Add(game);

                //    byte[] buffer;
                //    using (var stream = new MemoryStream())
                //    {
                //        PgnWriter pgnWriter = new PgnWriter(stream);
                //        pgnWriter.Write(database);

                //        buffer = stream.ToArray();
                //    }

                //    if (buffer != null)
                //    {
                //        var text = Convert.ToBase64String(buffer);

                //        var t = Stopwatch.StartNew();

                //        var process = Process.Start("PgnTool.exe", text);
                //        process.WaitForExit();

                //        t.Stop();

                //        Console.WriteLine($"\t{++count}   {text.Length}   {t.Elapsed}");
                //    }
                //}

                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to delete '{file}'");
                }
                progress++;

                var percentage = Math.Round(100.0 * progress / files.Length, 4);
                Console.WriteLine($"{percentage}%");
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

    private static bool IsGoodElo(Game game)
    {
        if (game.AdditionalInfo != null)
        {
            var elo = game.AdditionalInfo
                .Where(info => info.Name.ToLower()
                .Contains("elo"))
                .Select(info =>
                {
                    return int.TryParse(info.Value, out var el) ? el : 0;
                })
                .ToList();

            if (elo != null && elo.Count >= 2 && elo.Min() >= 2000)
            {
                return true;
            }
        }
        return false;
    }
}