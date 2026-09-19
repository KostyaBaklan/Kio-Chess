namespace StockfishBenchmark.Models;

public class BenchmarkSummary
{
    public int    TotalEngineMoves   { get; set; }
    public double TotalDurationMs    { get; set; }
    public double AvgDurationMs      { get; set; }
    public double MinDurationMs      { get; set; }
    public double MaxDurationMs      { get; set; }
    public double StdDevDurationMs   { get; set; }
    public double MedianDurationMs   { get; set; }
    public int    FinalTtCount       { get; set; }
    public int    MaxTtCount         { get; set; }
    public double AvgTtCount         { get; set; }
    public double MaxProcessMemoryMB { get; set; }
    public double AvgProcessMemoryMB { get; set; }
}
