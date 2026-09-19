namespace StockfishBenchmark.Models;

public class BenchmarkSession
{
    public string Strategy { get; set; }      // "lmr" | "lmrd" | "id" | "asp"
    public short  Depth    { get; set; }      // 8–11
    public string Color    { get; set; }      // "w" = engine Black | "b" = engine White
    public int    MoveCount { get; set; }     // number of engine moves to record
    public short  StockfishDepth { get; set; }
    public int    StockfishElo   { get; set; }
    public DateTime RunDate        { get; set; }
    public string   MachineName    { get; set; }
    public int      ProcessorCount { get; set; }
    public string   RuntimeVersion { get; set; }

    public string DisplayName =>
        $"{Strategy.ToUpper()} d{Depth} {(Color == "b" ? "White" : "Black")} " +
        $"{MoveCount}moves {RunDate:yyyy-MM-dd HH:mm}";
}
