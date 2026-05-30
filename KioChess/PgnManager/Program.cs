using Engine.Interfaces.Config;
using Engine.Pgn.Streaming;
using GamesServices;
using System.Diagnostics;
using Tools.Common;

namespace PgnManager;

internal class Program
{
    private static int _elo;
    private static int _eloCount;
    private static int _configElo;
    private static Dictionary<string, int> _suggestedElos = null!;
    private static async Task Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        IBookConfiguration bookConfiguration = Boot.GetService<IConfigurationProvider>().BookConfiguration;
        _elo = bookConfiguration.Elo;
        _eloCount = bookConfiguration.EloCount;
        _configElo = _elo;

        try
        {
            
            CountElo(timer);

            await ProcessPgnFilesAsync(timer);

        }
        finally
        {

        }

        timer.Stop();

        Console.WriteLine(timer.Elapsed);

        Console.WriteLine("PGN DONE !!!");

        Console.ReadLine();
    }

    private static async Task ProcessPgnFilesAsync(Stopwatch timer)
    {
#if DEBUG
        var process = Process.Start(@$"..\..\..\GsServer\bin\Debug\net9.0\GsServer.exe");
        process.WaitForExit(100);
#else
        var process = Process.Start(@$"..\..\..\GsServer\bin\Release\net9.0\GsServer.exe");
        process.WaitForExit(100);
#endif

        SequenceClient client = new SequenceClient();
        var serviceClient = client.GetClient();
        await serviceClient.CallAsync("Initialize");

        int totalCount = 0;

        // Calculate optimal parallelism based on processor count
        // For I/O-bound external processes, use more than CPU count
        int maxParallelism = 7 * Environment.ProcessorCount / 10;
        Console.WriteLine($"Using {maxParallelism} parallel tasks (Processor count: {Environment.ProcessorCount})");

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
            {
                var file = files[fileIndex];
                int fileElo = _suggestedElos != null && _suggestedElos.TryGetValue(file, out var elo) ? elo : _elo;

                Console.WriteLine($"\nProcessing file {fileIndex + 1}/{files.Length}: {Path.GetFileName(file)}");
                Console.WriteLine($"Using ELO threshold: {fileElo}");

                using (var streamer = new PgnGameTextStreamer(file))
                {
                    // SemaphoreSlim for efficient concurrency control
                    using var semaphore = new SemaphoreSlim(maxParallelism, maxParallelism);

                    // Pre-allocate list with estimated capacity to avoid resizing
                    var tasks = new List<Task>(capacity: 1000);

                    // Ultra-fast streaming with inline ELO filter
                    foreach (var game in streamer.StreamWithEloFilter(fileElo))
                    {
                        totalCount++;
                        var localCount = totalCount;

                        // Wait for available slot
                        await semaphore.WaitAsync();

                        var task = Task.Run(async () =>
                        {
                            try
                            {
                                var t = Stopwatch.StartNew();

                                ProcessStartInfo info = new ProcessStartInfo
                                {
                                    FileName = "PgnTool.exe",
                                    ArgumentList = { game.GameText }
                                };

                                var proc = Process.Start(info);
                                await proc.WaitForExitAsync();

                                t.Stop();

                                // Capture progress at logging time, not at game capture time
                                var currentProgress = streamer.ProgressPercent;
                                Console.WriteLine($"{fileIndex + 1}/{files.Length}   {localCount}   {currentProgress:F6}%   {t.Elapsed}   {timer.Elapsed}");
                            }
                            finally
                            {
                                // Release slot for next task
                                semaphore.Release();
                            }
                        });

                        tasks.Add(task);
                    }

                    // Wait for all tasks to complete before proceeding
                    // Critical: ensures all games are processed before file deletion
                    await Task.WhenAll(tasks);
                }

                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to delete '{file}'");
                }
            }

            await serviceClient.CallAsync("Save");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());
            Console.WriteLine("Pizdets !!!");
        }
        finally
        {
            await client.CloseAsync();

            if (_suggestedElos != null)
            {
                foreach (var file in _suggestedElos)
                {
                    Console.WriteLine($"Finished '{file.Key}' ELO = {file.Value}");
                }
            }
        }
    }

    private static void CountElo(Stopwatch timer)
    {
        int totalCount = 0;
        int totalGames = 0;
        List<double> percentages = new List<double>();

        _suggestedElos = new Dictionary<string, int>();

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
            {
                var file = files[fileIndex];
                Console.WriteLine($"\nAnalyzing file {fileIndex + 1}/{files.Length}: {Path.GetFileName(file)}");

                using (var streamer = new PgnGameTextStreamer(file))
                {
                    List<EloStatistic> eloStats = new List<EloStatistic>();

                    foreach (var game in streamer.CollectEloStatistics())
                    {
                        // Just count games, no processing
                        totalGames++;
                        if(game.MinElo >= _elo)
                        {
                            totalCount++;
                            eloStats.Add(game);

                            if (totalCount%1000 == 0)
                            {
                                Console.WriteLine($" T={timer.Elapsed} C={totalCount}  P={streamer.ProgressPercent:F6}%"); 
                            }
                        }
                    }

                    int gamesAboveThreshold = eloStats.Count(e => e.MinElo >= _elo);
                    

                    // Build ELO distribution
                    var eloDistribution = new Dictionary<int, int>();
                    for (int elo = _elo; elo < 4000; elo += 5)
                    {
                        eloDistribution[elo] = eloStats.Count(e => e.MinElo >= elo);
                    }

                    // Find optimal ELO threshold
                    int suggestedElo = _elo;
                    foreach (var kvp in eloDistribution.OrderBy(x => x.Key))
                    {
                        Console.WriteLine($"  {kvp.Key}   {kvp.Value}");

                        if (kvp.Value < _eloCount)
                        {
                            break;
                        }
                        suggestedElo = kvp.Key;
                    }

                    _suggestedElos[file] = suggestedElo;

                    double percentage = eloStats.Count > 0
                        ? Math.Round(100.0 * gamesAboveThreshold / eloStats.Count, 6)
                        : 0;
                    percentages.Add(percentage);

                    Console.WriteLine($"  Total games: {eloStats.Count}");
                    Console.WriteLine($"  Games >= {_elo}: {gamesAboveThreshold} ({percentage}%)");
                    Console.WriteLine($"  Suggested ELO: {suggestedElo}");
                }
            }

            // Print summary
            double overallPercentage = totalGames > 0
                ? Math.Round(100.0 * totalCount / totalGames, 6)
                : 0;

            Console.WriteLine("\n   ------    ");
            Console.WriteLine(_elo);

            for (int i = 0; i < percentages.Count; i++)
            {
                Console.WriteLine($"\t{percentages[i]}%");
            }

            Console.WriteLine($"Total {overallPercentage}%");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());
            Console.WriteLine("Pizdets !!!");
        }
    }
}