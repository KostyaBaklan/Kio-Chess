namespace StockfishBenchmark.Models;

public class BenchmarkResult
{
    public string  Version { get; set; } = "1.0";
    public BenchmarkSession Session { get; set; }
    public List<BenchmarkMoveRecord> Moves { get; set; } = new();
    public BenchmarkSummary Summary { get; set; }

    public static BenchmarkSummary ComputeSummary(List<BenchmarkMoveRecord> moves)
    {
        var eng = moves.Where(m => m.IsEngineMove).ToList();
        if (eng.Count == 0) return new BenchmarkSummary();

        var durations = eng.Select(m => m.DurationMs).OrderBy(d => d).ToList();
        double avg = durations.Average();
        double variance = durations.Sum(d => (d - avg) * (d - avg)) / durations.Count;

        return new BenchmarkSummary
        {
            TotalEngineMoves   = eng.Count,
            TotalDurationMs    = durations.Sum(),
            AvgDurationMs      = avg,
            MinDurationMs      = durations.First(),
            MaxDurationMs      = durations.Last(),
            StdDevDurationMs   = Math.Sqrt(variance),
            MedianDurationMs   = durations[durations.Count / 2],
            FinalTtCount       = eng.Last().TtCount,
            MaxTtCount         = eng.Max(m => m.TtCount),
            AvgTtCount         = eng.Average(m => m.TtCount),
            MaxProcessMemoryMB = moves.Max(m => m.ProcessMemoryMB),
            AvgProcessMemoryMB = moves.Average(m => m.ProcessMemoryMB)
        };
    }
}
