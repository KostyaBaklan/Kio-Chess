using System.Diagnostics;

namespace Engine.Pgn.Streaming;

/// <summary>
/// Analyzes ELO distribution in large PGN files to find optimal filtering thresholds.
/// Optimized for 200GB+ files from lichess.com
/// </summary>
public class PgnEloAnalyzer
{
    public class EloAnalysisResult
    {
        public string FilePath { get; set; }
        public int TotalGames { get; set; }
        public int GamesAboveThreshold { get; set; }
        public int SuggestedElo { get; set; }
        public int ConfiguredElo { get; set; }
        public double PercentageAboveThreshold { get; set; }
        public Dictionary<int, int> EloDistribution { get; set; }
        public TimeSpan AnalysisTime { get; set; }

        public EloAnalysisResult()
        {
            EloDistribution = new Dictionary<int, int>();
        }
    }

    private readonly int _targetGameCount;
    private readonly int _startElo;
    private readonly int _maxElo;
    private readonly int _step;

    public PgnEloAnalyzer(int targetGameCount, int startElo = 0, int maxElo = 4000, int step = 5)
    {
        _targetGameCount = targetGameCount;
        _startElo = startElo;
        _maxElo = maxElo;
        _step = step;
    }

    /// <summary>
    /// Analyzes a single PGN file to determine optimal ELO threshold.
    /// Streams through file once, collecting ELO statistics.
    /// </summary>
    public EloAnalysisResult AnalyzeFile(string filePath, Action<ProgressInfo> onProgress = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new EloAnalysisResult
        {
            FilePath = filePath,
            ConfiguredElo = _startElo
        };

        using (var reader = new PgnStreamReader(filePath))
        {
            // Collect all ELOs
            var elos = new List<int>();
            int gameCount = 0;

            var games = reader.ReadGameInfo();

            if (onProgress != null)
            {
                games = games.WithProgress(reader, onProgress, reportEveryN: 1000);
            }

            foreach (var game in games)
            {
                gameCount++;
                if (game.MinElo >= _startElo)
                {
                    elos.Add(game.MinElo);
                }
            }

            result.TotalGames = gameCount;
            result.GamesAboveThreshold = elos.Count;

            // Calculate distribution
            for (int elo = _startElo; elo <= _maxElo; elo += _step)
            {
                result.EloDistribution[elo] = elos.Count(e => e >= elo);
            }

            // Find optimal ELO
            result.SuggestedElo = _startElo;
            foreach (var kvp in result.EloDistribution.OrderBy(x => x.Key))
            {
                if (kvp.Value < _targetGameCount)
                {
                    break;
                }
                result.SuggestedElo = kvp.Key;
            }

            result.PercentageAboveThreshold = gameCount > 0 
                ? Math.Round(100.0 * elos.Count / gameCount, 2)
                : 0;
        }

        stopwatch.Stop();
        result.AnalysisTime = stopwatch.Elapsed;

        return result;
    }

    /// <summary>
    /// Analyzes multiple PGN files in a directory.
    /// Returns analysis results for each file.
    /// </summary>
    public List<EloAnalysisResult> AnalyzeDirectory(
        string directoryPath,
        string searchPattern = "*.pgn",
        Action<string, int, int> onFileStart = null,
        Action<ProgressInfo> onProgress = null)
    {
        var files = Directory.GetFiles(directoryPath, searchPattern);
        var results = new List<EloAnalysisResult>();

        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            onFileStart?.Invoke(file, i + 1, files.Length);

            var result = AnalyzeFile(file, onProgress);
            results.Add(result);

            Console.WriteLine($"File {i + 1}/{files.Length}: {Path.GetFileName(file)}");
            Console.WriteLine($"  Total Games: {result.TotalGames}");
            Console.WriteLine($"  Games >= {_startElo}: {result.GamesAboveThreshold} ({result.PercentageAboveThreshold}%)");
            Console.WriteLine($"  Suggested ELO: {result.SuggestedElo}");
            Console.WriteLine($"  Analysis Time: {result.AnalysisTime}");
            Console.WriteLine();
        }

        return results;
    }

    /// <summary>
    /// Prints detailed ELO distribution table for a result.
    /// </summary>
    public void PrintDistribution(EloAnalysisResult result, int minCount = 0)
    {
        Console.WriteLine($"\nELO Distribution for {Path.GetFileName(result.FilePath)}:");
        Console.WriteLine("ELO\tGames");
        Console.WriteLine("---\t-----");

        foreach (var kvp in result.EloDistribution.OrderBy(x => x.Key))
        {
            if (kvp.Value >= minCount)
            {
                Console.WriteLine($"{kvp.Key}\t{kvp.Value}");
            }
        }
    }

    /// <summary>
    /// Creates a summary report of all analyzed files.
    /// </summary>
    public void PrintSummary(List<EloAnalysisResult> results)
    {
        Console.WriteLine("\n========== SUMMARY ==========");
        Console.WriteLine($"Configured ELO: {_startElo}");
        Console.WriteLine($"Target Game Count: {_targetGameCount}");
        Console.WriteLine($"Files Analyzed: {results.Count}");
        Console.WriteLine();

        int totalGames = results.Sum(r => r.TotalGames);
        int totalAboveThreshold = results.Sum(r => r.GamesAboveThreshold);
        double overallPercentage = totalGames > 0 ? Math.Round(100.0 * totalAboveThreshold / totalGames, 2) : 0;

        Console.WriteLine($"Total Games: {totalGames:N0}");
        Console.WriteLine($"Total Above {_startElo}: {totalAboveThreshold:N0} ({overallPercentage}%)");
        Console.WriteLine();

        Console.WriteLine("Per-File Results:");
        Console.WriteLine("File\tGames\tSuggested ELO\t% Above");
        Console.WriteLine("----\t-----\t-------------\t-------");

        foreach (var result in results)
        {
            Console.WriteLine($"{Path.GetFileName(result.FilePath)}\t{result.GamesAboveThreshold}\t{result.SuggestedElo}\t{result.PercentageAboveThreshold}%");
        }

        Console.WriteLine($"\nTotal Percentage: {overallPercentage}%");
        Console.WriteLine("============================\n");
    }
}
