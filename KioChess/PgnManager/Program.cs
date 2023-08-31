using OpeningMentor.Chess.Model;
using OpeningMentor.Chess.Pgn;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        object sync = new object();

        int count = 0;

        foreach (var file in Directory.EnumerateFiles(@"C:\Dev\PGN","*.pgn"))
        {
            PgnReader pgnReader = new PgnReader();
            IEnumerable<Game> games = pgnReader.ReadGamesFromFile(file);

            List<string> arguments = new List<string>();

            Parallel.ForEach(games.Where(IsGoodElo).Take(8), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, game =>
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
                    var process = Process.Start("PgnTool.exe", text);
                    process.WaitForExit();
                    lock (sync)
                    {
                        Console.WriteLine($"{++count} {text.Length}");
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
            //        var process = Process.Start("PgnTool.exe", text);
            //        process.WaitForExit();
            //        Console.WriteLine($"{++count} {text.Length}");
            //    }
            //} 
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