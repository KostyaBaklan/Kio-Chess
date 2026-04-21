using DataAccess.Interfaces;
using Engine.Interfaces.Config;
using GamesServices;
using System.Diagnostics;
using System.Text;
using Tools.Common;

internal class Program
{
    private static int _elo;
    private static int _eloCount;
    private static int _configElo;
    private static Dictionary<string, int> _suggestedElos;
    private static IOpeningDbService _dataAccessService;
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        IBookConfiguration bookConfiguration = Boot.GetService<IConfigurationProvider>().BookConfiguration;
        _elo = bookConfiguration.Elo;
        _eloCount = bookConfiguration.EloCount;
        _configElo = _elo;

        _dataAccessService = Boot.GetService<IOpeningDbService>();
        try
        {
            _dataAccessService.Connect();

            CountElo(timer);

            ProcessPgnFiles(timer);

        }
        finally
        {
            _dataAccessService.Disconnect();
        }

        timer.Stop();

        Console.WriteLine(timer.Elapsed);

        Console.WriteLine("PGN DONE !!!");

        Console.ReadLine();
    }

    private static void ProcessPgnFiles(Stopwatch timer)
    {
#if DEBUG

        var process = Process.Start(@$"..\..\..\GsServer\bin\Debug\net9.0\GsServer.exe");
        process.WaitForExit(100);
#else
        var process = Process.Start(@$"..\..\..\GsServer\bin\Release\net9.0\GsServer.exe");
        process.WaitForExit(100);
# endif

        SequenceClient client = new SequenceClient();
        var service = client.GetService();
        service.Initialize();

        object sync = new object();

        int count = 0;
        int f = 0;

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            foreach (var file in files)
            {
                if (_suggestedElos != null)
                {
                    if (_suggestedElos.TryGetValue(file, out var elo) && elo > _configElo)
                    {
                        _elo = elo;
                    } 
                }
                else
                {
                    _elo = 0;
                }

                f++;

                var ff = $"{f}/{files.Length}";

                int white = 0;
                int black = 0;

                var tasks = new List<Task>();

                StringBuilder stringBuilder = new StringBuilder();

                using (var reader = new StreamReader(file))
                {
                    var size = 100.0 / reader.BaseStream.Length;

                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.ToLower().StartsWith("[event "))
                        {
                            if (Math.Min(white, black) > _elo)
                            {
                                var gameAsString = stringBuilder.ToString();

                                if (!string.IsNullOrWhiteSpace(gameAsString))
                                {
                                    var progress = Math.Round(reader.BaseStream.Position * size, 6);
                                    var c = ++count;

                                    var task = Task.Factory.StartNew(() =>
                                    {
                                        var t = Stopwatch.StartNew();

                                        ProcessStartInfo info = new ProcessStartInfo
                                        {
                                            FileName = "PgnTool.exe",
                                            ArgumentList = { gameAsString }
                                        };

                                        var process = Process.Start(info);
                                        process.WaitForExit();

                                        t.Stop(); 
                                        
                                        Console.WriteLine($"{ff}   {c}   {progress}%   {t.Elapsed}   {timer.Elapsed}");
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

            service.Save();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());

            Console.WriteLine("Pizdets !!!");
        }
        finally
        {
            client.Close();

            foreach (var file in _suggestedElos)
            {
                Console.WriteLine($"Finished '{file.Key}' ELO = {file.Value}");
            }
        }
    }

    private static void CountElo(Stopwatch timer)
    {
        int totalCount = 0;
        int f = 0;
        int totalGames = 0;

        _suggestedElos = new Dictionary<string, int>();

        List<double> elo = new List<double>();

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            foreach (var file in files)
            {
                f++;

                _suggestedElos[file] = _elo;

                List<int> elos = new List<int>();

                int white = 0;
                int black = 0;
                int count = 0;
                int games = 0;

                var tasks = new List<Task>();

                StringBuilder stringBuilder = new StringBuilder();

                using (var reader = new StreamReader(file))
                {
                    var size = 100.0 / reader.BaseStream.Length;

                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.ToLower().StartsWith("[event"))
                        {
                            var el = Math.Min(white, black);
                            if (el >= _elo)
                            {
                                elos.Add(el);
                                count++;
                                var progress = Math.Round(reader.BaseStream.Position * size, 6);

                                Console.WriteLine($"{f}/{files.Length}   {++totalCount}   {totalGames}   {progress}%   {timer.Elapsed}");
                            }

                            white = 0;
                            black = 0;

                            stringBuilder = new StringBuilder(line);
                            totalGames++;
                            games++;
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

                elo.Add(Math.Round(100.0 * count / games, 6));

                int maxElo = _elo;
                for(int i = _elo+5;i < 4000; i += 5)
                {
                    var eloV = elos.Count(a => a >= i);
                    Console.WriteLine($"{i}   {eloV}");
                    if(eloV < _eloCount)
                    {
                        break;
                    }
                    maxElo = i;
                }

                _suggestedElos[file] = maxElo;

                Console.WriteLine($"Suggested = {_suggestedElos[file]}");
            }

            elo.Add(Math.Round(100.0 * totalCount / totalGames, 6));

            Console.WriteLine("   ------    ");
            Console.WriteLine(_elo);
            
            for (int i = 0; i < elo.Count - 1; i++)
            {
                double e = elo[i];
                Console.WriteLine($"\t{e}%");
            }
            Console.WriteLine($"Total {elo.Last()}%");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());

            Console.WriteLine("Pizdets !!!");
        }
    }
}