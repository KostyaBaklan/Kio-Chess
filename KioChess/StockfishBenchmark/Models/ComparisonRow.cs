namespace StockfishBenchmark.Models;

public class ComparisonRow
{
    public int    MoveNumber      { get; set; }
    public string UciMove         { get; set; }
    public string MoveNotation    { get; set; }
    public double BaselineDurationMs { get; set; }
    public double NewDurationMs      { get; set; }
    public double DeltaMs      => NewDurationMs - BaselineDurationMs;
    public double DeltaPercent => BaselineDurationMs > 0
        ? (NewDurationMs - BaselineDurationMs) / BaselineDurationMs * 100.0
        : 0.0;
    public int    BaselineTtCount { get; set; }
    public int    NewTtCount      { get; set; }
    public int    DeltaTtCount    => NewTtCount - BaselineTtCount;
    public double BaselineMemoryMB { get; set; }
    public double NewMemoryMB      { get; set; }
    public double DeltaMemoryMB    => NewMemoryMB - BaselineMemoryMB;
    public bool   IsRegression { get; set; }  // engine chose a different move
    public bool   IsDiverged   { get; set; }  // position diverged due to earlier regression
    public string Status => IsDiverged   ? "DIVERGED"
                          : IsRegression ? "REGRESSION"
                          : DeltaPercent < -1.0 ? "FASTER"
                          : DeltaPercent >  1.0 ? "SLOWER"
                          : "SAME";
}
